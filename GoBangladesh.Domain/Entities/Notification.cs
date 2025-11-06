using System.ComponentModel.DataAnnotations.Schema;

namespace GoBangladesh.Domain.Entities;

public class Notification : Entity
{
    public string Title { get; set; }
    public string Message { get; set; }
    public string OrganizationId { get; set; }
    public string BannerUrl { get; set; }
    [ForeignKey("OrganizationId")]
    public Organization Organization { get; set; }
}