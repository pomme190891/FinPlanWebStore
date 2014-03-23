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
        public string Price { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        //public string Catergory { get; set; }
    }

    public class ProductManagement
    {
        public static string getConnection()
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

        public static IEnumerable<Product> GetProducts(ProductType type)
        {
            ///Establishing a connection db
            var products = new List<Product>();
            using (var connection = new SqlConnection(getConnection()))
            {
                
                var  _sql = @"SELECT * FROM [dbo].[products]";
                if (type != ProductType.All)
                {
                    _sql += " WHERE categoriesID = @t";
                }
                var cmd = new SqlCommand(_sql, connection);

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
                            ModifiedDate = reader.IsDBNull(4) ? null: (DateTime?)reader.GetDateTime(4) ,
                            Price = Convert.ToDecimal(String.Format("{0:0.00}", reader.GetSqlMoney(5).ToDecimal())).ToString()

                        };

                        products.Add(product);
                    }
                }
                return products;
            }
        }
    }
}