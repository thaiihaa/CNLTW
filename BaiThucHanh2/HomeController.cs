using Microsoft.AspNetCore.Mvc;

namespace StudentManagement.Controllers
{
    public class HomeController : Controller
    {
        // Bài 1: Hiển thị nội dung theo yêu cầu (thay tên/email bằng thông tin của bạn)
        private const string StudentName = "Nguyễn Thái Hà";
        private const string StudentEmail = "haahaa05022006@gmail.com";

        public IActionResult Index()
        {
            return Content("Welcome to ASP.NET MVC");
        }

        public IActionResult About()
        {
            return Content(StudentName);
        }

        public IActionResult Contact()
        {
            return Content(StudentEmail);
        }
    }
}
