using Arrapa.ClinicOS.Application.Abstractions.Billing;
using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Application.Features.Billing.Common;
using Arrapa.ClinicOS.Contracts.Billing;
using Arrapa.ClinicOS.Domain.Billing;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Shared.Pagination;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Billing;

// ==================== List ====================
public sealed record ListInvoicesQuery(
    int PageNumber, int PageSize,
    Guid? PatientId, string? Status,
    DateTimeOffset? From, DateTimeOffset? To,
    string? Search) : IRequest<Result<PaginatedList<InvoiceSummaryResponse>>>;

public sealed class ListInvoicesQueryHandler : IRequestHandler<ListInvoicesQuery, Result<PaginatedList<InvoiceSummaryResponse>>>
{
    private readonly IApplicationDbContext _db;
    public ListInvoicesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<PaginatedList<InvoiceSummaryResponse>>> Handle(ListInvoicesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Invoices.AsNoTracking()
            .Include(i => i.Patient)
            .Include(i => i.Appointment)
            .AsQueryable();

        if (request.PatientId.HasValue) query = query.Where(i => i.PatientId == request.PatientId);
        if (request.From.HasValue) query = query.Where(i => i.IssuedAt >= request.From || i.CreatedOn >= request.From);
        if (request.To.HasValue) query = query.Where(i => (i.IssuedAt ?? i.CreatedOn) <= request.To);
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<InvoiceStatus>(request.Status, ignoreCase: true, out var s))
        {
            query = query.Where(i => i.Status == s);
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(i =>
                i.InvoiceNumber.ToLower().Contains(term) ||
                i.Patient!.MRN.ToLower().Contains(term) ||
                i.Patient!.FirstName.ToLower().Contains(term) ||
                i.Patient!.LastName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var rows = await query
            .OrderByDescending(i => i.CreatedOn)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedList<InvoiceSummaryResponse>(
            rows.Select(BillingMapping.ToSummary).ToList(),
            totalCount, request.PageNumber, request.PageSize);
    }
}

// ==================== Get ====================
public sealed record GetInvoiceQuery(Guid Id) : IRequest<Result<InvoiceDetailResponse>>;

public sealed class GetInvoiceQueryHandler : IRequestHandler<GetInvoiceQuery, Result<InvoiceDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    public GetInvoiceQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<InvoiceDetailResponse>> Handle(GetInvoiceQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _db.Invoices.AsNoTracking()
            .Include(i => i.Patient)
            .Include(i => i.Appointment)
            .Include(i => i.LineItems)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken).ConfigureAwait(false);

        return invoice is null ? BillingErrors.NotFound : BillingMapping.ToDetail(invoice);
    }
}

// ==================== Create ====================
public sealed record CreateInvoiceCommand(
    Guid PatientId, Guid? AppointmentId,
    DateTimeOffset? DueAt,
    decimal DiscountAmount, decimal TaxAmount,
    string? Currency, string? Notes,
    IReadOnlyList<CreateInvoiceLineItemRequest> LineItems) : IRequest<Result<InvoiceDetailResponse>>;

public sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TaxAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LineItems).NotNull().NotEmpty().WithMessage("At least one line item is required.");
        RuleForEach(x => x.LineItems).ChildRules(l =>
        {
            l.RuleFor(x => x.Description).NotEmpty().MaximumLength(300);
            l.RuleFor(x => x.Quantity).GreaterThan(0);
            l.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
            l.RuleFor(x => x.ServiceCode).MaximumLength(50);
        });
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Currency).MaximumLength(10);
    }
}

