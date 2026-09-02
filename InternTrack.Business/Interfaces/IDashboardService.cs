using InternTrack.Business.Common;
using InternTrack.Core.DTOs;

namespace InternTrack.Business.Interfaces;

public interface IDashboardService
{
    Task<ServiceResult<DashboardStatsDto>> GetStatsAsync(
        int userId,
        string role
    );
}