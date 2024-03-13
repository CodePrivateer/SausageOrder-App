using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace BrodWorschdApp.Pages
{
    public class LicenseModel : BasePageModel
    {
        public LicenseModel(DatabaseHandler databaseHandler, ILogger<IndexModel> logger) : base(databaseHandler, logger)
        {
        }

        public void OnGet()
        {

        }
    }

}
