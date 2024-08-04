namespace BrodWorschdApp
{
    public class SearchModel : Dictionary<string, string>
    {
        public Dictionary<string, string>? CurrentFilters { get; set; }
        public List<T> FilterList<T>(List<T> list, Dictionary<string, string> filters)
        {
            var filteredList = new List<T>();

            // Wenn kein Filter bereitgestellt wird, geben Sie alle Elemente zurück
            if (filters.Any(f => !string.IsNullOrEmpty(f.Value)))
            {
                foreach (var item in list)
                {
                    bool matchesAllFilters = true;

                    foreach (var filter in filters)
                    {
                        var propertyInfo = typeof(T).GetProperty(filter.Key);
                        if (propertyInfo != null)
                        {
                            var value = propertyInfo.GetValue(item)?.ToString();
                            if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (!value.ToLower().Contains(filter.Value.ToLower()))
                                {
                                    matchesAllFilters = false;
                                    break;
                                }
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
                // Wenn keine Filter bereitgestellt wurden, geben Sie die ursprüngliche Liste zurück
                filteredList = list;
            }
            return filteredList;
        }
    }
}