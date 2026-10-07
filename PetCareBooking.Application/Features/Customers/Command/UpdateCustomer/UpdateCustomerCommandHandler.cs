using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Customers.Commands.UpdateCustomer
{
    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<Customer> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCustomerCommandHandler(IGenericRepository<Customer> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _repository.GetByIdAsync(request.Id);
            if (customer == null)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Customer not found."
                };
            }

            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                customer.FullName = request.FullName;
            }

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                var isPhoneExisted = await _repository.GetQueryable()
                    .AnyAsync(c => c.PhoneNumber == request.PhoneNumber && c.Id != request.Id, cancellationToken);

                if (isPhoneExisted)
                {
                    return new ApiResponse<bool>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Phone number is already in use by another customer."
                    };
                }

                customer.PhoneNumber = request.PhoneNumber;
            }

            if (request.Address != null)
            {
                customer.Address = request.Address;
            }

            customer.UpdatedAt = DateTime.UtcNow;

            _repository.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<bool>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Customer information updated successfully.",
                Result = true
            };
        }
    }
}