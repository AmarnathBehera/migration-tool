using BDO.GWT.Domain.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Domain.Core.Retrievers;

public interface IRetriever<TEntity> where TEntity : IEntity
{
    Task<IEnumerable<TEntity>> RetrieveEntitiesAsync(Action<IQuery<TEntity>>? decorateQuery = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<TEntity>> RetrieveEntitiesAsync(Action<IQuery<TEntity>>? decorateQuery = null, string[]? cacheDependencies = null, CancellationToken cancellationToken = default);

    Task<TEntity?> RetrieveEntityAsync(Action<IQuery<TEntity>>? decorateQuery = null, CancellationToken cancellationToken = default);

    Task<TEntity?> RetrieveEntityAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<TEntity>> RetrieveJoinedEntitiesAsync(Action<IMultiObjectQuery<TEntity>>? decorateQuery = null, CancellationToken cancellationToken = default);
}
