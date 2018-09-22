using System.ComponentModel.DataAnnotations;

namespace Products.API.Models
{
    public class ProductForCreation
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public decimal Price { get; set; }
        public string Category { get; set; }
    }
}
