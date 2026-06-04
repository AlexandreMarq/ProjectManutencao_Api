namespace AppCoel.Models
{
    public class SummariesResponse<TSummary> : IResponse
        where TSummary : class, ISummary
    {
        public IReadOnlyCollection<TSummary> Summaries { get; set; } = Array.Empty<TSummary>();
    }
}
