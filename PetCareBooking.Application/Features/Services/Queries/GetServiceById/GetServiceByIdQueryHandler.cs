//using MediatR;
//using PetCareBooking.Application.Common.Models;
//using PetCareBooking.Application.DTOs;
//using PetCareBooking.Application.Interfaces;
//using PetCareBooking.Domain.Entities;

//namespace PetCareBooking.Application.Features.Services.Queries.GetServiceById
//{
//    public class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, ApiResponse<ServiceDTO>>
//    {
//        private readonly IGenericRepository<Service> _repository;

//        public GetServiceByIdQueryHandler(IGenericRepository<Service> repository)
//        {
//            _repository = repository;
//        }

//        public async Task<ApiResponse<ServiceDTO>> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
//        {
//            var service = await _repository.GetByIdAsync(request.Id);

//            if (service == null)
//            {
//                return new ApiResponse<ServiceDTO>
//                {
//                    IsSuccess = false,
//                    StatusCode = 404,
//                    Message = $"Không tìm thấy dịch vụ với ID: {request.Id}",
//                    Result = null
//                };
//            }

//            // Map sang DTO
//            var serviceDto = new ServiceDTO
//            {
//                Id = service.Id,
//                ServiceName = service.ServiceName,
//                ServiceType = service.ServiceType,
//                Price = service.Price,
//                Status = service.Status
//            };

//            // Trả về ApiResponse thành công 200
//            return new ApiResponse<ServiceDTO>
//            {
//                IsSuccess = true,
//                StatusCode = 200,
//                Message = "Lấy thông tin dịch vụ thành công.",
//                Result = serviceDto
//            };
//        }
//    }
//}
