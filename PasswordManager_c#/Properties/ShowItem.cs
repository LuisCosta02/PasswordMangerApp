using Microsoft.VisualBasic.ApplicationServices;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace PasswordManager_c_.Properties
{
    public partial class ShowItem : Form
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
        string titleentry = SessionManager.SelectedTitle;
        int entryid = SessionManager.entryid;
        bool isFavourite;
        private byte[] imageData;
        private bool dragging = false;
        private Point startPoint = new Point(0, 0);




        public ShowItem()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
        }

        private void ShowItem_Load(object sender, EventArgs e)

        {



            string masterPassword = GetMasterPassword();

            string encryptedPassword = "";
            string ivBase64 = "";
            string saltBase64 = "";

            using (var connection = new NpgsqlConnection(SessionManager.connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT titleentry, website, username, passworduser, notes, createdat, updatedat, iv, salt, imageData FROM passwordentries WHERE entryid = @entryid AND userid = @userId";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@entryid", entryid);
                        command.Parameters.AddWithValue("@userId", UserId);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                textBox1.Text = reader["titleentry"].ToString();
                                textBox5.Text = reader["website"].ToString();
                                textBox2.Text = reader["username"].ToString();
                                encryptedPassword = reader["passworduser"].ToString();
                                textBox6.Text = reader["notes"].ToString();
                                label2.Text = reader["createdat"].ToString();
                                label7.Text = reader["updatedat"].ToString();
                                ivBase64 = reader["iv"].ToString();
                                saltBase64 = reader["salt"].ToString();
                                if (reader["imageData"] != DBNull.Value)
                                {
                                    byte[] imageBytes = (byte[])reader["imageData"];
                                    using (var ms = new MemoryStream(imageBytes))
                                    {
                                        pictureBox1.Image = Image.FromStream(ms);
                                    }
                                }
                                else
                                {

                                    pictureBox1.Image = null;
                                }
                            }
                            else
                            {
                                MessageBox.Show("No data found for the given entry ID", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }


            try
            {
                Console.WriteLine($"IV Base64: {ivBase64}");
                Console.WriteLine($"Salt Base64: {saltBase64}");

                byte[] salt = Convert.FromBase64String(saltBase64);
                byte[] aesKey = PBKDF2Helper.DeriveKey(masterPassword, salt, 10000, 32);


                AESHelper aesHelper = new AESHelper(aesKey);
                string decryptedPassword = aesHelper.Decrypt(encryptedPassword, ivBase64);
                textBox4.Text = decryptedPassword;
            }
            catch (FormatException fe)
            {
                MessageBox.Show($"Base64 Format Error: {fe.Message}", "Format Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var connection = new NpgsqlConnection(SessionManager.connectionString))
            {
                connection.Open();

                string selectQuery = "SELECT favouriteItem FROM PasswordEntries WHERE entryid = @entryid AND userId = @UserId";

                using (var command = new NpgsqlCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@entryid", entryid);
                    command.Parameters.AddWithValue("@UserId", UserId);

                    var result = command.ExecuteScalar();

                    if (result != null && (bool)result)
                    {
                        textBox1.ForeColor = Color.Goldenrod;
                        isFavourite = true;
                    }
                    else
                    {
                        textBox1.ForeColor = Color.Black;
                        isFavourite = false;
                    }
                }
            }
        }





        private string GetMasterPassword()
        {

            return "UserMasterPassword";
        }



        private void pictureBox9_Click_1(object sender, EventArgs e)
        {
            this.Close();
            PasswordManager pMForm = new PasswordManager();
            pMForm.Show();
        }

        private void pictureBox8_Click_1(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            isFavouriteorNot();
        }

        private void isFavouriteorNot()
        {

            isFavourite = !isFavourite;

            if (isFavourite)
            {
                textBox1.ForeColor = Color.Goldenrod;
                MessageBox.Show("Website/App Added to Favourites!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                textBox1.ForeColor = Color.Black;
                MessageBox.Show("Website/App Removed from Favourites!", "Removed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


            using (var connection = new NpgsqlConnection(SessionManager.connectionString))
            {
                connection.Open();

                string updateQuery = "UPDATE PasswordEntries SET favouriteItem = @FavouriteItem WHERE entryid = @entryid AND userId = @UserId";

                using (var command = new NpgsqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@FavouriteItem", isFavourite);
                    command.Parameters.AddWithValue("@entryid", entryid);
                    command.Parameters.AddWithValue("@UserId", UserId);

                    command.ExecuteNonQuery();
                }
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox4.Text);

            MessageBox.Show("Password Copied to Transfer Area");
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            if (textBox4.PasswordChar == '\0')
            {
                pictureBox4.BringToFront();
                textBox4.PasswordChar = '*';
            }
        }


        private void pictureBox4_Click(object sender, EventArgs e)
        {

            if (textBox4.PasswordChar == '*')
            {
                pictureBox5.BringToFront();
                textBox4.PasswordChar = '\0';
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {


            button3.Show();
            label3.Show();
            pictureBox2.Show();
            textBox2.ReadOnly = false;
            textBox4.ReadOnly = false;
            pictureBox3.BackColor = Color.White;
            pictureBox4.BackColor = Color.White;
            textBox5.ReadOnly = false;
            textBox6.ReadOnly = false;





        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var connection = new NpgsqlConnection(SessionManager.connectionString))
            {
                connection.Open();

                string deleteQuery = "DELETE FROM PasswordEntries WHERE entryid = @entryid AND UserId = @UserId";

                using (var command = new NpgsqlCommand(deleteQuery, connection))
                {

                    command.Parameters.AddWithValue("@entryid", entryid);
                    command.Parameters.AddWithValue("@UserId", UserId);

                    command.ExecuteNonQuery();


                }
            }
            MessageBox.Show("Entry deleted with success", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            PasswordManager pMForm = new PasswordManager();
            pMForm.Show();
            this.Close();


        }

        private void button3_Click(object sender, EventArgs e)
        {
            string title = textBox1.Text;
            string username = textBox2.Text;
            string password = textBox4.Text;
            string website = textBox5.Text;
            string notes = textBox6.Text;

            string encryptedPassword = "";
            string ivBase64 = "";
            string saltBase64 = "";

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please Enter All Required Fields", "WARN", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            string masterPassword = GetMasterPassword();

            using (NpgsqlConnection connection = new NpgsqlConnection(SessionManager.connectionString))
            {
                try
                {
                    connection.Open();


                    string selectQuery = "SELECT titleentry, iv, salt FROM PasswordEntries WHERE entryid = @entryid AND userid = @UserId";
                    string currentTitle = "";
                    using (var command = new NpgsqlCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@entryid", entryid);
                        command.Parameters.AddWithValue("@UserId", UserId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currentTitle = reader["titleentry"].ToString();
                                ivBase64 = reader["iv"].ToString();
                                saltBase64 = reader["salt"].ToString();
                            }
                            else
                            {
                                MessageBox.Show("No data found for the given entry ID", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                        }
                    }


                    if (title != currentTitle)
                    {
                        string checkQuery = "SELECT COUNT(*) FROM PasswordEntries WHERE userid = @UserId AND titleentry = @Title AND entryid <> @entryid";
                        using (NpgsqlCommand checkCmd = new NpgsqlCommand(checkQuery, connection))
                        {
                            checkCmd.Parameters.AddWithValue("@UserId", UserId);
                            checkCmd.Parameters.AddWithValue("@Title", title);
                            checkCmd.Parameters.AddWithValue("@entryid", entryid);

                            int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                            if (count > 0)
                            {
                                MessageBox.Show("Title already exists for this user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                    }


                    byte[] salt = Convert.FromBase64String(saltBase64);


                    byte[] aesKey = PBKDF2Helper.DeriveKey(masterPassword, salt, 10000, 32);

                    AESHelper aesHelper = new AESHelper(aesKey);


                    encryptedPassword = aesHelper.Encrypt(password, ivBase64);

                    string updateQuery = "UPDATE PasswordEntries SET titleentry = @titleentry, username = @username, passworduser = @passworduser, website = @website, notes = @notes, iv = @iv, salt = @salt, imagedata = @imageData WHERE entryid = @entryid AND userid = @UserId";

                    using (var updateCommand = new NpgsqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@titleentry", title);
                        updateCommand.Parameters.AddWithValue("@username", username);
                        updateCommand.Parameters.AddWithValue("@passworduser", encryptedPassword);
                        updateCommand.Parameters.AddWithValue("@website", website);
                        updateCommand.Parameters.AddWithValue("@notes", notes);
                        updateCommand.Parameters.AddWithValue("@iv", ivBase64);
                        updateCommand.Parameters.AddWithValue("@salt", saltBase64);
                        updateCommand.Parameters.AddWithValue("@entryid", entryid);
                        updateCommand.Parameters.AddWithValue("@UserId", UserId);

                        if (imageData == null)
                        {
                            updateCommand.Parameters.AddWithValue("@imageData", DBNull.Value);
                        }
                        else
                        {
                            updateCommand.Parameters.AddWithValue("@imageData", imageData);
                        }

                        int rowsAffected = updateCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Data updated successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            PasswordManager pMForm = new PasswordManager();
                            pMForm.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Data update failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this.startPoint.X, p.Y - this.startPoint.Y);


            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
