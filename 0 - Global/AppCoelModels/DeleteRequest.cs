namespace AppCoel.Models
{
    public class DeleteRequest<TIdType> : IResponse
    {
        required public TIdType Id { get; set; }
    }
}
