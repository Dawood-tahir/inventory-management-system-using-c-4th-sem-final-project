using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace final_vp_project
{
    public partial class catagory : Form
    {
        private const string NamePlaceholder = "Enter category name";
        private const string SearchPlaceholder = "Search category...";

        private int selectedCategoryId = -1;

        public catagory()
        {
            InitializeComponent();

            btnSave.Click += btnSave_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnSearch.Click += btnSearch_Click;
            btnClear.Click += btnClear_Click;

            dgvCategories.CellClick += dgvCategories_CellClick;

            txtCategoryName.Enter += txtCategoryName_Enter;
            txtCategoryName.Leave += txtCategoryName_Leave;
            txtSearch.Enter += txtSearch_Enter;
            txtSearch.Leave += txtSearch_Leave;
        }

        private void catagory_Load(object sender, EventArgs e)
        {
            LoadCategoriesToGrid();
        }

        private void LoadCategoriesToGrid()
        {
            dgvCategories.Rows.Clear();

            MySqlConnection conn = DatabaseHelper.GetConnection();

            try
            {
                conn.Open();

                string query = "SELECT id, name, created_date FROM categories ORDER BY id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32("id");
                    string name = reader.GetString("name");
                    string createdDate = reader.GetString("created_date");

                    dgvCategories.Rows.Add(id, name, createdDate);
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

        private void dgvCategories_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = dgvCategories.Rows[e.RowIndex];

            selectedCategoryId = Convert.ToInt32(row.Cells["colCategoryID"].Value);

            txtCategoryName.Text = row.Cells["colCategoryName"].Value.ToString();
            txtCategoryName.ForeColor = Color.Black;
        }

        private void txtCategoryName_Enter(object sender, EventArgs e)
        {
            if (txtCategoryName.Text == NamePlaceholder)
            {
                txtCategoryName.Text = "";
                txtCategoryName.ForeColor = Color.Black;
            }
        }

        private void txtCategoryName_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
            {
                txtCategoryName.Text = NamePlaceholder;
                txtCategoryName.ForeColor = Color.Gray;
            }
        }

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == SearchPlaceholder)
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = SearchPlaceholder;
                txtSearch.ForeColor = Color.Gray;
            }
        }

        private bool NameAlreadyExists(string name, int ignoreId)
        {
            bool found = false;

            MySqlConnection conn = DatabaseHelper.GetConnection();

            try
            {
                conn.Open();

                string query = "SELECT id FROM categories WHERE LOWER(name) = LOWER(@name)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", name);

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32("id");
                    if (id != ignoreId)
                    {
                        found = true;
                    }
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

            return found;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string name = (txtCategoryName.Text == NamePlaceholder) ? "" : txtCategoryName.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a category name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (NameAlreadyExists(name, -1))
            {
                MessageBox.Show("A category with that name already exists.", "Duplicate Category",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MySqlConnection conn = DatabaseHelper.GetConnection();

            try
            {
                conn.Open();

                string query = "INSERT INTO categories (name, created_date) VALUES (@name, @createdDate)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@createdDate", DateTime.Now.ToShortDateString());

                cmd.ExecuteNonQuery();

                LoadCategoriesToGrid();
                ClearForm();

                MessageBox.Show("Category added successfully.", "Success",
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
            if (selectedCategoryId == -1)
            {
                MessageBox.Show("Please select a category from the list first.", "No Category Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = (txtCategoryName.Text == NamePlaceholder) ? "" : txtCategoryName.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a category name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (NameAlreadyExists(name, selectedCategoryId))
            {
                MessageBox.Show("Another category already has that name.", "Duplicate Category",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MySqlConnection conn = DatabaseHelper.GetConnection();

            try
            {
                conn.Open();

                string selectQuery = "SELECT name FROM categories WHERE id = @id";
                MySqlCommand selectCmd = new MySqlCommand(selectQuery, conn);
                selectCmd.Parameters.AddWithValue("@id", selectedCategoryId);
                object oldNameResult = selectCmd.ExecuteScalar();

                if (oldNameResult == null)
                {
                    MessageBox.Show("That category no longer exists.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string oldName = oldNameResult.ToString();

                string updateQuery = "UPDATE categories SET name = @name WHERE id = @id";
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn);
                updateCmd.Parameters.AddWithValue("@name", name);
                updateCmd.Parameters.AddWithValue("@id", selectedCategoryId);
                updateCmd.ExecuteNonQuery();

                string productQuery = "UPDATE products SET category = @newName WHERE category = @oldName";
                MySqlCommand productCmd = new MySqlCommand(productQuery, conn);
                productCmd.Parameters.AddWithValue("@newName", name);
                productCmd.Parameters.AddWithValue("@oldName", oldName);
                productCmd.ExecuteNonQuery();

                LoadCategoriesToGrid();
                ClearForm();

                MessageBox.Show("Category updated successfully.", "Success",
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

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedCategoryId == -1)
            {
                MessageBox.Show("Please select a category from the list first.", "No Category Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MySqlConnection conn = DatabaseHelper.GetConnection();
            int productsUsingIt = 0;
            string categoryName = "";

            try
            {
                conn.Open();

                string nameQuery = "SELECT name FROM categories WHERE id = @id";
                MySqlCommand nameCmd = new MySqlCommand(nameQuery, conn);
                nameCmd.Parameters.AddWithValue("@id", selectedCategoryId);
                object nameResult = nameCmd.ExecuteScalar();

                if (nameResult == null)
                {
                    return;
                }

                categoryName = nameResult.ToString();

                string countQuery = "SELECT COUNT(*) FROM products WHERE category = @name";
                MySqlCommand countCmd = new MySqlCommand(countQuery, conn);
                countCmd.Parameters.AddWithValue("@name", categoryName);
                productsUsingIt = Convert.ToInt32(countCmd.ExecuteScalar());
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                conn.Close();
                return;
            }

            string confirmMessage = "Are you sure you want to delete this category?";
            if (productsUsingIt > 0)
            {
                confirmMessage = productsUsingIt + " product(s) are currently using this category. " +
                    "Deleting it will not remove those products, but they will keep the old category name. " +
                    "Are you sure you want to delete it?";
            }

            DialogResult result = MessageBox.Show(confirmMessage, "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                conn.Close();
                return;
            }

            try
            {
                string deleteQuery = "DELETE FROM categories WHERE id = @id";
                MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, conn);
                deleteCmd.Parameters.AddWithValue("@id", selectedCategoryId);
                deleteCmd.ExecuteNonQuery();

                LoadCategoriesToGrid();
                ClearForm();
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
            string keyword = (txtSearch.Text == SearchPlaceholder) ? "" : txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                LoadCategoriesToGrid();
                return;
            }

            dgvCategories.Rows.Clear();

            MySqlConnection conn = DatabaseHelper.GetConnection();
            int rowCount = 0;

            try
            {
                conn.Open();

                string query = "SELECT id, name, created_date FROM categories WHERE name LIKE @keyword ORDER BY id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32("id");
                    string name = reader.GetString("name");
                    string createdDate = reader.GetString("created_date");

                    dgvCategories.Rows.Add(id, name, createdDate);
                    rowCount = rowCount + 1;
                }

                reader.Close();

                if (rowCount == 0)
                {
                    MessageBox.Show("No categories matched your search.", "Search Results",
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
            ClearForm();
        }

        private void ClearForm()
        {
            
        }

        private void btnClear_Click_1(object sender, EventArgs e)
        {
            selectedCategoryId = -1;

            txtCategoryName.Text = NamePlaceholder;
            txtCategoryName.ForeColor = Color.Gray;

            txtSearch.Text = SearchPlaceholder;
            txtSearch.ForeColor = Color.Gray;

            dgvCategories.ClearSelection();
            LoadCategoriesToGrid();
        }

        private void txtCategoryName_TextChanged(object sender, EventArgs e)
        {

        }
    }
}