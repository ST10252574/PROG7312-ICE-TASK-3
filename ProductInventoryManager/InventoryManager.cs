namespace ProductInventoryManager
{
    // All the inventory logic lives here and knows nothing about the GUI,
    // so it can be tested (or reused in a console app) on its own.
    public class InventoryManager
    {
        // ProductID is the key, the Product is the value
        private readonly Dictionary<int, Product> products = new Dictionary<int, Product>();

        public int Count
        {
            get { return products.Count; }
        }

        // Puts a starting set of ten products into the dictionary
        public void LoadStarterProducts()
        {
            products.Clear();

            products.Add(101, new Product(101, "Laptop", 5000m));
            products.Add(102, new Product(102, "JBL Speaker", 8000m));
            products.Add(103, new Product(103, "Tablet", 2300m));
            products.Add(104, new Product(104, "Wireless Mouse", 350m));
            products.Add(105, new Product(105, "Mechanical Keyboard", 1200m));
            products.Add(106, new Product(106, "27-inch Monitor", 3400m));
            products.Add(107, new Product(107, "USB-C Hub", 450m));
            products.Add(108, new Product(108, "Webcam", 900m));
            products.Add(109, new Product(109, "Noise-Cancelling Headphones", 2800m));
            products.Add(110, new Product(110, "External Hard Drive", 1500m));
        }

        // ---------- Add ----------

        public bool TryAdd(int id, string name, decimal price, out string message)
        {
            if (!IsValid(id, name, price, out message))
            {
                return false;
            }

            if (products.ContainsKey(id))
            {
                message = "A product with ID " + id + " already exists. Use Update to change it.";
                return false;
            }

            products.Add(id, new Product(id, name.Trim(), price));
            message = "Added \"" + name.Trim() + "\" with ID " + id + ".";
            return true;
        }

        // ---------- Update ----------

        public bool TryUpdate(int id, string name, decimal price, out string message)
        {
            if (!IsValid(id, name, price, out message))
            {
                return false;
            }

            if (!products.TryGetValue(id, out Product existing))
            {
                message = "No product with ID " + id + " was found to update.";
                return false;
            }

            existing.Name = name.Trim();
            existing.Price = price;
            message = "Updated product " + id + ".";
            return true;
        }

        // ---------- Delete ----------

        public bool TryDelete(int id, out string message)
        {
            if (!products.TryGetValue(id, out Product existing))
            {
                message = "No product with ID " + id + " was found to delete.";
                return false;
            }

            products.Remove(id);
            message = "Deleted \"" + existing.Name + "\" (ID " + id + ").";
            return true;
        }

        // ---------- Search ----------

        // Looks a product up by its key. Returns null when it isn't there.
        public Product GetProduct(int id)
        {
            products.TryGetValue(id, out Product found);
            return found;
        }

        // Matches the text against product names (ignoring upper/lower case).
        // If the text is a whole number, a product with that exact ID counts too.
        public List<Product> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return GetAll();
            }

            term = term.Trim();
            List<Product> matches = new List<Product>();

            foreach (Product product in products.Values)
            {
                bool nameMatches = product.Name.Contains(term, StringComparison.OrdinalIgnoreCase);
                bool idMatches = int.TryParse(term, out int searchedId) && product.ProductId == searchedId;

                if (nameMatches || idMatches)
                {
                    matches.Add(product);
                }
            }

            matches.Sort((a, b) => a.ProductId.CompareTo(b.ProductId));
            return matches;
        }

        // ---------- Display ----------

        // Every product, lowest ID first
        public List<Product> GetAll()
        {
            List<Product> all = new List<Product>(products.Values);
            all.Sort((a, b) => a.ProductId.CompareTo(b.ProductId));
            return all;
        }

        // ---------- Checks ----------

        private static bool IsValid(int id, string name, decimal price, out string message)
        {
            if (id <= 0)
            {
                message = "The Product ID must be a positive whole number.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                message = "Please enter a product name.";
                return false;
            }

            if (price < 0)
            {
                message = "The price can't be negative.";
                return false;
            }

            message = string.Empty;
            return true;
        }
    }
}
