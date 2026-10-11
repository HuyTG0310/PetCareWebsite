using PetCareBooking.Domain.Common;

namespace PetCareBooking.Domain.Entities
{
    public class RoomType : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
