using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models
{
    public class Consignee
    {
        public int Id { get; set; }
        [MaxLength(50, ErrorMessage = "Название компании не может превышать 50 символов.")]
        public string CompanyName { get; set; } = "";
        [Required(ErrorMessage = "Контактное лицо обязательно.")]
        [MaxLength(100, ErrorMessage = "Имя контактного лица не может превышать 100 символов.")]
        public string ContactPerson { get; set; } = "";
        [Phone(ErrorMessage = "Некорректный номер телефона.")]
        public string PhoneNumber { get; set; } = "";
        [EmailAddress(ErrorMessage = "Некорректный адрес электронной почты.")]
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public virtual List<TransportOrder>? TransportOrders { get; set; }
    }
}
