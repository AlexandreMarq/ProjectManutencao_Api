using AppCoel.Models.CustomValidators;

namespace AppCoel.Models
{
    public class GetBtParentIdRequest<TIdType> : IRequest
    {
        [RequiredGuid]
        required public TIdType ParentId { get; set; }
    }
}
