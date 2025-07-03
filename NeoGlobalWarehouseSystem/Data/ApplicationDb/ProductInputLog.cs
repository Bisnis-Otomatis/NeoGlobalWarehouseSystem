using System.ComponentModel.DataAnnotations;

namespace NeoGlobalWarehouseSystem.Data.ApplicationDb
{
    public class ProductInputLog
    {
        [Key]
        public int Id { get; set; }

        public Product Product { get; set; } = new();
        public int ProductId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
        public int QuantityInput { get; set; }
    }
}
