namespace BrodWorschdApp.Pages
{
    public class IndexModel : BasePageModel
    {

        public bool IsOrderViewVisible { get; set; }

        public IndexModel(DatabaseHandler databaseHandler, ILogger<IndexModel> logger) : base(databaseHandler, logger)
        {
        }

        public async Task OnGetAsync()
        {
            await GetSums();
        }
        public async Task OnPostToggleOrderViewAsync(int customerId, string orderNumber, string isOrderViewVisible = "")
        {
            if (isOrderViewVisible == "True")
            {
                IsOrderViewVisible = false;
            }else if (isOrderViewVisible == "False")
            {
                IsOrderViewVisible = true;
            }else if(string.IsNullOrEmpty(isOrderViewVisible)) 
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

            await GetSums();
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

        public async Task OnPostBookOrderItem(int productId, int customerId, string orderNumber)
        {
            CustomerId = customerId;
            OrderNumber = orderNumber;
            OrderStatus = "Anzeigen";
            IsOrderViewVisible = true;

            // Die Bestellung auf gebucht setzen
            await _databaseHandler.UpdateOrderItemStatus(orderNumber, productId);
            // Die Bestellmengen vom Lagerinhalt pro Produkt abziehen
            await UpdateInventoryAfterBookingItem(orderNumber, productId);

            OrderDetails = await GetOrderDetails(orderNumber);

            CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == CustomerId);
            ProductList = await _databaseHandler.GetDataFromTable<ProductsTable>(x => true);

            await OnGetAsync();
        }
        public async Task OnPostPayOrderItem(int productId, int customerId, string orderNumber)
        {
            CustomerId = customerId;
            OrderNumber = orderNumber;
            OrderStatus = "Anzeigen";
            IsOrderViewVisible = true;

            // Die Bestellung auf bezahlt setzen
            await _databaseHandler.UpdateOrderItemPayStatus(orderNumber, productId);

            OrderDetails = await GetOrderDetails(orderNumber);

            CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == CustomerId);
            ProductList = await _databaseHandler.GetDataFromTable<ProductsTable>(x => true);

            await OnGetAsync();
        }
        public async Task OnPostStornoPayOrderItem(int productId, int customerId, string orderNumber)
        {
            CustomerId = customerId;
            OrderNumber = orderNumber;
            OrderStatus = "Anzeigen";
            IsOrderViewVisible = true;

            // Die Bestellung auf bezahlt setzen
            await _databaseHandler.UpdateOrderItemPayStatus(orderNumber, productId, "");

            OrderDetails = await GetOrderDetails(orderNumber);

            CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == CustomerId);
            ProductList = await _databaseHandler.GetDataFromTable<ProductsTable>(x => true);

            await OnGetAsync();
        }
        public async Task OnPostStornoOrderItem(int productId, int customerId, string orderNumber)
        {
            CustomerId = customerId;
            OrderNumber = orderNumber;
            OrderStatus = "Anzeigen";
            IsOrderViewVisible = true;

            // Die Bestellung auf storniert setzen
            await _databaseHandler.UpdateOrderItemStatus(orderNumber, productId, "");
            // Die Bestellmengen zum Lagerinhalt pro Produkt hinzufügen
            await UpdateInventoryAfterStornoItem(orderNumber, productId);

            OrderDetails = await GetOrderDetails(orderNumber);

            CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == CustomerId);
            ProductList = await _databaseHandler.GetDataFromTable<ProductsTable>(x => true);

            await OnGetAsync();
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
        public async Task OnPostPayOrder(int customerId, string orderNumber)
        {
            CustomerId = customerId;
            OrderNumber = orderNumber;
            OrderStatus = "Anzeigen";
            IsOrderViewVisible = true;

            // Die Bestellung auf gebucht setzen
            await _databaseHandler.UpdateDataInTable<CustomerOrdersTable>(o => o.OrderNumber == orderNumber, entity => entity.Paid = "paid");
            // Die Bestellmengen vom Lagerinhalt pro Produkt abziehen
            await UpdateInventoryAfterBooking(orderNumber);

            OrderDetails = await GetOrderDetails(orderNumber);

            CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == CustomerId);
            ProductList = await _databaseHandler.GetDataFromTable<ProductsTable>(x => true);

            await OnGetAsync();
        }
        public async Task OnPostStornoPaidOrder(int customerId, string orderNumber)
        {
            CustomerId = customerId;
            OrderNumber = orderNumber;
            OrderStatus = "Anzeigen";
            IsOrderViewVisible = true;
            // Die Bestellung auf ungebucht setzen
            await _databaseHandler.UpdateDataInTable<CustomerOrdersTable>(o => o.OrderNumber == orderNumber, entity => entity.Paid = string.Empty);
            // Die Bestellmengen zum Lagerinhalt pro Produkt hinzuzählen
            await UpdateInventoryAfterCancellation(orderNumber);

            OrderDetails = await GetOrderDetails(orderNumber);

            CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == CustomerId);
            ProductList = await _databaseHandler.GetDataFromTable<ProductsTable>(x => true);

            await OnGetAsync();
        }
    }
}
