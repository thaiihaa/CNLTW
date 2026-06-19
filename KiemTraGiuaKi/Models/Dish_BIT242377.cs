using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KiemTraGiuaKi.Models;

public class Dish_BIT242377
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên không được để trống.")]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Price phải lớn hơn 0.")]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "PreparationTime phải lớn hơn 0.")]
    public int PreparationTime { get; set; }

    public bool IsAvailable { get; set; }

    public string? Description { get; set; }

    public int DishCategoryId { get; set; }

    [ForeignKey("DishCategoryId")]
    public DishCategory_BIT242377? DishCategory { get; set; }

    public ICollection<DishImage_BIT242377> DishImages { get; set; } = new List<DishImage_BIT242377>();
}
