﻿namespace BrodWorschdApp
{
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class DatabaseHandler
    {
        private readonly DatabaseContext _context;

        public DatabaseHandler(DatabaseContext context)
        {
            _context = context;
        }
        // Liste Abfragen ohne Suchkriterium  oder Methode ohne Prädikat
        public async Task<List<T>> GetDataFromTable<T>() where T : class
        {
            return await _context.Set<T>().ToListAsync();
        }

        // Liste Abfragen mit Suchkriterium oder Methode mit Prädikat
        public async Task<List<T>> GetDataFromTable<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await _context.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task AddDataToTable<T>(T entity) where T : class
        {
            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();
        }
        // Liste ändern mit Suchkriterium oder Methode mit Prädikat
        public async Task UpdateDataInTable<T>(Expression<Func<T, bool>> predicate, Action<T> updateAction) where T : class
        {
            var entities = await _context.Set<T>().Where(predicate).ToListAsync();
            foreach (var entity in entities)
            {
                updateAction(entity);
            }
            await _context.SaveChangesAsync();
        }
        // Eintrag ändern mit Suchkriterium ohne Prädikat
        public async Task UpdateDataInTable<T>(T entity) where T : class
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateOrderItemStatus(string orderNumber, int productId, string status = "booked")
        {
            // Find the order
            var order = await _context.Set<CustomerOrdersTable>()
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.ProductID == productId);

            if (order != null)
            {
                // Update the status
                order.Booked = status;

                // Save the changes
                await UpdateDataInTable(order);
            }
        }
        public async Task UpdateOrderItemPayStatus(string orderNumber, int productId, string status = "paid")
        {
            // Find the order
            var order = await _context.Set<CustomerOrdersTable>()
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.ProductID == productId);

            if (order != null)
            {
                // Update the status
                order.Paid = status;

                // Save the changes
                await UpdateDataInTable(order);
            }
        }


        // Liste löschen mit Suchkriterium oder Methode mit Prädikat
        public async Task DeleteDataFromTable<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var entities = _context.Set<T>().Where(predicate);
            _context.Set<T>().RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        // Löschen mit einem Suchkriterium  oder Methode ohne Prädikat
        public async Task DeleteDataFromTable<T>(int id) where T : class
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        // Produkt mit ProduktId finden
        public async Task<T> FindProductById<T>(int id) where T : class, new()
        {
            var product = await _context.Set<T>().FindAsync(id);
            return product ?? new T();
        }

        // Customer Bestellungen auflisten
        public async Task<List<CustomerOrdersTable>> GetCustomerOrdersWithDetails(Expression<Func<CustomerOrdersTable, bool>> predicate)
        {
            return await _context.CustomerOrders
                .Include(co => co.Customer)
                .Where(predicate)
                .ToListAsync();
        }
        // Bestellmenge je Produkt in eine Dictionary auflisten
        public async Task<Dictionary<int, int>> GetAllOrderedQuantitiesPerProduct()
        {
            return await _context.CustomerOrders
                .GroupBy(co => co.ProductID)
                .Select(g => new { ProductID = g.Key, TotalQuantity = g.Sum(co => co.Quantity) })
                .ToDictionaryAsync(t => t.ProductID, t => t.TotalQuantity);
        }
        // Nur die nicht gebuchten Bestellmenge je Produkt in eine Dictionary auflisten
        public async Task<Dictionary<int, int>> GetOrderedQuantitiesPerProduct()
        {
            return await _context.CustomerOrders
                .Where(co => string.IsNullOrEmpty(co.Booked))
                .GroupBy(co => co.ProductID)
                .Select(g => new { ProductID = g.Key, TotalQuantity = g.Sum(co => co.Quantity) })
                .ToDictionaryAsync(t => t.ProductID, t => t.TotalQuantity);
        }
        // Nur die gebuchten Bestellmenge je Produkt in eine Dictionary auflisten
        public async Task<Dictionary<int, int>> GetBookedQuantitiesPerProduct()
        {
            return await _context.CustomerOrders
                .Where(co => co.Booked == "booked")
                .GroupBy(co => co.ProductID)
                .Select(g => new { ProductID = g.Key, TotalQuantity = g.Sum(co => co.Quantity) })
                .ToDictionaryAsync(t => t.ProductID, t => t.TotalQuantity);
        }
    }
    public class DatabaseContext : DbContext
    {
        public DbSet<CustomersTable> Customers { get; set; }
        public DbSet<CustomerOrdersTable> CustomerOrders { get; set; }
        public DbSet<ProductsTable> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString;

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                // Verwenden Sie den relativen Pfad zur SQLite-Datei in Ihrem Projektordner
                // connectionString = $"Data Source={Path.Combine(Directory.GetCurrentDirectory(), "BrodWorschdApp", "SqliteDB", "brodworschdapp_db.db")}";
                connectionString = "Data Source=S:\\brodworschdapp\\SqliteDB\\brodworschdapp_db.db";
            }
            else
            {
                //connectionString = "Data Source=/home/admin/brodworschdapp/SqliteDB/brodworschdapp_db.db";
                connectionString = "Data Source=/home/aspnetcore/brodworschdapp/SqliteDB/brodworschdapp_db.db";
            }

            optionsBuilder.UseSqlite(connectionString);
        }
    }
    public class CustomersTable
    {
        public int ID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public virtual ICollection<CustomerOrdersTable>? Orders { get; set; }  // Navigationseigenschaft für Bestellungen hinzufügen
    }

    public class CustomerOrdersTable
    {
        public int ID { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int CustomerID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public string Date { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? PickUpName { get; set; }
        public string? Booked { get; set; }
        public string? Paid { get; set; }
        public virtual CustomersTable? Customer { get; set; }
        // Sie können hier weitere Eigenschaften hinzufügen, die eine Bestellung haben könnte
    }
    public class ProductsTable
    {
        public int ID { get; set; }
        public string? ProductName { get; set; }
        public float? Price { get; set; }
        public int? Inventory { get; set; }
        public float? Size { get; set; }
        public string? Unit { get; set; }
        // Sie können hier weitere Eigenschaften hinzufügen, die eine Bestellung haben könnte
    }
    public class GroupedOrder
    {
        public string OrderNumber { get; set; }
        public List<CustomerOrdersTable> Items { get; set; }
        public float TotalPrice { get; set; }
        public float TotalDelivered { get; set; }
        public float TotalOpen { get; set; }
        public float TotalPaid { get; set; }
        public List<ProductsTable> Products { get; set; }  // Liste der Produkte hinzufügen
        // public string? PickUpName { get; set; }

        public GroupedOrder()
        {
            OrderNumber = string.Empty;
            Items = new List<CustomerOrdersTable>();
            Products = new List<ProductsTable>();  // Initialisieren Sie die Liste der Produkte
            TotalPrice = 0;
        }
    }
}
