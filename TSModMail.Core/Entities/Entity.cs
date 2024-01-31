using System;

namespace TSModMail.Core.Entities;

/// <summary>
/// A serializable entity base.
/// </summary>
public abstract class Entity<TId>
{
    public TId Id { get; private set; }

    protected Entity(TId id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
    }
}