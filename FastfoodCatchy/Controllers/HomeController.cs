using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FastfoodCatchy.Controllers
{
    public class HomeController : Controller
    {
        // Trang chủ - Ai cũng truy cập được
        public IActionResult Index()
        {
            ViewBag.Message = TempData["LoginMessage"] ?? TempData["LogoutMessage"];
            return View();
        }

        // Trang dành cho user đã đăng nhập (cả Admin và User)
        [Authorize]
        public IActionResult UserDashboard()
        {
            return View();
        }

        // Trang About - Ai cũng truy cập được
        public IActionResult About()
        {
            return View();
        }
    }
}
