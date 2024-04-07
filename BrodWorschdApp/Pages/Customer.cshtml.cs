using BrodWorschdApp;
using BrodWorschdApp.Pages;
using Microsoft.AspNetCore.Mvc;

public class CustomerModel : BasePageModel
{
    public bool IsNewCustomerFormVisible { get; set; }
    public bool IsEditCustomerFormVisible { get; set; }

    public PaginationViewModel<CustomersTable> Pagination { get; set; } = new PaginationViewModel<CustomersTable>();

    public CustomerModel(DatabaseHandler databaseHandler, ILogger<CustomerModel> logger, LanguageService languageService) :
            base(databaseHandler, logger, languageService)
    {
    }

    public async Task<IActionResult> OnGetAsync(string culture, int currentPage = 1)
    {
        // Retrieve the customer list from the database here
        var customers = await _databaseHandler.GetDataFromTable<CustomersTable>();

        // Set the current page
        Pagination.CurrentPage = currentPage;

        // Call the Paginate method and assign the result to CustomerList
        CustomerList = Pagination.Paginate(customers, Pagination.CurrentPage);

        // Calculate the total number of pages
        Pagination.TotalPages = Pagination.GetTotalPages(customers);

        // Setup of the Language
        if (!string.IsNullOrEmpty(culture))
        {
            _languageservice.SetCulture(culture);
            return RedirectToPage();
        }
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

