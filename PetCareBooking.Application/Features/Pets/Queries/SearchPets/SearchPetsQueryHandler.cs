using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Pet;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Pets.Queries.SearchPets
{
    public class SearchPetsQueryHandler : IRequestHandler<SearchPetsQuery, PagedResult<PetListResponseDTO>>
    {
        private readonly IGenericRepository<Pet> _petRepository;

        public SearchPetsQueryHandler(IGenericRepository<Pet> petRepository)
        {
            _petRepository = petRepository;
        }

        public async Task<PagedResult<PetListResponseDTO>> Handle(SearchPetsQuery request, CancellationToken cancellationToken)
        {
            int pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            int pageSize = request.PageSize < 1 ? 10 : (request.PageSize > 100 ? 100 : request.PageSize);

            var query = _petRepository.GetQueryable()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                string keyword = request.Keyword.Trim();
                query = query.Where(p => p.Name.Contains(keyword));
            }

            if (request.Species.HasValue)
            {
                query = query.Where(p => p.Species == request.Species.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == request.IsActive.Value);
            }

            int totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PetListResponseDTO
                {
                    Id = p.Id,
                    CustomerId = p.CustomerId,
                    Name = p.Name,
                    Species = p.Species,
                    Breed = p.Breed,
                    Weight = p.Weight,
                    IsActive = p.IsActive
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<PetListResponseDTO>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageIndex,
                PageSize = pageSize
            };
        }
    }
}
