using Microsoft.EntityFrameworkCore;

namespace Core.Utilities
{
    public static class PaginationHelper
    {
        public static async Task<(int page, int pageSize)> NormalizePaginationAsync<T>(IQueryable<T> query, int page, int pageSize, int defaultPageSize = 10)
        {
            page = Math.Max(page, 1);
            pageSize = pageSize == 0 ? defaultPageSize : pageSize;

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
            page = Math.Min(page, totalPages == 0 ? 1 : totalPages);

            return (page, pageSize);
        }
    }
}
