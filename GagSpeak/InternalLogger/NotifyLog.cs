using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Plugin.Services;

namespace GagSpeak.GSLogger;
// taken directly off ECommens, for the sake of not including the full lib. ref code is here:
// https://github.com/NightmareXIV/ECommons/blob/fd3ceab5345b2a42eb51f998d6dcf6b696643f45/ECommons/Logging/

public static class Notify
{
    private static IFramework _framework = GagSpeak._services.GetService<IFramework>();
    private static INotificationManager _notificationManager = GagSpeak._services.GetService<INotificationManager>();

    public static void Success(string s) {
        _ = new TickScheduler(delegate {
            var notification = new Notification {
                Content = s,
                Title = GagSpeak.Name,
                Type = NotificationType.Success
            };
            _notificationManager.AddNotification(notification);
        }, _framework);
    }

    public static void Info(string s) {
        _ = new TickScheduler(delegate {
            var notification = new Notification {
                Content = s,
                Title = GagSpeak.Name,
                Type = NotificationType.Info
            };
            _notificationManager.AddNotification(notification);
        }, _framework);
    }

    public static void Error(string s) {
        _ = new TickScheduler(delegate {
            var notification = new Notification {
                Content = s,
                Title = GagSpeak.Name,
                Type = NotificationType.Error
            };
            _notificationManager.AddNotification(notification);
        }, _framework);
    }

    public static void Warning(string s) {
        _ = new TickScheduler(delegate {
            var notification = new Notification {
                Content = s,
                Title = GagSpeak.Name,
                Type = NotificationType.Warning
            };
            _notificationManager.AddNotification(notification);
        }, _framework);
    }

    public static void Plain(string s) {
        _ = new TickScheduler(delegate {
            var notification = new Notification {
                Content = s,
                Title = GagSpeak.Name,
                Type = NotificationType.None
            };
            _notificationManager.AddNotification(notification);
        }, _framework);
    }
}

/* Old format prior to .NET 8

public static void Success(string s) { 
    _ = new TickScheduler(delegate {_uiBuilder.AddNotification(s, GagSpeak.Name, NotificationType.Info);}, _framework);
}

public static void Info(string s) {
    _ = new TickScheduler(delegate {_uiBuilder.AddNotification(s, GagSpeak.Name, NotificationType.Info);}, _framework);
}

public static void Error(string s) {
    _ = new TickScheduler(delegate {_uiBuilder.AddNotification(s, GagSpeak.Name, NotificationType.Error);}, _framework);
}

public static void Warning(string s) {
    _ = new TickScheduler(delegate {_uiBuilder.AddNotification(s, GagSpeak.Name, NotificationType.Warning);}, _framework);
}

public static void Plain(string s) {
    _ = new TickScheduler(delegate {_uiBuilder.AddNotification(s, GagSpeak.Name, NotificationType.None);}, _framework);
}
*/