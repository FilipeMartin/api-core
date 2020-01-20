using System.Collections.Generic;

namespace taesa_aprovador_api.Models.PushNotification
{
    public class NotificationResponse
    {
        public long multicast_id {get; set;}
        public int success {get; set;}
        public int failure {get; set;}
        public long canonical_ids {get; set;}
        public List<NotificationResult> results {get; set;}
    }
}