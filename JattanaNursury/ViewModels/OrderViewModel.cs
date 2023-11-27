﻿using JattanaNursury.Models;
using System.ComponentModel.DataAnnotations;

namespace JattanaNursury.ViewModels
{
    public class OrderViewModel
    {
        [Required]
        public string? CustomerName { get; set; }

        [RegularExpression(@"^(?:\+?91[\-\s]?)?[789]\d{9}$|^[789]\d{4}[\-\s]?\d{5}$|^$", ErrorMessage = "Invalid phone")]
        public string PhoneNumber { get; set; } = string.Empty;
        public string? FullAddress { get; set; }
        [RegularExpression(@"^(?:\s*|[\w._%+-]+@[\w.-]+\.[a-zA-Z]{2,})$", ErrorMessage = "Invalid email format")]
        public string EmailAddress { get; set; } = string.Empty;

        //Client wants discount to be value not percentage
        [Range(1, 500, ErrorMessage = "Discount must be between 1 - 500")]
        public decimal Discount { get; set; }
        public string? Employee { get; set; }
        public List<ProductOrderDetail> Products { get; set; } = new List<ProductOrderDetail>();
    }
    public class ProductOrderDetail 
    {
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
    }
}