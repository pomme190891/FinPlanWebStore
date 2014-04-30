using FinPlanWeb.Database;

namespace FinPlanWeb.DTOs
{
    public class EditUserDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string SurName { get; set; }
        public string FirmName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public string ConfirmPassword { get; set; }

        public UserManagement.User ToUser()
        {
            return new UserManagement.User
                {
                    Id = Id,
                    UserName = UserName,
                    FirstName = FirstName,
                    SurName = SurName,
                    FirmName = FirmName,
                    Password = Password,
                    Email = Email,
                    IsAdmin = IsAdmin
                };
        }
    }
}