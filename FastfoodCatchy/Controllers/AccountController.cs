using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using FastfoodCatchy.Models;
using FastfoodCatchy.Data;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FastfoodCatchy.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(UserModel user)
        {
            if (!ModelState.IsValid)
            {
                // Nếu nhập sai form, hiển thị lại view cùng thông báo lỗi
                return View(user);
            }

            // Kiểm tra email đã tồn tại chưa
            if (UserStore.UserExists(user.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được đăng ký");
                return View(user);
            }

            // Lưu user vào UserStore (sau này sẽ lưu vào Database)
            UserStore.AddUser(user);
            
            TempData["SuccessMessage"] = $"Đăng ký thành công cho {user.FullName}. Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            // Nhận thông báo đăng ký thành công từ TempData
            ViewBag.Message = TempData["SuccessMessage"];
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ email và mật khẩu!";
                return View();
            }

            // Tìm user trong UserStore
            var user = UserStore.GetUserByEmail(Email);

            if (user != null && user.Password == Password)
            {
                // Tạo claims cho user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // Ghi nhớ đăng nhập
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2) // Hết hạn sau 2 giờ
                };

                // Đăng nhập user
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                TempData["LoginMessage"] = $"Đăng nhập thành công! Xin chào {user.FullName}";

                // Chuyển hướng dựa trên role
                if (user.Role == UserRole.Admin)
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
            return View();
        }

        // GET: /Account/Logout
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["LogoutMessage"] = "Đã đăng xuất thành công!";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
