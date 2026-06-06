using AppCoel.Models;

namespace AppCoel.Core.Services
{
    internal interface IDeleteEntityIPutEntity<TRequest, TResponse>
        where TRequest : class, IRequest
        where TResponse : class, IResponse
    {
        Task<TResponse> DeleteEntityAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}
