using KiemTraGiuaKi.Data;
using KiemTraGiuaKi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KiemTraGiuaKi.Controllers;

public class DishesController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public DishesController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index(string searchString, int? categoryId, bool? isAvailable, decimal? minPrice, decimal? maxPrice, string sortOrder)
    {
        ViewData["CurrentSearch"] = searchString;
        ViewData["CurrentCategory"] = categoryId;
        ViewData["CurrentIsAvailable"] = isAvailable;
        ViewData["CurrentMinPrice"] = minPrice;
        ViewData["CurrentMaxPrice"] = maxPrice;
        ViewData["CurrentSort"] = sortOrder;
        ViewData["PriceAscSort"] = string.IsNullOrEmpty(sortOrder) ? "price_desc" : "";
        ViewData["PriceDescSort"] = sortOrder == "price_desc" ? "price_asc" : "price_desc";
        ViewData["TimeSort"] = sortOrder == "time" ? "time_desc" : "time";

        var categories = await _context.DishCategories_BIT242377.ToListAsync();
        ViewBag.Categories = categories;

        var dishes = _context.Dishes_BIT242377
            .Include(d => d.DishCategory)
            .Include(d => d.DishImages)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            dishes = dishes.Where(d => d.Name.Contains(searchString));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            dishes = dishes.Where(d => d.DishCategoryId == categoryId.Value);
        }

        if (isAvailable.HasValue)
        {
            dishes = dishes.Where(d => d.IsAvailable == isAvailable.Value);
        }

        if (minPrice.HasValue)
        {
            dishes = dishes.Where(d => d.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            dishes = dishes.Where(d => d.Price <= maxPrice.Value);
        }

        if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
        {
            ViewBag.ErrorMessage = "Khoảng giá không hợp lệ.";
        }

        dishes = sortOrder switch
        {
            "price_desc" => dishes.OrderByDescending(d => d.Price),
            "time" => dishes.OrderBy(d => d.PreparationTime),
            "time_desc" => dishes.OrderByDescending(d => d.PreparationTime),
            _ => dishes.OrderBy(d => d.Price)
        };

        return View(await dishes.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var dish = await _context.Dishes_BIT242377
            .Include(d => d.DishCategory)
            .Include(d => d.DishImages)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (dish == null)
        {
            return NotFound();
        }

        return View(dish);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["DishCategoryId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.DishCategories_BIT242377.ToListAsync(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Dish_BIT242377 dish, List<IFormFile> imageFiles, List<string> imageUrls)
    {
        var categoryExists = await _context.DishCategories_BIT242377.AnyAsync(c => c.Id == dish.DishCategoryId);
        if (!categoryExists)
        {
            ModelState.AddModelError("DishCategoryId", "DishCategoryId không tồn tại.");
        }

        if (ModelState.IsValid)
        {
            _context.Add(dish);
            await _context.SaveChangesAsync();

            bool isFirst = true;
            
            // Handle uploaded files
            if (imageFiles != null && imageFiles.Any())
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "dishes");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                foreach (var file in imageFiles.Where(f => f != null && f.Length > 0))
                {
                    // Check file extension
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ImageFiles", "Chỉ chấp nhận file ảnh dạng JPG, PNG, GIF.");
                        continue;
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    _context.DishImages_BIT242377.Add(new DishImage_BIT242377
                    {
                        DishId = dish.Id,
                        ImageUrl = "/images/dishes/" + uniqueFileName,
                        IsThumbnail = isFirst
                    });
                    isFirst = false;
                }
            }

            // Handle URL images
            if (imageUrls != null && imageUrls.Any())
            {
                foreach (var url in imageUrls.Where(u => !string.IsNullOrEmpty(u)))
                {
                    _context.DishImages_BIT242377.Add(new DishImage_BIT242377
                    {
                        DishId = dish.Id,
                        ImageUrl = url,
                        IsThumbnail = isFirst
                    });
                    isFirst = false;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        ViewData["DishCategoryId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.DishCategories_BIT242377.ToListAsync(), "Id", "Name", dish.DishCategoryId);
        return View(dish);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var dish = await _context.Dishes_BIT242377
            .Include(d => d.DishImages)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (dish == null)
        {
            return NotFound();
        }

        ViewData["DishCategoryId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.DishCategories_BIT242377.ToListAsync(), "Id", "Name", dish.DishCategoryId);
        return View(dish);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Dish_BIT242377 dish, List<IFormFile> imageFiles, List<string> imageUrls)
    {
        if (id != dish.Id)
        {
            return NotFound();
        }

        var categoryExists = await _context.DishCategories_BIT242377.AnyAsync(c => c.Id == dish.DishCategoryId);
        if (!categoryExists)
        {
            ModelState.AddModelError("DishCategoryId", "DishCategoryId không tồn tại.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(dish);
                await _context.SaveChangesAsync();

                // Handle uploaded files
                if (imageFiles != null && imageFiles.Any())
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "dishes");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    bool isFirst = !await _context.DishImages_BIT242377.AnyAsync(i => i.DishId == dish.Id);

                    foreach (var file in imageFiles.Where(f => f != null && f.Length > 0))
                    {
                        // Check file extension
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("ImageFiles", "Chỉ chấp nhận file ảnh dạng JPG, PNG, GIF.");
                            continue;
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        _context.DishImages_BIT242377.Add(new DishImage_BIT242377
                        {
                            DishId = dish.Id,
                            ImageUrl = "/images/dishes/" + uniqueFileName,
                            IsThumbnail = isFirst
                        });
                        isFirst = false;
                    }
                }

                // Handle URL images
                if (imageUrls != null && imageUrls.Any())
                {
                    bool isFirst = !await _context.DishImages_BIT242377.AnyAsync(i => i.DishId == dish.Id);
                    foreach (var url in imageUrls.Where(u => !string.IsNullOrEmpty(u)))
                    {
                        _context.DishImages_BIT242377.Add(new DishImage_BIT242377
                        {
                            DishId = dish.Id,
                            ImageUrl = url,
                            IsThumbnail = isFirst
                        });
                        isFirst = false;
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DishExists(dish.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        ViewData["DishCategoryId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.DishCategories_BIT242377.ToListAsync(), "Id", "Name", dish.DishCategoryId);
        return View(dish);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var dish = await _context.Dishes_BIT242377
            .Include(d => d.DishCategory)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (dish == null)
        {
            return NotFound();
        }

        return View(dish);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var dish = await _context.Dishes_BIT242377.FindAsync(id);
        if (dish != null)
        {
            _context.Dishes_BIT242377.Remove(dish);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> SetThumbnail(int dishId, int imageId)
    {
        var dish = await _context.Dishes_BIT242377.Include(d => d.DishImages).FirstOrDefaultAsync(d => d.Id == dishId);
        if (dish == null)
        {
            return NotFound();
        }

        var image = dish.DishImages.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
        {
            return NotFound();
        }

        foreach (var img in dish.DishImages)
        {
            img.IsThumbnail = false;
        }

        image.IsThumbnail = true;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = dishId });
    }

    public async Task<IActionResult> DeleteCategory(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.DishCategories_BIT242377
            .Include(c => c.Dishes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        if (category.Dishes.Any())
        {
            return BadRequest("Không cho phép xóa loại món ăn có món ăn sử dụng.");
        }

        return View(category);
    }

    private bool DishExists(int id)
    {
        return _context.Dishes_BIT242377.Any(e => e.Id == id);
    }
}
