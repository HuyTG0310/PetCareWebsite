using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.Vouchers.Commands.DeleteVoucher
{
    public class DeleteVoucherCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }

        public DeleteVoucherCommand()
        {
        }

        public DeleteVoucherCommand(Guid id)
        {
            Id = id;
        }
    }
}
