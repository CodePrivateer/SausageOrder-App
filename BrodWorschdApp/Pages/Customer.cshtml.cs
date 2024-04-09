using BrodWorschdApp;
using BrodWorschdApp.Pages;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

public class CustomerModel : BasePageModel
{
    public bool IsNewCustomerFormVisible { get; set; }
    public bool IsEditCustomerFormVisible { get; set; }

    public PaginationViewModel<CustomersTable> Pagination { get; set; } = new PaginationViewModel<CustomersTable>();

    public SearchModel SearchData { get; set; }

    public CustomerModel(DatabaseHandler databaseHandler, ILogger<CustomerModel> logger, LanguageService languageService) :
            base(databaseHandler, logger, languageService)
    {
        SearchData = new SearchModel
        {
            { "FirstName", "" },
            { "LastName", "" }
        };
    }

    public async Task<IActionResult> OnGetAsync(string culture, int currentPage = 1)
    {
        // Abrufen der Filterinformationen aus der Sitzung
        var filtersJson = HttpContext.Session.GetString("filters");
        if (!string.IsNullOrEmpty(filtersJson))
        {
            var filters = JsonConvert.DeserializeObject<SearchModel>(filtersJson);
            if (filters != null)
            {
                SearchData.CurrentFilters = filters;
                await OnPostSearch(filters, currentPage);
            }
        }
        else
        {
            // Retrieve the customer list from the database here
            var customers = await _databaseHandler.GetDataFromTable<CustomersTable>();

            GetPagination(customers, currentPage);
        }

        // Setup of the Language
        if (!string.IsNullOrEmpty(culture))
        {
            _languageservice.SetCulture(culture);
            return RedirectToPage();
        }
        return Page();
    }

    public void GetPagination(List<CustomersTable> customers, int currentPage = 1)
    {
        Pagination.CurrentPage = currentPage;

        // Call the Paginate method and assign the result to CustomerList
        CustomerList = Pagination.Paginate(customers, Pagination.CurrentPage);

        // Calculate the total number of pages
        Pagination.TotalPages = Pagination.GetTotalPages(customers);
    }

    public async Task<IActionResult> OnPostSearch(SearchModel data, int currentPage = 1)
    {
        // Retrieve the customer list from the database here
        var customers = await _databaseHandler.GetDataFromTable<CustomersTable>();
        var filteredCustomerList = SearchData.FilterList(customers, data);
        GetPagination(filteredCustomerList, currentPage);
        // Speichern Sie die Filterinformationen in der Sitzung
        HttpContext.Session.SetString("filters", JsonConvert.SerializeObject(data));
        SearchData.CurrentFilters = data;
        return Page();
    }

    public async Task OnPostCancelCustomer()
    {
        IsEditCustomerFormVisible = false;
        IsNewCustomerFormVisible = false;
        await OnGetAsync(Culture);
    }
    public async Task OnPostAddCustomerAsync(string firstName, string lastName)
    {
        if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
        {
            var newCustomer = new CustomersTable { FirstName = firstName, LastName = lastName };
            // Kunde hinzufügen
            await _databaseHandler.AddDataToTable(newCustomer);
        }
        else
        {
            ErrorMessage = "Kein Kunde hinzugefügt! Es fehlten Angaben, bei mindestens einem Feld wurde nichts eingetragen!";
        }
        // neu laden der Kundendaten
        await OnGetAsync(Culture);
    }
    public async Task OnPostToggleEditCustomerFormAsync(int customerId)
    {
        IsEditCustomerFormVisible = !IsEditCustomerFormVisible;
        CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>(x => x.ID == customerId);
    }
    public async Task OnPostEditCustomerAsync(string firstName, string lastName, int customerId)
    {
        if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
        {
            var EditedCustomer = new CustomersTable
            {
                ID = customerId,
                FirstName = firstName,
                LastName = lastName,
            };
            // Kunde hinzufügen
            await _databaseHandler.UpdateDataInTable(EditedCustomer);
        }
        else
        {
            ErrorMessage = "Kunde nicht geändert! Es fehlten Angaben, bei mindestens einem Feld wurde nichts eingetragen!";
        }
        // neu laden der Kundendaten
        await OnGetAsync(Culture);
    }
    public async Task OnPostDeleteCustomerAsync(int customerId)
    {
        await _databaseHandler.DeleteDataFromTable<CustomersTable>(customerId);
        await OnGetAsync(Culture);
    }

    public void OnPostToggleNewCustomerForm()
    {
        IsNewCustomerFormVisible = !IsNewCustomerFormVisible;
    }
}

