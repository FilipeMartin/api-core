using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;
using taesa_aprovador_api.Models;
using taesa_aprovador_api.Models.PushNotification;
using System.Linq;

namespace taesa_aprovador_api.Core
{
    public class PushNotification : IPushNotification
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _clientFactory;

        public PushNotification(AppDbContext context, IHttpClientFactory clientFactory)
        {
            _context = context;
            _clientFactory = clientFactory;
        }

        public async Task<bool> Send(NotificationScheme NotificationScheme)
        {   
            bool status = false;

            try{
                var tokensFcm = new List<string>();

                foreach(Notification Notification in NotificationScheme.Notifications){
                    tokensFcm.Add(Notification.TokenFcm);
                }

                var notification = new {
                    registration_ids = tokensFcm,
                    notification = new {
                        title = NotificationScheme.Title,
                        body = NotificationScheme.Body,
                        color = "#042769",
                        sound = "default",
                        some = "payload",
                        icon = "notification",
                        click_action = "FCM_PLUGIN_ACTIVITY",
                    },
                    data = new {
                        item = NotificationScheme.Data
                    }
                };

                string notificationJson = JsonConvert.SerializeObject(notification);
                var httpContent = new StringContent(notificationJson, Encoding.UTF8, "application/json");

                var client = _clientFactory.CreateClient("fcm");
                var result = await client.PostAsync("https://fcm.googleapis.com/fcm/send", httpContent);
                string resultContent = await result.Content.ReadAsStringAsync();
                NotificationResponse NotificationResponse = JsonConvert.DeserializeObject<NotificationResponse>(resultContent);

                if(NotificationResponse.success > 0){
                    status = true;
                }

                if(NotificationResponse.failure > 0){
                    int count = 0;
                    foreach(NotificationResult Result in NotificationResponse.results){
                        if(Result.message_id == null){
                            _context.Notifications.Remove(NotificationScheme.Notifications.ElementAt(count));
                        }
                        count++;
                    }
                    _context.SaveChanges();
                }

            } catch(HttpRequestException){
                // Gravar erro no LOG
            }
            return status;
        } 
    }
}