using AppCoel.Core.Infra.Database.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppCoel.Core.Infra.Database
{
    public interface IDatabaseRepository
    {
        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);

        public Task InsertAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        public Task InsertAndSaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        public Task UpadteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        public Task UpadteAndSaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        public Task DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        public Task DeleteAndSaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        public Task SaveAsync(CancellationToken cancellationToken = default);

        public Task<IReadOnlyCollection<TDestination>> GetAllAsync<TEntity, TDestination>(CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TDestination : class;

        public Task<IReadOnlyCollection<TEntity>> GetAllAsync<TEntity>(CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        public Task<IReadOnlyCollection<TDestination>> GetByConditionAsync<TEntity, TDestination>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TDestination : class;

        public Task<IReadOnlyCollection<TEntity>> GetByConditionAsync<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        public Task<TDestination?> GetFirstOrDefaultAsync<TEntity, TDestination>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TDestination : class;

        public Task<TEntity?> GetFirstOrDefaultAsync<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        public Task<bool> HasAnyAsync<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;
    }
}
