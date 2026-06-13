using System;
using FAST.SocietiesManagement.DAL.Repositories;
using FAST.SocietiesManagement.DAL.Interfaces;
using FAST.SocietiesManagement.BLL.Services;

namespace FAST.SocietiesManagement.UI
{
    /// <summary>
    /// Simple Service Locator for Dependency Injection within WinForms without importing external IoC libraries.
    /// </summary>
    public static class ServiceLocator
    {
        // Repositories
        public static IUserRepository UserRepository { get; private set; }
        public static ISocietyRepository SocietyRepository { get; private set; }
        public static IEventRepository EventRepository { get; private set; }

        // Services
        public static AuthService AuthService { get; private set; }
        public static EventService EventService { get; private set; }
        public static SocietyService SocietyService { get; private set; }
        public static MembershipService MembershipService { get; private set; }
        public static TaskService TaskService { get; private set; }
        public static EnterpriseService EnterpriseService { get; private set; }

        public static void Initialize()
        {
            UserRepository = new UserRepository();
            SocietyRepository = new SocietyRepository();
            EventRepository = new EventRepository();
            var membershipRepo = new MembershipRepository();
            var taskRepo = new TaskRepository();
            var enterpriseRepo = new EnterpriseRepository();

            AuthService = new AuthService(UserRepository);
            EventService = new EventService(EventRepository);
            SocietyService = new SocietyService(SocietyRepository);
            MembershipService = new MembershipService(membershipRepo);
            TaskService = new TaskService(taskRepo, membershipRepo);
            EnterpriseService = new EnterpriseService(enterpriseRepo, membershipRepo);
        }
    }
}
