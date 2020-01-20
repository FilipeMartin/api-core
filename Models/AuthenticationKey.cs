using System.ComponentModel.DataAnnotations;

namespace taesa_aprovador_api.Models
{
    public class AuthenticationKey
    {
        [Required]
        public string Key {get; set;}
    }
}