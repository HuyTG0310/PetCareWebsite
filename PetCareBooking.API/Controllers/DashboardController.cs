using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.API.Attributes;
using PetCareBooking.Application.Features.Dashboard.Queries.GetDashboardStats;

namespace PetCareBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequirePermission("DASHBOARD_VIEW")]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// [Admin / Receptionist] Lấy toàn bộ số liệu thống kê tổng quan (Doanh thu, Đơn đặt lịch, Trạng thái phòng, Công việc hôm nay, Biểu đồ 7 ngày)
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var response = await _mediator.Send(new GetDashboardStatsQuery());
            return StatusCode(response.StatusCode, response);
        }
    }
}
