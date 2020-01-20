using System.ComponentModel.DataAnnotations;

namespace taesa_aprovador_api.Models
{
    public class ItemStatus
    {
        [Required]
        public bool? Status {get; set;}

        public string Comments {get; set;}
    }
}