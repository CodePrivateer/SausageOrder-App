namespace BrodWorschdApp
{
    public interface IPaginationViewModel
    {
        int CurrentPage { get; set; }
        int TotalPages { get; set; }
        Dictionary<string, string> CultureStrings { get; set; }
    }

    public class PaginationViewModel<T> : IPaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<T> Items { get; set; } = new List<T>();
        public Dictionary<string, string> CultureStrings { get; set; } = new Dictionary<string, string>();


        public int GetTotalPages(List<T> items, int pageSize = 15)
        {
            var totalPages = (int)Math.Ceiling(items.Count / (double)pageSize);

            return totalPages;
        }

        public List<T> Paginate(List<T> items, int currentPage = 1, int pageSize = 15)
        {
            Items = items
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Items;
        }
    }
}
