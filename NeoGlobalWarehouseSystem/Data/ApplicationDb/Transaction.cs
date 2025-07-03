using System.ComponentModel.DataAnnotations;

namespace NeoGlobalWarehouseSystem.Data.ApplicationDb
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        public User ProcessedBy { get; set; } = new();
        public Teacher? Teacher { get; set; }
        public DateTime TimeStamp { get; set; }
        public long TotalPrice { get; set; }
        public List<TransactionProduct> TransactionProducts { get; set; } = new();
    }
}
