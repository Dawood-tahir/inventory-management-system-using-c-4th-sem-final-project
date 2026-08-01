using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace final_vp_project
{
    public partial class deshboard : Form
    {
        public deshboard()
        {
            InitializeComponent();
        }

        private void deshboard_Load(object sender, EventArgs e)
        {
            Rectangle workArea = Screen.FromControl(this).WorkingArea;
            if (workArea.Width < this.Width || workArea.Height < this.Height)
            {
                this.WindowState = FormWindowState.Maximized;
            }

            

            lblDate.Text = DateTime.Now.ToString("dd MMMM yyyy");

      
            LoadSampleRecentActivity();

            
        }

      
        private void LoadSampleRecentActivity()
        {
            dgvRecent.Rows.Clear();
            dgvRecent.Rows.Add("1", "Sample Laptop", "Electronics", "10", DateTime.Now.ToShortDateString(), "In Stock");
            dgvRecent.Rows.Add("2", "Sample Chair", "Furniture", "25", DateTime.Now.ToShortDateString(), "In Stock");
            dgvRecent.Rows.Add("3", "Sample Notebook", "Stationery", "100", DateTime.Now.ToShortDateString(), "In Stock");
        }

        // --- Sidebar navigation --------------------------------------------

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            
            LoadSampleRecentActivity();
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            OpenChildForm(new product());
        }

        private void btnCategories_Click(object sender, EventArgs e)
        {
            OpenChildForm(new catagory());
        }

        private void btnSuppliers_Click(object sender, EventArgs e)
        {
            
            MessageBox.Show("Supplier Management is coming in a future module.",
                "Not Available Yet", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnSales_Click(object sender, EventArgs e)
        {
            
            OpenChildForm(new parchasecs());
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            OpenChildForm(new report());
        }

        private void btnViewAll_Click(object sender, EventArgs e)
        {
            OpenChildForm(new report());
        }

        // --- Logout ----------------------------------------------------------

        private void btnLogout_Click(object sender, EventArgs e)
        {
            LogoutToLogin();
        }

        private void btnLogoutMenu_Click(object sender, EventArgs e)
        {
            LogoutToLogin();
        }

        private void LogoutToLogin()
        {
            DialogResult result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                login loginForm = new login();
                loginForm.FormClosed += (s, args) => this.Close();
                loginForm.Show();
                this.Hide();
            }
        }

       
        private void OpenChildForm(Form childForm)
        {
            childForm.FormClosed += (s, e) => this.Show();
            this.Hide();
            childForm.Show();
        }

        private void picUser_Click(object sender, EventArgs e)
        {

        }

        private void picCardSales_Click(object sender, EventArgs e)
        {

        }
    }
}