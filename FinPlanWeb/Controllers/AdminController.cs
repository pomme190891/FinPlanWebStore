using System;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using FinPlanWeb.Database;

namespace FinPlanWeb.Controllers
{
    public class AddUserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string SurName { get; set; }
        public string FirmName { get; set; }
        public DateTime AddedDate { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
    }
    public class AdminController : BaseController
    {
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            return View();

        }


        public ActionResult AddUser(UserManagement.User user)
        {
            UserManagement.AddUser(user);
            return Json(UserManagement.GetUserList());
        }

        public ActionResult UserList()
        {
            var users = UserManagement.GetUserList();
            ViewBag.Users = new JavaScriptSerializer().Serialize(users);
            return View();
        }

        public ActionResult UserUpdate()
        {
            return View();
        }

    }
}