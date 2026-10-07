using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Customers.Commands.DeleteCustomer
{
    public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<Customer> _customerRepository;
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCustomerCommandHandler(
            IGenericRepository<Customer> customerRepository,
            IGenericRepository<Booking> bookingRepository,
            IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.Id);
            if (customer == null)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Customer to delete was not found."
                };
            }

            bool hasBookings = await _bookingRepository.GetQueryable()
                .AnyAsync(b => b.CustomerId == request.Id, cancellationToken);

            if (hasBookings)
            {
                customer.IsActive = false;
                customer.UpdatedAt = DateTime.UtcNow;
                _customerRepository.Update(customer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new ApiResponse<bool>
                {
                    IsSuccess = true,
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Customer has booking history; account status was safely changed to Deactivated.",
                    Result = true
                };
            }
            else
            {
                _customerRepository.Delete(customer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new ApiResponse<bool>
                {
                    IsSuccess = true,
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Customer profile was permanently deleted from system.",
                    Result = true
                };
            }
        }
    }
}