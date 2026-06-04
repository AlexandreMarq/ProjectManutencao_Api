namespace AppCoel.Models
{
    public interface IEntity
    {
        public byte[]? RowVersion { get; set; }
    }
}
