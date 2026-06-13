using System.Text.RegularExpressions;
using FAST.SocietiesManagement.DAL.Interfaces;

namespace FAST.SocietiesManagement.BLL.Validators
{
    public class UserValidator
    {
        private readonly IUserRepository _userRepository;

        public UserValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public (bool IsValid, string ErrorMessage) ValidateStudentRegistration(
            string username,
            string email,
            string password,
            string confirmPassword,
            string fullName,
            string rollNumber,
            string department)
        {
            var account = ValidateAccount(username, email, password, confirmPassword);
            if (!account.IsValid) return account;

            if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length < 3)
                return (false, "Full name must be at least 3 characters.");
            if (string.IsNullOrWhiteSpace(rollNumber) || rollNumber.Trim().Length < 3)
                return (false, "Roll number is required.");
            if (string.IsNullOrWhiteSpace(department))
                return (false, "Department is required.");
            if (_userRepository.IsRollNumberTaken(rollNumber.Trim()))
                return (false, "Roll number is already registered.");

            return (true, string.Empty);
        }

        public (bool IsValid, string ErrorMessage) ValidateAccount(string username, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(username)) return (false, "Username cannot be empty.");
            if (username.Trim().Length < 3) return (false, "Username must be at least 3 characters long.");
            if (_userRepository.IsUsernameTaken(username.Trim())) return (false, "Username is already taken.");

            if (string.IsNullOrWhiteSpace(email)) return (false, "Email is required.");
            if (!Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) return (false, "Enter a valid email address.");
            if (_userRepository.IsEmailTaken(email.Trim())) return (false, "Email is already registered.");

            if (string.IsNullOrWhiteSpace(password)) return (false, "Password cannot be empty.");
            if (password.Length < 8) return (false, "Password must be at least 8 characters long.");
            if (!Regex.IsMatch(password, @"[A-Z]")) return (false, "Password must contain at least one uppercase letter.");
            if (!Regex.IsMatch(password, @"[0-9]")) return (false, "Password must contain at least one number.");
            if (password != confirmPassword) return (false, "Passwords do not match.");

            return (true, string.Empty);
        }

        public (bool IsValid, string ErrorMessage) ValidateLogin(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)) return (false, "Username cannot be empty.");
            if (string.IsNullOrWhiteSpace(password)) return (false, "Password cannot be empty.");

            return (true, string.Empty);
        }
    }
}
