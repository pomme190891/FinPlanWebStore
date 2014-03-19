using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Net;
	
 	
 	namespace FinPlanWeb.Database
 	{
        public class UserManagement
        {


        /// This part checks if the user with password is existing within the database 
        /// Taking the input of email address and password
        /// return if the data is existed or not
        public static string getConnection()
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["standard"].ConnectionString;
        }
	       
         private static string GetIP()
	        {
	            string strHostName = "";
	            strHostName = System.Net.Dns.GetHostName();
	
	            IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
	
	            string ipaddress = Convert.ToString(ipEntry.AddressList[2]);
	
	            return ipaddress.ToString();
		       }
	
	
	
 	        public static bool isValid(string _username, string _password)
            {
 	        ///Establishing a connection db
	
 	            using (var connection = new SqlConnection(getConnection()))
 	            {
 	                string _sql = @"SELECT [Username] FROM [dbo].[users] WHERE [Username] = @u AND [Password] = @p";
	                string _sql2 = @"UPDATE Users SET LastLogin = GETDATE() WHERE [Username] = @u AND [Password] =@p";
	                string _sql3 = @"UPDATE [dbo].[users] SET iplog = @ip WHERE [Username] = @u AND [Password] =@p";
	
	
 	                var cmd = new SqlCommand(_sql, connection);
	                var cmd2 = new SqlCommand(_sql2, connection);
	                var cmd3 = new SqlCommand(_sql3, connection);
 	
 	                connection.Open();
 	                cmd.Parameters

	                          .Add(new SqlParameter("@u", SqlDbType.NVarChar))
	                          .Value = _username;
 	                cmd.Parameters
	                          .Add(new SqlParameter("@p", SqlDbType.NVarChar))
	                          .Value = Helpers.SHA1.Encode(_password);
	                cmd2.Parameters
	                          .Add(new SqlParameter("@u", SqlDbType.NVarChar))
	                          .Value = _username;
	                cmd2.Parameters
	                          .Add(new SqlParameter("@p", SqlDbType.NVarChar))
	                          .Value = Helpers.SHA1.Encode(_password);
	                cmd3.Parameters
	                          .Add(new SqlParameter("@u", SqlDbType.NVarChar))
	                          .Value = _username;
	                cmd3.Parameters
	                          .Add(new SqlParameter("@p", SqlDbType.NVarChar))
	                          .Value = Helpers.SHA1.Encode(_password);
	                cmd3.Parameters.AddWithValue("@ip", GetIP());
	
 	
 	                var reader = cmd.ExecuteReader();
 	                if (reader.HasRows)
                    {
 	
 	                    reader.Dispose();
 	                    cmd.Dispose();
	                    cmd2.ExecuteNonQuery();
	                    cmd3.ExecuteNonQuery();
 	                    return true;
 	                }
                else
                {
                   reader.Dispose();
                    cmd.Dispose();
                    return false;
            }

        }
     }
            /// <summary>
            /// Check that the user is an admin or not
            /// </summary>
            /// <param name="_username"></param>
            /// <param name="_password"></param>
            /// <returns></returns>
            /// 

            public static bool isAdmin(string _username, string _password)
            {
                using (var connection = new SqlConnection(getConnection()))
                {
                    string _sql = @"SELECT [isAdmin] FROM [dbo].[users] WHERE [Username] = @u AND [Password] = @p";
                    var cmd = new SqlCommand(_sql, connection);

                    connection.Open();
                    cmd.Parameters
                            .Add(new SqlParameter("@u", SqlDbType.NVarChar))
                            .Value = _username;

                    cmd.Parameters
                       .Add(new SqlParameter("@p", SqlDbType.NVarChar))
                       .Value = Helpers.SHA1.Encode(_password);

                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {

                        bool IsAdmin = Convert.ToBoolean(reader["IsAdmin"]);

                        if (IsAdmin == true)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    } return false;
                }

            }

            public static void ExecuteInsert(string Username, string Password, string Email)
            {

                try
                {
                    SqlConnection con = new SqlConnection(getConnection());
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT INTO [dbo].[users](Username,Password,Email, isAdmin) VALUES (@Username, @Password, @Email, @IsAdmin)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@Username", Username);
                    cmd.Parameters.AddWithValue("@Password", Helpers.SHA1.Encode(Password));
                    cmd.Parameters.AddWithValue("@Email", Email);
                    cmd.Parameters.AddWithValue("@IsAdmin", 0);
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }

                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    string msg = "Insert errors";
                    msg += ex.Message;
                    throw new Exception(msg);
                }




            }


        }
    }

    
