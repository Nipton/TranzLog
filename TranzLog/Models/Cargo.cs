using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models
{
    public class Cargo
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Название груза обязательно для заполнения.")]
        public string Name { get; set; } = "";
        public string? Type { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Объём груза не может быть отрицательным.")]
        public double Volume { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Вес груза не может быть отрицательным.")]
        public double Weight { get; set; }
        [Required]

        [Range(0, double.MaxValue, ErrorMessage = "Длина груза не может быть отрицательной.")]
        public double Length { get; set; }
        [Required]

        [Range(0, double.MaxValue, ErrorMessage = "Ширина груза не может быть отрицательной.")]
        public double Width { get; set; }
        [Required]

        [Range(0, double.MaxValue, ErrorMessage = "Высота груза не может быть отрицательной.")]
        public double Height { get; set; }
        public double TotalSize => Length * Width * Height;
        public string? PackagingType { get; set; }
        public int? TransportOrderId { get; set; }
        public virtual TransportOrder? TransportOrder { get; set; }
    }
}
