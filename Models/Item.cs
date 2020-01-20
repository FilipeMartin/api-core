using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace taesa_aprovador_api.Models
{
    public class Item
    {
        [Key]
        [ObsoleteAttribute]
        public int Id {get; set;}

        [Required]
        public int? TaesaId {get; set;}

        [Required]
        [NotMapped]
        [EmailAddress]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Email {get; set;}

        [JsonIgnore]
        public int? UserId {get; set;}

        [ForeignKey("UserId")]
        [JsonIgnore]
        public User User {get; set;}

        [Required]
        public string Title {get; set;}

        [Required]
        public string Content {get; set;}

        [Required]
        public string SourceSystem {get; set;}

        [Required]
        [Column(TypeName="date")]
        public DateTime? DateLimit {get; set;}

        [Required]
        public string Approvers {get; set;}

        [Required]
        public string ApprovalType {get; set;}

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [ObsoleteAttribute]
        public string Comments {get; set;}

        [JsonIgnore]
        public bool? ApprovalStatus {get; set;}

        [Required]
        [DefaultValue(true)]
        [JsonIgnore]
        public bool Status {get; set;}

        [ObsoleteAttribute]
        public DateTime? CreatedAt {get; set;}
        
        [ObsoleteAttribute]
        public DateTime? UpdateAt {get; set;}

        public void FilterAttribute()
        {
            this.Comments = null;
            this.ApprovalStatus = null;
            this.Status = true;
            this.CreatedAt = DateTime.Now;
            this.UpdateAt = DateTime.Now;
        }

        public object GetItem()
        {
            var item = new {
                id = this.Id,
                taesaId = this.TaesaId,
                email = this.Email,
                title = this.Title,
                content = this.Content,
                sourceSystem = this.SourceSystem,
                dateLimit = this.DateLimit,
                approvers = this.Approvers,
                approvalType = this.ApprovalType,
                createdAt = this.CreatedAt,
                updateAt = this.UpdateAt
            };
            return item;
        }
    }
}