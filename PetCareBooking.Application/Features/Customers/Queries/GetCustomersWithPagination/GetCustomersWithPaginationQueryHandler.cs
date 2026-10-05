using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Customer;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Customers.Queries.GetCustomersWithPagination
{
    public class GetCustomersWithPaginationQueryHandler 
        : IRequestHandler<GetCustomersWithPaginationQuery, ApiResponse<PagedResult<CustomerListResponseDTO>>>
    {
        private readonly IGenericRepository<Customer> _repository;

        public GetCustomersWithPaginationQueryHandler(IGenericRepository<Customer> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<PagedResult<CustomerListResponseDTO>>> Handle(
            GetCustomersWithPaginationQuery request, 
            CancellationToken cancellationToken)
        {
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 10;
            if (request.PageSize > 100) request.PageSize = 100;

            var query = _repository.GetQueryable()
                .Where(c => (string.IsNullOrEmpty(request.SearchTerm) || 
                             c.FullName.Contains(request.SearchTerm) || 
                             c.Email.Contains(request.SearchTerm) || 
                             c.PhoneNumber.Contains(request.SearchTerm)) &&
                            (!request.IsActive.HasValue || c.IsActive == request.IsActive.Value));

            int totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new CustomerListResponseDTO
                {
                    Id = c.Id,
                    Email = c.Email,
                    FullName = c.FullName,
                    PhoneNumber = c.PhoneNumber,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    TotalPets = c.Pets.Count()
                })
                .ToListAsync(cancellationToken);

            return new ApiResponse<PagedResult<CustomerListResponseDTO>>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Get customers list successfully.",
                Result = new PagedResult<CustomerListResponseDTO>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                }
            };
        }
    }
}
