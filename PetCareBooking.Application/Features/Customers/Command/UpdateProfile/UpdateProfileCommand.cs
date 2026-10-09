using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Customer;

namespace PetCareBooking.Application.Features.Customers.Command.UpdateProfile
{
    public class UpdateCustomerProfileCommand : IRequest<ApiResponse<CustomerProfileDto>>
    {
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
    }
}