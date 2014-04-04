using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;


namespace FinPlanWeb.Database
{
    public class UserManagement
    {

        public class User
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string FirstName { get; set; }
            public string SurName { get; set; }
            public string FirmName { get; set; }
            public string Password { get; set; }
            public DateTime AddedDate { get; set; }
            public string Email { get; set; }
            public bool IsAdmin { get; set; }
            public DateTime? LastLogin { get; set; }
            public string IpLog { get; set; }
            public DateTime? ModifiedDate { get; set; }
        }





        /// This part checks if the user with password is existing within the database 
        /// Taking the input of email address and password
        /// return if the data is existed or not
        public static string GetConnection()
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["standard"].ConnectionString;
        }

        /// <summary>
        /// Getting the IP address on the user's change for tracking purposes.
        /// </summary>
        /// <returns></returns>
        protected static string GetIp()
        {
            var strHostName = Dns.GetHostName();
            var ipEntry = Dns.GetHostEntry(strHostName);
            var ipaddress = Convert.ToString(ipEntry.AddressList[2]);
            return ipaddress;
        }


        /// <summary>
        /// Check whether the user exists...
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool IsValid(string username, string password)
        {


            using (var connection = new SqlConnection(GetConnection()))
            {


                const string sql = @"SELECT [Username] FROM [dbo].[users] WHERE [Username] = @u AND [Password] = @p";
                const string sql2 = @"UPDATE Users SET LastLogin = GETDATE() WHERE [Username] = @u AND [Password] =@p";
                const string sql3 = @"UPDATE [dbo].[users] SET iplog = @ip WHERE [Username] = @u AND [Password] =@p";


                var cmd = new SqlCommand(sql, connection);
                var cmd2 = new SqlCommand(sql2, connection);
                var cmd3 = new SqlCommand(sql3, connection);

                connection.Open();
                cmd.Parameters

                          .Add(new SqlParameter("@u", SqlDbType.NVarChar))
                          .Value = username;
                cmd.Parameters
                          .Add(new SqlParameter("@p", SqlDbType.NVarChar))
                          .Value = Helpers.SHA1.Encode(password);
                cmd2.Parameters
                          .Add(new SqlParameter("@u", SqlDbType.NVarChar))
                          .Value = username;
                cmd2.Parameters
                          .Add(new SqlParameter("@p", SqlDbType.NVarChar))
                          .Value = Helpers.SHA1.Encode(password);
                cmd3.Parameters
                          .Add(new SqlParameter("@u", SqlDbType.NVarChar))
                          .Value = username;
                cmd3.Parameters
                          .Add(new SqlParameter("@p", SqlDbType.NVarChar))
                          .Value = Helpers.SHA1.Encode(password);
                cmd3.Parameters.AddWithValue("@ip", GetIp());


                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {

                    reader.Dispose();
                    cmd.Dispose();
                    cmd2.ExecuteNonQuery();
                    cmd3.ExecuteNonQuery();
                    return true;
                }
                reader.Dispose();
                cmd.Dispose();
                return false;
            }
        }


        /// <summary>
        /// Check that the user is an admin or not
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// 
        public static bool IsAdmin(string username, string password)
        {
            using (var connection = new SqlConnection(GetConnection()))
            {
                const string sql = @"SELECT [isAdmin] FROM [dbo].[users] WHERE [Username] = @u AND [Password] = @p";
                var cmd = new SqlCommand(sql, connection);

                connection.Open();
                cmd.Parameters
                        .Add(new SqlParameter("@u", SqlDbType.NVarChar))
                        .Value = username;

                cmd.Parameters
                   .Add(new SqlParameter("@p", SqlDbType.NVarChar))
                   .Value = Helpers.SHA1.Encode(password);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return false;
                    var isAdmin = Convert.ToBoolean(reader["isAdmin"]);
                    return isAdmin;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<User> GetUserList()
        {
            var users = new List<User>();
            using (var connection = new SqlConnection(GetConnection()))
            {
                const string sql = @"SELECT * FROM [dbo].[users]";
                var cmd = new SqlCommand(sql, connection);
                connection.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new User
                        {
                            Id = reader.GetInt32(0),
                            UserName = reader.GetString(1),
                            FirstName = reader.GetString(2),
                            SurName = reader.GetString(3),
                            FirmName = reader.GetString(4),
                            Password = reader.GetString(5),
                            AddedDate = reader.GetDateTime(6),
                            Email = reader.GetString(7),
                            IsAdmin = reader.GetBoolean(8),
                            LastLogin = reader.IsDBNull(9) ? null : (DateTime?)reader.GetDateTime(9),
                            IpLog = reader.GetValue(10).ToString(),
                            ModifiedDate = reader.IsDBNull(11) ? null : (DateTime?)reader.GetDateTime(11)
                        };

                        users.Add(user);
                    }

                }

                return users;
            }
        }


  public static void AddUser(User user)
        {

            try
            {
                var con = new SqlConnection(GetConnection());
                var cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.Text,
                    CommandText =
                        "INSERT INTO [dbo].[users](Username, Firstname, Surname, Firm, Password, RegDate, Email, isAdmin) VALUES (@Username, @Firstname, @Surname, @Firmname, @Password, @RegDate, @Email, @IsAdmin)"
                };
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Username", user.UserName);
                cmd.Parameters.AddWithValue("@Firstname", user.FirstName);
                cmd.Parameters.AddWithValue("@Surname", user.SurName);
                cmd.Parameters.AddWithValue("@Firmname", user.FirmName);
                cmd.Parameters.AddWithValue("@Password", Helpers.SHA1.Encode(user.Password));
                cmd.Parameters.AddWithValue("@RegDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@IsAdmin", user.IsAdmin);
                
                if (con.State != ConnectionState.Closed) return;
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (SqlException ex)
            {
                var msg = "Insert errors";
                msg += ex.Message;
                throw new Exception(msg);
            }
        }

    }
}


