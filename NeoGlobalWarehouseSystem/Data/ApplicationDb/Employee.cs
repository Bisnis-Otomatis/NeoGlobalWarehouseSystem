using NeoGlobalWarehouseSystem.Data.ApplicationDb.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace NeoGlobalWarehouseSystem.Data.ApplicationDb
{
    public class Employee : IEntityIdentificationData
    {
        [Key]
        public int Id { get; set; }

        public EmployeeType Type { get; set; } 
        public string Name { get; set; } = string.Empty;
        public string IdCardNumber { get; set; } = string.Empty;
        public List<Transaction> Transactions { get; set; } = new();
    }
}
