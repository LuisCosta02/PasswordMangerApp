using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace PasswordManager_c_.Properties
{
    public partial class PasswordManager : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
       (
           int nLeftRect,     // x-coordinate of upper-left corner
           int nTopRect,      // y-coordinate of upper-left corner
           int nRightRect,    // x-coordinate of lower-right corner
           int nBottomRect,   // y-coordinate of lower-right corner
           int nWidthEllipse, // height of ellipse
           int nHeightEllipse // width of ellipse
       );

        string nameUserLoggged = SessionManager.NameUserLogged;
        int userId = SessionManager.LoggedInUserId;
        string titleentry = SessionManager.SelectedTitle;
        bool isFavourite;
        private List<ListViewItem> allItems = new List<ListViewItem>();
        private bool dragging = false;
        private Point startPoint = new Point(0, 0);




        public PasswordManager()
        {
            InitializeComponent();

            LoadListViewFromDatabase();
            customizeDesignSubMenus();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

        }

        private void PasswordManager_Load(object sender, EventArgs e)
        {
            LoggedUserButton.Text = $"{nameUserLoggged}";
        }










        private void pictureBox9_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
           
            string searchText = textBox1.Text.ToLower();
            listView1.BeginUpdate();
            listView1.Items.Clear();

            var filteredItems = allItems.Where(item =>
                item.Text.ToLower().Contains(searchText) ||
                item.SubItems[1].Text.ToLower().Contains(searchText) ||
                item.SubItems[2].Text.ToLower().Contains(searchText)
            ).ToArray();

            listView1.Items.AddRange(filteredItems);
            listView1.EndUpdate();
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {

        }

        private void textBox1_Leave(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            AddItemForm NewItemForm = new AddItemForm();
            NewItemForm.Show();
            this.Hide();
        }

        private int LoggedInUserId;

        private void LoadListViewFromDatabase()
        {
            listView1.View = View.Details;
            listView1.Columns.Clear();
            listView1.Columns.Add("Website/App", 150);
            listView1.Columns.Add("Username", 250);
            listView1.Columns.Add("Notes", 250);

            using (var connection = new NpgsqlConnection(SessionManager.connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT titleentry, username, notes, favouriteItem FROM PasswordEntries WHERE userid = @UserId";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("UserId", userId);

                        using (var reader = command.ExecuteReader())
                        {


                            while (reader.Read())
                            {

                                string website = reader["titleentry"].ToString();
                                string username = reader["username"].ToString();
                                string notes = reader["notes"].ToString();
                                bool isFavourite = Convert.ToBoolean(reader["favouriteItem"]);

                                AddListViewItem(website, username, notes, isFavourite);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AddListViewItem(string website, string username, string notes, bool isFavourite)
        {
            var item = new ListViewItem(website);

            item.SubItems.Add(username);
            item.SubItems.Add(notes);


            if (isFavourite)
            {
                item.BackColor = Color.Goldenrod;
            }
            else
            {
                item.BackColor = Color.White;
            }
            allItems.Add(item);
            listView1.Items.Add(item);
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }
        private void customizeDesignSubMenus()
        {
            panel4.Visible = false;

        }

        private void hideSubMenu()
        {
            if (panel4.Visible == true)
            {
                panel4.Visible = false;

            }
        }

        private void showSubMenu(Panel subMenu)
        {
            if (subMenu.Visible == false)
            {
                hideSubMenu();
                subMenu.Visible = true;


            }
            else
            {
                subMenu.Visible = false;

            }
        }

        private void LoggedUserButton_Click(object sender, EventArgs e)
        {
            showSubMenu(panel4);
        }

        private void button5_Click(object sender, EventArgs e)
        {

            SessionManager.LoggedInUserId = 0;
            SessionManager.NameUserLogged = null;


            this.Close();
            var loginForm = new LoginForm();
            loginForm.Show();

            hideSubMenu();
        }


        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listView1.SelectedItems[0];
                string title = selectedItem.Text;
                string username = selectedItem.SubItems[1].Text;
                string notes = selectedItem.SubItems[2].Text;
                SessionManager.SelectedTitle = title;

                SessionManager.entryid = GetEntryIdByTitleAndUserId(title, userId);
               



                ShowItem SiForm = new ShowItem();
                SiForm.Show();
                this.Hide();
            }
        }

        private int GetEntryIdByTitleAndUserId(string title, int userId)
        {
            using (var connection = new NpgsqlConnection(SessionManager.connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT entryid FROM PasswordEntries WHERE userid = @UserId AND titleentry = @TitleEntry";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@TitleEntry", title);

                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            return Convert.ToInt32(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return -1;
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this.startPoint.X, p.Y - this.startPoint.Y);


            }
        }
    }
}
