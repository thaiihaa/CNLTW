using Microsoft.AspNetCore.Mvc;

namespace StudentManagement.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Detail(int? id)
        {
            if (id == null)
            {
                return Content("Lỗi: Chưa truyền tham số id.");
            }

            return Content($"Product ID = {id}");
        }

        public IActionResult Category(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Content("Lỗi: Chưa truyền tham số name.");
            }

            return Content($"Category = {name}");
        }
    }
}
