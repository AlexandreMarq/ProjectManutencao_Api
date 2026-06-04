namespace AppCoel.Models
{
    public class KeyResponse<TIdType> : IResponse
    {
        required public TIdType Id { get; set; }
    }
}
