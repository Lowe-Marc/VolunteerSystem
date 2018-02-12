using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GlobalNamespace
{
    public class Global
    {
        public static string loggedInUser = "";
        public static bool isLoggedIn = false;
        public static bool userIsManager = false;
        public static bool userIsAdmin = false;
        public static int userID = 0;
    }
}