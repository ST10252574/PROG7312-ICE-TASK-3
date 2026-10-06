namespace ProductInventoryManager
{
    // A single item in the shop. The ProductId is also the dictionary key,
    // but keeping it on the object too makes it easier to show in the grid.
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public Product(int productId, string name, decimal price)
        {
            ProductId = productId;
            Name = name;
            Price = price;
        }
    }
}
