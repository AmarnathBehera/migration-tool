using BDO.GWT.Domain.Core.Entities;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Domain.Core.Repositories;

public interface IRepository<TEntity> : IEntity
{
    Task InsertEntityAsync(TEntity entity, CancellationToken cancellationToken);

    Task UpdateEntityAsync(TEntity entity, CancellationToken cancellationToken);

    Task DeleteEntityAsync(TEntity entity, CancellationToken cancellationToken);

    Task DeleteEntitiesAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken cancellationToken);
}
