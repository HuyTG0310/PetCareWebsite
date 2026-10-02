using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Services.Commands.DeleteService
{
    public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Service> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteServiceCommandHandler(IGenericRepository<Service> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
        {
            var services = await _repository.FindAsync(s => s.Id == request.Id);
            var service = services.FirstOrDefault();

            if (service == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Service with ID {request.Id} not found",
                    Result = Guid.Empty
                };
            }


            if (!service.IsActive)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400, // Bad Request vì thao tác dư thừa
                    Message = "This service is already deleted (inactive).",
                    Result = service.Id
                };
            }

            service.IsActive = false;

            _repository.Update(service);
            await _unitOfWork.SaveChangesAsync(cancellationToken);


            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Service has been deleted successfully",
                Result = service.Id
            };
        }
    }
}
