using BugTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BugTracker.Services.Interfaces
{
    public interface IBTNotificationService
    {

        Task AddNotificationAsync(Notification notification);
        Task<List<Notification>> GetReceivedNotificationsAsync(string userId);
        Task<List<Notification>> GetSentNotificationsAsync(string userId);
        Task SendEmailNotificationsByRoleAsync(Notification notification, int companyId, string role);
        Task SendMembersEmailNotificationsAsync(Notification notification, List<BTUser> members);
        Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject);
    }
}
