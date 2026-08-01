using final_vp_project;
using MySql.Data.MySqlClient;   
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace final_vp_project
{
    public partial class login : Form
    {
      
        private const string UsernamePlaceholder = "Enter username";
        private const string PasswordPlaceholder = "Enter password";

       
        private int loginAttempts = 0;
        private const int MaxLoginAttempts = 3;

        public login()
        {
            InitializeComponent();
        }



        private void login_Load(object sender, EventArgs e)
        {
           
            txtPassword.UseSystemPasswordChar = true;
            txtUsername.ForeColor = Color.Gray;
            txtPassword.ForeColor = Color.Gray;
            chkShowPassword.Checked = false;

            
            this.AcceptButton = btnLogin;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                   
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message);
            }
        }

        private void pnlRight_Paint(object sender, PaintEventArgs e)
        {
            
        }

        // --- Username placeholder behavior ------------------------------
        private void txtUsername_Enter(object sender, EventArgs e)
        {
            if (txtUsername.Text == UsernamePlaceholder)
            {
                txtUsername.Text = "";
                txtUsername.ForeColor = Color.Black;
            }
        }

        private void txtUsername_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                txtUsername.Text = UsernamePlaceholder;
                txtUsername.ForeColor = Color.Gray;
            }
        }

        private void txtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                txtPassword.Focus();
            }
        }

        // --- Password placeholder behavior ------------------------------
        private void txtPassword_Enter(object sender, EventArgs e)
        {
            if (!chkShowPassword.Checked)
            {
                txtPassword.UseSystemPasswordChar = true;
            }

            if (txtPassword.Text == PasswordPlaceholder && txtPassword.ForeColor == Color.Gray)
            {
                txtPassword.Text = "";
                txtPassword.ForeColor = Color.Black;
            }
        }

        private void txtPassword_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                txtPassword.UseSystemPasswordChar = false;
                txtPassword.ForeColor = Color.Gray;
                txtPassword.Text = PasswordPlaceholder;
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnLogin_Click(sender, e);
            }
        }

        // --- Show / hide password ----------------------------------------
        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            bool isRealPassword = txtPassword.ForeColor == Color.Black;
            txtPassword.UseSystemPasswordChar = !(chkShowPassword.Checked && isRealPassword);
        }

        private void picEyeIcon_Click(object sender, EventArgs e)
        {
            chkShowPassword.Checked = !chkShowPassword.Checked;
        }

        // --- Database check ------------------------------------------------
        
        private bool IsValidLogin(string username, string password)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();

                    string query = "SELECT * FROM users WHERE username = @username AND password = @password";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    MySqlDataReader reader = cmd.ExecuteReader();
                    return reader.HasRows;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Database error: " + ex.Message, "Connection Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        // --- Login / Exit --------------------------------------------------
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = (txtUsername.Text == UsernamePlaceholder) ? "" : txtUsername.Text.Trim();
            string password = (txtPassword.Text == PasswordPlaceholder) ? "" : txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Login Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

           
            if (IsValidLogin(username, password))
            {
                loginAttempts = 0;

                MessageBox.Show("Login successful. Welcome, " + username + "!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                
                deshboard dashboardForm = new deshboard();
                dashboardForm.FormClosed += (s, args) => this.Close();
                dashboardForm.Show();
                this.Hide();
            }
            else
            {
                loginAttempts++;
                int remaining = MaxLoginAttempts - loginAttempts;

                if (remaining <= 0)
                {
                    MessageBox.Show("Too many failed login attempts. The application will now close.",
                        "Account Locked", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                    return;
                }

                MessageBox.Show($"Invalid username or password. {remaining} attempt(s) remaining.",
                    "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                txtPassword.Text = "";
                txtPassword.UseSystemPasswordChar = false;
                txtPassword.ForeColor = Color.Gray;
                txtPassword.Text = PasswordPlaceholder;
                txtPassword.Focus();
            }
        }


        private void btnExit_Click_1(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to exit?", "Confirm Exit",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void pnlLeft_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}