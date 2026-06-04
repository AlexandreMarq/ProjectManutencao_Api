namespace AppCoel.Models
{
    public class SearchRequest : IRequest
    {
        public int? Taake { get; set; }
        public int? SearchText { get; set; }
    }
}