public sealed class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Result<InvoiceDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IInvoiceNumberGenerator _numberGenerator;
    private readonly ISender _mediator;

    public CreateInvoiceCommandHandler(
        IApplicationDbContext db,
        ITenantContext tenantContext,
        IInvoiceNumberGenerator numberGenerator,
        ISender mediator)
    {
        _db = db;
        _tenantContext = tenantContext;
        _numberGenerator = numberGenerator;
        _mediator = mediator;
    }

    public async Task<Result<InvoiceDetailResponse>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<InvoiceDetailResponse>.Failure(tenantIdResult.Error);
        }
        var tenantId = tenantIdResult.Value;

        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken).ConfigureAwait(false);
        if (patient is null)
        {
            return BillingErrors.PatientNotFound;
        }

        string? currency = string.IsNullOrWhiteSpace(request.Currency) ? null : request.Currency.Trim();

        if (request.AppointmentId.HasValue)
        {
            var appt = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken).ConfigureAwait(false);
            if (appt is null)
            {
                return BillingErrors.AppointmentNotFound;
            }

            var alreadyInvoiced = await _db.Invoices.AnyAsync(i =>
                i.AppointmentId == appt.Id &&
                i.Status != InvoiceStatus.Cancelled,
                cancellationToken).ConfigureAwait(false);
            if (alreadyInvoiced)
            {
                return BillingErrors.AppointmentAlreadyInvoiced;
            }

            if (currency is null && !string.IsNullOrWhiteSpace(appt.Currency))
            {
                currency = appt.Currency;
            }
        }

        if (currency is null)
        {
            var tenantCurrency = await _db.Tenants.IgnoreQueryFilters()
                .Where(t => t.Id == tenantId)
                .Select(t => t.Currency)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            currency = string.IsNullOrWhiteSpace(tenantCurrency) ? "USD" : tenantCurrency;
        }

        var invoiceNumber = await _numberGenerator.GenerateAsync(tenantId, cancellationToken).ConfigureAwait(false);

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InvoiceNumber = invoiceNumber,
            PatientId = patient.Id,
            AppointmentId = request.AppointmentId,
            Status = InvoiceStatus.Draft,
            DueAt = request.DueAt,
            DiscountAmount = request.DiscountAmount,
            TaxAmount = request.TaxAmount,
            Currency = currency,
            Notes = request.Notes?.Trim()
        };

        foreach (var l in request.LineItems)
        {
            var line = new InvoiceLineItem
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Description = l.Description.Trim(),
                ServiceCode = string.IsNullOrWhiteSpace(l.ServiceCode) ? null : l.ServiceCode.Trim(),
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = Math.Round(l.Quantity * l.UnitPrice, 4)
            };
            invoice.LineItems.Add(line);
        }

        invoice.RecalculateTotals();

        await _db.Invoices.AddAsync(invoice, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await _mediator.Send(new GetInvoiceQuery(invoice.Id), cancellationToken).ConfigureAwait(false);
    }
}

// ==================== Update (draft-only) ====================
public sealed record UpdateInvoiceCommand(
    Guid Id,
    DateTimeOffset? DueAt,
    decimal DiscountAmount, decimal TaxAmount,
    string? Notes,
    IReadOnlyList<CreateInvoiceLineItemRequest> LineItems) : IRequest<Result<InvoiceDetailResponse>>;

public sealed class UpdateInvoiceCommandValidator : AbstractValidator<UpdateInvoiceCommand>
{
    public UpdateInvoiceCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TaxAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LineItems).NotNull().NotEmpty();
        RuleForEach(x => x.LineItems).ChildRules(l =>
        {
            l.RuleFor(x => x.Description).NotEmpty().MaximumLength(300);
            l.RuleFor(x => x.Quantity).GreaterThan(0);
            l.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
            l.RuleFor(x => x.ServiceCode).MaximumLength(50);
        });
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public sealed class UpdateInvoiceCommandHandler : IRequestHandler<UpdateInvoiceCommand, Result<InvoiceDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly ISender _mediator;

    public UpdateInvoiceCommandHandler(IApplicationDbContext db, ITenantContext tenantContext, ISender mediator)
    {
        _db = db;
        _tenantContext = tenantContext;
        _mediator = mediator;
    }

    public async Task<Result<InvoiceDetailResponse>> Handle(UpdateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<InvoiceDetailResponse>.Failure(tenantIdResult.Error);
        }

        var invoice = await _db.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (invoice is null) return BillingErrors.NotFound;
        if (!invoice.CanEdit) return BillingErrors.NotEditable;

        invoice.DueAt = request.DueAt;
        invoice.DiscountAmount = request.DiscountAmount;
        invoice.TaxAmount = request.TaxAmount;
        invoice.Notes = request.Notes?.Trim();

        // Replace line items wholesale — draft only, simplest correct behavior
        foreach (var existing in invoice.LineItems.ToList())
        {
            _db.InvoiceLineItems.Remove(existing);
        }
        invoice.LineItems.Clear();

        foreach (var l in request.LineItems)
        {
            invoice.LineItems.Add(new InvoiceLineItem
            {
                Id = Guid.NewGuid(),
                TenantId = tenantIdResult.Value,
                InvoiceId = invoice.Id,
                Description = l.Description.Trim(),
                ServiceCode = string.IsNullOrWhiteSpace(l.ServiceCode) ? null : l.ServiceCode.Trim(),
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = Math.Round(l.Quantity * l.UnitPrice, 4)
            });
        }

        invoice.RecalculateTotals();
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await _mediator.Send(new GetInvoiceQuery(invoice.Id), cancellationToken).ConfigureAwait(false);
    }
}

// ==================== Issue (Draft → Issued) ====================
public sealed record IssueInvoiceCommand(Guid Id) : IRequest<Result>;

