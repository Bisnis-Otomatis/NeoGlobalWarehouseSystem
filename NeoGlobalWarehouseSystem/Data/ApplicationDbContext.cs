using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NeoGlobalWarehouseSystem.Data.ApplicationDb;
using System.Reflection.Metadata;

namespace NeoGlobalWarehouseSystem.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User, IdentityRole<int>, int>(options)
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductInputLog> ProductInputLogs { get; set; }
        public DbSet<Employee> Teachers { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionProduct> TransactionProducts { get; set; }
    }
}
