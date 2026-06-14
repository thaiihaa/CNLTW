using BUOI6.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace BUOI6.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCover(IFormFile cover)
        {
            if (cover == null || cover.Length == 0)
            {
                TempData["CoverMessage"] = "Vui lòng chọn file ảnh.";
                return RedirectToAction("Index");
            }

            var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            Directory.CreateDirectory(imagesFolder);

            var destPath = Path.Combine(imagesFolder, "giao-dien-sach.png");
            using (var fs = new FileStream(destPath, FileMode.Create))
            {
                await cover.CopyToAsync(fs);
            }

            // Also write to top-level project "images" folder so existing static mapping 
            // (RequestPath = "/images") picks up the new file if that folder exists.
            var projectImages = Path.Combine(Directory.GetCurrentDirectory(), "images");
            try
            {
                Directory.CreateDirectory(projectImages);
                var altPath = Path.Combine(projectImages, "giao-dien-sach.png");
                System.IO.File.Copy(destPath, altPath, true);
            }
            catch
            {
                // ignore copy errors; primary file is in wwwroot/images
            }

            TempData["CoverMessage"] = "Ảnh bìa đã được cập nhật.";
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
