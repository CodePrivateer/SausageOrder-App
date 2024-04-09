using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;

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
        public float TotalOrdersSum { get; set; }
        public float TotalBookedOrdersSum { get; set; }
        public float TotalNotBookedOrdersSum { get; set; }
        public float TotalPaidSum { get; set; }

        // languageService
        public readonly LanguageService _languageservice;
        public Dictionary<string, string> CultureStrings { get; set; }
        public string Culture { get; set; }

        public BasePageModel(DatabaseHandler databaseHandler, ILogger<BasePageModel> logger, LanguageService languageService)
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

            //Language Service
            _languageservice = languageService;
            CultureStrings = _languageservice.GetAllStrings();
            Culture = "";
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
                decimal decimalValue = valueDecimal.Value;
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
                    TotalPrice = (float)group.Sum(item => item.Quantity * (products.FirstOrDefault(p => p.ID == item.ProductID)?.Price ?? 0)),
                    TotalDelivered = (float)group.Where(item => (item.Booked ?? "").ToLower() == "booked").Sum(item => item.Quantity * (products.FirstOrDefault(p => p.ID == item.ProductID)?.Price ?? 0)),
                    TotalOpen = (float)group.Where(item => (item.Booked ?? "").ToLower() == "").Sum(item => item.Quantity * (products.FirstOrDefault(p => p.ID == item.ProductID)?.Price ?? 0)),
                    TotalPaid = (float)group.Where(item => (item.Paid ?? "").ToLower() == "paid").Sum(item => item.Quantity * (products.FirstOrDefault(p => p.ID == item.ProductID)?.Price ?? 0))
                })
                .ToList();
        }

        public float CalculateTotalPaidSum(List<GroupedOrder> groupedOrders)
        {
            return groupedOrders.Sum(order => order.TotalPaid);
        }

        public float CalculateTotalBookedSum(List<GroupedOrder> groupedOrders)
        {
            return groupedOrders.Sum(order => order.TotalDelivered);
        }

        public float CalculateTotalNotBookedSum(List<GroupedOrder> groupedOrders)
        {
            return groupedOrders.Sum(order => order.TotalOpen);
        }
        public float CalculateTotal(List<GroupedOrder> groupedOrders)
        {
            return groupedOrders.Sum(order => order.TotalPrice);
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
            int customerId = orders.FirstOrDefault()?.CustomerID ?? 0;
            var orderQuantity = orders.ToDictionary(o => o.ProductID, o => o.Quantity);

            // Rufen Sie CalculateCosts mit den bestimmten Parametern auf
            await CalculateCost(customerId, orderQuantity);

            return orders;
        }

        public async Task CalculateCost(int customerId, Dictionary<int, int> orderQuantity)
        {
            CustomerId = customerId;
            CalculatedOrder.Clear();

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

        public async Task OnPostFilterOrders(string customerFirstName, string customerLastName, string orderNumber, string pickUpName)
        {
            var filteredOrders = new List<GroupedOrder>();

            await GetGroupedOrderList();

            // If no filter is provided, return all orders
            if (!string.IsNullOrEmpty(customerFirstName) || !string.IsNullOrEmpty(customerLastName) || !string.IsNullOrEmpty(orderNumber) || !string.IsNullOrEmpty(pickUpName))
            {
                foreach (var orderGroup in GroupedOrdersList)
                {
                    var customerMatches = orderGroup.Items.Any(order =>
                        (string.IsNullOrEmpty(customerFirstName) || order.Customer?.FirstName?.ToLower().Contains(customerFirstName.ToLower()) == true) &&
                        (string.IsNullOrEmpty(customerLastName) || order.Customer?.LastName?.ToLower().Contains(customerLastName.ToLower()) == true) &&
                        (string.IsNullOrEmpty(pickUpName) || order.PickUpName?.ToLower().Contains(pickUpName.ToLower()) == true)
                    );

                    var orderNumberMatches = orderGroup.Items.Any(order =>
                        string.IsNullOrEmpty(orderNumber) || order.OrderNumber.Contains(orderNumber)
                    );

                    if (customerMatches && orderNumberMatches)
                    {
                        filteredOrders.Add(orderGroup);
                    }
                }
                GroupedOrdersList = filteredOrders;
            }
            await GetSums(false);
        }

        public async Task GetSums(bool refreshData = true)
        {
            if (refreshData)
            {
                await GetGroupedOrderList();
            }

            TotalBookedOrdersSum = CalculateTotalBookedSum(GroupedOrdersList);
            TotalNotBookedOrdersSum = CalculateTotalNotBookedSum(GroupedOrdersList);
            TotalOrdersSum = CalculateTotal(GroupedOrdersList);
            TotalPaidSum = CalculateTotalPaidSum(GroupedOrdersList);
        }
        public async Task GetGroupedOrderList()
        {
            var orders = await _databaseHandler.GetCustomerOrdersWithDetails(co => true);
            var products = await _databaseHandler.GetDataFromTable<ProductsTable>(x => true);
            GroupedOrdersList = CalculateGroupedOrders(orders, products);
        }
        public async Task UpdateInventory(string orderNumber, int productId, bool isBooked)
        {
            // Get the order details
            var orderDetails = await GetOrderDetails(orderNumber);

            // Find the corresponding product in the ProductsTable
            var product = await _databaseHandler.FindProductById<ProductsTable>(productId);

            // Find the order with the given productId
            var order = orderDetails.FirstOrDefault(o => o.ProductID == productId);

            if (product != null && order != null)
            {
                if (isBooked)
                {
                    // Decrease the inventory if the order is being booked
                    product.Inventory -= order.Quantity;
                }
                else
                {
                    // Increase the inventory if the order is being storned
                    product.Inventory += order.Quantity;
                }

                // Save the changes to the database
                await _databaseHandler.UpdateDataInTable(product);
            }
        }
        public async Task UpdateInventoryAfterBooking(List<CustomerOrdersTable> unbookedProducts) // Ganze Bestellung
        {
            // Durchlaufen Sie jedes Produkt in der Bestellung
            foreach (var order in unbookedProducts)
            {
                // Finden Sie das entsprechende Produkt in der ProductsTable
                var product = await _databaseHandler.FindProductById<ProductsTable>(order.ProductID);

                // Aktualisieren Sie das Inventory des Produkts
                if (product != null && product.Inventory != null)
                {
                    product.Inventory -= order.Quantity;

                    // Speichern Sie die Änderungen in der Datenbank
                    await _databaseHandler.UpdateDataInTable(product);
                }
            }
        }

        public async Task UpdateInventoryAfterCancellation(string orderNumber) // Ganze Bestellung
        {
            // Holen Sie sich die Details der stornierten Bestellung
            var orderDetails = await GetOrderDetails(orderNumber);

            // Durchlaufen Sie jedes Produkt in der stornierten Bestellung
            foreach (var order in orderDetails)
            {
                // Finden Sie das entsprechende Produkt in der ProductsTable
                var product = await _databaseHandler.FindProductById<ProductsTable>(order.ProductID);

                // Aktualisieren Sie das Inventory des Produkts
                if (product != null && product.Inventory != null)
                {
                    product.Inventory += order.Quantity;

                    // Speichern Sie die Änderungen in der Datenbank
                    await _databaseHandler.UpdateDataInTable(product);
                }
            }
        }

        public class OrderItem
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal Cost { get; set; }
        }
    }
}
