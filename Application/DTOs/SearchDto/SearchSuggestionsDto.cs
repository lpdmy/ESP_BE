namespace EduShpere.Application.DTOs.SearchDto
{
    public class SearchSuggestionsDto
    {
        public IEnumerable<string> Users { get; set; } = new List<string>();
        public IEnumerable<string> Posts { get; set; } = new List<string>();
        public IEnumerable<string> Activities { get; set; } = new List<string>();
        public IEnumerable<string> Hashtags { get; set; } = new List<string>();
        public IEnumerable<string> RecentSearches { get; set; } = new List<string>();
    }

    public class TrendingSearchesDto
    {
        public IEnumerable<TrendingSearchItemDto> Items { get; set; } = new List<TrendingSearchItemDto>();
    }

    public class TrendingSearchItemDto
    {
        public string Query { get; set; } = string.Empty;
        public int SearchCount { get; set; }
        public bool IsTrending { get; set; }
        public DateTime LastSearched { get; set; }
    }
}
