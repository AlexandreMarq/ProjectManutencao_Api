namespace AppCoel.Models
{
    public class EntitiesResponse<TEntity> : IResponse
        where TEntity : class, IEntity
    {
        public IReadOnlyCollection<TEntity> entities { get; set; } = Array.Empty<TEntity>();
    }
}
