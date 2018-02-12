using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Create_User.Models
{
    public class CreateUser
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Manager")]
        public bool IsManager { get; set; }

        [Display(Name = "Administrator")]
        public bool IsAdmin { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Phone")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

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
                string _sql = @"SELECT Username FROM UserTable " +
                       "WHERE Username = \'" + _username + "\' AND Password = " + _password;
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

        public static bool submitUser(string _username, string _password, bool _isA, bool _isM, string _firstname,
                                     string _lastname, string _phone, string _email)
        {
            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            var _isAdmin = (_isA) ? 1 : 0;
            var _isManager = (_isM) ? 1 : 0;
            char[] temp = _password.ToCharArray();
            for (int i = 0; i < _password.Length; i++)
            {
                temp[i] = Convert.ToChar(temp[i] << 1);
            }
            string s = new string(temp);
            _password = s;
            using (cn)
            {

                string _sql = @"INSERT INTO UserTable (UserName, IsAdmin, IsManager, Password, FirstName, LastName, Phone, Email) VALUES("  + 
                    "'" + _username + "', " + _isAdmin + ", " + _isManager + ", '" + _password + "', '" + _firstname + 
                    "', '" + _lastname + "', '" + _phone + "', '" + _email + "')";
                var cmd = new SqlCommand(_sql, cn);
                cn.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                    return true;
                return false;
            }
        }

    }
}