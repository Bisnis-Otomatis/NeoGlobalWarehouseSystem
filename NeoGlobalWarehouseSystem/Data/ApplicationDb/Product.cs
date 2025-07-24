using NeoGlobalWarehouseSystem.Data.ApplicationDb.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

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

        public bool CanBeBoughtByStudents { get; set; }
        public List<EmployeeType> CanBeBoughtByEmployeeTypes { get; set; } = new();

        public List<TransactionProduct> TransactionProducts { get; set; } = new();
        public List<ProductInputLog> ProductInputLog { get; set; } = new();
    }
}
