using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;

namespace StudentManagement.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Info()
        {
            ViewBag.Name = "Nguyễn Văn A";
            ViewData["Age"] = 20;

            var model = new StudentInfo
            {
                Major = "CNTT"
            };

            return View(model);
        }
    }
}
