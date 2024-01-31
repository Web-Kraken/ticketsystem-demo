using System.Collections.Generic;
using System.Threading.Tasks;
using TSModMail.Core.Entities;

namespace TSModMail.Core;

/// <summary>
/// A base for CRUD (create, read, update delete) repositories.
/// </summary>
public interface ICrudBase<T, TId> where T : Entity<TId>
{
    public Task Insert(T item);

    public Task<IEnumerable<T>> GetAll();

    public Task<T?> GetById(TId oid);

    public Task Update(T item);

    public Task Delete(T item);
}
