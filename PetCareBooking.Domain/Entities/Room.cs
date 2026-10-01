using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Domain.Entities
{
    public class Room
    {
        public Guid Id { get; set; }
        public Guid RoomTypeId { get; set; }
        public string RoomName { get; set; } = null!;
        public RoomStatus Status { get; set; }

        public virtual RoomType RoomType { get; set; } = null!;
    }
}
