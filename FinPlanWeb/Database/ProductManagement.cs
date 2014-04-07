using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Net;

namespace FinPlanWeb.Database
{

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public double Price { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string PriceInStr
        {
            get { return string.Format("{0:0.00}", Price); }
        }
    }

    public class ProductManagement
    {
        public static string GetConnection()
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["standard"].ConnectionString;
        }

        public enum ProductType
        {
            All = 0,
            Cloud,
            DataTransafer,
            ITSupport
        }


        public static Product GetProduct(string productCode)
        {

            using (var connection = new SqlConnection(GetConnection()))
            {

                const string sql = @"SELECT * FROM [dbo].[products] where productCode = @c ";
                var cmd = new SqlCommand(sql, connection);

                cmd.Parameters

                          .Add(new SqlParameter("@c", SqlDbType.NVarChar))
                          .Value = productCode;

                connection.Open();
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    return new Product
                     {
                         Id = reader.GetInt32(0),
                         Code = reader.GetString(1),
                         Name = reader.GetString(2),
                         AddedDate = reader.GetDateTime(3),
                         ModifiedDate = reader.IsDBNull(4) ? null : (DateTime?)reader.GetDateTime(4),
                         Price = reader.GetSqlMoney(5).ToDouble()
                     };
                }
                reader.Dispose();
                cmd.Dispose();
                return null;
            }





        }



        /// <summary>
        /// Get all Products
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>

        public static IEnumerable<Product> GetProducts(ProductType type)
        {

            var products = new List<Product>();
            using (var connection = new SqlConnection(GetConnection()))
            {

                var sql = @"SELECT * FROM [dbo].[products]";
                if (type != ProductType.All)
                {
                    sql += " WHERE categoriesID = @t";
                }
                var cmd = new SqlCommand(sql, connection);

                connection.Open();
                if (type != ProductType.All)
                {
                    cmd.Parameters
                     .Add(new SqlParameter("@t", SqlDbType.Int))
                     .Value = (int)type;
                }


                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var product = new Product
                        {
                            Id = reader.GetInt32(0),
                            Code = reader.GetString(1),
                            Name = reader.GetString(2),
                            AddedDate = reader.GetDateTime(3),
                            ModifiedDate = reader.IsDBNull(4) ? null : (DateTime?)reader.GetDateTime(4),
                            Price = reader.GetSqlMoney(5).ToDouble()

                        };

                        products.Add(product);
                    }
                }
                return products;
            }
        }
    }
}