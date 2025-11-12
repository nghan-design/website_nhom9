# Quick Reference - Phân quyền ASP.NET Core

## 🚀 Chạy ứng dụng

```bash
cd FastfoodCatchy
dotnet restore
dotnet run
```

Truy cập: `https://localhost:5001`

## 🔑 Tài khoản demo

| Vai trò | Email | Password |
|---------|-------|----------|
| Admin | admin@gmail.com | 12345678 |
| User | (Đăng ký mới) | - |

## 📝 Cú pháp phân quyền

### Controller level
```csharp
// Chỉ Admin
[Authorize(Roles = "Admin")]
public class AdminController : Controller { }

// Cả Admin và Manager
[Authorize(Roles = "Admin,Manager")]
public class ReportController : Controller { }
```

### Action level
```csharp
public class ProductController : Controller
{
    // Ai cũng xem được
    public IActionResult List() { }
    
    // Phải đăng nhập
    [Authorize]
    public IActionResult Details(int id) { }
    
    // Chỉ Admin
    [Authorize(Roles = "Admin")]
    public IActionResult Create() { }
}
```

## 🎨 Kiểm tra trong View

```cshtml
@* Đã đăng nhập? *@
@if (User.Identity.IsAuthenticated)
{
    <p>Xin chào, @User.Identity.Name</p>
}

@* Kiểm tra vai trò *@
@if (User.IsInRole("Admin"))
{
    <a href="/Admin">Admin Panel</a>
}

@* Lấy thông tin Claims *@
<p>Email: @User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value</p>
<p>Role: @User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value</p>
```

## 🔧 Cấu hình trong Program.cs

```csharp
// 1. Thêm Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

// 2. Thêm Authorization
builder.Services.AddAuthorization();

// 3. Sử dụng middleware (THỨ TỰ QUAN TRỌNG!)
app.UseAuthentication();  // ← Phải đặt TRƯỚC
app.UseAuthorization();   // ← Phải đặt SAU
```

## 🔐 Đăng nhập trong Controller

```csharp
// Tạo Claims
var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, user.FullName),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Role, user.Role.ToString()) // "Admin" hoặc "User"
};

// Tạo Identity
var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

// Đăng nhập (lưu Cookie)
await HttpContext.SignInAsync(
    CookieAuthenticationDefaults.AuthenticationScheme,
    new ClaimsPrincipal(claimsIdentity),
    new AuthenticationProperties
    {
        IsPersistent = true,
        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
    });
```

## 🚪 Đăng xuất

```csharp
[Authorize]
public async Task<IActionResult> Logout()
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return RedirectToAction("Index", "Home");
}
```

## 📂 Cấu trúc thư mục

```
FastfoodCatchy/
├── Controllers/
│   ├── AccountController.cs    # Đăng ký, đăng nhập, đăng xuất
│   ├── AdminController.cs      # [Authorize(Roles = "Admin")]
│   └── HomeController.cs       # Trang chủ
├── Models/
│   ├── UserModel.cs            # Model user
│   └── UserRole.cs             # Enum: User, Admin
├── Data/
│   └── UserStore.cs            # Lưu trữ user (thay bằng DB sau)
├── Views/
│   ├── Account/                # Register, Login, AccessDenied
│   ├── Admin/                  # Trang Admin
│   └── Home/                   # Trang chủ
└── Program.cs                  # Cấu hình Authentication
```

## ⚡ Các lệnh hay dùng

```bash
# Restore packages
dotnet restore

# Build project
dotnet build

# Run project
dotnet run

# Watch mode (auto reload)
dotnet watch run

# Clean build
dotnet clean
```

## 🎯 Các trường hợp sử dụng

| Yêu cầu | Attribute | Kết quả |
|---------|-----------|---------|
| Ai cũng truy cập | (không có) | ✅ Guest, User, Admin |
| Phải đăng nhập | `[Authorize]` | ❌ Guest, ✅ User, ✅ Admin |
| Chỉ Admin | `[Authorize(Roles = "Admin")]` | ❌ Guest, ❌ User, ✅ Admin |
| Admin hoặc Manager | `[Authorize(Roles = "Admin,Manager")]` | ❌ Guest, ❌ User, ✅ Admin, ✅ Manager |

## 🐛 Troubleshooting

### Lỗi: "No authentication handler is registered"
**Giải pháp**: Kiểm tra `app.UseAuthentication()` đã được gọi chưa

### Lỗi: Không redirect sau login
**Giải pháp**: Kiểm tra `LoginPath` trong Cookie options

### Lỗi: Access Denied không hoạt động
**Giải pháp**: Kiểm tra `AccessDeniedPath` trong Cookie options

### Lỗi: User.IsInRole() luôn trả về false
**Giải pháp**: Kiểm tra đã thêm `ClaimTypes.Role` vào Claims chưa

## 📚 Tài liệu chi tiết

- `README.md` - Giới thiệu tổng quan
- `HUONG_DAN_PHAN_QUYEN.md` - Hướng dẫn chi tiết
- `GIAI_THICH_PHAN_QUYEN.md` - Giải thích cơ chế hoạt động
- `LUONG_HOAT_DONG.txt` - Sơ đồ luồng hoạt động

## 🔗 Links hữu ích

- [ASP.NET Core Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [ASP.NET Core Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/)
- [Cookie Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie)

---

**Lưu ý**: Đây là phiên bản demo. Trong production:
- Sử dụng Database thay vì UserStore
- Hash mật khẩu với BCrypt hoặc Identity
- Sử dụng HTTPS
- Thêm CSRF protection
- Implement rate limiting
