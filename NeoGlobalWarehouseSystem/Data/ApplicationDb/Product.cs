using NeoGlobalWarehouseSystem.Data.ApplicationDb.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace NeoGlobalWarehouseSystem.Data.ApplicationDb
{
    public class Product : IProductData
    {
        [Key]
        public int Id { get; set; }

        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public long Price { get; set; }

        public bool CanBeBoughtByTeachers { get; set; }
        public bool CanBeBoughtByStudents { get; set; }

        public List<TransactionProduct> TransactionProducts { get; set; } = new();
        public List<ProductInputLog> ProductInputLog { get; set; } = new();
    }
}
