namespace BrodWorschdApp
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
        public async Task DeleteDataFromTable<T>(int id) where T : class
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
    public class DatabaseContext : DbContext
    {
        public DbSet<CustomersTable> Customers { get; set; }
        public DbSet<CustomerOrdersTable> Orders { get; set; }
        public DbSet<ProductsTable> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString;

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                connectionString = "Data Source=C:\\Users\\ofran\\OneDrive\\Dokumente\\Visual Studio 2022\\SQLite\\brodworschdapp_db.db";
                //connectionString = "Data Source=/home/admin/brodworschdapp/sqlite/brodworschdapp_db.db";
            }
            else
            {
                connectionString = "Data Source=/home/admin/brodworschdapp/sqlite/brodworschdapp_db.db";
            }

            optionsBuilder.UseSqlite(connectionString);
        }
    }
    public class CustomersTable
    {
        public int ID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        // Sie können hier weitere Eigenschaften hinzufügen, die ein Kunde haben könnte
    }

    public class CustomerOrdersTable
    {
        public int ID { get; set; }
        public string CustomerID { get; set; }
        public string ProductID { get; set; }
        public virtual CustomersTable Customer { get; set; }
        public virtual ProductsTable Product { get; set; }
        // Sie können hier weitere Eigenschaften hinzufügen, die eine Bestellung haben könnte
    }
    public class ProductsTable
    {
        public int ID { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public float? Price { get; set; }
        public int? Inventory { get; set; }
        public float? Size { get; set; }
        public string? Unit { get; set; }
        // Sie können hier weitere Eigenschaften hinzufügen, die eine Bestellung haben könnte
    }
}
