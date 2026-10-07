using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Customer;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Customers.Queries.GetCustomerById
{
    public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, ApiResponse<CustomerResponseDTO>>
    {
        private readonly IGenericRepository<Customer> _repository;

        public GetCustomerByIdQueryHandler(IGenericRepository<Customer> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<CustomerResponseDTO>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            var customer = await _repository.GetQueryable()
                .Where(c => c.Id == request.Id)
                .Select(c => new CustomerResponseDTO
                {
                    Id = c.Id,
                    Email = c.Email,
                    FullName = c.FullName,
                    PhoneNumber = c.PhoneNumber,
                    Address = c.Address,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    Pets = c.Pets.Select(p => new PetSummaryDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Species = p.Species,
                        Breed = p.Breed,
                        Weight = p.Weight,
                        HealthNotes = p.HealthNotes,
                        IsActive = p.IsActive
                    }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (customer == null)
            {
                return new ApiResponse<CustomerResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Customer not found."
                };
            }

            return new ApiResponse<CustomerResponseDTO>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Result = customer
            };
        }
    }
}