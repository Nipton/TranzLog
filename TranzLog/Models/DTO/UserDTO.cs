using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        [MaxLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Имя пользователя может состоять только только из букв, цифр и символа подчерквания.")]
        public string UserName { get; set; } = "";
        public Role Role { get; set; }
        [MaxLength(50)]
        public string? FirstName { get; set; }
        [MaxLength(50)]
        public string? LastName { get; set; }
        [MaxLength(255)]
        [EmailAddress(ErrorMessage = "Неверный формат электронной почты.")]
        public string? Email { get; set; }
        [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Неверный формат телефонного номера.")]
        public string? PhoneNumber { get; set; }

        public DateTime CreatedDate { get; set; }
        [MinLength(8)]
        [MaxLength(40)]
        public string Password { get; set; } = null!;
    }
}