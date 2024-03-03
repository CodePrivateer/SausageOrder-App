using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrodWorschdApp.Pages
{
    public class BasePageModel : PageModel
    {
        protected readonly DatabaseHandler _databaseHandler;
        protected readonly ILogger _logger;
        public List<CustomersTable> CustomerList { get; set; }
        public List<ProductsTable> ProductList { get; set; }
        public List<GroupedOrder> GroupedOrdersList { get; set; }
        public int CustomerId { get; set; }
        public string OrderNumber { get; set; }
        public List<CustomerOrdersTable> OrderDetails { get; set; }
        public string OrderStatus { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<int, ProductsTable> Products { get; set; }
        public List<OrderItem> CalculatedOrder { get; set; }

        public BasePageModel(DatabaseHandler databaseHandler, ILogger<BasePageModel> logger)
        {
            _databaseHandler = databaseHandler;
            _logger = logger;
            CustomerList = new List<CustomersTable>();
            ProductList = new List<ProductsTable>();
            GroupedOrdersList = new List<GroupedOrder>();
            OrderDetails = new List<CustomerOrdersTable>();
            Products = new Dictionary<int, ProductsTable>();
            CalculatedOrder = new List<OrderItem>();
            ErrorMessage = string.Empty;
            OrderNumber = string.Empty;
            OrderStatus = string.Empty;
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

        public List<GroupedOrder> CalculateGroupedOrders(List<CustomerOrdersTable> orders, List<ProductsTable> products)
        {
            return orders
                .GroupBy(order => order.OrderNumber)
                .Select(group => new GroupedOrder
                {
                    OrderNumber = group.Key,
                    Items = group.ToList(),
                    TotalPrice = (float)group.Sum(item => item.Quantity * (products.FirstOrDefault(p => p.ID == item.ProductId)?.Price ?? 0))
                })
                .ToList();
        }
        public async Task<List<CustomerOrdersTable>> GetOrderDetails(string orderNumber)
        {
            // Übergabe der Bestellnummer
            OrderNumber = orderNumber;
            // Abrufen aller Produkte
            var allProducts = await _databaseHandler.GetDataFromTable<ProductsTable>();
            foreach (var product in allProducts)
            {
                if (!Products.ContainsKey(product.ID))
                {
                    Products.Add(product.ID, product);
                }
            }

            // Abrufen der Bestelldaten basierend auf der OrderNumber
            var orders = await _databaseHandler.GetDataFromTable<CustomerOrdersTable>(o => o.OrderNumber == orderNumber);

            // Bestimmen Sie die customerId und orderQuantity
            int customerId = orders.FirstOrDefault()?.CustomerId ?? 0;
            var orderQuantity = orders.ToDictionary(o => o.ProductId, o => o.Quantity);

            // Rufen Sie CalculateCosts mit den bestimmten Parametern auf
            await CalculateCost(customerId, orderQuantity);

            return orders;
        }
        public async Task CalculateCost(int customerId, Dictionary<int, int> orderQuantity)
        {
            CustomerId = customerId;
            foreach (var item in orderQuantity)
            {
                var productId = item.Key;
                var quantity = item.Value;
                var product = (await _databaseHandler.GetDataFromTable<ProductsTable>(p => p.ID == productId)).FirstOrDefault();
                if (product != null && product.Price.HasValue)
                {
                    var cost = (decimal)(product.Price.Value * quantity);
                    CalculatedOrder.Add(new OrderItem { ProductId = productId, Quantity = quantity, Cost = cost });
                }
            }
        }
    }
}
