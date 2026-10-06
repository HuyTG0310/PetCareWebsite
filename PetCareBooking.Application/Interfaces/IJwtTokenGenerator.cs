using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(Customer customer);
    }
}