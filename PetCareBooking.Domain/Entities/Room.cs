using PetCareBooking.Domain.Common;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Domain.Entities
{
    public class Room : BaseEntity
    {
        public Guid RoomTypeId { get; set; }
        public string RoomName { get; set; } = null!;
        public RoomStatus Status { get; set; }

        public virtual RoomType RoomType { get; set; } = null!;
    }
}
