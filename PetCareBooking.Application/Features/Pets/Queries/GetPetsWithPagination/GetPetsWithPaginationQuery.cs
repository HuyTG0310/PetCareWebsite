using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Pet;

namespace PetCareBooking.Application.Features.Pets.Queries.GetPetsWithPagination
{
    public class GetPetsWithPaginationQuery : IRequest<PagedResult<PetListResponseDTO>>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool? IsActive { get; set; }
    }
}
