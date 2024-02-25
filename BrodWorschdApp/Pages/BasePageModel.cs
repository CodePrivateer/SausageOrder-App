﻿using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrodWorschdApp.Pages
{
    public class BasePageModel : PageModel
    {
        protected readonly DatabaseHandler _databaseHandler;
        protected readonly ILogger _logger;
        public List<CustomersTable> CustomerList { get; set; }
        public List<CustomerOrdersTable> CustomerOrderList { get; set; }
        public string ErrorMessage { get; set; }

        public BasePageModel(DatabaseHandler databaseHandler, ILogger<BasePageModel> logger)
        {
            _databaseHandler = databaseHandler;
            _logger = logger;
            CustomerList = new List<CustomersTable>();
            CustomerOrderList = new List<CustomerOrdersTable>();
            ErrorMessage = string.Empty;
        }
    }
}