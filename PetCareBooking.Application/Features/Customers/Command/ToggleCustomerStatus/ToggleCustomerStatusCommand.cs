using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.Customers.Commands.ToggleCustomerStatus
{
    public class ToggleCustomerStatusCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
    }
}