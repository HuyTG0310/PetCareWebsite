using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.Staffs.Commands.DeleteStaff
{
    public class DeleteStaffCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }

        public DeleteStaffCommand(Guid id)
        {
            Id = id;
        }
    }
}