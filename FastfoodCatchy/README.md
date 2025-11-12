# FastfoodCatchy - Hệ thống Phân quyền Admin & User

## Giới thiệu

Dự án ASP.NET Core MVC với hệ thống phân quyền hoàn chỉnh cho Admin và User sử dụng Cookie Authentication.

## Tính năng

✅ Đăng ký tài khoản với validation  
✅ Đăng nhập/Đăng xuất  
✅ Phân quyền Admin và User  
✅ Cookie Authentication với Claims  
✅ Bảo vệ Controller/Action theo Role  
✅ Trang quản lý dành cho Admin  
✅ Trang cá nhân cho User  
✅ Access Denied page  
✅ Responsive UI với Bootstrap 5  

## Cài đặt

### Yêu cầu
- .NET 8.0 SDK hoặc cao hơn
- Visual Studio 2022 / VS Code / Rider

### Chạy ứng dụng

```bash
cd FastfoodCatchy
dotnet restore
dotnet run
```

Truy cập: `https://localhost:5001`

## Tài khoản demo

### Admin
- Email: `admin@gmail.com`
- Password: `12345678`

### User
Đăng ký tài khoản mới tại `/Account/Register`

## Cấu trúc phân quyền

### Admin có quyền:
- ✅ Truy cập tất cả trang Admin (`/Admin/*`)
- ✅ Quản lý danh sách người dùng
- ✅ Xem thống kê hệ thống
- ✅ Truy cập tất cả chức năng của User

### User có quyền:
- ✅ Xem trang cá nhân
- ✅ Quản lý thông tin cá nhân
- ✅ Xem đơn hàng (tính năng mở rộng)
- ❌ Không truy cập được trang Admin

## Cách sử dụng phân quyền

### 1. Bảo vệ Controller
```csharp
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    // Chỉ Admin truy cập được
}
```

### 2. Bảo vệ Action
```csharp
[Authorize] // Yêu cầu đăng nhập
public IActionResult Profile() { }

[Authorize(Roles = "Admin")] // Chỉ Admin
public IActionResult ManageUsers() { }

[Authorize(Roles = "Admin,User")] // Cả Admin và User
public IActionResult Dashboard() { }
```

### 3. Kiểm tra trong View
```cshtml
@if (User.IsInRole("Admin"))
{
    <a asp-controller="Admin" asp-action="Index">Admin Panel</a>
}
```

## Mở rộng

### Kết nối Database
Hiện tại dùng `UserStore` lưu trữ tạm trong memory. Để kết nối Database:

1. Cài đặt Entity Framework Core:
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

2. Tạo DbContext và Migration
3. Thay thế `UserStore` bằng `DbContext`

### Thêm vai trò mới
Chỉnh sửa `Models/UserRole.cs`:
```csharp
public enum UserRole
{
    User = 0,
    Admin = 1,
    Manager = 2,
    Staff = 3
}
```

### Hash mật khẩu
Cài đặt ASP.NET Core Identity để hash mật khẩu an toàn:
```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

## Tài liệu chi tiết

Xem file [HUONG_DAN_PHAN_QUYEN.md](HUONG_DAN_PHAN_QUYEN.md) để biết thêm chi tiết.

## Công nghệ sử dụng

- ASP.NET Core 8.0 MVC
- Cookie Authentication
- Claims-based Authorization
- Bootstrap 5
- Bootstrap Icons

## License

MIT License
