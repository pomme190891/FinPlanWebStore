using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FinPlanWeb.Database;

namespace FinPlanWeb.Controllers
{
    public class AdminController : BaseController
    {
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult UserList()
        {
            var users = UserManagement.GetUserList();
            ViewBag.Users = users;
            return View();
        }

        public ActionResult UserUpdate()
        {
            return View();
        }

    }
}
