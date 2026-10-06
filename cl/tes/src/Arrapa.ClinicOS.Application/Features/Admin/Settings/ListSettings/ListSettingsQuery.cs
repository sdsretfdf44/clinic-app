using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Contracts.Admin.Settings;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.Settings.ListSettings;

public sealed record ListSettingsQuery(string? Category) : IRequest<Result<IReadOnlyList<GlobalSettingResponse>>>;

public sealed class ListSettingsQueryHandler : IRequestHandler<ListSettingsQuery, Result<IReadOnlyList<GlobalSettingResponse>>>
{
    private readonly IApplicationDbContext _db;

    public ListSettingsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<GlobalSettingResponse>>> Handle(ListSettingsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.GlobalSettings.AsNoTracking().IgnoreQueryFilters().Where(s => !s.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(s => s.Category == request.Category);
        }

        var settings = await query
            .OrderBy(s => s.Category).ThenBy(s => s.Key)
            .Select(s => new GlobalSettingResponse(
                s.Id,
                s.Key,
                s.IsSecret ? "***" : s.Value,
                s.Category,
                s.DataType.ToString(),
                s.Description,
                s.IsSecret,
                s.UpdatedOn))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return settings;
    }
}
