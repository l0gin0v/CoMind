namespace CoMind.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedBy { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }
    
    public string? UpdatedBy { get; set; }
    
    public DateTimeOffset? DeletedAt { get; set; }
    
    public string? DeletedBy { get; set; }
    
    public bool IsDeleted => DeletedAt.HasValue;
}
