//using MediatR;
//using PetCareBooking.Application.Common.Models;
//using PetCareBooking.Application.Interfaces;
//using PetCareBooking.Domain.Entities;

//namespace PetCareBooking.Application.Features.Services.Commands.CreateService
//{
//    public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, ApiResponse<Guid>>
//    {
//        private readonly IGenericRepository<Service> _repository;
//        private readonly IUnitOfWork _unitOfWork;

//        public CreateServiceCommandHandler(IGenericRepository<Service> repository, IUnitOfWork unitOfWork)
//        {
//            _repository = repository;
//            _unitOfWork = unitOfWork;
//        }

//        public async Task<ApiResponse<Guid>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
//        {
//            var newService = new Service
//            {
//                ServiceName = request.ServiceName,
//                ServiceType = request.ServiceType,
//                Description = request.Description,
//                DurationMinutes = request.DurationMinutes,
//                PricingUnit = request.PricingUnit,
//                Price = request.Price,
//                Status = "ACTIVE"
//            };

//            await _repository.AddAsync(newService);
//            await _unitOfWork.SaveChangesAsync(cancellationToken);

//            return new ApiResponse<Guid>
//            {
//                IsSuccess = true,
//                StatusCode = 201,
//                Message = "Create service successfully",
//                Result = newService.Id
//            };
//        }
//    }
//}
