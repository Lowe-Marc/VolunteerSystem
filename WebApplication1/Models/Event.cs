using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace MVCFullCalendarDemo.Models
{
    public class Event
    {
        public int id { get; set; }

        public static int eventID = 0;
        public static int startTime = 0;

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateTime EndTime { get; set; }

        [Required]
        [Display(Name = "Total Shifts")]
        public int NumVolunteers { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Location")]
        public string Location { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        public static int staticID { get; set; }

        public List<WebApplication1.Models.Shift> shifts;

        public struct VolunteerIdentifier
        {
            public string firstName;
            public string lastName;
        }

        public struct EventTableDetails
        {
            public DataRowCollection shifts;
            public VolunteerIdentifier[] volunteers;
        }

        /*
         * Used for creating an event and submitting it to the database
         */
        public static bool submitEvent(string _title, DateTime _startDate, DateTime _endDate, int _totalShifts,
                                        string _location, string _description)
        {
            try
            {
                var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
                using (cn)
                {
                    string _sql = @"INSERT INTO Event (StartTime, EndTime, Description, NumVolunteers, Location, Title, Finalized) VALUES("
                        + "'" + _startDate + "', '" + _endDate + "', '" + _description + "', " + _totalShifts + ", '" + _location + "', '"
                        + _title + "', 0)";
                    var cmd = new SqlCommand(_sql, cn);
                    cn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    //if (rows > 0)
                    //    return true;
                    //return false;
                }
            }
            catch{
                return false;
            }
            return true;
        }

        /*
         * Queries database to determine if an event has been finalized by an admin yet or not
         * */
        public static bool eventIsFinalized(int eventID)
        {
            bool finalized = false;

            DataTable shiftTable = new DataTable();
            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            DataRowCollection rows;
            using (cn)
            {
                string _sql = @"SELECT Finalized FROM Event WHERE id = '" + eventID + "'";
                var cmd = new SqlCommand(_sql, cn);
                cn.Open();
                shiftTable.Load(cmd.ExecuteReader());
                rows = shiftTable.Rows;
                try
                {
                    finalized = Convert.ToBoolean(rows[0]["Finalized"]);
                }
                catch (InvalidCastException)
                {
                    //It's possible Finalized is null if the field was added to a database after it being populated
                    finalized = false;
                }
                cn.Close();
            }

            return finalized;
        }

        /*
         * Helper for getEventTableDetails, queries database for all shift info for a specific event
         */
        public static DataRowCollection getShiftsByEventID(int eventID)
        {
            DataTable shiftTable = new DataTable();
            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            DataRowCollection rows;
            using (cn)
            {
                string _sql = @"SELECT * FROM Shift WHERE EventID = '" + eventID + "'";
                var cmd = new SqlCommand(_sql, cn);
                cn.Open();
                shiftTable.Load(cmd.ExecuteReader());
                rows = shiftTable.Rows;
                cn.Close();
            }
            return rows;
        }

        /*
         * Used to populate the table in the event endpoint
         */
        public static EventTableDetails getEventTableDetails( int eventID )
        {
            EventTableDetails details = new EventTableDetails();
            DataRowCollection rows = MVCFullCalendarDemo.Models.Event.getShiftsByEventID(eventID);
            details.volunteers = new VolunteerIdentifier[rows.Count];
            details.shifts = rows;

            //Iterate through the list of shifts
            for( int i = 0; i < rows.Count; i++ )
            {
                DataRow row = rows[i];
                //Need to get the id of the volunteer for each shift and get the volunteers name
                //If there are no volunteers then the volunteers array will remain empty
                if (row["UserID"] != null)
                {
                    VolunteerIdentifier vi = getVolunteerByID((int)row["UserID"]);
                    details.volunteers[i] = vi;
                }
                
            }
            return details;
        }

        /*
         * Helper function to query the database for a volunteer based off their id
         **/
        private static VolunteerIdentifier getVolunteerByID( int id )
        {
            VolunteerIdentifier volunteer = new VolunteerIdentifier();
            DataTable userTable = new DataTable();
            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            DataRowCollection rows;
            using (cn)
            {
                string _sql = @"SELECT * FROM UserTable WHERE id = '" + id + "'";
                var cmd = new SqlCommand(_sql, cn);
                cn.Open();
                userTable.Load(cmd.ExecuteReader());
                rows = userTable.Rows;
                foreach( DataRow row in rows )
                {
                    volunteer.firstName = Convert.ToString(row["FirstName"]);
                    volunteer.lastName = Convert.ToString(row["LastName"]);
                }
                cn.Close();
            }
            return volunteer;
        }

        /*
         * Helper function to get back how many shifts at each time are required
         **/ 
         public static Dictionary<int,int> getPossibleShifts( int startTime, int endTime, int numVolunteers )
        {
            Dictionary<int, int> shifts = new Dictionary<int,int>();

            int shiftTime = startTime;
            while( shiftTime < endTime )
            {
                shifts.Add(shiftTime, numVolunteers);
                shiftTime += 2;
            }

            return shifts;
        }

        /*
         * Queries the database and populates/returns a copy of this Event Model based on eventID
         * */
        public static Event getEventModelByID(int eventID)
        {
            Event e = new Event();
            e.id = eventID;
            DataTable eventTable = new DataTable();
            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            DataRowCollection rows;
            using (cn)
            {
                string _sql = @"SELECT * FROM Event WHERE id = '" + eventID + "'";
                var cmd = new SqlCommand(_sql, cn);
                cn.Open();
                eventTable.Load(cmd.ExecuteReader());
                rows = eventTable.Rows;
                foreach (DataRow row in rows)
                {
                    e.Title = Convert.ToString(row["Title"]);
                    e.StartTime = Convert.ToDateTime(row["StartTime"]);
                    e.EndTime = Convert.ToDateTime(row["EndTime"]);
                    e.Location = Convert.ToString(row["Location"]);
                }
                cn.Close();
            }
            return e;
        }

        public static List<Tuple<string, int>> getVolunteerEmailsAndStartTimesByEventID(int eventID)
        {
            List<Tuple<string, int>> emailStartTimePairs = new List<Tuple<string, int>>();
            List<Tuple<int,int>> userIDStartTimePairs = new List<Tuple<int, int>>();
            DataTable eventTable = new DataTable();
            DataTable shiftTable = new DataTable();
            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            DataRowCollection rows;
            using (cn)
            {
                //Find the ID and starttime for each volunteer for the shift
                string _sql = @"SELECT * FROM Shift WHERE eventID = '" + eventID + "'";
                var cmd = new SqlCommand(_sql, cn);
                cn.Open();
                eventTable.Load(cmd.ExecuteReader());
                rows = eventTable.Rows;
                foreach (DataRow row in rows)
                {
                    userIDStartTimePairs.Add(new Tuple<int, int>(Convert.ToInt32(row["UserID"]), Convert.ToInt32(row["StartTime"])));
                }
                cn.Close();
            }

            cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            using (cn)
            {
                string emailQueryString = "";
                //Now we get the email for each volunteer
                for (int i = 0; i < userIDStartTimePairs.Count(); i++)
                {
                    if (i < userIDStartTimePairs.Count()-1)
                    {
                        emailQueryString = emailQueryString + "id = '" + userIDStartTimePairs[i].Item1 + "' OR ";
                    }
                    else
                    {
                        emailQueryString = emailQueryString + "id = '" + userIDStartTimePairs[i].Item1 + "'";
                    }
                }
                if (emailQueryString != "")
                {
                    string _sql = @"SELECT Email, id FROM UserTable WHERE " + emailQueryString;
                    var cmd = new SqlCommand(_sql, cn);
                    cn.Open();
                    shiftTable.Load(cmd.ExecuteReader());
                    rows = shiftTable.Rows;
                }
                foreach (DataRow row in rows)
                {
                    emailStartTimePairs.Add(new Tuple<string, int>(Convert.ToString(row["Email"]), Convert.ToInt32(row["id"])));
                }
                cn.Close();
            }
            return emailStartTimePairs;
        }

        /*
         * Submits a shift for a specific event into the database
         */
        public static bool submitShifts(string startDate, string endDate, int numVolunteers)
        {
            DateTime start = DateTime.ParseExact(startDate, "yyyy-mm-dd hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            DateTime end = DateTime.ParseExact(endDate, "yyyy-mm-dd hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            int startHour = start.Hour;
            int endHour = end.Hour;
            int numShifts = endHour - startHour;
            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            using (cn)
            {
                //string _sql = @"INSERT INTO Shift ( ";
            }

                return false;
        }

        /*
         * Deletes the event with eventID from the Event table
         * */
        public static void deleteEvent(int eventID)
        {
            Creating_a_custom_user_login_form.Models.User.CreateEmails(eventID, false);
            var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            using (cn)
            {
                string _sql1 = @"DELETE FROM Shift WHERE EventId = '" + eventID + "'";
                var cmd1 = new SqlCommand(_sql1, cn);
                cn.Open();
                cmd1.ExecuteNonQuery();
                cn.Close();
                string _sql = @"DELETE FROM Event WHERE id = '" + eventID + "'";
                var cmd = new SqlCommand(_sql, cn);
                cn.Open();
                cmd.ExecuteNonQuery();
                cn.Close();
            }
        }
    }

    public class EventContext : Context
    {
        public DbSet<Event> Events { get; set; }

        internal void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    
}