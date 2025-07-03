using Microsoft.AspNetCore.Identity;
using NeoGlobalWarehouseSystem.Data.ApplicationDb.Interfaces;

namespace NeoGlobalWarehouseSystem.Data.ApplicationDb
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class User : IdentityUser, IEntityIdentificationData
    {
        public string Name { get; set; } = string.Empty;
        public string IdCardNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Cashier;
        public List<Transaction> Transactions { get; set; } = new();
    }
}
