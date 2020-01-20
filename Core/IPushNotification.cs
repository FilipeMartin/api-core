using System.Threading.Tasks;
using taesa_aprovador_api.Models.PushNotification;

namespace taesa_aprovador_api.Core
{
    public interface IPushNotification
    {
        Task<bool> Send(NotificationScheme NotificationScheme);
    }
}