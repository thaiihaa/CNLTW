using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BUOI6.Models
{
    [Table("BookImage")]
    public class BookImage
    {
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        [StringLength(500)]
        public string ImagePath { get; set; } = string.Empty;

        public Book? Book { get; set; }
    }
}
