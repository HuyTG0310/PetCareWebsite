using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.RoomType;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.RoomTypes.Queries.GetRoomTypes
{
    public class GetRoomTypesQueryHandler : IRequestHandler<GetRoomTypesQuery, ApiResponse<List<RoomTypeResponseDTO>>>
    {
        private readonly IGenericRepository<RoomType> _roomTypeRepository;

        public GetRoomTypesQueryHandler(IGenericRepository<RoomType> roomTypeRepository)
        {
            _roomTypeRepository = roomTypeRepository;
        }

        public async Task<ApiResponse<List<RoomTypeResponseDTO>>> Handle(GetRoomTypesQuery request, CancellationToken cancellationToken)
        {
            var query = _roomTypeRepository.GetQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(rt => rt.Name.ToLower().Contains(term) ||
                                          (rt.Description != null && rt.Description.ToLower().Contains(term)));
            }

            var roomTypes = await query
                .Select(rt => new RoomTypeResponseDTO
                {
                    Id = rt.Id,
                    Name = rt.Name,
                    Description = rt.Description,
                    TotalRooms = rt.Rooms.Count
                })
                .OrderBy(rt => rt.Name)
                .ToListAsync(cancellationToken);

            return new ApiResponse<List<RoomTypeResponseDTO>>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Get room types successfully.",
                Result = roomTypes
            };
        }
    }
}
