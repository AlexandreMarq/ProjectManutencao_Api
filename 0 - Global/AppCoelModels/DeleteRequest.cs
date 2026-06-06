using AppCoel.Models.CustomValidators;

namespace AppCoel.Models
{
    public class DeleteRequest<TIdType> : IResponse
    {
        [RequiredGuid]
        required public TIdType Id { get; set; }
    }
}
