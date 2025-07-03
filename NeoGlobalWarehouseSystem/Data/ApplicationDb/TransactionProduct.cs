using NeoGlobalWarehouseSystem.Data.ApplicationDb.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace NeoGlobalWarehouseSystem.Data.ApplicationDb
{
    public class TransactionProduct : IProductData
    {
        [Key]
        public int Id { get; set; }

        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public long Price { get; set; }

        public Product Product { get; set; } = new();
        public int ProductId {  get; set; }

        public Transaction Transaction { get; set; } = new();
        public int TransactionId { get; set; }
    }
}
