using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace taesa_aprovador_api.Models
{
    public class Notification
    {
        [Key]
        public int Id {get; set;}

        public int? UserId {get; set;}

        [ForeignKey("UserId")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public User User {get; set;}

        [Required]
        public string Uuid {get; set;}

        [Required]
        public string TokenFcm {get; set;}

        public DateTime CreatedAt {get; set;}
        
        public DateTime UpdateAt {get; set;}
    }
}