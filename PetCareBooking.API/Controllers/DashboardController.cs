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
        /// [Admin / Receptionist] Get overall dashboard statistics (Revenue, Bookings, Room status, Today tasks, 7-day chart)
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var response = await _mediator.Send(new GetDashboardStatsQuery());
            return StatusCode(response.StatusCode, response);
        }
    }
}
