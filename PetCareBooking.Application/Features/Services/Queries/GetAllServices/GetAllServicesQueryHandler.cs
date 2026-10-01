//using MediatR;
//using PetCareBooking.Application.Common.Models;
//using PetCareBooking.Application.DTOs;
//using PetCareBooking.Application.Interfaces;
//using PetCareBooking.Domain.Entities;

//namespace PetCareBooking.Application.Features.Services.Queries.GetAllServices
//{
//    public class GetAllServicesQueryHandler : IRequestHandler<GetAllServicesQuery, ApiResponse<IEnumerable<ServiceDTO>>>
//    {
//        private readonly IGenericRepository<Service> _repository;

//        public GetAllServicesQueryHandler(IGenericRepository<Service> repository)
//        {
//            _repository = repository;
//        }

//        public async Task<ApiResponse<IEnumerable<ServiceDTO>>> Handle(GetAllServicesQuery request, CancellationToken cancellationToken)
//        {
//            var services = await _repository.GetAllAsync();

//            var serviceDtos = services.Select(s => new ServiceDTO
//            {
//                Id = s.Id,
//                ServiceName = s.ServiceName,
//                ServiceType = s.ServiceType,
//                Price = s.Price,
//                Status = s.Status
//            }).ToList();

//            return new ApiResponse<IEnumerable<ServiceDTO>>
//            {
//                IsSuccess = true,
//                StatusCode = 200,
//                Result = serviceDtos
//            };
//        }
//    }
//}
