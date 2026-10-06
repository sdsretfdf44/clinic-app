using Arrapa.ClinicOS.Application.Features.Admin.Tenants.ActivateTenant;
using Arrapa.ClinicOS.Application.Features.Admin.Tenants.AssignPlan;
using Arrapa.ClinicOS.Application.Features.Admin.Tenants.CreateTenant;
using Arrapa.ClinicOS.Application.Features.Admin.Tenants.DeleteTenant;
using Arrapa.ClinicOS.Application.Features.Admin.Tenants.GetTenant;
using Arrapa.ClinicOS.Application.Features.Admin.Tenants.ListTenants;
using Arrapa.ClinicOS.Application.Features.Admin.Tenants.SuspendTenant;
using Arrapa.ClinicOS.Application.Features.Admin.Tenants.UpdateTenant;
using Arrapa.ClinicOS.Contracts.Admin.Tenants;
using Arrapa.ClinicOS.Shared.Pagination;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Admin;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/tenants")]
public sealed class TenantsController : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] SortDirection sortDirection = SortDirection.Descending,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new ListTenantsQuery(pageNumber, pageSize, search, status, sortBy, sortDirection), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetTenantQuery(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTenantCommand(
            request.Name, request.Slug, request.ContactEmail, request.ContactPhone,
            request.TimeZone, request.Locale, request.Currency, request.SubscriptionPlanId,
            request.AdminFirstName, request.AdminLastName, request.AdminEmail);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Tenant created.");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateTenantCommand(id, request.Name, request.ContactEmail, request.ContactPhone,
            request.LogoUrl, request.TimeZone, request.Locale, request.Currency);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Tenant updated.");
    }

    [HttpPost("{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, [FromBody] SuspendTenantRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SuspendTenantCommand(id, request.Reason), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Tenant suspended.");
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new ActivateTenantCommand(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Tenant activated.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeleteTenantCommand(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Tenant deleted.");
    }

    [HttpPost("{id:guid}/plan")]
    public async Task<IActionResult> AssignPlan(Guid id, [FromBody] AssignPlanRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new AssignPlanCommand(id, request.SubscriptionPlanId, request.ExpiresAt), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Plan assigned.");
    }
}
