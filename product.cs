using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace final_vp_project
{
    public partial class product : Form
    {
        private const string NamePlaceholder = "Enter product name";
        private const string PricePlaceholder = "0.00";
        private const string CategoryPlaceholder = "Select category";
        private const string SupplierPlaceholder = "Select supplier";

        private int selectedProductId = -1;

        private string inputName = "";
        private string inputCategory = "";
        private string inputSupplier = "";
        private int inputQuantity = 0;
        private decimal inputPurchasePrice = 0;
        private decimal inputSellingPrice = 0;

        public product()
        {
            InitializeComponent();
            WireUpEvents();
        }

        private void WireUpEvents()
        {
            txtProductName.Enter += txtProductName_Enter;
            txtProductName.Leave += txtProductName_Leave;

            txtPurchasePrice.Enter += txtPurchasePrice_Enter;
            txtPurchasePrice.Leave += txtPurchasePrice_Leave;

            txtSellingPrice.Enter += txtSellingPrice_Enter;
            txtSellingPrice.Leave += txtSellingPrice_Leave;

            btnBrowseImage.Click += btnBrowseImage_Click;
            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnSearch.Click += btnSearch_Click;
            btnClear.Click += btnClear_Click;

            dgvProducts.CellClick += dgvProducts_CellClick;
        }

        private void product_Load(object sender, EventArgs e)
        {
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            dgvProducts.Rows.Clear();

            MySqlConnection conn = DatabaseHelper.GetConnection();

            try
            {
                conn.Open();

                string query = "SELECT id, name, category, supplier, quantity, purchase_price, selling_price FROM products ORDER BY id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32("id");
                    string name = reader.GetString("name");
                    string category = reader.GetString("category");
                    string supplier = reader.GetString("supplier");
                    int quantity = reader.GetInt32("quantity");
                    decimal purchasePrice = reader.GetDecimal("purchase_price");
                    decimal sellingPrice = reader.GetDecimal("selling_price");

                    dgvProducts.Rows.Add(id, name, category, supplier, quantity,
                        purchasePrice.ToString("F2"), sellingPrice.ToString("F2"));
                }

                reader.Close();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }

        private void txtProductName_Enter(object sender, EventArgs e)
        {
            if (txtProductName.Text == NamePlaceholder)
            {
                txtProductName.Text = "";
                txtProductName.ForeColor = Color.Black;
            }
        }

        private void txtProductName_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                txtProductName.Text = NamePlaceholder;
                txtProductName.ForeColor = Color.Gray;
            }
        }

        private void txtPurchasePrice_Enter(object sender, EventArgs e)
        {
            if (txtPurchasePrice.Text == PricePlaceholder && txtPurchasePrice.ForeColor == Color.Gray)
            {
                txtPurchasePrice.Text = "";
                txtPurchasePrice.ForeColor = Color.Black;
            }
        }

        private void txtPurchasePrice_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPurchasePrice.Text))
            {
                txtPurchasePrice.Text = PricePlaceholder;
                txtPurchasePrice.ForeColor = Color.Gray;
            }
        }

        private void txtSellingPrice_Enter(object sender, EventArgs e)
        {
            if (txtSellingPrice.Text == PricePlaceholder && txtSellingPrice.ForeColor == Color.Gray)
            {
                txtSellingPrice.Text = "";
                txtSellingPrice.ForeColor = Color.Black;
            }
        }

        private void txtSellingPrice_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSellingPrice.Text))
            {
                txtSellingPrice.Text = PricePlaceholder;
                txtSellingPrice.ForeColor = Color.Gray;
            }
        }

        private bool ValidateInputs()
        {
            inputName = (txtProductName.Text == NamePlaceholder) ? "" : txtProductName.Text.Trim();
            inputCategory = (cmbCategory.Text == CategoryPlaceholder) ? "" : cmbCategory.Text.Trim();
            inputSupplier = (cmbSupplier.Text == SupplierPlaceholder) ? "" : cmbSupplier.Text.Trim();
            inputQuantity = (int)nudQuantity.Value;
            inputPurchasePrice = 0;
            inputSellingPrice = 0;

            if (string.IsNullOrEmpty(inputName))
            {
                MessageBox.Show("Please enter a product name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(inputCategory))
            {
                MessageBox.Show("Please select a category.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(inputSupplier))
            {
                MessageBox.Show("Please select a supplier.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string purchaseText = (txtPurchasePrice.Text == PricePlaceholder && txtPurchasePrice.ForeColor == Color.Gray)
                ? "" : txtPurchasePrice.Text.Trim();
            string sellingText = (txtSellingPrice.Text == PricePlaceholder && txtSellingPrice.ForeColor == Color.Gray)
                ? "" : txtSellingPrice.Text.Trim();

            bool purchaseOk = decimal.TryParse(purchaseText, out inputPurchasePrice);
            if (!purchaseOk || inputPurchasePrice < 0)
            {
                MessageBox.Show("Please enter a valid purchase price.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            bool sellingOk = decimal.TryParse(sellingText, out inputSellingPrice);
            if (!sellingOk || inputSellingPrice < 0)
            {
                MessageBox.Show("Please enter a valid selling price.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            MySqlConnection conn = DatabaseHelper.GetConnection();

            try
            {
                conn.Open();

                string query = "INSERT INTO products (name, category, supplier, quantity, purchase_price, selling_price) " +
                    "VALUES (@name, @category, @supplier, @quantity, @purchasePrice, @sellingPrice)";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", inputName);
                cmd.Parameters.AddWithValue("@category", inputCategory);
                cmd.Parameters.AddWithValue("@supplier", inputSupplier);
                cmd.Parameters.AddWithValue("@quantity", inputQuantity);
                cmd.Parameters.AddWithValue("@purchasePrice", inputPurchasePrice);
                cmd.Parameters.AddWithValue("@sellingPrice", inputSellingPrice);

                cmd.ExecuteNonQuery();

                RefreshGrid();
                ClearFields();

                MessageBox.Show("Product added successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedProductId == -1)
            {
                MessageBox.Show("Please select a product from the list first.", "No Product Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateInputs())
            {
                return;
            }

            MySqlConnection conn = DatabaseHelper.GetConnection();

            try
            {
                conn.Open();

                string query = "UPDATE products SET name = @name, category = @category, supplier = @supplier, " +
                    "quantity = @quantity, purchase_price = @purchasePrice, selling_price = @sellingPrice " +
                    "WHERE id = @id";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", inputName);
                cmd.Parameters.AddWithValue("@category", inputCategory);
                cmd.Parameters.AddWithValue("@supplier", inputSupplier);
                cmd.Parameters.AddWithValue("@quantity", inputQuantity);
                cmd.Parameters.AddWithValue("@purchasePrice", inputPurchasePrice);
                cmd.Parameters.AddWithValue("@sellingPrice", inputSellingPrice);
                cmd.Parameters.AddWithValue("@id", selectedProductId);

                int rowsChanged = cmd.ExecuteNonQuery();

                if (rowsChanged == 0)
                {
                    MessageBox.Show("That product no longer exists.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    RefreshGrid();
                    ClearFields();

                    MessageBox.Show("Product updated successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedProductId == -1)
            {
                MessageBox.Show("Please select a product from the list first.", "No Product Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this product?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            MySqlConnection conn = DatabaseHelper.GetConnection();

            try
            {
                conn.Open();

                string query = "DELETE FROM products WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", selectedProductId);
                cmd.ExecuteNonQuery();

                RefreshGrid();
                ClearFields();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = (txtProductName.Text == NamePlaceholder) ? "" : txtProductName.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                RefreshGrid();
                return;
            }

            dgvProducts.Rows.Clear();

            MySqlConnection conn = DatabaseHelper.GetConnection();
            int rowCount = 0;

            try
            {
                conn.Open();

                string query = "SELECT id, name, category, supplier, quantity, purchase_price, selling_price " +
                    "FROM products WHERE name LIKE @search ORDER BY id";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32("id");
                    string name = reader.GetString("name");
                    string category = reader.GetString("category");
                    string supplier = reader.GetString("supplier");
                    int quantity = reader.GetInt32("quantity");
                    decimal purchasePrice = reader.GetDecimal("purchase_price");
                    decimal sellingPrice = reader.GetDecimal("selling_price");

                    dgvProducts.Rows.Add(id, name, category, supplier, quantity,
                        purchasePrice.ToString("F2"), sellingPrice.ToString("F2"));

                    rowCount = rowCount + 1;
                }

                reader.Close();

                if (rowCount == 0)
                {
                    MessageBox.Show("No products found matching \"" + searchText + "\".", "No Results",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

       

        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = dgvProducts.Rows[e.RowIndex];

            selectedProductId = Convert.ToInt32(row.Cells["colID"].Value);

            txtProductName.Text = row.Cells["colProductName"].Value.ToString();
            txtProductName.ForeColor = Color.Black;

            cmbCategory.Text = row.Cells["colCategory"].Value.ToString();
            cmbSupplier.Text = row.Cells["colSupplier"].Value.ToString();
            nudQuantity.Value = Convert.ToInt32(row.Cells["colQuantity"].Value);

            txtPurchasePrice.Text = row.Cells["colPurchasePrice"].Value.ToString();
            txtPurchasePrice.ForeColor = Color.Black;

            txtSellingPrice.Text = row.Cells["colSellingPrice"].Value.ToString();
            txtSellingPrice.ForeColor = Color.Black;
        }

        private void btnBrowseImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    picProductImage.Image = Image.FromFile(dialog.FileName);
                }
                catch
                {
                    MessageBox.Show("Could not load the selected image.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClear_Click_1(object sender, EventArgs e)
        {
            ClearFields();

        }
        private void ClearFields()
        {
            txtProductName.Text = NamePlaceholder;
            txtProductName.ForeColor = Color.Gray;

            txtPurchasePrice.Text = PricePlaceholder;
            txtPurchasePrice.ForeColor = Color.Gray;

            txtSellingPrice.Text = PricePlaceholder;
            txtSellingPrice.ForeColor = Color.Gray;

            cmbCategory.Text = CategoryPlaceholder;
            cmbSupplier.Text = SupplierPlaceholder;
            nudQuantity.Value = 0;

            picProductImage.Image = final_vp_project.Properties.Resources.box1;

            selectedProductId = -1;
            dgvProducts.ClearSelection();
        }

        private void dgvProducts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void picProductImage_Click(object sender, EventArgs e)
        {

        }

        private void cmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}