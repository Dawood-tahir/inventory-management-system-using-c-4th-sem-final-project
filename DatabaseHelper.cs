using System;
using MySql.Data.MySqlClient;
namespace final_vp_project   // <-- changed from FinalVPProject to match your project
{
    public class DatabaseHelper
    {
        private static string server = "localhost";
        private static string database = "vp_project_db";
        private static string uid = "root";
        private static string password = "Dawood_2007$";    

        private static string connectionString =
            "Server=" + server + ";Database=" + database + ";Uid=" + uid + ";Pwd=" + password + ";";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}