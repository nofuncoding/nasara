namespace Nasara.Core;

public class NotificationSystem
{
    private INotificationInject[] _injects;
    private NotificationInjectType _currentInjectType;

    public void SetInjectType(NotificationInjectType type) => _currentInjectType = type;
    
    public void Notify(Notification notification)
    {
        foreach (var i in _injects)
        {
            if (i.GetType() != _currentInjectType) continue;
            
            i.Notify(notification);
            return;
        }
        
        // No inject is type of _currentInjectType
        if (_injects.Length > 0)
            _injects[0].Notify(notification);
        else
            Logger.LogError("Failed to notify, no inject is given", "NotificationSystem");
    }

    public void AddInject(INotificationInject inject)
    {
        foreach (var i in _injects)
        {
            if (i.GetType() == inject.GetType())
            {
                Logger.LogWarn($"A NotificationInject for {inject.GetType()} is specified, replacing");
            }
        }
    }
}

public struct Notification
{
    public string Title;
    public string Description;
}

public enum NotificationInjectType
{
    System, // the system provided notification
    Software, // this program provided
}

public interface INotificationInject // TODO weird name.
{
    void Notify(Notification notification);
    NotificationInjectType GetType();
}