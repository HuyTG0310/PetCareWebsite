using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.Staffs.Commands.ChangeStaffPassword
{
    public class ChangeStaffPasswordCommand : IRequest<ApiResponse<bool>>
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
