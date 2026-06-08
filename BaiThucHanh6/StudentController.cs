using baiThucHanh6.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace baiThucHanh6.Controllers
{
    public class StudentController : Controller
    {
        private static readonly List<Student> _students = new()
        {
            new Student { Id = 1, Name = "Nguyễn Văn An", Email = "an.nguyen@email.com", Phone = "0901234567" },
            new Student { Id = 2, Name = "Trần Thị Bình", Email = "binh.tran@email.com", Phone = "0912345678" },
            new Student { Id = 3, Name = "Lê Văn Cường", Email = "cuong.le@email.com", Phone = "0923456789" }
        };

        private static int _nextId = 4;

        public IActionResult Index(string? search, string? sortBy, string? sortOrder)
        {
            var query = _students.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.Phone.Contains(search));
            }

            sortBy ??= "Id";
            sortOrder ??= "asc";
            var ascending = sortOrder != "desc";

            query = sortBy switch
            {
                "Name" => ascending ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name),
                "Email" => ascending ? query.OrderBy(s => s.Email) : query.OrderByDescending(s => s.Email),
                "Phone" => ascending ? query.OrderBy(s => s.Phone) : query.OrderByDescending(s => s.Phone),
                _ => ascending ? query.OrderBy(s => s.Id) : query.OrderByDescending(s => s.Id)
            };

            ViewBag.Search = search;
            ViewBag.SortByList = new SelectList(
                new[] { new { Value = "Id", Text = "Mã SV" }, new { Value = "Name", Text = "Họ tên" }, new { Value = "Email", Text = "Email" }, new { Value = "Phone", Text = "Số điện thoại" } },
                "Value", "Text", sortBy);
            ViewBag.SortOrderList = new SelectList(
                new[] { new { Value = "asc", Text = "Tăng dần" }, new { Value = "desc", Text = "Giảm dần" } },
                "Value", "Text", sortOrder);

            return View(query.ToList());
        }

        public IActionResult Detail(int id)
        {
            var student = _students.FirstOrDefault(s => s.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Student model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Id = _nextId++;
            _students.Add(model);

            TempData["SuccessMessage"] = "Thêm sinh viên thành công!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var student = _students.FirstOrDefault(s => s.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Student model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var student = _students.FirstOrDefault(s => s.Id == model.Id);
            if (student == null)
            {
                return NotFound();
            }

            student.Name = model.Name;
            student.Email = model.Email;
            student.Phone = model.Phone;

            TempData["SuccessMessage"] = "Cập nhật sinh viên thành công!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var student = _students.FirstOrDefault(s => s.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var student = _students.FirstOrDefault(s => s.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            _students.Remove(student);

            TempData["SuccessMessage"] = "Xóa sinh viên thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
