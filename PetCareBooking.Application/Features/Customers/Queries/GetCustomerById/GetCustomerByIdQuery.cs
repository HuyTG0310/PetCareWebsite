using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Customer;

namespace PetCareBooking.Application.Features.Customers.Queries.GetCustomerById
{
    public class GetCustomerByIdQuery : IRequest<ApiResponse<CustomerResponseDTO>>
    {
        public Guid Id { get; set; }
    }
}