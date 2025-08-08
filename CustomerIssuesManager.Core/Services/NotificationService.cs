using System;
using System.Threading.Tasks;

namespace CustomerIssuesManager.Core.Services
{
    public enum NotificationType
    {
        Success,
        Warning,
        Error,
        Info
    }

    public interface INotificationService
    {
        Task ShowNotificationAsync(string message, NotificationType type = NotificationType.Info);
        Task ShowLoadingIndicatorAsync(string message = "جاري التحميل...");
        Task HideLoadingIndicatorAsync();
    }
} 