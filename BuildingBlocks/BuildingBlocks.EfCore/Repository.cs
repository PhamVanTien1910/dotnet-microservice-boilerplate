using BuildingBlocks.Domain.Models;
using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.EfCore
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity
    {
        protected readonly DbContext _context;
        protected readonly DbSet<TEntity> _dbSet;
        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }
        public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public async Task<int> CountAsync(Specification<TEntity>? spec = null, CancellationToken cancellationToken = default)
        {
            var query = SpecificationEvaluator.GetQuery(_dbSet.AsQueryable(), spec, applyPaging: false);
            return await query.CountAsync(cancellationToken);
        }

        public void Delete(TEntity entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(entity);
        }

        public void DeleteRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            _dbSet.RemoveRange(entities);
        }

        public Task<bool> ExistsAsync(Specification<TEntity>? spec, CancellationToken cancellationToken = default)
        {
            var query = SpecificationEvaluator.GetQuery(_dbSet.AsQueryable(), spec, applyPaging: false);
            return query.AnyAsync(cancellationToken);
        }

        public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(id);
        }

        public Task<TEntity?> GetBySpecAsync(Specification<TEntity> spec, CancellationToken cancellationToken = default)
        {
            var query = SpecificationEvaluator.GetQuery(_dbSet.AsQueryable(), spec, applyPaging: false);
            return query.FirstOrDefaultAsync(cancellationToken);
        }

        public IQueryable<TEntity> GetQuery()
        {
            return _dbSet.AsQueryable().AsNoTracking();
        }

        public Task<List<TEntity>> ListAsync(Specification<TEntity>? spec, CancellationToken cancellationToken = default)
        {
            if (spec == null)
            {
                return _dbSet.ToListAsync(cancellationToken);
            }

            var query = SpecificationEvaluator.GetQuery(_dbSet.AsQueryable(), spec);
            return query.ToListAsync(cancellationToken);
        }

        public async Task<(IReadOnlyCollection<TEntity>, int)> ListPagedAsync(Specification<TEntity> spec, CancellationToken cancellationToken = default)
        {
            // Query with paging for items
            var itemsQuery = SpecificationEvaluator.GetQuery(_dbSet.AsQueryable(), spec, applyPaging: true);
            var items = await itemsQuery.ToListAsync(cancellationToken);

            // Total count with filters but without paging
            var countQuery = SpecificationEvaluator.GetQuery(_dbSet.AsQueryable(), spec, applyPaging: false);
            var totalCount = await countQuery.CountAsync(cancellationToken);
            return (items, totalCount);
        }

        public void Update(TEntity entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Update(entity);
        }

        public void UpdateRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            _dbSet.UpdateRange(entities);
        }
    }
}