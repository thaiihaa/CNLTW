using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KiemTraGiuaKi.Models;

public class DishCategory_BIT242377
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên không được để trống.")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<Dish_BIT242377> Dishes { get; set; } = new List<Dish_BIT242377>();
}
