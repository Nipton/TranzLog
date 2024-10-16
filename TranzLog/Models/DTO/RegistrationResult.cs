namespace TranzLog.Models.DTO
{
    public class RegistrationResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string? Message { get; set; }
    }
}
