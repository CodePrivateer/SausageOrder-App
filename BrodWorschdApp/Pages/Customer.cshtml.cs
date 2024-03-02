using BrodWorschdApp;
using BrodWorschdApp.Pages;

public class CustomerModel : BasePageModel
{
    public bool IsNewCustomerFormVisible { get; set; }
    public bool IsEditCustomerFormVisible { get; set; }

    public CustomerModel(DatabaseHandler databaseHandler, ILogger<CustomerModel> logger) : base(databaseHandler, logger)
    {
    }

    public async Task OnGetAsync()
    {
        // Hier können Sie die Kundenliste aus der Datenbank abrufen
        CustomerList = await _databaseHandler.GetDataFromTable<CustomersTable>();
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
        await OnGetAsync();
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
        await OnGetAsync();
    }
    public async Task OnPostDeleteCustomerAsync(int customerId)
    {
        await _databaseHandler.DeleteDataFromTable<CustomersTable>(customerId);
        await OnGetAsync();
    }

    public void OnPostToggleNewCustomerForm()
    {
        IsNewCustomerFormVisible = !IsNewCustomerFormVisible;
    }
    public async Task OnPostCancelEditCustomerAsync()
    {
        await OnGetAsync();
    }
}

