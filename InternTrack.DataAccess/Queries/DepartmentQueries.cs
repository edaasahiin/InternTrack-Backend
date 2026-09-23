using InternTrack.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace InternTrack.DataAccess.Queries;

internal static class DepartmentQueries
{
    // Composes the database query; execution stays in DepartmentRepository.
    public static IQueryable<Department> ByNameIncludingInactive(
        IQueryable<Department> departments,
        string name,
        int? excludeId)
    {
        var normalizedName = name.Trim().ToLowerInvariant();

        var query = departments.IgnoreQueryFilters()
            .Where(department => department.Name.Trim().ToLower() == normalizedName);

        if (excludeId.HasValue)
        {
            query = query.Where(department => department.Id != excludeId.Value);
        }

        return query;
    }
}
