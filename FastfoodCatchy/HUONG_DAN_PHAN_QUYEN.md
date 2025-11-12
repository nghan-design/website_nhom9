# Hướng dẫn Hệ thống Phân quyền Admin và User

## Tổng quan

Hệ thống FastfoodCatchy đã được cài đặt phân quyền với 2 vai trò:
- **Admin**: Quản trị viên - có quyền truy cập tất cả chức năng quản lý
- **User**: Người dùng thông thường - chỉ truy cập các chức năng cơ bản

## Cấu trúc dự án

```
FastfoodCatchy/
├── Controllers/
│   ├── AccountController.cs      # Xử lý đăng ký, đăng nhập, đăng xuất
│   ├── AdminController.cs        # Các chức năng dành cho Admin
│   └── HomeController.cs         # Trang chủ và chức năng chung
├── Models/
│   ├── UserModel.cs              # Model người dùng với validation
│   └── UserRole.cs               # Enum định nghĩa vai trò
├── Data/
│   └── UserStore.cs              # Lưu trữ user tạm thời (thay bằng DB sau)
├── Views/
│   ├── Account/                  # Views đăng ký, đăng nhập
│   ├── Admin/                    # Views dành cho Admin
│   ├── Home/                     # Views trang chủ
│   └── Shared/                   # Layout chung
└── Program.cs                    # Cấu hình Cookie Authentication
```

## Cách hoạt động

### 1. Authentication (Xác thực)

Sử dụng **Cookie Authentication** của ASP.NET Core:

```csharp
// Trong Program.cs
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });
```

### 2. Authorization (Phân quyền)

Sử dụng **Claims-based Authorization**:

```csharp
// Tạo claims khi đăng nhập
var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, user.FullName),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Role, user.Role.ToString()) // "Admin" hoặc "User"
};
```

### 3. Bảo vệ Controller/Action

#### Bảo vệ toàn bộ Controller (chỉ Admin):
```csharp
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    // Tất cả actions trong đây chỉ Admin mới truy cập được
}
```

#### Bảo vệ từng Action:
```csharp
[Authorize] // Yêu cầu đăng nhập (cả Admin và User)
public IActionResult UserDashboard()
{
    return View();
}

[Authorize(Roles = "Admin")] // Chỉ Admin
public IActionResult ManageUsers()
{
    return View();
}

[Authorize(Roles = "User")] // Chỉ User
public IActionResult MyOrders()
{
    return View();
}

[Authorize(Roles = "Admin,User")] // Cả Admin và User
public IActionResult Profile()
{
    return View();
}
```

## Tài khoản mặc định

### Admin:
- **Email**: admin@gmail.com
- **Password**: 12345678
- **Quyền**: Truy cập tất cả trang Admin, quản lý user, thống kê

### User (đăng ký mới):
- Đăng ký qua form `/Account/Register`
- Mặc định có vai trò **User**
- Có thể chọn vai trò khi đăng ký (trong thực tế nên bỏ option này)

## Các tính năng chính

### 1. Đăng ký (Register)
- URL: `/Account/Register`
- Validation: Email, mật khẩu tối thiểu 8 ký tự, xác nhận mật khẩu
- Lưu user vào UserStore (sau này thay bằng Database)

### 2. Đăng nhập (Login)
- URL: `/Account/Login`
- Kiểm tra email/password
- Tạo Cookie với Claims (Name, Email, Role)
- Chuyển hướng:
  - Admin → `/Admin/Index`
  - User → `/Home/Index`

### 3. Đăng xuất (Logout)
- URL: `/Account/Logout`
- Xóa Cookie authentication
- Chuyển về trang chủ

### 4. Trang Admin
- `/Admin/Index` - Dashboard admin
- `/Admin/ManageUsers` - Quản lý danh sách user
- `/Admin/Dashboard` - Thống kê hệ thống

### 5. Access Denied
- URL: `/Account/AccessDenied`
- Hiển thị khi user không có quyền truy cập

## Kiểm tra quyền trong View

```cshtml
@* Kiểm tra đã đăng nhập chưa *@
@if (User.Identity.IsAuthenticated)
{
    <p>Xin chào, @User.Identity.Name</p>
}

@* Kiểm tra vai trò *@
@if (User.IsInRole("Admin"))
{
    <a asp-controller="Admin" asp-action="Index">Trang Admin</a>
}

@* Lấy thông tin từ Claims *@
<p>Email: @User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value</p>
<p>Vai trò: @User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value</p>
```

## Mở rộng hệ thống

### 1. Thêm vai trò mới
```csharp
// Trong UserRole.cs
public enum UserRole
{
    User = 0,
    Admin = 1,
    Manager = 2,  // Thêm vai trò mới
    Staff = 3
}
```

### 2. Kết nối Database
Thay `UserStore.cs` bằng Entity Framework Core:

```csharp
// Thêm vào Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Trong AccountController
private readonly ApplicationDbContext _context;

public AccountController(ApplicationDbContext context)
{
    _context = context;
}

// Lưu user
_context.Users.Add(user);
await _context.SaveChangesAsync();
```

### 3. Thêm nhiều quyền phức tạp
Sử dụng **Policy-based Authorization**:

```csharp
// Trong Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("RequireManagerOrAdmin", policy =>
        policy.RequireRole("Admin", "Manager"));
    
    options.AddPolicy("MinimumAge18", policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(18)));
});

// Sử dụng
[Authorize(Policy = "RequireAdminRole")]
public IActionResult AdminOnly() { }
```

### 4. Hash mật khẩu (Bảo mật)
```csharp
// Thêm package: Microsoft.AspNetCore.Identity
using Microsoft.AspNetCore.Identity;

// Trong AccountController
private readonly IPasswordHasher<UserModel> _passwordHasher;

// Khi đăng ký
user.Password = _passwordHasher.HashPassword(user, user.Password);

// Khi đăng nhập
var result = _passwordHasher.VerifyHashedPassword(user, user.Password, inputPassword);
if (result == PasswordVerificationResult.Success)
{
    // Đăng nhập thành công
}
```

## Chạy ứng dụng

```bash
cd FastfoodCatchy
dotnet restore
dotnet run
```

Truy cập: `https://localhost:5001` hoặc `http://localhost:5000`

## Lưu ý bảo mật

1. **Không lưu mật khẩu plain text**: Sử dụng hash (BCrypt, PBKDF2, hoặc Identity)
2. **HTTPS**: Luôn dùng HTTPS trong production
3. **Anti-forgery token**: Đã có `@Html.AntiForgeryToken()` trong forms
4. **Validation**: Đã có validation ở cả client và server
5. **Session timeout**: Cookie hết hạn sau 2 giờ
6. **Không cho user tự chọn role**: Xóa dropdown Role trong form Register

## Troubleshooting

### Lỗi: "Unable to resolve service for type 'ApplicationDbContext'"
→ Chưa cấu hình DbContext, hiện tại dùng UserStore tạm thời

### Lỗi: "No authentication handler is registered"
→ Kiểm tra `app.UseAuthentication()` đã được gọi trước `app.UseAuthorization()`

### Không redirect sau login
→ Kiểm tra `LoginPath` trong Cookie options

### Access Denied không hoạt động
→ Kiểm tra `AccessDeniedPath` trong Cookie options

## Tài liệu tham khảo

- [ASP.NET Core Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [ASP.NET Core Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/)
- [Cookie Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie)
