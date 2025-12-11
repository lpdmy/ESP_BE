namespace EduShpere.Application.DTOs.SearchDto
{
    public enum SearchSortBy
    {
        Relevance = 0,
        MostRecent = 1,
        MostPopular = 2,
        MostCommented = 3,
        Alphabetical = 4
    }

    public class SearchSortingDto
    {
        public SearchSortBy SortBy { get; set; } = SearchSortBy.Relevance;
        public bool SortDescending { get; set; } = true;
    }
}
