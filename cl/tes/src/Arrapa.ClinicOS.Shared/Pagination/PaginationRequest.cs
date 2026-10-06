using Arrapa.ClinicOS.Shared.Constants;

namespace Arrapa.ClinicOS.Shared.Pagination;

/// <summary>
/// Standard pagination / sorting / search request supplied via query string.
/// </summary>
public class PaginationRequest
{
    private int _pageNumber = 1;
    private int _pageSize = AppConstants.Pagination.DefaultPageSize;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => AppConstants.Pagination.DefaultPageSize,
            > AppConstants.Pagination.MaxPageSize => AppConstants.Pagination.MaxPageSize,
            _ => value
        };
    }

    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
}

public enum SortDirection
{
    Ascending = 0,
    Descending = 1
}
