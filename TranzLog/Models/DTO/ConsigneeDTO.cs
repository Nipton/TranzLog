﻿using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models.DTO
{
    public class ConsigneeDTO
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string CompanyName { get; set; } = "";
        [Required]
        [MaxLength(100)]
        public string ContactPerson { get; set; } = "";
        [Phone]
        public string PhoneNumber { get; set; } = "";
        [EmailAddress]
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
    }
}
