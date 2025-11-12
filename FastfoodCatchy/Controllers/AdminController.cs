using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FastfoodCatchy.Models;
using FastfoodCatchy.Data;

namespace FastfoodCatchy.Controllers
{
    [Authorize(Roles = "Admin")] // Chỉ Admin mới truy cập được
    public class AdminController : Controller
    {
        // GET: /Admin/Index
        public IActionResult Index()
        {
            ViewBag.Message = TempData["LoginMessage"];
            return View();
        }

        // GET: /Admin/ManageUsers
        public IActionResult ManageUsers()
        {
            var users = UserStore.GetAllUsers();
            return View(users);
        }

        // GET: /Admin/Dashboard
        public IActionResult Dashboard()
        {
            var totalUsers = UserStore.GetAllUsers().Count;
            ViewBag.TotalUsers = totalUsers;
            return View();
        }
    }
}
