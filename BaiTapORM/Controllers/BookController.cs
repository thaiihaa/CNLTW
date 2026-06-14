using BUOI6.Models;
using BUOI6.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using BUOI6.Data;
using Microsoft.EntityFrameworkCore;

namespace BUOI6.Controllers
{
    public class BookController : Controller
    {
        private readonly IBookRepository _bookRepository;
        private readonly IWebHostEnvironment _env;
        private readonly BookManagementDbContext _db;

        public BookController(IBookRepository bookRepository, IWebHostEnvironment env, BookManagementDbContext db)
        {
            _bookRepository = bookRepository;
            _env = env;
            _db = db;
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
            ViewBag.Authors = _db.Authors.ToList();
            return View();
        }

        // POST: /Book/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book, IFormFile[] images, string? newAuthorName)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Authors = _db.Authors.ToList();
                return View(book);
            }

            // If user provided a new author name, create it and assign
            if (!string.IsNullOrWhiteSpace(newAuthorName))
            {
                var na = new Models.Author { Name = newAuthorName.Trim() };
                _db.Authors.Add(na);
                await _db.SaveChangesAsync();
                book.AuthorId = na.Id;
            }

            // Save images
            if (images != null && images.Length > 0)
            {
                book.Images = new List<BookImage>();
                var imagesPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "images", "books");
                Directory.CreateDirectory(imagesPath);

                foreach (var img in images)
                {
                    if (img == null || img.Length == 0) continue;
                    var fileName = Path.GetRandomFileName() + Path.GetExtension(img.FileName);
                    var filePath = Path.Combine(imagesPath, fileName);
                    using var stream = System.IO.File.Create(filePath);
                    await img.CopyToAsync(stream);

                    book.Images.Add(new BookImage { FileName = Path.Combine("images", "books", fileName) });
                }
            }

            await _bookRepository.AddAsync(book);

            TempData["SuccessMessage"] = "Thêm sách thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Book/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var book = await _db.Books.Include(b => b.Images).FirstOrDefaultAsync(b => b.Id == id.Value);
            if (book == null)
                return NotFound();
            ViewBag.Authors = _db.Authors.ToList();
            return View(book);
        }

        // POST: /Book/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book, IFormFile[] images, string? newAuthorName)
        {
            if (id != book.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Authors = _db.Authors.ToList();
                return View(book);
            }

            // Update existing book entity
            var existing = await _db.Books.Include(b => b.Images).FirstOrDefaultAsync(b => b.Id == id);
            if (existing == null) return NotFound();

            existing.Title = book.Title;
            // If a new author name is provided, create and assign; otherwise use selected AuthorId
            if (!string.IsNullOrWhiteSpace(newAuthorName))
            {
                var na = new Models.Author { Name = newAuthorName.Trim() };
                _db.Authors.Add(na);
                await _db.SaveChangesAsync();
                existing.AuthorId = na.Id;
            }
            else
            {
                existing.AuthorId = book.AuthorId;
            }
            existing.Price = book.Price;
            existing.Quantity = book.Quantity;
            existing.Description = book.Description;

            if (images != null && images.Length > 0)
            {
                var imagesPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "images", "books");
                Directory.CreateDirectory(imagesPath);

                foreach (var img in images)
                {
                    if (img == null || img.Length == 0) continue;
                    var fileName = Path.GetRandomFileName() + Path.GetExtension(img.FileName);
                    var filePath = Path.Combine(imagesPath, fileName);
                    using var stream = System.IO.File.Create(filePath);
                    await img.CopyToAsync(stream);

                    existing.Images ??= new List<BookImage>();
                    existing.Images.Add(new BookImage { FileName = Path.Combine("images", "books", fileName) });
                }
            }

            await _db.SaveChangesAsync();

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

            await _bookRepository.DeleteAsync(id);

            TempData["SuccessMessage"] = "Xóa sách thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
