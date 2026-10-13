using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Dashboard;

namespace PetCareBooking.Application.Features.Dashboard.Queries.GetDashboardStats
{
    public class GetDashboardStatsQuery : IRequest<ApiResponse<DashboardStatsResponseDTO>>
    {
    }
}

