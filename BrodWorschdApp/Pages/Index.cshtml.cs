using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrodWorschdApp.Pages
{
    public class IndexModel : BasePageModel
    {

        public bool IsOrderViewVisible { get; set; }

        public IndexModel(DatabaseHandler databaseHandler, ILogger<IndexModel> logger) : base(databaseHandler, logger)
        {
            GroupedOrdersList = new List<GroupedOrder>();
        }

        public async Task OnGetAsync()
        {
            var orders = await _databaseHandler.GetCustomerOrdersWithDetails(co => true);
            var products = await _databaseHandler.GetDataFromTable<ProductsTable>(x => true);
            GroupedOrdersList = CalculateGroupedOrders(orders, products);
        }
        public async Task OnPostToggleOrderViewAsync(int customerId, string orderNumber)
        {
            IsOrderViewVisible = !IsOrderViewVisible;

            var orders = await _databaseHandler.GetCustomerOrdersWithDetails(co => co.OrderNumber == orderNumber);
            var products = await _databaseHandler.GetDataFromTable<ProductsTable>(x => true);

            CustomerId = customerId;
            OrderNumber = orderNumber;
            OrderStatus = "Anzeigen";

            if (IsOrderViewVisible)
            {
                OrderDetails = await GetOrderDetails(orderNumber);
            }
            CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == CustomerId);
            ProductList = await _databaseHandler.GetDataFromTable<ProductsTable>();
        }

    }
}
