using AppCoel.Models;

namespace AppCoel.Core.Services
{
    public interface IGetEntity<TRequest, TResponse>
        where TRequest : class, IRequest
        where TResponse : class, IResponse
    {
        Task<TResponse> GetEntityAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}
