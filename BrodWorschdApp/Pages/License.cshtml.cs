using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.Diagnostics;

namespace BrodWorschdApp.Pages
{
    public class LicenseModel : BasePageModel
    {
        public LicenseModel(DatabaseHandler databaseHandler, ILogger<IndexModel> logger, LanguageService languageService) :
            base(databaseHandler, logger, languageService)
        {
        }

        public async Task<IActionResult> OnGetAsync(string culture)
        {
            await GetSums();

            // Setup of the Language
            if (!string.IsNullOrEmpty(culture))
            {
                _languageservice.SetCulture(culture);
                return RedirectToPage();
            }
            return Page();
        }
    }

}
