namespace Triplog.Media.Domain.Abstractions;

public abstract class Entity<TId> where TId : struct
{
    public TId Id { get; protected set; }

    protected Entity(TId id) { Id = id; }

    protected Entity() { } // For EF Core
}
