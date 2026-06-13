using System;
using System.Windows.Forms;
using FAST.SocietiesManagement.UI.Forms;

namespace FAST.SocietiesManagement.UI
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Set high DPI and modern flat styles
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize Dependency Injection
            ServiceLocator.Initialize();

            // Auto-Seed Initial Admin Account
            EnsureAdminCreated();

            // Run Login Form
            Application.Run(new LoginForm());
        }

        private static void EnsureAdminCreated()
        {
            if (!ServiceLocator.UserRepository.IsUsernameTaken("admin"))
            {
                var (hash, salt) = FAST.SocietiesManagement.Core.Utilities.PasswordHasher.HashPassword("admin123");
                ServiceLocator.UserRepository.CreateUser(new FAST.SocietiesManagement.Core.DTOs.UserDto
                {
                    Username = "admin",
                    Email = "admin@fast.edu.pk",
                    PasswordHash = hash,
                    Salt = salt,
                    Role = FAST.SocietiesManagement.Core.Enums.RoleType.Admin,
                    IsActive = true
                });
            }
        }
    }
}
