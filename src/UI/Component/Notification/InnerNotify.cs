using Godot;
using Nasara.Core;

namespace Nasara.UI.Component.Notification;

public partial class InnerNotify : Control, INotificationInject
{
    public NotificationInjectType GetInjectType() => NotificationInjectType.Software;

    public void Notify(Core.Notification notification)
    {
        
    }
}