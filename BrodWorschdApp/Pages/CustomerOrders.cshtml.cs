namespace BrodWorschdApp.Pages

{
    public class OrderItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Cost { get; set; }
    }

    public class CustomerOrdersModel : BasePageModel
    {
        public bool IsEditOrderFormVisible { get; set; }
        public bool IsNewOrderFormVisible { get; set; }

        public CustomerOrdersModel(DatabaseHandler databaseHandler, ILogger<CustomerOrdersModel> logger) : base(databaseHandler, logger)
        {
        }
        public async Task OnPostToggleEditOrderForm(int customerId, string orderNumber)
        {
            CustomerId = customerId;
            OrderNumber = orderNumber;
            OrderStatus = "bearbeiten";
            IsEditOrderFormVisible = !IsEditOrderFormVisible;
            if (IsEditOrderFormVisible)
            {
                OrderDetails = await GetOrderDetails(orderNumber);
            }
            CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == CustomerId);
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

            // Rufen Sie OnPostCalculateCosts mit den bestimmten Parametern auf
            await OnPostCalculateCosts(customerId, orderQuantity);

            return orders;
        }

        public async Task OnGetAsync(int customerId)
        {
            CustomerId = customerId;
            OrderStatus = "";

            var orders = await _databaseHandler.GetCustomerOrdersWithDetails(co => co.CustomerId == CustomerId);
            var products = await _databaseHandler.GetDataFromTable<ProductsTable>(x => true);
            GroupedOrdersList = CalculateGroupedOrders(orders, products);
            CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == CustomerId);
        }

        public async Task OnPostCancelAction(int customerId)
        {
            CustomerId= customerId;
            IsEditOrderFormVisible = false;
            IsNewOrderFormVisible = false;
            await OnGetAsync(customerId);
        }

        public async Task OnPostToggleNewOrderForm(int customerId)
        {
            CustomerId = customerId;
            OrderStatus = "erstellen";
            IsNewOrderFormVisible = !IsNewOrderFormVisible;
            if (IsNewOrderFormVisible)
            {
                ProductList = await _databaseHandler.GetDataFromTable<ProductsTable>();
            }
            CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == CustomerId);
        }

        public async Task OnPostSubmitOrder(Dictionary<int, int> orderQuantity, int customerId, string userName, string? orderNumber = null)
        {
            OrderNumber = orderNumber ?? string.Empty;
            if(userName == null || userName == "")
            {
                ErrorMessage = "Es muss ein Bearbeiter eingetragen werden!";
            }
            if (orderNumber == null)
            {
                // Ermitteln Sie die maximale OrderNumber
                var maxOrderNumber = (await _databaseHandler.GetDataFromTable<CustomerOrdersTable>(x => true))
                .Select(x => int.TryParse(x.OrderNumber.Substring(2), out var num) ? num : (int?)null)
                .Max() ?? 0;

                // Erhöhen Sie die maximale OrderNumber um eins und formatieren Sie sie als vierstellige Zahl
                var orderNumberSuffix = (maxOrderNumber + 1).ToString("D4");

                // Extrahieren Sie die letzten beiden Ziffern des aktuellen Jahres
                var yearSuffix = DateTime.Now.Year.ToString().Substring(2);

                // Kombinieren Sie das Jahr und die OrderNumber
                orderNumber = yearSuffix + orderNumberSuffix;
            }

            foreach (var item in orderQuantity)
            {
                var productId = item.Key;
                var quantity = item.Value;

                // Wenn die Menge 0 ist, überspringen Sie diesen Artikel
                if (quantity == 0)
                {
                    continue;
                }

                // Abrufen der bestehenden Bestellung aus der Datenbank
                var order = (await _databaseHandler.GetDataFromTable<CustomerOrdersTable>(o => o.OrderNumber == orderNumber && o.ProductId == productId)).FirstOrDefault();

                if (order != null)
                {
                    // Ändern Sie die Eigenschaften der Bestellung
                    order.Quantity = quantity; // Setzen Sie die Quantity
                    order.Date = DateTime.Now.ToString();
                    order.UserName = userName;

                    // Aktualisieren Sie die Bestellung in der Datenbank
                    await _databaseHandler.UpdateDataInTable(order);
                }
                else
                {
                    // Wenn die Bestellung nicht existiert, erstellen Sie eine neue
                    order = new CustomerOrdersTable
                    {
                        OrderNumber = orderNumber,
                        CustomerId = customerId,
                        ProductId = productId, // Setzen Sie die ProductId
                        Quantity = quantity, // Setzen Sie die Quantity
                        Date = DateTime.Now.ToString(),
                        UserName = userName,
                        // Sie können hier weitere Eigenschaften hinzufügen, die eine Bestellung haben könnte
                    };

                    // Fügen Sie die Bestellung zur Datenbank hinzu
                    await _databaseHandler.AddDataToTable(order);
                }
            }
            await OnGetAsync(customerId);
        }
        public async Task OnPostCalculateCosts(int customerId, Dictionary<int, int> orderQuantity, string? orderNumber = null)
        {
            CustomerId = customerId;
            CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == CustomerId);
            ProductList = await _databaseHandler.GetDataFromTable<ProductsTable>();
            IsNewOrderFormVisible = true;
            await CalculateCost(customerId, orderQuantity);

            if (orderNumber != null)
            {
                OrderStatus = "bearbeiten";
                OrderNumber = orderNumber;
            }
        }

        //public async Task OnPostCalculateCosts(int customerId, Dictionary<int, int> orderQuantity, string? orderNumber = null)
        //{
        //    CalculatedOrder = new List<OrderItem>();
        //    CustomerId = customerId;
        //    ProductList = await _databaseHandler.GetDataFromTable<ProductsTable>();
        //    IsNewOrderFormVisible = true;

        //    if (orderNumber != null)
        //    {
        //        OrderStatus = "bearbeiten";
        //        OrderNumber = orderNumber;
        //    }

        //    foreach (var item in orderQuantity)
        //    {
        //        var productId = item.Key;
        //        var quantity = item.Value;
        //        var product = (await _databaseHandler.GetDataFromTable<ProductsTable>(p => p.ID == productId)).FirstOrDefault();
        //        if (product != null && product.Price.HasValue)
        //        {
        //            var cost = (decimal)(product.Price.Value * quantity);
        //            CalculatedOrder.Add(new OrderItem { ProductId = productId, Quantity = quantity, Cost = cost });
        //        }
        //    }
        //    CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == CustomerId);
        //}
        public async Task OnPostDeleteOrder(string orderNumber, int customerId)
        {
            // Abrufen aller Bestellungen basierend auf der OrderNumber
            var orders = await _databaseHandler.GetDataFromTable<CustomerOrdersTable>(o => o.OrderNumber == orderNumber);

            foreach (var order in orders)
            {
                // Löschen Sie die Bestellung aus der Datenbank
                await _databaseHandler.DeleteDataFromTable<CustomerOrdersTable>(order.ID);
            }
            await OnGetAsync(customerId);
        }
    }
}
