# Giải thích Hệ thống Phân quyền Admin và User

## Câu hỏi của bạn
> "Dựa vào AccountController.cs trong controller làm thế nào để làm chế độ phân quyền admin và người đăng ký?"

## Trả lời

Tôi đã tạo một hệ thống phân quyền hoàn chỉnh cho bạn. Dưới đây là giải thích chi tiết:

---

## 1. Các thành phần chính

### A. Model - Định nghĩa vai trò

**UserRole.cs** - Enum định nghĩa 2 vai trò:
```csharp
public enum UserRole
{
    User = 0,    // Người dùng thông thường
    Admin = 1    // Quản trị viên
}
```

**UserModel.cs** - Thêm thuộc tính Role:
```csharp
public class UserModel
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public UserRole Role { get; set; } = UserRole.User; // Mặc định là User
}
```

### B. Authentication - Xác thực người dùng

**Program.cs** - Cấu hình Cookie Authentication:
```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";           // Trang đăng nhập
        options.AccessDeniedPath = "/Account/AccessDenied"; // Trang bị từ chối
        options.ExpireTimeSpan = TimeSpan.FromHours(2); // Cookie hết hạn sau 2h
    });
```

**Quan trọng**: Phải gọi middleware theo đúng thứ tự:
```csharp
app.UseAuthentication();  // Phải đặt TRƯỚC
app.UseAuthorization();   // Phải đặt SAU
```

### C. AccountController - Xử lý đăng nhập

**Khi đăng nhập thành công**, tạo Claims chứa thông tin user:
```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, user.FullName),      // Tên
    new Claim(ClaimTypes.Email, user.Email),        // Email
    new Claim(ClaimTypes.Role, user.Role.ToString()) // Vai trò: "Admin" hoặc "User"
};

// Tạo Cookie với Claims
var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

// Đăng nhập user (lưu Cookie)
await HttpContext.SignInAsync(
    CookieAuthenticationDefaults.AuthenticationScheme,
    new ClaimsPrincipal(claimsIdentity));
```

**Chuyển hướng theo vai trò**:
```csharp
if (user.Role == UserRole.Admin)
{
    return RedirectToAction("Index", "Admin");  // Admin → trang Admin
}
else
{
    return RedirectToAction("Index", "Home");   // User → trang chủ
}
```

### D. Authorization - Phân quyền

**AdminController.cs** - Chỉ Admin mới truy cập được:
```csharp
[Authorize(Roles = "Admin")]  // ← Đây là chìa khóa phân quyền!
public class AdminController : Controller
{
    public IActionResult Index()
    {
        // Chỉ user có Role = "Admin" mới vào được đây
        return View();
    }
    
    public IActionResult ManageUsers()
    {
        // Quản lý danh sách user
        var users = UserStore.GetAllUsers();
        return View(users);
    }
}
```

**HomeController.cs** - Phân quyền linh hoạt:
```csharp
public class HomeController : Controller
{
    // Ai cũng truy cập được (không cần đăng nhập)
    public IActionResult Index()
    {
        return View();
    }
    
    // Phải đăng nhập (cả Admin và User)
    [Authorize]
    public IActionResult UserDashboard()
    {
        return View();
    }
}
```

---

## 2. Cách hoạt động

### Luồng đăng ký:
1. User điền form → POST `/Account/Register`
2. Kiểm tra validation (email, password, v.v.)
3. Lưu user vào `UserStore` với Role mặc định là `User`
4. Chuyển đến trang Login

### Luồng đăng nhập:
1. User nhập email/password → POST `/Account/Login`
2. Tìm user trong `UserStore`
3. Nếu đúng:
   - Tạo **Claims** chứa Name, Email, **Role**
   - Lưu vào **Cookie**
   - Chuyển hướng theo Role:
     - Admin → `/Admin/Index`
     - User → `/Home/Index`

### Luồng truy cập trang được bảo vệ:
1. User truy cập `/Admin/Index`
2. Middleware kiểm tra:
   - ✅ Đã đăng nhập chưa? (có Cookie không?)
   - ✅ Có Role "Admin" không? (kiểm tra Claim)
3. Nếu không:
   - Chưa đăng nhập → Chuyển đến `/Account/Login`
   - Không có quyền → Chuyển đến `/Account/AccessDenied`

---

## 3. Các cách phân quyền

