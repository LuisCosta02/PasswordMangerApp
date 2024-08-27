using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager_c_
{
    internal class SessionManager
    {
        public static int LoggedInUserId { get; set; }
        public static string NameUserLogged { get; set; }

        public static string SelectedTitle { get; set; }

        public static int entryid { get; set; }

        //Connection to DB
        public static string connectionString = "Host=localhost;Database=;Username=;Password=;Persist Security Info=True";

    }
}
