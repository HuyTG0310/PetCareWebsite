using MediatR;
using Microsoft.AspNetCore.Http;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Pets.Commands.CreatePet
{
    public class CreatePetCommandHandler : IRequestHandler<CreatePetCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Pet> _petRepository;
        private readonly IGenericRepository<Customer> _customerRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePetCommandHandler(
            IGenericRepository<Pet> petRepository,
            IGenericRepository<Customer> customerRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _petRepository = petRepository;
            _customerRepository = customerRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(CreatePetCommand request, CancellationToken cancellationToken)
        {
            Guid targetCustomerId = request.CustomerId;

            // Phân quyền tạo thú cưng:
            if (!_currentUserService.IsAdminOrStaff)
            {
                // Customer thông thường: Bắt buộc lấy CustomerId từ Token đăng nhập (ngăn chặn IDOR)
                if (!_currentUserService.UserId.HasValue)
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = "User is not authenticated.",
                        Result = Guid.Empty
                    };
                }

                targetCustomerId = _currentUserService.UserId.Value;
            }
            else
            {
                // Nhân viên/Admin tạo hộ tại quầy: Bắt buộc chỉ định CustomerId hợp lệ
                if (targetCustomerId == Guid.Empty)
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "CustomerId is required when staff creates a pet on behalf of a customer.",
                        Result = Guid.Empty
                    };
                }
            }

            var customer = await _customerRepository.GetByIdAsync(targetCustomerId);
            if (customer == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Customer not found.",
                    Result = Guid.Empty
                };
            }

            var pet = new Pet
            {
                Id = Guid.NewGuid(),
                CustomerId = targetCustomerId,
                Name = request.Name.Trim(),
                Species = request.Species,
                Breed = request.Breed?.Trim(),
                Weight = request.Weight,
                Age = request.Age,
                HealthNotes = request.HealthNotes?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _petRepository.AddAsync(pet);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status201Created,
                Message = "Pet created successfully.",
                Result = pet.Id
            };
        }
    }
}
