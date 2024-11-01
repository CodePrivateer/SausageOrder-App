using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BrodWorschdApp.Pages
{
    public class IndexModel : BasePageModel
    {
        public bool IsOrderViewVisible { get; set; }
        public PaginationViewModel<GroupedOrder> Pagination { get; set; } = new PaginationViewModel<GroupedOrder>();
        public SearchModel SearchData { get; set; } = new SearchModel();
        public IndexModel(DatabaseHandler databaseHandler, ILogger<IndexModel> logger, LanguageService languageService) : 
            base(databaseHandler, logger, languageService)
        {
                SearchData = new SearchModel
            {
                { "OrderNumber", "" },
                { "FirstName", "" },
                { "LastName", "" },
                { "PickUpName", "" }
            };
            SearchData.CultureStrings = CultureStrings;
        }

        public async Task<IActionResult> OnGetAsync(string culture, int currentPage = 1)
        {
            await GetSums();
            // Abrufen der Filterinformationen aus der Sitzung
            var filtersJson = HttpContext.Session.GetString("filterGroupedOrder");
            if (!string.IsNullOrEmpty(filtersJson))
            {
                var filters = JsonConvert.DeserializeObject<SearchModel>(filtersJson);
                if (filters != null)
                {
                    await OnPostSearch(filters, currentPage);
                }
            }
            else
            {
                // Retrieve the customer list from the database here
                await GetGroupedOrderList();

                GetPagination(GroupedOrdersList, currentPage);
            }
            // Setup of the Language
            if (!string.IsNullOrEmpty(culture))
            {
                _languageservice.SetCulture(culture);
                return RedirectToPage();
            }
            return Page();
        }

        public void GetPagination(List<GroupedOrder> groupedOrdersList, int currentPage = 1)
        {
            Pagination.CurrentPage = currentPage;
            int pagePerSite = 10;
            // Call the Paginate method and assign the result to CustomerList
            GroupedOrdersList = Pagination.Paginate(groupedOrdersList, Pagination.CurrentPage, pagePerSite);

            // Calculate the total number of pages
            Pagination.TotalPages = Pagination.GetTotalPages(groupedOrdersList, pagePerSite);
        }

        public async Task<IActionResult> OnPostSearch(SearchModel data, int currentPage = 1)
        {
            // Retrieve the GroupedOrder list from the database here
            await GetGroupedOrderList();
            var filteredGroupedOrdersList = SearchData.FilterList(GroupedOrdersList, data);
            GetPagination(filteredGroupedOrdersList, currentPage);
            // Speichern Sie die Filterinformationen in der Sitzung
            HttpContext.Session.SetString("filterGroupedOrder", JsonConvert.SerializeObject(data));
            SearchData.CurrentFilters = data;
            return Page();
        }
        public async Task<IActionResult> OnPostClearFilters(int currentPage = 1)
        {
            // Retrieve the GroupedOrder list from the database here
            await GetGroupedOrderList();
            GetPagination(GroupedOrdersList, currentPage);
            return Page();
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
            // Orderdetails Laden und Post der Seite
            await HandleOrderDetails(customerId, orderNumber, IsOrderViewVisible);
        }

        public async Task OnPostBookOrder(int customerId, string orderNumber)
        {
            // Erstellen Sie eine Liste der Produkte, die noch nicht gebucht sind
            var unbookedProducts = await _databaseHandler.GetDataFromTable<CustomerOrdersTable>(o => o.OrderNumber == orderNumber && o.Booked != "booked");

            // Die Bestellung auf gebucht setzen
            await _databaseHandler.UpdateDataInTable<CustomerOrdersTable>(o => o.OrderNumber == orderNumber, entity => entity.Booked = "booked");

            // Die Bestellmengen vom Lagerinhalt pro Produkt abziehen
            await UpdateInventoryAfterBooking(unbookedProducts);
            // Orderdetails Laden zum Post der Seite
            await HandleOrderDetails(customerId, orderNumber);
        }

        public async Task OnPostBookOrderItem(int productId, int customerId, string orderNumber)
        {
            // Die Bestellung auf gebucht setzen
            await _databaseHandler.UpdateOrderItemStatus(orderNumber, productId);
            // Die Bestellmengen vom Lagerinhalt pro Produkt abziehen
            await UpdateInventory(orderNumber, productId, true);
            // Orderdetails Laden zum Post der Seite
            await HandleOrderDetails(customerId, orderNumber);
        }
        public async Task OnPostPayOrderItem(int productId, int customerId, string orderNumber)
        {
            // Die Bestellung auf bezahlt setzen
            await _databaseHandler.UpdateOrderItemPayStatus(orderNumber, productId);
            // Orderdetails Laden zum Post der Seite
            await HandleOrderDetails(customerId, orderNumber);
        }
        public async Task OnPostStornoPayOrderItem(int productId, int customerId, string orderNumber)
        {
            // Die Bestellung auf bezahlt setzen
            await _databaseHandler.UpdateOrderItemPayStatus(orderNumber, productId, "");
            // Orderdetails Laden zum Post der Seite
            await HandleOrderDetails(customerId, orderNumber);
        }
        public async Task OnPostStornoOrderItem(int productId, int customerId, string orderNumber)
        {
            // Die Bestellung auf storniert setzen
            await _databaseHandler.UpdateOrderItemStatus(orderNumber, productId, "");
            // Die Bestellmengen zum Lagerinhalt pro Produkt hinzuf�gen
            await UpdateInventory(orderNumber, productId, false);
            // Orderdetails Laden zum Post der Seite
            await HandleOrderDetails(customerId, orderNumber);
        }

        public async Task OnPostStornoOrder(int customerId, string orderNumber)
        {
            // Die Bestellung auf ungebucht setzen
            await _databaseHandler.UpdateDataInTable<CustomerOrdersTable>(o => o.OrderNumber == orderNumber, entity => entity.Booked = string.Empty);
            // Die Bestellmengen zum Lagerinhalt pro Produkt hinzuz�hlen
            await UpdateInventoryAfterCancellation(orderNumber);
            // Orderdetails Laden zum Post der Seite
            await HandleOrderDetails(customerId, orderNumber);
        }
        public async Task OnPostPayOrder(int customerId, string orderNumber)
        {
            // Die Bestellung auf gebucht setzen
            await _databaseHandler.UpdateDataInTable<CustomerOrdersTable>(o => o.OrderNumber == orderNumber, entity => entity.Paid = "paid");
            // Orderdetails Laden zum Post der Seite
            await HandleOrderDetails(customerId, orderNumber);
        }
        public async Task OnPostStornoPaidOrder(int customerId, string orderNumber)
        {
            // Die Bestellung auf ungebucht setzen
            await _databaseHandler.UpdateDataInTable<CustomerOrdersTable>(o => o.OrderNumber == orderNumber, entity => entity.Paid = string.Empty);
            // Orderdetails Laden zum Post der Seite
            await HandleOrderDetails(customerId, orderNumber);
        }

        public async Task HandleOrderDetails(int customerId, string orderNumber, bool isOrderViewVisible = true)
        {
            CustomerId = customerId;
            OrderNumber = orderNumber;
            OrderStatus = "Anzeigen";
            IsOrderViewVisible = isOrderViewVisible;

            OrderDetails = await GetOrderDetails(orderNumber);
            CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == CustomerId);
            ProductList = await _databaseHandler.GetDataFromTable<ProductsTable>(x => true);

            await OnGetAsync(Culture);
        }
    }
}
