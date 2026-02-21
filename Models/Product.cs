using System.ComponentModel.DataAnnotations;

namespace RepositoryApp.Models
{
    public class Product
    {
        [Required]
        [Display(Name = "Product Name")]
        public string Name { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be non-negative")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }
    }
}
