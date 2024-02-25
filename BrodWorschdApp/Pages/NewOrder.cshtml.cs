using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrodWorschdApp.Pages
{
    public class NewOrderModel : PageModel
    {
        public int CustomerId { get; set; }
        public void OnGet(int customerId)
        {
            CustomerId = customerId;
        }
    }
}
