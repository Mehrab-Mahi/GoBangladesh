using System;

namespace GoBangladesh.Application.DTOs.Card;

public class CardDataDto
{
    public string Id { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime LastModifiedTime { get; set; }
    public string CreatedBy { get; set; }
    public string LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public bool IsRegistered { get; set; }
    public string CardNumber { get; set; }
    public string Status { get; set; }
    public decimal Balance { get; set; }
    public string OrganizationId { get; set; }
    public Domain.Entities.Organization Organization { get; set; }
}