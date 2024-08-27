using System.Diagnostics;
using System.Windows.Forms;
using Npgsql;
using PasswordManager_c_.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using BCrypt.Net;
using System.Runtime.InteropServices;

namespace PasswordManager_c_
{
    public partial class LoginForm : Form
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

        private bool dragging = false;
        private Point startPoint = new Point(0, 0);


        public LoginForm()
        {
            InitializeComponent();
            ConfigurePlaceholderTextBox();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
        }



        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }


        private void ConfigurePlaceholderTextBox()
        {
            
            SignIn_TextBox.Text = "Email address";
            SignIn_TextBox.ForeColor = System.Drawing.Color.Gray;
            textBox1.Text = "Secret Key";
            textBox1.ForeColor = System.Drawing.Color.Gray;
            textBox2.Text = "Master Password";
            textBox2.ForeColor = System.Drawing.Color.Gray;
            textBox2.PasswordChar = '\0'; 

           
            SignIn_TextBox.Enter += SignIn_TextBox_Enter;
            SignIn_TextBox.Leave += SignIn_TextBox_Leave;

            textBox1.Enter += textBox1_Enter;
            textBox1.Leave += textBox1_Leave;

            textBox2.Enter += textBox2_Enter;
            textBox2.Leave += textBox2_Leave;
            textBox2.TextChanged += textBox2_TextChanged;
        }

        private void SignIn_TextBox_Enter(object sender, EventArgs e)
        {
            if (SignIn_TextBox.Text == "Email address")
            {
                SignIn_TextBox.Text = "";
                SignIn_TextBox.ForeColor = System.Drawing.Color.Black;
            }
        }

        private void SignIn_TextBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SignIn_TextBox.Text))
            {
                SignIn_TextBox.Text = "Email address";
                SignIn_TextBox.ForeColor = System.Drawing.Color.Gray;
            }
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "Secret Key")
            {
                textBox1.Text = "";
                textBox1.ForeColor = System.Drawing.Color.Black;
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = "Secret Key";
                textBox1.ForeColor = System.Drawing.Color.Gray;
            }
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (textBox2.Text == "Master Password")
            {
                textBox2.Text = "";
                textBox2.ForeColor = System.Drawing.Color.Black;
                textBox2.PasswordChar = '*'; // Start masking input
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                textBox2.Text = "Master Password";
                textBox2.ForeColor = System.Drawing.Color.Gray;
                textBox2.PasswordChar = '\0'; // Show plain text
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.ForeColor == System.Drawing.Color.Gray && textBox2.Text != "Master Password")
            {
                textBox2.ForeColor = System.Drawing.Color.Black;
                textBox2.PasswordChar = '*';
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            submitLogin();

        }

        private void submitLogin() {

            string email = SignIn_TextBox.Text.Trim();
            string password = textBox2.Text;
            string securityKey = textBox1.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(securityKey))
            {
                MessageBox.Show("Email, password, and security key are required.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (NpgsqlConnection connection = new NpgsqlConnection(SessionManager.connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT nome_user,userid, password_user, security_key FROM usersTable WHERE email_user = @Email";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = reader.GetInt32(1);
                                string nameUser = reader.GetString(0);
                                string storedHashedPassword = reader.GetString(2);
                                string storedSecurityKey = reader.GetString(3);

                               
                                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, storedHashedPassword);

                                
                                bool isSecurityKeyValid = securityKey == storedSecurityKey;

                                if (isPasswordValid && isSecurityKeyValid)
                                {
                                    
                                    SessionManager.LoggedInUserId = userId;
                                    SessionManager.NameUserLogged = nameUser;

                                    
                                    PasswordManager pMForm = new PasswordManager();
                                    pMForm.Show();
                                    this.Hide();
                                }
                                else
                                {
                                    MessageBox.Show("Invalid credentials. Please check your email, password, and security key.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("User not found. Please check your email address.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
            
            
            
          
        

        private void SignIn_TextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "http://localhost/PhpPasswordManager/sign-up.html");
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            if (textBox2.PasswordChar == '\0')
            {
                pictureBox5.BringToFront();
                textBox2.PasswordChar = '*';
            }
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            if (textBox2.PasswordChar == '*')
            {
                pictureBox4.BringToFront();
                textBox2.PasswordChar = '\0';
            }
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }



        private void panel4_MouseUp_1(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void panel4_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void panel4_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this.startPoint.X, p.Y - this.startPoint.Y);


            }
        }
    }
}
