using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace ProductInventoryManager
{
    public class MainForm : Form
    {
        // The form only talks to the inventory through this object
        private readonly InventoryManager inventory = new InventoryManager();

        private DataGridView productGrid;
        private TextBox idBox;
        private TextBox nameBox;
        private TextBox priceBox;
        private TextBox searchBox;
        private Label statusLabel;

        public MainForm()
        {
            BuildInterface();

            inventory.LoadStarterProducts();
            ShowProducts(inventory.GetAll());
            SetStatus("Inventory loaded: " + inventory.Count + " products.", false);
        }

        // ---------- Building the window ----------

        private void BuildInterface()
        {
            Text = "Product Inventory Manager";
            ClientSize = new Size(820, 580);
            MinimumSize = new Size(760, 480);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);

            Label title = new Label
            {
                Text = "Product Inventory Manager",
                Font = new Font("Segoe UI Semibold", 16F),
                Dock = DockStyle.Top,
                Height = 46,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };

            // Search row
            FlowLayoutPanel searchPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 48,
                Padding = new Padding(10, 8, 10, 0)
            };

            searchBox = new TextBox { Width = 220, Margin = new Padding(3, 4, 10, 3) };
            searchBox.KeyDown += SearchBox_KeyDown;

            Button searchButton = MakeButton("Search");
            searchButton.Click += (s, e) => RunSearch();

            Button showAllButton = MakeButton("Show All");
            showAllButton.Click += (s, e) => ShowEverything();

            searchPanel.Controls.Add(MakeLabel("Search (name or ID)"));
            searchPanel.Controls.Add(searchBox);
            searchPanel.Controls.Add(searchButton);
            searchPanel.Controls.Add(showAllButton);

            // Product details group: input boxes on the first row, buttons on the second
            GroupBox detailsGroup = new GroupBox
            {
                Text = "Product details",
                Dock = DockStyle.Top,
                Height = 120,
                Padding = new Padding(10, 6, 10, 6)
            };

            FlowLayoutPanel detailsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = true
            };

            idBox = new TextBox { Width = 80, Margin = new Padding(3, 4, 10, 3) };
            nameBox = new TextBox { Width = 220, Margin = new Padding(3, 4, 10, 3) };
            priceBox = new TextBox { Width = 100, Margin = new Padding(3, 4, 10, 3) };

            Button addButton = MakeButton("Add");
            addButton.Click += (s, e) => AddProduct();

            Button updateButton = MakeButton("Update");
            updateButton.Click += (s, e) => UpdateProduct();

            Button deleteButton = MakeButton("Delete");
            deleteButton.Click += (s, e) => DeleteProduct();

            Button clearButton = MakeButton("Clear Fields");
            clearButton.Click += (s, e) => ClearFields();

            detailsFlow.Controls.Add(MakeLabel("Product ID"));
            detailsFlow.Controls.Add(idBox);
            detailsFlow.Controls.Add(MakeLabel("Name"));
            detailsFlow.Controls.Add(nameBox);
            detailsFlow.Controls.Add(MakeLabel("Price (R)"));
            detailsFlow.Controls.Add(priceBox);
            detailsFlow.SetFlowBreak(priceBox, true);
            detailsFlow.Controls.Add(addButton);
            detailsFlow.Controls.Add(updateButton);
            detailsFlow.Controls.Add(deleteButton);
            detailsFlow.Controls.Add(clearButton);

            detailsGroup.Controls.Add(detailsFlow);

            // The grid that displays the dictionary contents
            productGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            productGrid.Columns.Add("colId", "Product ID");
            productGrid.Columns.Add("colName", "Name");
            productGrid.Columns.Add("colPrice", "Price");
            productGrid.Columns[0].Width = 110;
            productGrid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            productGrid.Columns[2].Width = 140;
            productGrid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            productGrid.CellClick += ProductGrid_CellClick;

            statusLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };

            // Fill goes in first so the docked edges are carved out around it
            Controls.Add(productGrid);
            Controls.Add(statusLabel);
            Controls.Add(detailsGroup);
            Controls.Add(searchPanel);
            Controls.Add(title);
        }

        private static Label MakeLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Margin = new Padding(6, 8, 2, 0)
            };
        }

        private static Button MakeButton(string text)
        {
            return new Button
            {
                Text = text,
                Width = 110,
                Height = 32,
                Margin = new Padding(3, 6, 6, 3)
            };
        }

        // ---------- Button actions ----------

        private void AddProduct()
        {
            if (!TryReadId(out int id) || !TryReadPrice(out decimal price))
            {
                return;
            }

            if (inventory.TryAdd(id, nameBox.Text, price, out string message))
            {
                ShowEverything();
                ClearFields();
                SetStatus(message, false);
            }
            else
            {
                SetStatus(message, true);
            }
        }

        private void UpdateProduct()
        {
            if (!TryReadId(out int id) || !TryReadPrice(out decimal price))
            {
                return;
            }

            if (inventory.TryUpdate(id, nameBox.Text, price, out string message))
            {
                ShowEverything();
                SetStatus(message, false);
            }
            else
            {
                SetStatus(message, true);
            }
        }

        private void DeleteProduct()
        {
            if (!TryReadId(out int id))
            {
                return;
            }

            Product product = inventory.GetProduct(id);
            if (product == null)
            {
                SetStatus("No product with ID " + id + " was found to delete.", true);
                return;
            }

            // Deleting can't be undone, so double check first
            DialogResult answer = MessageBox.Show(
                "Delete \"" + product.Name + "\" (ID " + id + ")?",
                "Confirm delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            if (inventory.TryDelete(id, out string message))
            {
                ShowEverything();
                ClearFields();
                SetStatus(message, false);
            }
            else
            {
                SetStatus(message, true);
            }
        }

        private void RunSearch()
        {
            string term = searchBox.Text.Trim();
            List<Product> results = inventory.Search(term);
            ShowProducts(results);

            if (term.Length == 0)
            {
                SetStatus("Showing all " + results.Count + " products.", false);
            }
            else if (results.Count == 0)
            {
                SetStatus("No products matched \"" + term + "\".", true);
            }
            else
            {
                SetStatus(results.Count + " product(s) matched \"" + term + "\".", false);
            }
        }

        // Resets the search box and shows the full inventory again
        private void ShowEverything()
        {
            searchBox.Clear();
            ShowProducts(inventory.GetAll());
        }

        private void ClearFields()
        {
            idBox.Clear();
            nameBox.Clear();
            priceBox.Clear();
            idBox.Focus();
        }

        // ---------- Events ----------

        // Clicking a row copies it into the boxes, ready to edit or delete
        private void ProductGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            int id = Convert.ToInt32(productGrid.Rows[e.RowIndex].Cells[0].Value);
            Product product = inventory.GetProduct(id);

            if (product != null)
            {
                idBox.Text = product.ProductId.ToString();
                nameBox.Text = product.Name;
                priceBox.Text = product.Price.ToString("0.##", CultureInfo.CurrentCulture);
            }
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                RunSearch();
                e.SuppressKeyPress = true;
            }
        }

        // ---------- Display helpers ----------

        private void ShowProducts(List<Product> list)
        {
            productGrid.Rows.Clear();

            foreach (Product product in list)
            {
                productGrid.Rows.Add(product.ProductId, product.Name, FormatPrice(product.Price));
            }

            productGrid.ClearSelection();
        }

        private static string FormatPrice(decimal price)
        {
            return "R" + price.ToString("N2", CultureInfo.CurrentCulture);
        }

        private void SetStatus(string message, bool isError)
        {
            statusLabel.ForeColor = isError ? Color.Firebrick : Color.DarkGreen;
            statusLabel.Text = message;
        }

        // ---------- Reading the input boxes ----------

        private bool TryReadId(out int id)
        {
            if (int.TryParse(idBox.Text.Trim(), out id))
            {
                return true;
            }

            SetStatus("Please enter the Product ID as a whole number.", true);
            idBox.Focus();
            return false;
        }

        private bool TryReadPrice(out decimal price)
        {
            // Allow a leading R, and accept either "." or "," as the decimal mark
            string text = priceBox.Text.Trim().TrimStart('R', 'r').Trim();

            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out price) ||
                decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out price))
            {
                return true;
            }

            SetStatus("Please enter the price as a number, for example 249.99.", true);
            priceBox.Focus();
            return false;
        }
    }
}
