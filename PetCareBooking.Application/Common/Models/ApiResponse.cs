namespace PetCareBooking.Application.Common.Models
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; } = true;
        public int StatusCode { get; set; } = 200;
        public string Message { get; set; } = "Success";
        public T? Result { get; set; }
    }
}
