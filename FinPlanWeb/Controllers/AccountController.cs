using System.Web.Mvc;
using System.Web.Security;
using FinPlanWeb.Database;
using FinPlanWeb.DTOs;
using FinPlanWeb.Models;


namespace FinPlanWeb.Controllers
{
    public class AccountController : BaseController
    {
        //
        // GET: /User/

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult LogIn()
        {
            return View();
        }


        public ActionResult CustomerPage()
        {
            return View();
        }

        public ActionResult AdminPage()

        {
            var users = UserManagement.GetUserList();
            ViewBag.Users = users;
            return View();
        }


        public ActionResult ResetPassword()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Login(User user)
        {
            if (ModelState.IsValid)
            {
                if (UserManagement.IsValid(user.Username, user.Password))
                {
                    if (UserManagement.IsAdmin(user.Username, user.Password))
                    {
                        FormsAuthentication.SetAuthCookie(user.Username, user.RememberMe);
                        Session["User"] = new UserLoginDto { Username = user.Username };
                        return RedirectToAction("AdminPage", "Account");
                    }
                    FormsAuthentication.SetAuthCookie(user.Username, user.RememberMe);
                    Session["User"] = new UserLoginDto { Username = user.Username };
                    return RedirectToAction("CustomerPage", "Account");
                }
                ModelState.AddModelError("General", "Password is incorrect!");
            }
            return View(user);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]

        public ActionResult Registration(Register register)
        {
            if (ModelState.IsValid)
            {
                if (register.Password == register.ConfirmPassword)
                {
                    //UserManagement.AddUser(register.Username, register.Password, register.EmailAddress);
                    //register.Username = "";
                    //register.EmailAddress = "";
                    //ModelState.Clear();
                }
            }
            else
                ModelState.AddModelError("", "Missing some field(s)");
            return View("Register", register);
        }
    }
}
