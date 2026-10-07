using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Customer> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateCustomerCommandHandler(IGenericRepository<Customer> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var isExisted = await _repository.GetQueryable()
                .AnyAsync(c => c.Email == request.Email || c.PhoneNumber == request.PhoneNumber, cancellationToken);

            if (isExisted)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Email or phone number already exists in the system."
                };
            }

            var customer = new Customer
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                IsActive = true
            };

            await _repository.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status201Created,
                Message = "Customer account created successfully.",
                Result = customer.Id
            };
        }
    }
}