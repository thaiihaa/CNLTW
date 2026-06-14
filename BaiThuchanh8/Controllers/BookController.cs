using BUOI6.Models;
using BUOI6.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace BUOI6.Controllers
{
    public class BookController : Controller
    {
        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png"
        };

        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png"
        };

        private readonly IBookRepository _bookRepository;
        private readonly IWebHostEnvironment _env;

        public BookController(IBookRepository bookRepository, IWebHostEnvironment env)
        {
            _bookRepository = bookRepository;
            _env = env;
        }

        // GET: /Book
        public async Task<IActionResult> Index(string? search, string? sortBy, string? sortOrder)
        {
            var books = await _bookRepository.GetAllAsync(search, sortBy, sortOrder);

            ViewBag.Search = search;
            ViewBag.SortBy = sortBy ?? "Id";
            ViewBag.SortOrder = sortOrder ?? "asc";

            return View(books);
        }

        // GET: /Book/Detail/5
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
                return NotFound();

            var book = await _bookRepository.GetByIdAsync(id.Value);
            if (book == null)
                return NotFound();

            return View(book);
        }

        // GET: /Book/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Book/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book, List<IFormFile>? imageFiles)
        {
            var validImageFiles = GetUploadedFiles(imageFiles);
            ValidateImageFiles(validImageFiles);

            if (!ModelState.IsValid)
                return View(book);

            await _bookRepository.AddAsync(book);

            if (validImageFiles.Count > 0)
            {
                var imgs = new List<BookImage>();
                foreach (var file in validImageFiles)
                {
                    var savedPath = await SaveImageAsync(file);
                    imgs.Add(new BookImage { ImagePath = savedPath });
                }

                await _bookRepository.AddImagesAsync(book.Id, imgs);
            }

            TempData["SuccessMessage"] = "Thêm sách thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Book/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var book = await _bookRepository.GetByIdAsync(id.Value);
            if (book == null)
                return NotFound();

            return View(book);
        }

        // POST: /Book/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book, List<IFormFile>? imageFiles)
        {
            if (id != book.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(book);

            var existing = await _bookRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            var validImageFiles = GetUploadedFiles(imageFiles);
            ValidateImageFiles(validImageFiles);

            if (!ModelState.IsValid)
                return View(existing);

            existing.Title = book.Title;
            existing.Author = book.Author;
            existing.Price = book.Price;
            existing.Quantity = book.Quantity;
            existing.Description = book.Description;

            if (validImageFiles.Count > 0)
            {
                var imgs = new List<BookImage>();
                foreach (var file in validImageFiles)
                {
                    var savedPath = await SaveImageAsync(file);
                    imgs.Add(new BookImage { ImagePath = savedPath });
                }

                await _bookRepository.AddImagesAsync(existing.Id, imgs);
            }

            await _bookRepository.UpdateAsync(existing);

            TempData["SuccessMessage"] = "Cập nhật sách thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Book/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var book = await _bookRepository.GetByIdAsync(id.Value);
            if (book == null)
                return NotFound();

            return View(book);
        }

        // POST: /Book/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
                return NotFound();

            if (book.Images != null && book.Images.Count > 0)
            {
                foreach (var img in book.Images)
                {
                    DeleteImageFile(img.ImagePath);
                }
            }

            await _bookRepository.DeleteAsync(id);

            TempData["SuccessMessage"] = "Xóa sách thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId, int bookId)
        {
            var img = await _bookRepository.GetImageByIdAsync(imageId);
            if (img == null)
                return NotFound();

            DeleteImageFile(img.ImagePath);
            await _bookRepository.DeleteImageAsync(imageId);

            return RedirectToAction(nameof(Edit), new { id = bookId });
        }

        private static List<IFormFile> GetUploadedFiles(List<IFormFile>? files)
        {
            return files?
                .Where(file => file != null && file.Length > 0)
                .ToList() ?? new List<IFormFile>();
        }

        private void ValidateImageFiles(List<IFormFile> files)
        {
            foreach (var file in files)
            {
                if (!IsSupportedImage(file))
                {
                    ModelState.AddModelError("imageFiles", "Chỉ chấp nhận ảnh JPG hoặc PNG.");
                    return;
                }
            }
        }

        private static bool IsSupportedImage(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            return AllowedImageExtensions.Contains(extension)
                && AllowedImageContentTypes.Contains(file.ContentType);
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var uploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "images");
            if (!Directory.Exists(uploads))
                Directory.CreateDirectory(uploads);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploads, fileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/{fileName}";
        }

        private void DeleteImageFile(string imagePath)
        {
            try
            {
                var webRootPath = _env.WebRootPath ?? "wwwroot";
                var path = Path.Combine(webRootPath, imagePath.TrimStart('/', '\\'));
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
            catch
            {
                // Ignore file cleanup errors so deleting the database record still succeeds.
            }
        }
    }
}
