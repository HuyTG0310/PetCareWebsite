namespace PetCareBooking.Domain.Entities
{
    public class StaffRole
    {
        public Guid StaffId { get; set; }
        public Guid RoleId { get; set; }

        public virtual Staff Staff { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
}
