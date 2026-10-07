using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Services.Commands.UpdateService
{
    public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Service> _serviceRepository;
        private readonly IGenericRepository<ServicePrice> _priceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateServiceCommandHandler(IGenericRepository<Service> serviceRepository, IGenericRepository<ServicePrice> priceRepository,IUnitOfWork unitOfWork)
        {
            _serviceRepository = serviceRepository;
            _priceRepository = priceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
        {
            // 1. Fetch current Service along with old Prices list
            var services = await _serviceRepository.FindAsync(s => s.Id == request.Id, "ServicePrices");
            var existingService = services.FirstOrDefault();

            if (existingService == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Service with ID {request.Id} not found.",
                    Result = Guid.Empty
                };
            }

            // 2. Update basic information of parent Service
            existingService.Name = request.Name;
            existingService.Description = request.Description;
            existingService.ServiceType = request.ServiceType;
            existingService.IsActive = request.IsActive;


            _serviceRepository.Update(existingService);

            if (existingService.ServicePrices != null && existingService.ServicePrices.Any())
            {
                // Must use .ToList() to create static copy before deleting to avoid 'Collection was modified' exception
                var oldPrices = existingService.ServicePrices.ToList();
                foreach (var oldPrice in oldPrices)
                {
                    _priceRepository.Delete(oldPrice);
                }
            }

            // Add newly submitted prices
            if (request.Prices != null && request.Prices.Any())
            {
                foreach (var priceDto in request.Prices)
                {
                    await _priceRepository.AddAsync(new ServicePrice
                    {
                        Id = Guid.NewGuid(),
                        ServiceId = existingService.Id, // REQUIRED: set ServiceId for relationship
                        MinWeight = priceDto.MinWeight,
                        MaxWeight = priceDto.MaxWeight,
                        Price = priceDto.Price,
                        PricingUnit = priceDto.PricingUnit
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Update service successfully.",
                Result = existingService.Id
            };
        }
    }
}
