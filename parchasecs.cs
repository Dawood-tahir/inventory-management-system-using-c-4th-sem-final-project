using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace final_vp_project
{
    public partial class parchasecs : Form
    {
        private const string ProductPlaceholder = "Select Product...";

        private int selectedPurchaseId = -1;
        private string selectedOldProductName = "";
        private int selectedOldQuantity = 0;
        private int inputProductId = 0;
        private string inputProductName = "";
        private int inputQuantity = 0;
        private decimal inputPrice = 0;

        public parchasecs()
        {
            InitializeComponent();

            btnSave.Click += btnSave_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnClear.Click += btnClear_Click;

            dgvPurchaseHistory.CellClick += dgvPurchaseHistory_CellClick;

            txtQuantity.Enter += txtQuantity_Enter;
            txtQuantity.Leave += txtQuantity_Leave;
            txtPurchasePrice.Enter += txtPurchasePrice_Enter;
            txtPurchasePrice.Leave += txtPurchasePrice_Leave;

            LoadProductChoices();

            dtpPurchaseDate.Value = DateTime.Now;

            LoadPurchaseHistoryToGrid();
        }

        private void LoadProductChoices()
        {
            ProductData.LoadFromDatabase();

            cboProduct.Items.Clear();
            cboProduct.Items.Add(ProductPlaceholder);

            for (int i = 0; i < ProductData.Products.Count; i++)
            {
                cboProduct.Items.Add(ProductData.Products[i].Name);
            }

            cboProduct.SelectedIndex = 0;
        }

        private void LoadPurchaseHistoryToGrid()
        {
            dgvPurchaseHistory.Rows.Clear();

            MySqlConnection conn = DatabaseHelper.GetConnection();

            try
            {
                conn.Open();

                string query = "SELECT id, product_name, quantity, purchase_price, purchase_date, total_amount FROM purchases ORDER BY id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32("id");
                    string productName = reader.GetString("product_name");
                    int quantity = reader.GetInt32("quantity");
                    decimal purchasePrice = reader.GetDecimal("purchase_price");
                    string purchaseDate = reader.GetString("purchase_date");
                    decimal totalAmount = reader.GetDecimal("total_amount");

                    dgvPurchaseHistory.Rows.Add(id, productName, quantity,
                        purchasePrice.ToString("F2"), purchaseDate, totalAmount.ToString("F2"));
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

        private void dgvPurchaseHistory_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = dgvPurchaseHistory.Rows[e.RowIndex];

            selectedPurchaseId = Convert.ToInt32(row.Cells["colPurchaseID"].Value);
            selectedOldProductName = row.Cells["colProduct"].Value.ToString();
            selectedOldQuantity = Convert.ToInt32(row.Cells["colQuantity"].Value);

            cboProduct.Text = selectedOldProductName;

            txtQuantity.Text = selectedOldQuantity.ToString();
            txtQuantity.ForeColor = Color.Black;

            txtPurchasePrice.Text = row.Cells["colPurchasePrice"].Value.ToString();
            txtPurchasePrice.ForeColor = Color.Black;

            DateTime parsedDate;
            bool ok = DateTime.TryParse(row.Cells["colPurchaseDate"].Value.ToString(), out parsedDate);
            if (ok)
            {
                dtpPurchaseDate.Value = parsedDate;
            }
        }

        private void txtQuantity_Enter(object sender, EventArgs e)
        {
            if (txtQuantity.Text == "0" && txtQuantity.ForeColor == Color.Gray)
            {
                txtQuantity.Text = "";
                txtQuantity.ForeColor = Color.Black;
            }
        }

        private void txtQuantity_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                txtQuantity.Text = "0";
                txtQuantity.ForeColor = Color.Gray;
            }
        }

        private void txtPurchasePrice_Enter(object sender, EventArgs e)
        {
            if (txtPurchasePrice.Text == "0.00" && txtPurchasePrice.ForeColor == Color.Gray)
            {
                txtPurchasePrice.Text = "";
                txtPurchasePrice.ForeColor = Color.Black;
            }
        }

        private void txtPurchasePrice_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPurchasePrice.Text))
            {
                txtPurchasePrice.Text = "0.00";
                txtPurchasePrice.ForeColor = Color.Gray;
            }
        }

        private bool ValidateInputs()
        {
            inputProductId = 0;
            inputProductName = "";
            inputQuantity = 0;
            inputPrice = 0;

            if (cboProduct.Text == ProductPlaceholder || string.IsNullOrWhiteSpace(cboProduct.Text))
            {
                MessageBox.Show("Please select a product.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            bool productFound = false;
            for (int i = 0; i < ProductData.Products.Count; i++)
            {
                if (ProductData.Products[i].Name == cboProduct.Text)
                {
                    inputProductId = ProductData.Products[i].Id;
                    inputProductName = ProductData.Products[i].Name;
                    productFound = true;
                }
            }

            if (!productFound)
            {
                MessageBox.Show("That product could not be found. It may have been deleted.", "Product Not Found",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!int.TryParse(txtQuantity.Text, out inputQuantity) || inputQuantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity (1 or higher).", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(txtPurchasePrice.Text, out inputPrice) || inputPrice < 0)
            {
                MessageBox.Show("Please enter a valid purchase price (0 or higher).", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void AdjustProductQuantity(string productName, int changeAmount)
        {
            MySqlConnection conn = DatabaseHelper.GetConnection();

            try
            {
                conn.Open();

                string selectQuery = "SELECT quantity FROM products WHERE name = @name";
                MySqlCommand selectCmd = new MySqlCommand(selectQuery, conn);
                selectCmd.Parameters.AddWithValue("@name", productName);
                object result = selectCmd.ExecuteScalar();

                if (result == null)
                {
                    return;
                }

                int currentQuantity = Convert.ToInt32(result);
                int newQuantity = currentQuantity + changeAmount;

                if (newQuantity < 0)
                {
                    newQuantity = 0;
                }

                string updateQuery = "UPDATE products SET quantity = @quantity WHERE name = @name";
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn);
                updateCmd.Parameters.AddWithValue("@quantity", newQuantity);
                updateCmd.Parameters.AddWithValue("@name", productName);
                updateCmd.ExecuteNonQuery();
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            decimal total = inputQuantity * inputPrice;

            MySqlConnection conn = DatabaseHelper.GetConnection();

            try
            {
                conn.Open();

                string query = "INSERT INTO purchases (product_id, product_name, quantity, purchase_price, purchase_date, total_amount) " +
                    "VALUES (@productId, @productName, @quantity, @price, @date, @total)";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@productId", inputProductId);
                cmd.Parameters.AddWithValue("@productName", inputProductName);
                cmd.Parameters.AddWithValue("@quantity", inputQuantity);
                cmd.Parameters.AddWithValue("@price", inputPrice);
                cmd.Parameters.AddWithValue("@date", dtpPurchaseDate.Value.ToShortDateString());
                cmd.Parameters.AddWithValue("@total", total);

                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                conn.Close();
                return;
            }

            conn.Close();

            AdjustProductQuantity(inputProductName, inputQuantity);

            LoadPurchaseHistoryToGrid();
            ClearForm();

            MessageBox.Show("Purchase recorded and stock updated successfully.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedPurchaseId == -1)
            {
                MessageBox.Show("Please select a purchase from the list first.", "No Purchase Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateInputs())
            {
                return;
            }

            decimal total = inputQuantity * inputPrice;

            MySqlConnection conn = DatabaseHelper.GetConnection();

            try
            {
                conn.Open();

                string query = "UPDATE purchases SET product_id = @productId, product_name = @productName, " +
                    "quantity = @quantity, purchase_price = @price, purchase_date = @date, total_amount = @total " +
                    "WHERE id = @id";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@productId", inputProductId);
                cmd.Parameters.AddWithValue("@productName", inputProductName);
                cmd.Parameters.AddWithValue("@quantity", inputQuantity);
                cmd.Parameters.AddWithValue("@price", inputPrice);
                cmd.Parameters.AddWithValue("@date", dtpPurchaseDate.Value.ToShortDateString());
                cmd.Parameters.AddWithValue("@total", total);
                cmd.Parameters.AddWithValue("@id", selectedPurchaseId);

                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                conn.Close();
                return;
            }

            conn.Close();

            AdjustProductQuantity(selectedOldProductName, -selectedOldQuantity);
            AdjustProductQuantity(inputProductName, inputQuantity);

            LoadPurchaseHistoryToGrid();
            ClearForm();

            MessageBox.Show("Purchase updated and stock adjusted successfully.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedPurchaseId == -1)
            {
                MessageBox.Show("Please select a purchase from the list first.", "No Purchase Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this purchase? The stock added by it will be removed too.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            MySqlConnection conn = DatabaseHelper.GetConnection();

            try
            {
                conn.Open();

                string query = "DELETE FROM purchases WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", selectedPurchaseId);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                conn.Close();
                return;
            }

            conn.Close();

            AdjustProductQuantity(selectedOldProductName, -selectedOldQuantity);

            LoadPurchaseHistoryToGrid();
            ClearForm();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            selectedPurchaseId = -1;
            selectedOldProductName = "";
            selectedOldQuantity = 0;

            LoadProductChoices();

            txtQuantity.Text = "0";
            txtQuantity.ForeColor = Color.Gray;

            txtPurchasePrice.Text = "0.00";
            txtPurchasePrice.ForeColor = Color.Gray;

            dtpPurchaseDate.Value = DateTime.Now;

            dgvPurchaseHistory.ClearSelection();
        }

        private void parchasecs_Load(object sender, EventArgs e)
        {

        }

        private void pnlInfo_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}