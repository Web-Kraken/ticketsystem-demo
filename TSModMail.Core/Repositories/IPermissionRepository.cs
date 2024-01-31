using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TSModMail.Core.Entities;

namespace TSModMail.Core.Repositories;

/// <summary>
/// Repository for <see cref="PermissionRecord"/>.
/// </summary>
public interface IPermissionRepository
    : ICrudBase<PermissionRecord, Guid>
{
    Task<IEnumerable<PermissionRecord>> GetForRegion(Guid region);
    Task<IEnumerable<PermissionRecord>> GetForMember(GuildMemberKey key);
    Task<PermissionRecord?> GetForMember(GuildMemberKey key, Guid region);
}
