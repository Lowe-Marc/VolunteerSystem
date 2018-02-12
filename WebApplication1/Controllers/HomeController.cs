using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace WebApplication1.Controllers
{
    public class IndexViewModel
    {
        public string url { get; set; }
        public int ID { get; set; }
        public static int i;
        public static int[] ids;
        public static int arrSize;

        public IndexViewModel( int[] idArr )
        {
            ids = idArr;
            arrSize = ids.Length;
        }
    }

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //Query DB for Events when page is loaded
            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            string _sql = @"SELECT * FROM Event";
            var cmd = new SqlCommand(_sql, cn);
            cn.Open();

            //Load results into table
            DataTable table = new DataTable();
            table.Load(cmd.ExecuteReader());
            cn.Close();

            int[] ids = new int[table.Rows.Count];
            string[] titles = new string[table.Rows.Count];
            string[] dates = new string[table.Rows.Count];
            DateTime tempDate;
            int index = 0;
            foreach (DataRow row in table.Rows )
            {
                ids[index] = (int)row["id"];
                titles[index] = Convert.ToString(row["Title"]);
                tempDate = Convert.ToDateTime(row["StartTime"]);
                //dates[index] = tempDate.ToString("yyyy-MM-dd HH:mm:ss");
                dates[index] = Convert.ToString(tempDate);
                index++;
            }

            ViewData["IDs"] = ids;
            ViewData["ArrSize"] = ids.Length;
            ViewData["Titles"] = titles;
            ViewData["Dates"] = dates;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Login()
        {
            ViewBag.Message = "Enter Your Login Credentials";

            return View();
        }

        public ActionResult CreateUser()
        {
            return View();
        }

        public ActionResult UserPage()
        {
            //Query for the users personal information
            string id = (string)Url.RequestContext.RouteData.Values["id"];
            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            string _sql = "SELECT * FROM UserTable WHERE id = " + id;
            var cmd = new SqlCommand(_sql, cn);
            cn.Open();
            //Load results into table
            DataTable table = new DataTable();
            table.Load(cmd.ExecuteReader());
            cn.Close();
            DataRow row = table.Rows[0];

            string userName = Convert.ToString(row["UserName"]);
            string firstName = Convert.ToString(row["FirstName"]);
            string lastName = Convert.ToString(row["LastName"]);
            string email = Convert.ToString(row["Email"]);
            string phone = Convert.ToString(row["Phone"]);
            ViewData["UserName"] = userName;
            ViewData["FirstName"] = firstName;
            ViewData["LastName"] = lastName;
            ViewData["Email"] = email;
            ViewData["Phone"] = phone;

            //Query for all the shifts the user is working
            cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            _sql = "SELECT * FROM Shift WHERE UserID = " + id;
            cmd = new SqlCommand(_sql, cn);
            cn.Open();
            //Load results into table
            table = new DataTable();
            table.Load(cmd.ExecuteReader());
            cn.Close();
            List<int> eventIDs = new List<int>();
            List<int> shiftTimes = new List<int>();
            foreach (DataRow r in table.Rows)
            {
                shiftTimes.Add(Convert.ToInt32(r["StartTime"]));
                eventIDs.Add(Convert.ToInt32(r["EventID"]));
            }

            //Query for all the events the user is working at
            cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            string eventQueryString = "";
            if (eventIDs.Count() != 0)
            {
                for (int i = 0; i < eventIDs.Count(); i++)
                {
                    if (i < eventIDs.Count() - 1)
                    {
                        eventQueryString = eventQueryString + "id = '" + eventIDs[i] + "' OR ";
                    }
                    else
                    {
                        eventQueryString = eventQueryString + "id = '" + eventIDs[i] + "'";
                    }
                }
                _sql = "SELECT * FROM Event WHERE " + eventQueryString;
                cmd = new SqlCommand(_sql, cn);
                cn.Open();
                //Load results into table
                table = new DataTable();
                table.Load(cmd.ExecuteReader());
                cn.Close();
            }
            //Collect all shift times and event names into a list of tuples for the view
            List<Tuple<DateTime, string>> shiftTimeEventNameTuples = new List<Tuple<DateTime, string>>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                shiftTimeEventNameTuples.Add(new Tuple<DateTime, string>((DateTime)table.Rows[i]["StartTime"], (string)table.Rows[i]["Title"]));
            }

            ViewData["ShiftTimeEventNameTuples"] = shiftTimeEventNameTuples;

            return View();
        }

        public ActionResult ManageUser()
        {
            List < Creating_a_custom_user_login_form.Models.User > allUsers = Creating_a_custom_user_login_form.Models.User.getAllUsers();
            ViewData["AllUsers"] = allUsers;

            return View();
        }

        [HttpPost]
        public ActionResult DeleteUser(string submit)
        {
            int userID = Int32.Parse(submit.Split(new char[0])[2]);
            Creating_a_custom_user_login_form.Models.User.staticID = userID;
            return View();
        }

        public ActionResult AddEvent()
        {
            return View();
        }

        public ActionResult DeleteEventConfirmation()
        {
            return View();
        }

        public ActionResult DeleteEvent()
        {
            MVCFullCalendarDemo.Models.Event.deleteEvent(MVCFullCalendarDemo.Models.Event.staticID);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Shifts(int id){
            List<Models.Shift> shifts = new List<Models.Shift>();

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {

                connection.Open();
                String sql = "Select * From Shift WHERE EventId = " + id;

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var s = new Models.Shift();
                            s.Id = Convert.ToInt32(reader["id"]);
                            s.EventId = Convert.ToInt32(reader["EventId"]);
                            s.UserId = Convert.ToInt32(reader["UserId"]);
                            s.StartTime = Convert.ToDateTime(reader["StartTime"]);
                            s.EndTime = s.StartTime.AddHours(2); //Convert.ToDateTime(reader["EndTime"]);
                            shifts.Add(s);
                        }
                    }
                }
            }


            return View();
        }

        public ActionResult SubmitEvent(string title, DateTime startTime, int numVolunteers, string location, string description)
        {
            DateTime endTime = startTime.AddHours(2);
            if(MVCFullCalendarDemo.Models.Event.submitEvent(title, startTime, endTime, numVolunteers, location, description)){
                return View();
            }
            return View("Home/AddEvent");
        }

        public ActionResult Event()
        {
            string id = (string)Url.RequestContext.RouteData.Values["id"];
            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            string _sql = @"SELECT * FROM Event WHERE id = " + id;
            var cmd = new SqlCommand(_sql, cn);
            cn.Open();
            //Load results into table
            DataTable table = new DataTable();
            table.Load(cmd.ExecuteReader());
            cn.Close();
            DataRow row = table.Rows[0];

            DateTime startTime = Convert.ToDateTime(row["StartTime"]);
            DateTime endTime = Convert.ToDateTime(row["EndTime"]);
            string description = Convert.ToString(row["description"]);
            int numVolunteers = Convert.ToInt32(row["NumVolunteers"]);
            string location = Convert.ToString(row["Location"]);
            string title = Convert.ToString(row["Title"]);

            ViewData["id"] = id;
            ViewData["Title"] = title;
            ViewData["StartTime"] = startTime;
            ViewData["EndTime"] = endTime;
            ViewData["Description"] = description;
            ViewData["NumVolunteers"] = numVolunteers;
            ViewData["Location"] = location;

            return View();
        }
    }
}