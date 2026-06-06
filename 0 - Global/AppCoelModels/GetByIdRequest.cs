using AppCoel.Models.CustomValidators;

namespace AppCoel.Models
{
    public class GetByIdRequest<TIdType> : IRequest
    {
        [RequiredGuid]
        required public TIdType Id { get; set; }
    }
}
