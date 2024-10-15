using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models.DTO
{
    public class UserDTO
    {
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";

    }
}