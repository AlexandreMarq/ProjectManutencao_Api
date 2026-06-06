using AppCoel.Models;

namespace AppCoel.Core.Services
{
    public interface IGetSummary<TRequest, TResponse>
        where TRequest : class, IRequest
        where TResponse : class, IResponse 
    {
        Task<TResponse> GetSummaryAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}
