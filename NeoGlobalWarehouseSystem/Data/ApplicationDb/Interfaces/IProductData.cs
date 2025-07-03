namespace NeoGlobalWarehouseSystem.Data.ApplicationDb.Interfaces
{
    public interface IProductData
    {
        string Barcode { get; set; }
        string Name { get; set; }
        int Quantity { get; set; }
        long Price { get; set; }
    }
}
