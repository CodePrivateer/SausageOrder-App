using System.Globalization;
using System.Resources;
using System.Reflection;
using System.Collections;

namespace BrodWorschdApp
{
    public class LanguageService
    {
        private readonly ResourceManager _resourceManager;
        public CultureInfo CurrentCulture { get; private set; }

        public LanguageService()
        {
            _resourceManager = new ResourceManager("BrodWorschdApp.Language", Assembly.GetExecutingAssembly());
            CurrentCulture = CultureInfo.CurrentCulture;
        }

        public string GetString(string name)
        {
            string? value = _resourceManager.GetString(name, CurrentCulture);
            return value ?? string.Empty;
        }

        public void SetCulture(string culture)
        {
            CurrentCulture = new CultureInfo(culture);
        }

        public Dictionary<string, string> GetAllStrings()
        {
            var resourceSet = _resourceManager.GetResourceSet(CurrentCulture, true, true);
            if (resourceSet != null)
            {
                return resourceSet.Cast<DictionaryEntry>().ToDictionary(
                    entry => entry.Key?.ToString() ?? "",
                    entry => entry.Value?.ToString() ?? "");
            }
            return new Dictionary<string, string>();
        }
    }
}