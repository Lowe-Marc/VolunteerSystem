using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Net.Mail;
using System.Web.UI;

namespace Creating_a_custom_user_login_form.Models
{
    public class User
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember on this computer")]
        public bool RememberMe { get; set; }

        public int id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool isManager { get; set; }
        public bool isAdmin { get; set; }
        public static int staticID { get; set; }
        public static string staticFirstName { get; set; }
        public static string staticLastName { get; set; }
        public static string staticUserName { get; set; }
        public static bool staticIsManager { get; set; }
        public static bool staticIsAdmin { get; set; }

        /// <summary>
        /// Checks if user with given password exists in the database
        /// </summary>
        /// <param name="_username">User name</param>
        /// <param name="_password">User password</param>
        /// <returns>True if user exist and password is correct</returns>
        public bool IsValid(string _username, string _password)
        {
            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            using (cn)
            {
                string _sql = @"SELECT UserName, IsManager, IsAdmin FROM UserTable " +
                       "WHERE UserName = \'" + _username + "\' AND Password = \'" + _password + "\'";
                var cmd = new SqlCommand(_sql, cn);

                cmd.Parameters
                    .Add(new SqlParameter(_username, SqlDbType.NVarChar))
                    .Value = _username;
                cmd.Parameters
                    .Add(new SqlParameter(_password, SqlDbType.NVarChar))
                    .Value = Helpers.SHA1.Encode(_password);
                cn.Open();

                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    DataTable table = new DataTable();
                    table.Load(reader);

                    DataRow row = table.Rows[0];
                    isManager = (bool)row["isManager"];
                    GlobalNamespace.Global.userIsManager = isManager;
                    isAdmin = (bool)row["isAdmin"];
                    GlobalNamespace.Global.userIsAdmin = isAdmin;

                    reader.Dispose();
                    cmd.Dispose();
                    return true;
                }
                else
                {
                    reader.Dispose();
                    cmd.Dispose();
                    return false;
                }
            }
        }

        /*
         * Created primarily for use when a user logs in, it will store the id as a global variable 
         **/
        public static int getUserIDByUserName(string userName )
        {
            int id = 0;

            DataTable shiftTable = new DataTable();
            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            DataRowCollection rows;
            using (cn)
            {
                string _sql = @"SELECT id FROM UserTable WHERE UserName = '" + userName + "'";
                var cmd = new SqlCommand(_sql, cn);
                cn.Open();
                shiftTable.Load(cmd.ExecuteReader());
                rows = shiftTable.Rows;
                cn.Close();
                foreach (DataRow row in rows)
                {
                    id = Convert.ToInt32(row["id"]);
                }
            }
            return id;
        }

        /*
         * Submits a shift for the eventID at startTime
         * */
        public static void takeShift(int eventID, int startTime )
        {
            int userID = GlobalNamespace.Global.userID;
            List<int> userIds = new List<int>();
            using (SqlConnection connection = new SqlConnection
                   (ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString)){

                connection.Open();
                String sql = "SELECT UserID FROM Shift WHERE EventId IN(SELECT id FROM Event WHERE StartTime = (SELECT StartTime FROM Event WHERE id = " + eventID + "))";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int s;
                            s = Convert.ToInt32(reader["UserId"]);
                            userIds.Add(s);
                        }
                    }
                }
                connection.Close();
            }
            if(userIds.Contains(userID)){
                return;
            }

            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            using (cn)
            {
                string _sql = @"INSERT INTO Shift (UserID, EventID, StartTime) VALUES(" +
                    "'" + userID + "', '" + eventID + "', '" + startTime + "')";

                var cmd = new SqlCommand(_sql, cn);

                cn.Open();
                cmd.ExecuteNonQuery();
                cn.Close();
            }
        }

        /*
         * This is called when an admin clicks the finalize event button. 
         * It will flag the event to be finalized in the database and send confirmation
         * emails to all the volunteers schedules to work for it.
         * */
        public static void finalizeEvent(int eventID)
        {
            int userID = GlobalNamespace.Global.userID;
            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            using (cn)
            {
                string _sql = "UPDATE Event SET Finalized = '1' WHERE id = '" + eventID + "'";
                var cmd = new SqlCommand(_sql, cn);
                cn.Open();
                cmd.ExecuteNonQuery();
                cn.Close();
            }
            CreateEmails(eventID, true);
        }

        /*
         * Sends confirmation emails to all volunteers scheduled to work for the event with id eventID
         * This is the final step in finalizing an event.
         * */
        public static void CreateEmails(int eventID, bool finalizing)
        {
            MVCFullCalendarDemo.Models.Event e = MVCFullCalendarDemo.Models.Event.getEventModelByID(eventID);
            List<Tuple<string, int>> emailStartTimePairs = MVCFullCalendarDemo.Models.Event.getVolunteerEmailsAndStartTimesByEventID(eventID);

            foreach (Tuple<string, int> pair in emailStartTimePairs)
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress("JoeMama@yahoo.com");
                message.To.Add(pair.Item1);
                if (finalizing)
                {
                    message.Subject = "Volunteer Shift Confirmation";
                    message.Body = "Greetings! \n\n This message is to confirm that you are assigned to volunteer for the following event shift:\n\t "
                        + "Time: " + e.StartTime + "\n\t"
                        + "Title: " + e.Title + "\n\t"
                        + "Location: " + e.Location + "\n\n Thanks!";
                }
                else
                {
                    message.Subject = "Volunteer Event Deletion";
                    message.Body = "Greetings! \n\n This message is to notify you the following event has been canceled:\n\t "
                        +"Time: " + e.StartTime + "\n\t"
                    + "Title: " + e.Title + "\n\t"
                    + "Location: " + e.Location + "\n\n Thanks!";
                }
#pragma warning disable CS0618 // Type or member is obsolete
                SmtpClient mysmtpclient = new SmtpClient("smtp.gmail.com");
                mysmtpclient.Port = 587;
                mysmtpclient.Credentials = new System.Net.NetworkCredential("joemamavolunteer@gmail.com", "joemama13");
                mysmtpclient.EnableSsl = true;
                mysmtpclient.Send(message);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        /*
         * Helper to get every user in the UserTable
         * */
        public static List<User> getAllUsers()
        {
            List<User> allUsers = new List<User>();

            DataTable userTable = new DataTable();
            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            DataRowCollection rows;
            using (cn)
            {
                string _sql = "SELECT * FROM UserTable";
                var cmd = new SqlCommand(_sql, cn);
                cn.Open();
                userTable.Load(cmd.ExecuteReader());
                rows = userTable.Rows;
                cn.Close();

                User user;
                foreach (DataRow row in rows)
                {
                    user = new User();
                    user.FirstName = Convert.ToString(row["FirstName"]);
                    user.LastName = Convert.ToString(row["LastName"]);
                    user.UserName = Convert.ToString(row["UserName"]);
                    user.id = Convert.ToInt32(row["id"]);
                    user.isAdmin = Convert.ToBoolean(row["isAdmin"]);
                    user.isManager = Convert.ToBoolean(row["isManager"]);
                    allUsers.Add(user);
                }
            }

            return allUsers;
        }

        /*
         * Removes a user from the UserTable
         * */
        public static void deleteUser(int userID)
        {
            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            using (cn)
            {
                string _sql = @"DELETE FROM UserTable WHERE id = '" + userID + "'";
                var cmd = new SqlCommand(_sql, cn);
                cn.Open();
                cmd.ExecuteNonQuery();
                cn.Close();
            }
        }

        /*
         * Promotes a user to the position of manager
         * */
        public static void promoteUserToManager(int userID)
        {
            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            using (cn)
            {
                string _sql = @"UPDATE UserTable SET isManager = '1' WHERE id = '" + userID + "'";
                var cmd = new SqlCommand(_sql, cn);
                cn.Open();
                cmd.ExecuteNonQuery();
                cn.Close();
            }
        }

        /*
         * Promotes a user to the position of administrator
         * */
        public static void promoteUserToAdmin(int userID)
        {
            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            using (cn)
            {
                string _sql = @"UPDATE UserTable SET isManager = '1', isAdmin = '1' WHERE id = '" + userID + "'";
                var cmd = new SqlCommand(_sql, cn);
                cn.Open();
                cmd.ExecuteNonQuery();
                cn.Close();
            }
        }
    }
}
