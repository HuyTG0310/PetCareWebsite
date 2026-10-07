using MediatR;
using Microsoft.AspNetCore.Http;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Customers.Commands.ToggleCustomerStatus
{
    public class ToggleCustomerStatusCommandHandler : IRequestHandler<ToggleCustomerStatusCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<Customer> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ToggleCustomerStatusCommandHandler(IGenericRepository<Customer> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(ToggleCustomerStatusCommand request, CancellationToken cancellationToken)
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

            customer.IsActive = !customer.IsActive;

            _repository.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<bool>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = customer.IsActive ? "Account unlocked successfully." : "Account locked successfully.",
                Result = customer.IsActive
            };
        }
    }
}