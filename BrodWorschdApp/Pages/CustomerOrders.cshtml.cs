using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BrodWorschdApp.Pages

{
    public class CustomerOrdersModel : BasePageModel
    {
        public bool IsEditOrderFormVisible { get; set; }
        public bool IsNewOrderFormVisible { get; set; }

        public CustomerOrdersModel(DatabaseHandler databaseHandler, ILogger<CustomerOrdersModel> logger, LanguageService languageService) :
            base(databaseHandler, logger, languageService)
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
            ProductList = await _databaseHandler.GetDataFromTable<ProductsTable>();
        }

        public async Task<IActionResult> OnGetAsync(int customerId, string culture)
        {
            CustomerId = customerId;
            OrderStatus = "";

            var orders = await _databaseHandler.GetCustomerOrdersWithDetails(co => co.CustomerId == CustomerId);
            var products = await _databaseHandler.GetDataFromTable<ProductsTable>(x => true);
            GroupedOrdersList = CalculateGroupedOrders(orders, products);
            CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == CustomerId);

            // Setup of the Language
            if (!string.IsNullOrEmpty(culture))
            {
                _languageservice.SetCulture(culture);
                return RedirectToPage();
            }
            return Page();
        }

        public async Task OnPostCancelAction(int customerId)
        {
            CustomerId= customerId;
            IsEditOrderFormVisible = false;
            IsNewOrderFormVisible = false;
            await OnGetAsync(customerId, Culture);
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

        public async Task OnPostSubmitOrder(Dictionary<int, int> orderQuantity, int customerId, string userName, string pickUpName, string? orderNumber = null)
        {
            OrderNumber = orderNumber ?? string.Empty;
            if(userName == null || userName == "")
            {
                // ErrorMessage = "Bearbeiterfeld war leer, 'Benutzer-1' eingetragen!";
                userName = "Benutzer-1";
            }
            if(pickUpName==null || pickUpName == "")
            {
                pickUpName = "Selbstabholer";
            }
            PickUpName = pickUpName;
            UserName = userName;

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

                // Abrufen der bestehenden Bestellung aus der Datenbank
                var order = (await _databaseHandler.GetDataFromTable<CustomerOrdersTable>(o => o.OrderNumber == orderNumber && o.ProductId == productId)).FirstOrDefault();

                if (order != null)
                {
                    Console.WriteLine(order.Quantity + " " + quantity);
                    if (order.Quantity >= 0 && quantity == 0)
                    {
                        await _databaseHandler.DeleteDataFromTable<CustomerOrdersTable>(o => o.OrderNumber == orderNumber && o.ProductId == productId);
                        continue;
                    }

                    // Ändern Sie die Eigenschaften der Bestellung
                    order.Quantity = quantity; // Setzen Sie die Quantity
                    order.Date = DateTime.Now.ToString();
                    order.UserName = userName;
                    order.PickUpName = pickUpName;

                    // Aktualisieren Sie die Bestellung in der Datenbank
                    await _databaseHandler.UpdateDataInTable(order);
                }
                else if (quantity != 0)
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
                        PickUpName = pickUpName,
                        // Sie können hier weitere Eigenschaften hinzufügen, die eine Bestellung haben könnte
                    };

                    // Fügen Sie die Bestellung zur Datenbank hinzu
                    await _databaseHandler.AddDataToTable(order);
                }
            }
            await OnGetAsync(customerId, Culture);
        }
        public async Task OnPostCalculateCosts(int customerId, Dictionary<int, int> orderQuantity, string? orderNumber = null)
        {
            CustomerId = customerId;
            CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == CustomerId);
            ProductList = await _databaseHandler.GetDataFromTable<ProductsTable>(x => true);

            await CalculateCost(customerId, orderQuantity);

            if (orderNumber != null)
            {
                OrderStatus = "bearbeiten";
                OrderNumber = orderNumber;
                IsEditOrderFormVisible = true;
            }
            else
            {
                IsNewOrderFormVisible = true;
            }
        }

        public async Task OnPostDeleteOrder(string orderNumber, int customerId)
        {
            // Abrufen aller Bestellungen basierend auf der OrderNumber
            var orders = await _databaseHandler.GetDataFromTable<CustomerOrdersTable>(o => o.OrderNumber == orderNumber);

            foreach (var order in orders)
            {
                // Löschen Sie die Bestellung aus der Datenbank
                await _databaseHandler.DeleteDataFromTable<CustomerOrdersTable>(order.ID);
            }
            await OnGetAsync(customerId, Culture);
        }
    }
}
