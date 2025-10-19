namespace BuildingBlocks.Common.Domain;

/// <summary>
/// Base class for all entities in the domain.
/// </summary>
public abstract class Entity
{
    private int? _requestedHashCode;
    
    public virtual Guid Id { get; protected set; }
    
    public bool IsTransient()
    {
        return Id == default;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not Entity entity)
            return false;
            
        if (ReferenceEquals(this, entity))
            return true;
            
        if (GetType() != entity.GetType())
            return false;
            
        if (entity.IsTransient() || IsTransient())
            return false;
            
        return entity.Id == Id;
    }
    
    public override int GetHashCode()
    {
        if (IsTransient())
            return base.GetHashCode();
            
        _requestedHashCode ??= Id.GetHashCode() ^ 31;
        
        return _requestedHashCode.Value;
    }
    
    public static bool operator ==(Entity? left, Entity? right)
    {
        return left?.Equals(right) ?? Equals(right, null);
    }
    
    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}

