using FastfoodCatchy.Models;
using System.Collections.Generic;
using System.Linq;

namespace FastfoodCatchy.Data
{
    // Lưu trữ tạm thời user trong memory (sau này thay bằng Database)
    public static class UserStore
    {
        private static List<UserModel> _users = new List<UserModel>
        {
            // Tài khoản admin mặc định
            new UserModel
            {
                FullName = "Administrator",
                Email = "admin@gmail.com",
                Password = "12345678",
                PhoneNumber = "0123456789",
                Role = UserRole.Admin
            }
        };

        public static void AddUser(UserModel user)
        {
            _users.Add(user);
        }

        public static UserModel GetUserByEmail(string email)
        {
            return _users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
        }

        public static bool UserExists(string email)
        {
            return _users.Any(u => u.Email.ToLower() == email.ToLower());
        }

        public static List<UserModel> GetAllUsers()
        {
            return _users;
        }
    }
}