public sealed class IssueInvoiceCommandHandler : IRequestHandler<IssueInvoiceCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public IssueInvoiceCommandHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _db.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (invoice is null) return BillingErrors.NotFound;
        if (!invoice.CanIssue) return BillingErrors.NotIssuable;
        if (invoice.LineItems.Count == 0) return BillingErrors.NoLineItems;

        invoice.Status = InvoiceStatus.Issued;
        invoice.IssuedAt = _clock.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

// ==================== Cancel ====================
public sealed record CancelInvoiceCommand(Guid Id, string Reason) : IRequest<Result>;

public sealed class CancelInvoiceCommandValidator : AbstractValidator<CancelInvoiceCommand>
{
    public CancelInvoiceCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class CancelInvoiceCommandHandler : IRequestHandler<CancelInvoiceCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public CancelInvoiceCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(CancelInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (invoice is null) return BillingErrors.NotFound;
        if (!invoice.CanCancel) return BillingErrors.NotCancellable;

        invoice.Status = InvoiceStatus.Cancelled;
        invoice.CancelledAt = _clock.UtcNow;
        invoice.CancelledByUserId = _currentUser.UserId;
        invoice.CancellationReason = request.Reason.Trim();

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

// ==================== Record payment ====================
public sealed record RecordPaymentCommand(
    Guid InvoiceId,
    decimal Amount,
    string Method,
    string? Reference,
    DateTimeOffset? PaidAt,
    string? Notes) : IRequest<Result<InvoiceDetailResponse>>;

public sealed class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Method).NotEmpty();
        RuleFor(x => x.Reference).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, Result<InvoiceDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly ISender _mediator;

    public RecordPaymentCommandHandler(
        IApplicationDbContext db,
        ITenantContext tenantContext,
        ICurrentUserService currentUser,
        IDateTimeProvider clock,
        ISender mediator)
    {
        _db = db;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _clock = clock;
        _mediator = mediator;
    }

    public async Task<Result<InvoiceDetailResponse>> Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<InvoiceDetailResponse>.Failure(tenantIdResult.Error);
        }

        if (!Enum.TryParse<PaymentMode>(request.Method, ignoreCase: true, out var method))
        {
            return BillingErrors.InvalidPaymentMethod;
        }

        var invoice = await _db.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken).ConfigureAwait(false);
        if (invoice is null) return BillingErrors.NotFound;
        if (!invoice.CanPay) return BillingErrors.NotPayable;

        var paidAt = request.PaidAt ?? _clock.UtcNow;
        if (paidAt > _clock.UtcNow.AddMinutes(1))
        {
            return BillingErrors.PaymentInFuture;
        }

        var remaining = invoice.Total - invoice.AmountPaid;
        if (request.Amount > remaining + 0.0001m)
        {
            return BillingErrors.PaymentTooLarge;
        }

        var payment = new InvoicePayment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantIdResult.Value,
            InvoiceId = invoice.Id,
            Amount = request.Amount,
            Method = method,
            Reference = string.IsNullOrWhiteSpace(request.Reference) ? null : request.Reference.Trim(),
            PaidAt = paidAt,
            RecordedByUserId = _currentUser.UserId ?? Guid.Empty,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };
        invoice.Payments.Add(payment);

        invoice.RecalculatePaid();
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await _mediator.Send(new GetInvoiceQuery(invoice.Id), cancellationToken).ConfigureAwait(false);
    }
}

// ==================== Void a payment (soft-delete) ====================
public sealed record VoidPaymentCommand(Guid InvoiceId, Guid PaymentId) : IRequest<Result<InvoiceDetailResponse>>;

public sealed class VoidPaymentCommandHandler : IRequestHandler<VoidPaymentCommand, Result<InvoiceDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ISender _mediator;

    public VoidPaymentCommandHandler(IApplicationDbContext db, ISender mediator)
    {
        _db = db;
        _mediator = mediator;
    }

    public async Task<Result<InvoiceDetailResponse>> Handle(VoidPaymentCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken).ConfigureAwait(false);
        if (invoice is null) return BillingErrors.NotFound;

        var payment = invoice.Payments.FirstOrDefault(p => p.Id == request.PaymentId && !p.IsDeleted);
        if (payment is null) return BillingErrors.PaymentNotFound;

        // Soft delete via ISoftDelete flag — the interceptor keeps the row for audit
        payment.IsDeleted = true;

        invoice.RecalculatePaid();
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await _mediator.Send(new GetInvoiceQuery(invoice.Id), cancellationToken).ConfigureAwait(false);
    }
}

// ==================== Delete (Draft only) ====================
public sealed record DeleteInvoiceCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteInvoiceCommandHandler : IRequestHandler<DeleteInvoiceCommand, Result>
{
    private readonly IApplicationDbContext _db;
    public DeleteInvoiceCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (invoice is null) return BillingErrors.NotFound;
        if (!invoice.CanEdit) return BillingErrors.NotEditable;

        _db.Invoices.Remove(invoice);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
