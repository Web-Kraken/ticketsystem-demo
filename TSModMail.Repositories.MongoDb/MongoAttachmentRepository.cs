using MongoDB.Driver;
using TSModMail.Core.Entities;
using TSModMail.Core.Repositories;

namespace TSModMail.Repositories.MongoDb;

/// <inheritdoc cref="IAttachmentRepository"/>
public class MongoAttachmentRepository
    : MongoRepository<AttachmentMap, ulong>, IAttachmentRepository
{
    public MongoAttachmentRepository(IMongoCollection<AttachmentMap> col)
        : base(col)
    {
    }
}
