using System.ComponentModel.DataAnnotations;

namespace NeoGlobalWarehouseSystem.Data.ApplicationDb
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        public User ProcessedBy { get; set; } = null!;
        public int ProcessedById { get; set; }

        public string CustomerName { get; set; } = "";

        public Teacher? Teacher { get; set; }
        public int? TeacherId { get; set; }

        public DateTime TimeStamp { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
        public List<TransactionProduct> TransactionProducts { get; set; } = new();
    }
}
