namespace AppCoel.Models
{
    public class GetBtParentIdRequest<TIdType> : IRequest
    {
        required public TIdType ParentId { get; set; }
    }
}
