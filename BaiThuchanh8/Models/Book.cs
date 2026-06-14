using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BUOI6.Models
{
    [Table("Book")]
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sách không được để trống")]
        [Display(Name = "Tên sách")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tác giả không được để trống")]
        [Display(Name = "Tác giả")]
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá sách không được để trống")]
        [Range(0.01, 100000000, ErrorMessage = "Giá sách phải lớn hơn 0")]
        [Display(Name = "Giá (VNĐ)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Display(Name = "Mô tả")]
        [StringLength(500)]
        public string? Description { get; set; }

        public ICollection<BookImage>? Images { get; set; }
    }
}
