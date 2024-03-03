using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrodWorschdApp.Pages
{
    public class BasePageModel : PageModel
    {
        protected readonly DatabaseHandler _databaseHandler;
        protected readonly ILogger _logger;
        public List<CustomersTable> CustomerList { get; set; }
        public List<ProductsTable> ProductList { get; set; }
        public string ErrorMessage { get; set; }

        public BasePageModel(DatabaseHandler databaseHandler, ILogger<BasePageModel> logger)
        {
            _databaseHandler = databaseHandler;
            _logger = logger;
            CustomerList = new List<CustomersTable>();
            ProductList = new List<ProductsTable>();
            ErrorMessage = string.Empty;
        }
        public static string FormatAsEuro(float? valueFloat = null, decimal? valueDecimal = null)
        {
            if (valueFloat.HasValue)
            {
                decimal decimalValue = (decimal)valueFloat.Value;
                return decimalValue.ToString("C", new System.Globalization.CultureInfo("de-DE"));
            }
            else if (valueDecimal.HasValue)
            {
                decimal decimalValue = (decimal)valueDecimal.Value;
                return decimalValue.ToString("C", new System.Globalization.CultureInfo("de-DE"));
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
