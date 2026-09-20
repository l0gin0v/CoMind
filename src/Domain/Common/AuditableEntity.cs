namespace CoMind.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedBy { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }
    
    public string? UpdatedBy { get; set; }
    
    public bool IsDeleted { get; set; }
    
    public DateTimeOffset DeletedAt { get; set; }
    
    public string? DeletedBy { get; set; }
    
    public int Version { get; private set; }

    public DateTimeOffset LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}
