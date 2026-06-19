using KiemTraGiuaKi.Models;
using Microsoft.EntityFrameworkCore;

namespace KiemTraGiuaKi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<DishCategory_BIT242377> DishCategories_BIT242377 { get; set; }
    public DbSet<Dish_BIT242377> Dishes_BIT242377 { get; set; }
    public DbSet<DishImage_BIT242377> DishImages_BIT242377 { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DishCategory_BIT242377>().ToTable("DishCategories_BIT242377");
        modelBuilder.Entity<Dish_BIT242377>().ToTable("Dishes_BIT242377");
        modelBuilder.Entity<DishImage_BIT242377>().ToTable("DishImages_BIT242377");

        modelBuilder.Entity<Dish_BIT242377>()
            .HasIndex(d => new { d.Name, d.DishCategoryId })
            .IsUnique()
            .HasDatabaseName("IX_Dish_Name_CategoryId");

        modelBuilder.Entity<DishCategory_BIT242377>().HasData(
            new DishCategory_BIT242377 { Id = 1, Name = "Món mặn", Description = "Các món ăn mặn" },
            new DishCategory_BIT242377 { Id = 2, Name = "Món chay", Description = "Các món ăn chay" },
            new DishCategory_BIT242377 { Id = 3, Name = "Tráng miệng", Description = "Các món tráng miệng" }
        );

        modelBuilder.Entity<Dish_BIT242377>().HasData(
            new Dish_BIT242377 { Id = 1, Name = "Phở bò", Price = 45000, PreparationTime = 15, IsAvailable = true, Description = "Phở bò truyền thống", DishCategoryId = 1 },
            new Dish_BIT242377 { Id = 2, Name = "Bánh xèo", Price = 35000, PreparationTime = 20, IsAvailable = true, Description = "Bánh xèo giòn tan", DishCategoryId = 1 },
            new Dish_BIT242377 { Id = 3, Name = "Cà tím luộc", Price = 20000, PreparationTime = 10, IsAvailable = true, Description = "Cà tím nấu chay", DishCategoryId = 2 },
            new Dish_BIT242377 { Id = 4, Name = "Canh rau cải", Price = 25000, PreparationTime = 8, IsAvailable = true, Description = "Canh rau cải ngọt", DishCategoryId = 2 },
            new Dish_BIT242377 { Id = 5, Name = "Chè đậu đỏ", Price = 15000, PreparationTime = 25, IsAvailable = true, Description = "Chè đậu đỏ ngọt ngào", DishCategoryId = 3 }
        );


    }
}
