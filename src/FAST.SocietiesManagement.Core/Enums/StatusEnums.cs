namespace FAST.SocietiesManagement.Core.Enums
{
    public enum EventStatusEnum
    {
        Draft = 1,
        PendingApproval = 2,
        Approved = 3,
        Cancelled = 4,
        Completed = 5
    }

    public enum SocietyStatusEnum
    {
        Pending = 1,
        Active = 2,
        Suspended = 3
    }

    public enum MembershipStatusEnum
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Suspended = 4
    }

    public enum TaskStatusEnum
    {
        ToDo = 1,
        InProgress = 2,
        Completed = 3
    }
}