### Cách 1: Bảo vệ toàn bộ Controller
```csharp
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    // TẤT CẢ actions trong đây chỉ Admin mới vào được
}
```

### Cách 2: Bảo vệ từng Action
```csharp
public class ProductController : Controller
{
    // Ai cũng xem được
    public IActionResult List() { }
    
    // Phải đăng nhập (cả Admin và User)
    [Authorize]
    public IActionResult Details(int id) { }
    
    // Chỉ Admin
    [Authorize(Roles = "Admin")]
    public IActionResult Create() { }
    
    // Chỉ Admin
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id) { }
}
```

### Cách 3: Nhiều vai trò
```csharp
// Cả Admin VÀ Manager đều được phép
[Authorize(Roles = "Admin,Manager")]
public IActionResult SpecialFeature() { }
```

### Cách 4: Kiểm tra trong View
```cshtml
@* Kiểm tra đã đăng nhập *@
@if (User.Identity.IsAuthenticated)
{
    <p>Xin chào, @User.Identity.Name</p>
}

@* Kiểm tra vai trò *@
@if (User.IsInRole("Admin"))
{
    <a href="/Admin">Trang Admin</a>
}
else if (User.IsInRole("User"))
{
    <a href="/Home/UserDashboard">Trang cá nhân</a>
}

@* Lấy thông tin từ Claims *@
<p>Email: @User.FindFirst(ClaimTypes.Email)?.Value</p>
<p>Vai trò: @User.FindFirst(ClaimTypes.Role)?.Value</p>
```

---

## 4. Tại sao cách này tốt?

### ✅ Ưu điểm:
1. **Đơn giản**: Chỉ cần thêm `[Authorize(Roles = "Admin")]`
2. **Bảo mật**: ASP.NET Core tự động kiểm tra, không thể bypass
3. **Linh hoạt**: Dễ dàng thêm vai trò mới
4. **Chuẩn**: Theo best practice của Microsoft
5. **Tích hợp sẵn**: Không cần thư viện bên ngoài

### ⚠️ Lưu ý:
1. **Thứ tự middleware**: `UseAuthentication()` phải đặt TRƯỚC `UseAuthorization()`
2. **Claims**: Role phải được lưu vào Claims khi đăng nhập
3. **Cookie**: Cần cấu hình `LoginPath` và `AccessDeniedPath`

---

## 5. Mở rộng

### Thêm vai trò mới:
```csharp
public enum UserRole
{
    User = 0,
    Admin = 1,
    Manager = 2,    // ← Thêm vai trò mới
    Staff = 3
}

// Sử dụng
[Authorize(Roles = "Admin,Manager")]
public IActionResult ManagerFeature() { }
```

### Phân quyền phức tạp hơn (Policy-based):
```csharp
// Trong Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("RequireAdminOrManager", policy =>
        policy.RequireRole("Admin", "Manager"));
});

// Sử dụng
[Authorize(Policy = "RequireAdmin")]
public IActionResult AdminOnly() { }
```

---

## 6. Tóm tắt

**3 bước để phân quyền:**

1. **Cấu hình Authentication** (Program.cs):
   ```csharp
   builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
       .AddCookie();
   ```

2. **Lưu Role vào Claims** khi đăng nhập (AccountController):
   ```csharp
   var claims = new List<Claim>
   {
       new Claim(ClaimTypes.Role, user.Role.ToString())
   };
   await HttpContext.SignInAsync(...);
   ```

3. **Bảo vệ Controller/Action** với `[Authorize]`:
   ```csharp
   [Authorize(Roles = "Admin")]
   public class AdminController : Controller { }
   ```

---

## 7. Chạy thử

```bash
cd FastfoodCatchy
dotnet run
```

**Tài khoản Admin có sẵn:**
- Email: `admin@gmail.com`
- Password: `12345678`

**Thử nghiệm:**
1. Đăng nhập với tài khoản Admin → Vào được `/Admin/Index` ✅
2. Đăng ký tài khoản User mới → Không vào được `/Admin/Index` ❌ (hiện trang Access Denied)

---

## Kết luận

Hệ thống phân quyền này sử dụng:
- **Cookie Authentication** để xác thực
- **Claims-based Authorization** để phân quyền
- **Role-based Access Control** để bảo vệ tài nguyên

Đây là cách chuẩn và được khuyến nghị bởi Microsoft cho ASP.NET Core! 🎉
