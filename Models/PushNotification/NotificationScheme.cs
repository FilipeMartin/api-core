using System.Collections.Generic;

namespace taesa_aprovador_api.Models.PushNotification
{
    public class NotificationScheme
    {
        public string Title {get; set;}
        public string Body {get; set;}
        public object Data {get; set;}
        public ICollection<Notification> Notifications {get; set;}
    }
}