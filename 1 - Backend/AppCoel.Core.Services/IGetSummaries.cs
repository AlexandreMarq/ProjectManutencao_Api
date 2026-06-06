using AppCoel.Models;

namespace AppCoel.Core.Services
{
    public interface IGetSummaries<TRequest, TResponse>
        where TRequest : class, IRequest
        where TResponse : class, IResponse
    {
        Task<TResponse> GetSummariesAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}
