using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Customer;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Customers.Command.UpdateProfile
{
    public class UpdateCustomerProfileHandler : IRequestHandler<UpdateCustomerProfileCommand, ApiResponse<CustomerProfileDto>>
    {
        private readonly IGenericRepository<Customer> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateCustomerProfileHandler(
            IGenericRepository<Customer> repository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<CustomerProfileDto>> Handle(UpdateCustomerProfileCommand request, CancellationToken cancellationToken)
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

            // Check if phone number is already registered by any other account
            var isPhoneTaken = await _repository.GetQueryable()
                .AnyAsync(c => c.PhoneNumber == request.PhoneNumber && c.Id != customer.Id, cancellationToken);

            if (isPhoneTaken)
            {
                return new ApiResponse<CustomerProfileDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Phone number is already associated with another account."
                };
            }

            // Update attributes
            customer.FullName = request.FullName;
            customer.PhoneNumber = request.PhoneNumber;
            customer.Address = request.Address;
            customer.AvatarUrl = request.AvatarUrl;
            customer.UpdatedAt = DateTime.UtcNow;

            _repository.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<CustomerProfileDto>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Profile updated successfully.",
                Result = new CustomerProfileDto
                {
                    Id = customer.Id,
                    Email = customer.Email,
                    FullName = customer.FullName,
                    PhoneNumber = customer.PhoneNumber,
                    Address = customer.Address,
                    AvatarUrl = customer.AvatarUrl,
                    CreatedAt = customer.CreatedAt
                }
            };
        }
    }
}