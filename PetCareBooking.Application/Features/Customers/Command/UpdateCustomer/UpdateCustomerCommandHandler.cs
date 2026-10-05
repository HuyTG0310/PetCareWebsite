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
                    Message = "Không tìm thấy khách hàng."
                };
            }

            // Chỉ cập nhật Họ và tên nếu được truyền lên
            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                customer.FullName = request.FullName;
            }

            // Chỉ cập nhật và kiểm tra trùng Số điện thoại nếu được truyền lên
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
                        Message = "Số điện thoại này đã được sử dụng bởi khách hàng khác."
                    };
                }

                customer.PhoneNumber = request.PhoneNumber;
            }

            // Cập nhật Địa chỉ nếu được truyền lên (cho phép truyền rỗng hoặc địa chỉ mới)
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
                Message = "Cập nhật thông tin khách hàng thành công.",
                Result = true
            };
        }
    }
}