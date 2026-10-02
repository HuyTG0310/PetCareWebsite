using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Services.Commands.CreateService
{
    public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Service> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateServiceCommandHandler(IGenericRepository<Service> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
        {
            var newService = new Service
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                ServiceType = request.ServiceType,
                IsActive = true,
                ServicePrices = new List<ServicePrice>()
            };


            if (request.Prices != null && request.Prices.Any())
            {
                foreach (var priceDTO in request.Prices)
                {
                    newService.ServicePrices.Add(new ServicePrice
                    {
                        Id = Guid.NewGuid(),
                        MinWeight = priceDTO.MinWeight,
                        MaxWeight = priceDTO.MaxWeight,
                        Price = priceDTO.Price,
                        PricingUnit = priceDTO.PricingUnit
                    });
                }
            }

            await _repository.AddAsync(newService);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = 201,
                Message = "Create service successfully",
                Result = newService.Id
            };
        }
    }
}
