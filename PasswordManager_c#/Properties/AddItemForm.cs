using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using Npgsql;
using static System.Windows.Forms.DataFormats;
using System.Security.Cryptography;

namespace PasswordManager_c_.Properties
{
    public partial class AddItemForm : Form
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




        int UserId = SessionManager.LoggedInUserId;
        private byte[] imageData;
        private bool dragging = false;
        private Point startPoint = new Point(0, 0);




        public AddItemForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            



        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            PasswordManager pMForm = new PasswordManager();
            pMForm.Show();
            this.Close();

        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            openFileDialog.Title = "Select a Picture";

            
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
               
                pictureBox1.Image = Image.FromFile(openFileDialog.FileName);

                
                imageData = File.ReadAllBytes(openFileDialog.FileName);


            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }
        private AddItemForm AddForm;


        private void button3_Click(object sender, EventArgs e)
        {
            {
                string title = textBox1.Text;
                string username = textBox2.Text;
                string password = textBox4.Text;
                string website = textBox5.Text;
                string notes = textBox6.Text;
                bool isFavourite = checkBox1.Checked;

                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Please Enter All Required Fields", "WARN", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                
                string masterPassword = GetMasterPassword();

               
                byte[] salt = GenerateSalt();

                
                byte[] aesKey = PBKDF2Helper.DeriveKey(masterPassword, salt, 10000, 32);

                AESHelper aesHelper = new AESHelper(aesKey);

                
                (string encryptedPassword, string iv) = aesHelper.Encrypt(password);

                using (NpgsqlConnection connection = new NpgsqlConnection(SessionManager.connectionString))
                {
                    try
                    {
                        connection.Open();

                       
                        string checkQuery = "SELECT COUNT(*) FROM passwordEntries WHERE userid = @UserId AND titleentry = @Title";
                        using (NpgsqlCommand checkCmd = new NpgsqlCommand(checkQuery, connection))
                        {
                            checkCmd.Parameters.AddWithValue("@UserId", UserId);
                            checkCmd.Parameters.AddWithValue("@Title", title);

                            int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                            if (count > 0)
                            {
                                MessageBox.Show("Title already exists for this user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                        
                        string query = "INSERT INTO passwordEntries (userid, titleentry, username, passworduser, website, notes, favouriteitem, iv, salt, imagedata) VALUES (@UserId, @Title, @Username, @Password, @Website, @Notes, @favouriteitem, @IV, @Salt, @imageData)";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@UserId", UserId);
                            cmd.Parameters.AddWithValue("@Title", title);
                            cmd.Parameters.AddWithValue("@Username", username);
                            cmd.Parameters.AddWithValue("@Password", encryptedPassword);
                            cmd.Parameters.AddWithValue("@Website", website);
                            cmd.Parameters.AddWithValue("@Notes", notes);
                            cmd.Parameters.AddWithValue("@favouriteitem", isFavourite);
                            cmd.Parameters.AddWithValue("@IV", iv);
                            cmd.Parameters.AddWithValue("@Salt", Convert.ToBase64String(salt));
                            if (imageData == null)
                            {
                                cmd.Parameters.AddWithValue("@imageData", DBNull.Value);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@imageData", imageData);
                            }

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Data successfully inserted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                PasswordManager pMForm = new PasswordManager();
                                pMForm.Show();
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Failed to insert data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        
        private byte[] GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[16];
                rng.GetBytes(salt);
                return salt;
            }
        }

        private string GetMasterPassword()
        {
            
            return "UserMasterPassword";
        }

        private void AddItemForm_Load(object sender, EventArgs e)
        {

        }

        private void AddItemForm_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void AddItemForm_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void AddItemForm_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this.startPoint.X, p.Y - this.startPoint.Y);


            }
        }
    }
}
