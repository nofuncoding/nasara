using System.Collections.Generic;
using System.Linq;

namespace Nasara.Core;

public class NotificationSystem
{
    private List<INotificationInject> _injects = [];
    private NotificationInjectType _currentInjectType;

    public void SetInjectType(NotificationInjectType type) => _currentInjectType = type;
    
    public void Notify(Notification notification)
    {
        // Find a inject that is currently using
        foreach (var i in _injects.Where(i => i.GetInjectType() == _currentInjectType))
        {
            i.Notify(notification);
            return;
        }

        // No inject is type of _currentInjectType
        if (_injects.Count > 0)
            _injects[0].Notify(notification);
        else
            Logger.LogError("Failed to notify, no inject is given");
    }

    public void AddInject(INotificationInject inject)
    {
        // Replace the redundant inject if found
        foreach (var i in _injects.Where(i => i.GetInjectType() == inject.GetInjectType()))
        {
            Logger.LogWarn($"A NotificationInject for {inject.GetInjectType()} is specified, replacing");
            _injects.Remove(i);
            _injects.Add(inject);
            return;
        }

        // Else just add it
        _injects.Add(inject);
        Logger.Log($"A NotificationInject ({nameof(inject)}) for {inject.GetInjectType()} has been added");
        // TODO
    }
}

public struct Notification
{
    public string Title;
    public string Description;
}

public enum NotificationInjectType
{
    System,     // the system provided notification
    Software,   // the app layout provided
}

public interface INotificationInject // TODO weird name.
{
    void Notify(Notification notification);
    NotificationInjectType GetInjectType();
}