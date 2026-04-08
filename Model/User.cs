namespace Namaya.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Plain for now, use hashing later
        public string Role { get; set; } // Admin or Student
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
