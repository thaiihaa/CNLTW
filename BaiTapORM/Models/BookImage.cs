using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BUOI6.Models
{
    [Table("NguyenThaiHa_19062006_BookImage")]
    public class BookImage
    {
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }

        [ForeignKey("BookId")]
        public Book? Book { get; set; }

        // Stored file name relative to wwwroot/images/books
        [Required]
        [StringLength(260)]
        public string FileName { get; set; } = string.Empty;
    }
}
