using BUOI6.Models;
using Microsoft.AspNetCore.Mvc;

namespace BUOI6.Controllers
{
    public class StudentController : Controller
    {
        private static readonly List<Student> _students = new()
        {
            new Student { Id = 1, Name = "Nguyễn Văn A", Email = "nguyenvana@email.com", Phone = "0901234567", Class = "CNTT01" },
            new Student { Id = 2, Name = "Trần Thị B", Email = "tranthib@email.com", Phone = "0912345678", Class = "CNTT02" },
            new Student { Id = 3, Name = "Lê Văn C", Email = "levanc@email.com", Phone = "0923456789", Class = "CNTT01" }
        };

        private static int _nextId = 4;

        public IActionResult Index(string? search, string? sortBy, string? sortOrder)
        {
            var query = _students.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(s =>
                    s.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.Phone.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.Class.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            sortBy ??= "Id";
            sortOrder = sortOrder == "desc" ? "desc" : "asc";

            query = sortBy switch
            {
                "Name" => sortOrder == "desc" ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
                "Email" => sortOrder == "desc" ? query.OrderByDescending(s => s.Email) : query.OrderBy(s => s.Email),
                "Class" => sortOrder == "desc" ? query.OrderByDescending(s => s.Class) : query.OrderBy(s => s.Class),
                _ => sortOrder == "desc" ? query.OrderByDescending(s => s.Id) : query.OrderBy(s => s.Id)
            };

            ViewBag.Search = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            return View(query.ToList());
        }

        public IActionResult Detail(int id)
        {
            var student = _students.FirstOrDefault(s => s.Id == id);
            if (student == null)
                return NotFound();

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
                return View(model);

            model.Id = _nextId++;
            _students.Add(model);

            TempData["SuccessMessage"] = "Thêm sinh viên thành công!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var student = _students.FirstOrDefault(s => s.Id == id);
            if (student == null)
                return NotFound();

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Student model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var student = _students.FirstOrDefault(s => s.Id == model.Id);
            if (student == null)
                return NotFound();

            student.Name = model.Name;
            student.Email = model.Email;
            student.Phone = model.Phone;
            student.Class = model.Class;

            TempData["SuccessMessage"] = "Cập nhật sinh viên thành công!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var student = _students.FirstOrDefault(s => s.Id == id);
            if (student == null)
                return NotFound();

            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var student = _students.FirstOrDefault(s => s.Id == id);
            if (student == null)
                return NotFound();

            _students.Remove(student);

            TempData["SuccessMessage"] = "Xóa sinh viên thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
