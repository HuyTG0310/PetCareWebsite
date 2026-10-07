using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.CareRecords.Commands.DeleteCareRecord
{
    public class DeleteCareRecordCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }

        public DeleteCareRecordCommand()
        {
        }

        public DeleteCareRecordCommand(Guid id)
        {
            Id = id;
        }
    }
}
