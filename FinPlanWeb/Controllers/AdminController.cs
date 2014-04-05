using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private const int pageSize = 10;

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Paging(int num)
        {
            var users = ApplyPaging(UserManagement.GetUserList(), num);
            return Json(users, JsonRequestBehavior.AllowGet);
        }

        public IEnumerable<UserManagement.User> ApplyPaging(IEnumerable<UserManagement.User> users, int num)
        {
            return users.Skip((num - 1) * pageSize)
                .Take(pageSize);
        }

        public ActionResult Dashboard()
        {
            var allUsers = UserManagement.GetUserList();
            var filteredUsers = ApplyPaging(allUsers, 1);
            ViewBag.Users = new JavaScriptSerializer().Serialize(filteredUsers);
            ViewBag.TotalUsersPage = (int)Math.Ceiling(((double)allUsers.Count() / (double)pageSize));
            return View();
        }


        public ActionResult AddUser(UserManagement.User user)
        {
            var validationMessage = Validate(user, true);
            if (!validationMessage.Any())
            {
                UserManagement.AddUser(user);
            }

            return Json(new
            {
                users = UserManagement.GetUserList(),
                passed = !validationMessage.Any(),
                validationMessage = string.Join("</br>", validationMessage)
            });
        }

        public ActionResult UserList()
        {
            return View();
        }

        public ActionResult UserUpdate(UserManagement.User user)
        {
            var validationMessage = Validate(user, false);
            if (!validationMessage.Any())
            {
                UserManagement.UpdateUser(user);
            }

            return Json(new
            {
                users = UserManagement.GetUserList(),
                passed = !validationMessage.Any(),
                validationMessage = string.Join("<br/>", validationMessage)
            });
        }

        public List<string> Validate(UserManagement.User user, bool isCreating)
        {
            var validationMessage = new List<string>();

            if (isCreating && string.IsNullOrEmpty(user.UserName))
            {
                validationMessage.Add("Username is empty.");
            }

            if (isCreating && string.IsNullOrEmpty(user.Password))
            {
                validationMessage.Add("Password is empty."); 
            }

            if (string.IsNullOrEmpty(user.FirstName))
            {
                validationMessage.Add("First Name is empty.");
            }

            if (string.IsNullOrEmpty(user.SurName))
            {
                validationMessage.Add("Surname is empty.");
            }

            if (string.IsNullOrEmpty(user.FirmName))
            {
                validationMessage.Add("Firmname is empty.");
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                validationMessage.Add("Email is empty.");
            }
            else
            {

                var regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                var match = regex.Match(user.Email);
                if (!match.Success)
                    validationMessage.Add("Invalid email format.");
            }

            if (isCreating && !string.IsNullOrEmpty(user.Password))
            {
                var regex = new Regex(@"^.*(?=.{6,})(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$");
                var match = regex.Match(user.Password);
                if (!match.Success)
                    validationMessage.Add("Invalid Password. Password must contain at least a digit, a uppercase and a lowercase letter. Mininum 6 characters are required. ");
            }


            return validationMessage;
        }

    }
}