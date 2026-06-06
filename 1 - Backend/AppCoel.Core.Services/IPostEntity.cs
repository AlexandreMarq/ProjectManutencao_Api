using AppCoel.Models;

namespace AppCoel.Core.Services
{
    public interface IPostEntity<TRequest, TResponse>
        where TRequest : class, IRequest
        where TResponse : class, IResponse
    {
        Task<TResponse> PostEntityAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}
