using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Staff;
using PetCareBooking.Application.Interfaces;
using StaffEntity = PetCareBooking.Domain.Entities.Staff;

namespace PetCareBooking.Application.Features.Staffs.Queries.GetStaffsWithPagination
{
    public class GetStaffsWithPaginationQueryHandler
        : IRequestHandler<GetStaffsWithPaginationQuery, ApiResponse<PagedResult<StaffListResponseDto>>>
    {
        private readonly IGenericRepository<StaffEntity> _repository;

        public GetStaffsWithPaginationQueryHandler(IGenericRepository<StaffEntity> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<PagedResult<StaffListResponseDto>>> Handle(
            GetStaffsWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.GetQueryable().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(s => s.FullName.ToLower().Contains(term) ||
                                         s.Email.ToLower().Contains(term) ||
                                         s.PhoneNumber.Contains(term) ||
                                         (s.Address != null && s.Address.ToLower().Contains(term)) ||
                                         s.StaffRoles.Any(sr => sr.Role.RoleName.ToLower().Contains(term)));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(s => s.Status == request.Status.Value);
            }

            int totalCount = await query.CountAsync(cancellationToken);

            int pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            int pageSize = request.PageSize < 1 ? 10 : (request.PageSize > 100 ? 100 : request.PageSize);

            var items = await query
                .OrderBy(s => s.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new StaffListResponseDto
                {
                    Id = s.Id,
                    Email = s.Email,
                    FullName = s.FullName,
                    PhoneNumber = s.PhoneNumber,
                    Address = s.Address,
                    AvatarUrl = s.AvatarUrl,
                    YearsOfExperience = s.YearsOfExperience,
                    Status = s.Status,
                    Roles = s.StaffRoles.Select(sr => sr.Role.RoleName).ToList()
                })
                .ToListAsync(cancellationToken);

            return new ApiResponse<PagedResult<StaffListResponseDto>>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Staff list retrieved successfully.",
                Result = new PagedResult<StaffListResponseDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }
            };
        }
    }
}