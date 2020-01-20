using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace taesa_aprovador_api.Models
{
    public class User
    {
        [Key]
        public int Id {get; set;}

        [Required]
        [EmailAddress(ErrorMessage="{0} Inválido.")]
        [StringLength(128, MinimumLength = 3)]
        public string Email {get; set;}

        [Required]
        [NotMapped]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Password {get; set;}

        [NotMapped]
        public string Login {get; set;}

        [Required]
        public ICollection<Notification> Notifications {get; set;}

        public DateTime CreatedAt {get; set;}
        
        public DateTime UpdateAt {get; set;}
    }
}