namespace BrodWorschdApp
{
    public interface ISearchModel
    {
        Dictionary<string, string> CurrentFilters { get; set; }
        Dictionary<string, string> CultureStrings { get; set; }
        List<T> FilterList<T>(List<T> list, Dictionary<string, string> filters);
    }

    public class SearchModel : Dictionary<string, string>, ISearchModel
    {
        public Dictionary<string, string>? CurrentFilters { get; set; }
        public Dictionary<string, string> CultureStrings { get; set; } = new Dictionary<string, string>();
        public List<T> FilterList<T>(List<T> list, Dictionary<string, string> filters)
        {
            var filteredList = new List<T>();

            // Entferne den Token-Eintrag aus den Filtern
            filters.Remove("__RequestVerificationToken");

            if (filters.Values.Any(v => !string.IsNullOrWhiteSpace(v)))
            {
                foreach (var item in list)
                {
                    bool matchesAllFilters = true;

                    foreach (var filter in filters)
                    {
                        var filterValue = filter.Value?.Trim() ?? string.Empty;

                        if (!string.IsNullOrEmpty(filterValue))
                        {
                            if (!MatchesFilter(item, filter.Key, filterValue))
                            {
                                matchesAllFilters = false;
                                break;
                            }
                        }
                    }

                    if (matchesAllFilters)
                    {
                        filteredList.Add(item);
                    }
                }
            }
            else
            {
                filteredList = list;
            }

            return filteredList;
        }

        private bool MatchesFilter(object item, string filterKey, string filterValue)
        {
            var propertyInfo = item.GetType().GetProperty(filterKey);
            if (propertyInfo != null)
            {
                var value = propertyInfo.GetValue(item)?.ToString()?.Trim() ?? string.Empty;
                return value.ToLower().Contains(filterValue.ToLower());
            }
            else
            {
                // Überprüfen, ob die Eigenschaft eine Liste ist
                var listProperties = item.GetType().GetProperties().Where(p => typeof(IEnumerable<object>).IsAssignableFrom(p.PropertyType));
                foreach (var listProperty in listProperties)
                {
                    var listValue = listProperty.GetValue(item) as IEnumerable<object>;
                    if (listValue != null)
                    {
                        foreach (var listItem in listValue)
                        {
                            if (MatchesFilter(listItem, filterKey, filterValue))
                            {
                                return true;
                            }
                        }
                    }
                }

                // Überprüfen, ob die Eigenschaft eine Navigationseigenschaft ist
                var navigationProperties = item.GetType().GetProperties().Where(p => p.PropertyType.IsClass && p.PropertyType != typeof(string) && !typeof(IEnumerable<object>).IsAssignableFrom(p.PropertyType));
                foreach (var navigationProperty in navigationProperties)
                {
                    var navigationValue = navigationProperty.GetValue(item);
                    if (navigationValue != null)
                    {
                        if (MatchesFilter(navigationValue, filterKey, filterValue))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}