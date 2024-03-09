namespace BrodWorschdApp.Pages
{
    public class BookedModel : BasePageModel
    {

        public bool IsOrderViewVisible { get; set; }

        public BookedModel(DatabaseHandler databaseHandler, ILogger<BookedModel> logger) : base(databaseHandler, logger)
        {
        }

        public async Task OnGetAsync()
        {
            await GetGroupedOrderList();
            TotalBookedOrdersSum = CalculateTotalSum(GroupedOrdersList);
        }
        public async Task OnPostToggleOrderViewAsync(int customerId, string orderNumber, string isOrderViewVisible = "")
        {
            if (isOrderViewVisible == "True")
            {
                IsOrderViewVisible = false;
            }
            else if (isOrderViewVisible == "False")
            {
                IsOrderViewVisible = true;
            }
            else if (string.IsNullOrEmpty(isOrderViewVisible))
            {
                IsOrderViewVisible = !IsOrderViewVisible;
            }

            CustomerId = customerId;
            OrderNumber = orderNumber;
            OrderStatus = "Anzeigen";

            if (IsOrderViewVisible)
            {
                OrderDetails = await GetOrderDetails(orderNumber);
            }
            CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == CustomerId);
            ProductList = await _databaseHandler.GetDataFromTable<ProductsTable>(x => true);

            await OnGetAsync();
        }

        public async Task OnPostBookOrder(int customerId, string orderNumber)
        {
            CustomerId = customerId;
            OrderNumber = orderNumber;
            OrderStatus = "Anzeigen";
            IsOrderViewVisible = true;

            // Die Bestellung auf gebucht setzen
            await _databaseHandler.UpdateDataInTable<CustomerOrdersTable>(o => o.OrderNumber == orderNumber, entity => entity.Booked = "booked");
            // Die Bestellmengen vom Lagerinhalt pro Produkt abziehen
            await UpdateInventoryAfterBooking(orderNumber);

            OrderDetails = await GetOrderDetails(orderNumber);

            CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == CustomerId);
            ProductList = await _databaseHandler.GetDataFromTable<ProductsTable>(x => true);

            await OnGetAsync();
        }
        public async Task UpdateInventoryAfterBooking(string orderNumber)
        {
            // Holen Sie sich die Details der Bestellung
            var orderDetails = await GetOrderDetails(orderNumber);

            // Durchlaufen Sie jedes Produkt in der Bestellung
            foreach (var order in orderDetails)
            {
                // Finden Sie das entsprechende Produkt in der ProductsTable
                var product = await _databaseHandler.FindProductById<ProductsTable>(order.ProductId);

                // Aktualisieren Sie das Inventory des Produkts
                if (product != null && product.Inventory != null)
                {
                    product.Inventory -= order.Quantity;

                    // Speichern Sie die Änderungen in der Datenbank
                    await _databaseHandler.UpdateDataInTable<ProductsTable>(product);
                }
            }
        }

        public async Task OnPostStornoOrder(int customerId, string orderNumber)
        {
            CustomerId = customerId;
            OrderNumber = orderNumber;
            OrderStatus = "Anzeigen";
            IsOrderViewVisible = true;
            // Die Bestellung auf ungebucht setzen
            await _databaseHandler.UpdateDataInTable<CustomerOrdersTable>(o => o.OrderNumber == orderNumber, entity => entity.Booked = string.Empty);
            // Die Bestellmengen zum Lagerinhalt pro Produkt hinzuzählen
            await UpdateInventoryAfterCancellation(orderNumber);

            OrderDetails = await GetOrderDetails(orderNumber);

            CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == CustomerId);
            ProductList = await _databaseHandler.GetDataFromTable<ProductsTable>(x => true);

            await OnGetAsync();
        }
        public async Task UpdateInventoryAfterCancellation(string orderNumber)
        {
            // Holen Sie sich die Details der stornierten Bestellung
            var orderDetails = await GetOrderDetails(orderNumber);

            // Durchlaufen Sie jedes Produkt in der stornierten Bestellung
            foreach (var order in orderDetails)
            {
                // Finden Sie das entsprechende Produkt in der ProductsTable
                var product = await _databaseHandler.FindProductById<ProductsTable>(order.ProductId);

                // Aktualisieren Sie das Inventory des Produkts
                if (product != null && product.Inventory != null)
                {
                    product.Inventory += order.Quantity;

                    // Speichern Sie die Änderungen in der Datenbank
                    await _databaseHandler.UpdateDataInTable<ProductsTable>(product);
                }
            }
        }
    }
}
