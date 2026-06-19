using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KiemTraGiuaKi.Models;

public class DishImage_BIT242377
{
    [Key]
    public int Id { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsThumbnail { get; set; }

    public int DishId { get; set; }

    [ForeignKey("DishId")]
    public Dish_BIT242377? Dish { get; set; }
}
