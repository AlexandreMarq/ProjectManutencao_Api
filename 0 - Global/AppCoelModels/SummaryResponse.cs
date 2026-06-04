namespace AppCoel.Models
{
    internal class SummaryResponse<TSummary> : IResponse
        where TSummary : class, ISummary
    {
        required public TSummary Summary { get; set; }
    }
}
