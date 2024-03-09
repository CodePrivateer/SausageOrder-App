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
        public string PickUpName { get; set; }
        public string UserName { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<int, ProductsTable> Products { get; set; }
        public Dictionary<int, int> OrderedQuantitiesPerProduct { get; set; }
        public List<OrderItem> CalculatedOrder { get; set; }
        public float TotalBookedOrdersSum { get; set; }
        public float TotalNotBookedOrdersSum { get; set; }

        public BasePageModel(DatabaseHandler databaseHandler, ILogger<BasePageModel> logger)
        {
            _databaseHandler = databaseHandler;
            _logger = logger;
            CustomerList = new List<CustomersTable>();
            ProductList = new List<ProductsTable>();
            GroupedOrdersList = new List<GroupedOrder>();
            OrderDetails = new List<CustomerOrdersTable>();
            Products = new Dictionary<int, ProductsTable>();
            OrderedQuantitiesPerProduct = new Dictionary<int, int>();
            CalculatedOrder = new List<OrderItem>();
            ErrorMessage = string.Empty;
            OrderNumber = string.Empty;
            OrderStatus = string.Empty;
            UserName = string.Empty;
            PickUpName = string.Empty;
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

        public float CalculateTotalSum(List<GroupedOrder> groupedOrders)
        {
            return groupedOrders
                .Where(order => order.Items.All(item => item.Booked?.ToLower() == "booked"))
                .Sum(order => order.TotalPrice);
        }

        public float CalculateTotalNotBookedSum(List<GroupedOrder> groupedOrders)
        {
            return groupedOrders
                .Where(order => order.Items.All(item => item.Booked?.ToLower() == ""))
                .Sum(order => order.TotalPrice);
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

        public async Task OnPostFilterOrders(string customerFirstName, string customerLastName, string orderNumber)
        {
            var filteredOrders = new List<GroupedOrder>();

            await GetGroupedOrderList();

            // If no filter is provided, return all orders
            if (!string.IsNullOrEmpty(customerFirstName) || !string.IsNullOrEmpty(customerLastName) || !string.IsNullOrEmpty(orderNumber))
            {
                foreach (var orderGroup in GroupedOrdersList)
                {
                    var customerMatches = orderGroup.Items.Any(order =>
                        (string.IsNullOrEmpty(customerFirstName) || order.Customer?.FirstName?.ToLower().Contains(customerFirstName.ToLower()) == true) &&
                        (string.IsNullOrEmpty(customerLastName) || order.Customer?.LastName?.ToLower().Contains(customerLastName.ToLower()) == true)
                    );

                    var orderNumberMatches = orderGroup.Items.Any(order =>
                        (string.IsNullOrEmpty(orderNumber) || order.OrderNumber.Contains(orderNumber))
                    );

                    if (customerMatches && orderNumberMatches)
                    {
                        filteredOrders.Add(orderGroup);
                    }
                }
                GroupedOrdersList = filteredOrders;
            }
            TotalBookedOrdersSum = CalculateTotalSum(GroupedOrdersList);
            TotalNotBookedOrdersSum = CalculateTotalNotBookedSum(GroupedOrdersList);
        }
        public async Task GetGroupedOrderList()
        {
            var orders = await _databaseHandler.GetCustomerOrdersWithDetails(co => true);
            var products = await _databaseHandler.GetDataFromTable<ProductsTable>(x => true);
            GroupedOrdersList = CalculateGroupedOrders(orders, products);
        }

        public class OrderItem
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal Cost { get; set; }
        }
    }
}
