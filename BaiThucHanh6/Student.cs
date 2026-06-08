using System.ComponentModel.DataAnnotations;

namespace baiThucHanh6.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sinh viên không được để trống")]
        [Display(Name = "Họ tên")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;
    }
}
