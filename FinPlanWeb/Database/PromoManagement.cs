using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FinPlanWeb.Database
{
    public class PromoManagement
    {
        public class Promotion
        {
            public string CodeId { get; set; }
            public int Rate { get; set; }
            public double Price { get; set; }
        }


        public static string GetConnection()
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["standard"].ConnectionString;
        }

        public static Promotion GetPromotion(string code)
        {

            using (var connection = new SqlConnection(GetConnection()))
            {

                const string sql = @"SELECT * FROM [dbo].[promotion] where codeID = @c ";
                var cmd = new SqlCommand(sql, connection);

                cmd.Parameters

                          .Add(new SqlParameter("@c", SqlDbType.NVarChar))
                          .Value = code;

                connection.Open();
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    return new Promotion
                    {
                        CodeId = reader.GetString(0),
                        Rate = reader.GetInt32(1),
                        Price = reader.GetSqlMoney(2).ToDouble()

                    };
                }
                reader.Dispose();
                cmd.Dispose();
                return null;
            }


        }



    }
}