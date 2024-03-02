using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrodWorschdApp.Pages
{
    public class ProductsModel : BasePageModel
    {
        public bool IsNewProductFormVisible { get; set; }
        public bool IsEditProductFormVisible { get; set; }

        public ProductsModel(DatabaseHandler databaseHandler, ILogger<ProductsModel> logger) : base(databaseHandler, logger)
        {
        }

        public async Task OnGetAsync()
        {
            ProductList = await _databaseHandler.GetDataFromTable<ProductsTable>();
        }
        public void OnPostToggleNewProductForm()
        {
            IsNewProductFormVisible = !IsNewProductFormVisible;
        }
        public async Task OnPostToggleEditProductFormAsync(int productId)
        {
            IsEditProductFormVisible = !IsEditProductFormVisible;
            ProductList = await _databaseHandler.GetDataFromTable<ProductsTable>(x => x.ID == productId);
        }
        public async Task OnPostAddProductAsync(string productName, float productPrice, int productInventory, float productSize, string productSizeUnit)
        {
            if (!string.IsNullOrEmpty(productName))
            {
                var newProduct = new ProductsTable
                {
                    ProductName = productName,
                    Price = productPrice,
                    Inventory = productInventory,
                    Size = productSize,
                    Unit = productSizeUnit
                };
                // Kunde hinzufügen
                await _databaseHandler.AddDataToTable(newProduct);
            }
            else
            {
                ErrorMessage = "Kein Produkt hinzugefügt! Es fehlten Angaben, bei mindestens einem Feld wurde nichts eingetragen!";
            }
            // neu laden der Kundendaten
            await OnGetAsync();
        }
        public async Task OnPostDeleteProductAsync(int productId)
        {
            await _databaseHandler.DeleteDataFromTable<ProductsTable>(productId);
            await OnGetAsync();
        }
        public async Task OnPostEditProductAsync(string productName, float productPrice, int productInventory, float productSize, string productSizeUnit, int productId)
        {
            if (!string.IsNullOrEmpty(productName))
            {
                var EditedProduct = new ProductsTable
                {
                    ID = productId,
                    ProductName = productName,
                    Price = productPrice,
                    Inventory = productInventory,
                    Size = productSize,
                    Unit = productSizeUnit
                };
                // Kunde hinzufügen
                await _databaseHandler.UpdateDataInTable(EditedProduct);
            }
            else
            {
                ErrorMessage = "Kein Produkt hinzugefügt! Es fehlten Angaben, bei mindestens einem Feld wurde nichts eingetragen!";
            }
            // neu laden der Kundendaten
            await OnGetAsync();
        }
        public async Task OnPostCancelEditProductAsync()
        {
            await OnGetAsync();
        }
    }
}
