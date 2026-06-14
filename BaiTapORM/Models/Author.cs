using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BUOI6.Models
{
    [Table("NguyenThaiHa_19062006_Author")]
    public class Author
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public List<Book>? Books { get; set; }
    }
}
