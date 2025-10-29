using BuildingBlocks.Domain.Models;
using BuildingBlocks.Domain.Specifications;

namespace BuildingBlocks.Domain.Repositories;

public interface IRepository<TEntity> where TEntity : Entity
{
    IQueryable<TEntity> GetQuery();
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TEntity?> GetBySpecAsync(Specification<TEntity> spec, CancellationToken cancellationToken = default);
    Task<List<TEntity>> ListAsync(Specification<TEntity>? spec, CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<TEntity>, int)> ListPagedAsync(Specification<TEntity> spec, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Specification<TEntity>? spec, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Specification<TEntity>? spec = null, CancellationToken cancellationToken = default);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    void Update(TEntity entity, CancellationToken cancellationToken = default);
    void UpdateRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    void Delete(TEntity entity, CancellationToken cancellationToken = default);
    void DeleteRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
