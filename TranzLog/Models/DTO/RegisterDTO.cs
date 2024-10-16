namespace TranzLog.Models.DTO
{
    public class RegisterDTO
    {
        public string UserName { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Password { get; set; } = null!;
    }
}
