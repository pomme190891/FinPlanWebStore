﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using FinPlanWeb.DTOs;
using FinPlanWeb.Database;

namespace FinPlanWeb.Controllers
{
    public class AdminController : BaseController
    {
        // GET: /Admin/
        private const int pageSize = 10;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Filter(int isInactive)
        {
            var list = UserManagement.GetAllUserList();
            if (isInactive == 1)
            {
                list = list.Where(x => x.IsDeleted);
            }
            else if (isInactive == 0)
            {
                list = list.Where(x => x.IsDeleted == false);
            }
            var users = ApplyPaging(list, 1);

            var totalPage = (int)Math.Ceiling(((double)list.Count() / (double)pageSize));

            return Json(new { users, totalPage }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Paging(int num, int isInactive)
        {
            var list = UserManagement.GetAllUserList();
            if (isInactive == 1)
            {
                list = list.Where(x => x.IsDeleted);
            }
            else if (isInactive == 0)
            {
                list = list.Where(x => x.IsDeleted == false);
            }
            var users = ApplyPaging(list, num);

            return Json(users, JsonRequestBehavior.AllowGet);
        }

        public IEnumerable<UserManagement.User> ApplyPaging(IEnumerable<UserManagement.User> users, int num)
        {
            return users.Skip((num - 1) * pageSize)
                .Take(pageSize);
        }

        public ActionResult Dashboard()
        {
            PopulateUserAdministration();
            return View();
        }

        private void PopulateUserAdministration()
        {
            var allUsers = UserManagement.GetAllUserList();
            var filteredUsers = ApplyPaging(allUsers, 1);
            ViewBag.Users = new JavaScriptSerializer().Serialize(filteredUsers);
            ViewBag.TotalUsersPage = (int) Math.Ceiling(((double) allUsers.Count()/(double) pageSize));
        }

        public ActionResult AddUser(EditUserDTO user)
        {
            var validationIds = new List<string>();
            var validationMessage = Validate(user, true, out validationIds);
            if (!validationMessage.Any())
            {
                UserManagement.AddUser(user.ToUser());
            }

            return Json(new
            {
                users = UserManagement.GetAllUserList(),
                passed = !validationMessage.Any(),
                validationIds,
                validationMessage = string.Join("</br>", validationMessage)
            });
        }

        public ActionResult UserList()
        {
            return View();
        }

        public ActionResult UserUpdate(EditUserDTO user)
        {
            var validationIds = new List<string>();
            var validationMessage = Validate(user, false, out validationIds);
            if (!validationMessage.Any())
            {
                UserManagement.UpdateUser(user.ToUser());
            }

            return Json(new
            {
                users = UserManagement.GetAllUserList(),
                passed = !validationMessage.Any(),
                validationIds,
                validationMessage = string.Join("<br/>", validationMessage)
            });
        }


        public ActionResult DeleteUser(string username)
        {
            UserManagement.DeleteUser(username);
            return Json(new
            {
                users = UserManagement.GetAllUserList()
            });
        }

        public ActionResult ChangePassword(int userId, string newPassword)
        {
            var validationIds = new List<string>();
            var validationMessage = ValidatePassword(newPassword, out validationIds);
            if (!validationMessage.Any())
            {
                UserManagement.UpdateUserPassword(userId,newPassword);
            }

            return Json(new
            {
                passed = !validationMessage.Any(),
                validationIds,
                validationMessage = string.Join("</br>", validationMessage)
            });
        }

        public List<string> ValidatePassword(string newPassword, out List<string> invalidIds)
        {
            var validationMessage = new List<string>();
            var validationId = new List<string>();

            if (string.IsNullOrEmpty(newPassword))
            {
                validationMessage.Add("New password is empty. ");
                validationId.Add("NewPassword");
            }

            if (!string.IsNullOrEmpty(newPassword))
            {
                var regex = new Regex(@"^.*(?=.{6,})(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$");
                var match = regex.Match(newPassword);
                if (!match.Success)
                {
                    validationMessage.Add("Invalid Password. Password must contain at least a digit, a uppercase and a lowercase letter. Mininum 6 characters are required. ");
                    validationId.Add("NewPassword");
                }
            }

            invalidIds = validationId;
            return validationMessage;
        }

        public List<string> Validate(EditUserDTO user, bool isCreating, out List<string> invalidIds)
        {
            var validationMessage = new List<string>();
            var validationId = new List<string>();

            if (isCreating && string.IsNullOrEmpty(user.UserName))
            {
                validationMessage.Add("Username is empty.");
                validationId.Add("Username");
            }

            if (isCreating && !string.IsNullOrEmpty(user.UserName))
            {
                if (UserManagement.IsValidUsername(user.UserName))
                {
                    validationMessage.Add("Username already exists in the database.");
                    validationId.Add("Username");
                }
            }

            if (isCreating && !string.IsNullOrEmpty(user.Password) && (user.ConfirmPassword != user.Password))
            {
                validationMessage.Add("Confirm Password does not match with the password.");
                validationId.Add("ConfirmPassword");

            }

            if (isCreating && !string.IsNullOrEmpty(user.UserName))
            {
                if (UserManagement.IsValidUsername(user.UserName))
                {
                    validationMessage.Add("Username already exists in the database.");
                }
            }

            if (isCreating && !string.IsNullOrEmpty(user.Password) && (user.ConfirmPassword != user.Password))
            {
                validationMessage.Add("Confirm Password does not match with the password.");

            }

            if (isCreating && string.IsNullOrEmpty(user.Password))
            {
                validationMessage.Add("Password is empty.");
                validationId.Add("Password");
            }

            if (string.IsNullOrEmpty(user.FirstName))
            {
                validationMessage.Add("First Name is empty.");
                validationId.Add("Firstname");
            }

            if (string.IsNullOrEmpty(user.SurName))
            {
                validationMessage.Add("Surname is empty.");
                validationId.Add("Surname");
            }

            if (string.IsNullOrEmpty(user.FirmName))
            {
                validationMessage.Add("Firmname is empty.");
                validationId.Add("Firmname");
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                validationMessage.Add("Email is empty.");
                validationId.Add("Email");
            }
            else
            {

                var regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                var match = regex.Match(user.Email);
                if (!match.Success)
                {
                    validationMessage.Add("Invalid email format.");
                    validationId.Add("Email");
                }
            }

            if (isCreating && !string.IsNullOrEmpty(user.Password))
            {
                var regex = new Regex(@"^.*(?=.{6,})(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$");
                var match = regex.Match(user.Password);
                if (!match.Success)
                {
                    validationMessage.Add("Invalid Password. Password must contain at least a digit, a uppercase and a lowercase letter. Mininum 6 characters are required. ");
                    validationId.Add("Password");
                }
            }

            invalidIds = validationId;
            return validationMessage;
        }

        public ActionResult ResetPassword(UserManagement.User user)
        {
            return null;
        }
    }
}