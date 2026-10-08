using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Customer;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Customers.Queries.GetProfile
{
    public class GetCustomerProfileQueryHandler : IRequestHandler<GetCustomerProfileQuery, ApiResponse<CustomerProfileDto>>
    {
        private readonly IGenericRepository<Customer> _repository;
        private readonly ICurrentUserService _currentUserService;

        public GetCustomerProfileQueryHandler(
            IGenericRepository<Customer> repository,
            ICurrentUserService currentUserService)
        {
            _repository = repository;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<CustomerProfileDto>> Handle(GetCustomerProfileQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return new ApiResponse<CustomerProfileDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized access."
                };
            }

            var customer = await _repository.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == userId.Value, cancellationToken);

            if (customer == null || !customer.IsActive)
            {
                return new ApiResponse<CustomerProfileDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Account is inactive or not found."
                };
            }

            var result = new CustomerProfileDto
            {
                Id = customer.Id,
                Email = customer.Email,
                FullName = customer.FullName,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                AvatarUrl = customer.AvatarUrl,
                CreatedAt = customer.CreatedAt
            };

            return new ApiResponse<CustomerProfileDto>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Profile retrieved successfully.",
                Result = result
            };
        }
    }
}
