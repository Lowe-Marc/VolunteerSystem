using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Creating_a_custom_user_login_form.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult TakeShift(string submit)
        {
            int eventID = MVCFullCalendarDemo.Models.Event.eventID;
            int shiftTime = Int32.Parse(submit.Split(new char[0])[3]);
            Creating_a_custom_user_login_form.Models.User.takeShift(eventID, shiftTime);
            return RedirectToAction("Event/"+eventID, "Home");
        }

        [HttpPost]
        public ActionResult FinalizeEvent()
        {
            int eventID = MVCFullCalendarDemo.Models.Event.eventID;
            int shiftTime = MVCFullCalendarDemo.Models.Event.startTime;
            Creating_a_custom_user_login_form.Models.User.finalizeEvent(eventID);

            return RedirectToAction("Event/" + eventID, "Home");
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Models.User user)
        {
            if (ModelState.IsValid)
            {
                if(user.UserName != "sysadmin"){
                    char[] temp = user.Password.ToCharArray();
                    for (int i = 0; i < user.Password.Length; i++){
                        temp[i] = Convert.ToChar(temp[i] <<1);
                    }
                    string s = new string(temp);
                    user.Password = s;
                }
                    
                if (user.IsValid(user.UserName, user.Password))
                {
                    GlobalNamespace.Global.isLoggedIn = true;
                    GlobalNamespace.Global.loggedInUser = user.UserName;
                    GlobalNamespace.Global.userID = Creating_a_custom_user_login_form.Models.User.getUserIDByUserName(user.UserName);
                    FormsAuthentication.SetAuthCookie(user.UserName, user.RememberMe);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Login data is incorrect!");
                    return RedirectToAction("Login", "Home");
                }
            }
            return RedirectToAction("Index", "Home");
        }
        public ActionResult Logout()
        {
            GlobalNamespace.Global.isLoggedIn = false;
            GlobalNamespace.Global.loggedInUser = "";
            GlobalNamespace.Global.userIsAdmin = false;
            GlobalNamespace.Global.userIsManager = false;
            GlobalNamespace.Global.userID = 0;
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult CreateUser(String username, string password, string confirmpassword, bool isAdmin, bool isManager,
                                      string firstName, string lastName, string phone, string email)
        {
            if (!password.Equals(confirmpassword))
            {
                return RedirectToAction("CreateUser", "Home");
            }
            else
            {
                Create_User.Models.CreateUser.submitUser(username, password, isAdmin, isManager, firstName, lastName, phone, email);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult DeleteUser()
        {
            Creating_a_custom_user_login_form.Models.User.deleteUser(Creating_a_custom_user_login_form.Models.User.staticID);
            return RedirectToAction("ManageUser", "Home");
        }

        [HttpPost]
        public ActionResult PromoteUserToManager(string submit)
        {
            int userID = Int32.Parse(submit.Split(new char[0])[2]);
            Creating_a_custom_user_login_form.Models.User.promoteUserToManager(userID);
            return RedirectToAction("ManageUser", "Home");
        }

        [HttpPost]
        public ActionResult PromoteUserToAdmin(string submit)
        {
            int userID = Int32.Parse(submit.Split(new char[0])[2]);
            Creating_a_custom_user_login_form.Models.User.promoteUserToManager(userID);
            return RedirectToAction("ManageUser", "Home");
        }
    }
}
