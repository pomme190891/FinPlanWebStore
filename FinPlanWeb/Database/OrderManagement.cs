using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FinPlanWeb.Models;


namespace FinPlanWeb.Database
{
    public class OrderManagement
    {
        public class OrderItem
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public int OrderId { get; set; }
        }

        public class Order
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string PayerEmail { get; set; }
            public DateTime PaymentDate { get; set; }
            public string PaymentStatus { get; set; }
            public decimal Gross { get; set; }
            public decimal Currency { get; set; }
            public DateTime DateCreated { get; set; }
            public string PaypalId { get; set; }
            public string DirectDebitId { get; set; }
            public string CodeId { get; set; }

            public string PaymentType { get; set; }
        }

        public static string GetConnection()
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["standard"].ConnectionString;
        }

        /// <summary>
        /// Get all order records with a particular id.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static IEnumerable<OrderItem> GetOrderItems(int orderId)
        {
            var orderItems = new List<OrderItem>();
            using (var connection = new SqlConnection(GetConnection()))
            {
                const string sql =
                    @"SELECT [ID]
                      ,[ProductID]
                      ,[qty]
                      ,[orderID]
                      FROM [finplanweb].[dbo].[orderItems]" +
                      " WHERE orderId = @orderId";
                var cmd = new SqlCommand(sql, connection);
                cmd.Parameters
                     .Add(new SqlParameter("@orderId", SqlDbType.Int))
                     .Value = orderId;
                connection.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var order = new OrderItem
                        {
                            Id = reader.GetInt32(0),
                            ProductId = reader.GetInt32(1),
                            Quantity = reader.GetInt32(2),
                            OrderId = reader.GetInt32(2)
                        };
                        orderItems.Add(order);
                    }
                }
                return orderItems;
            }
        }

        /// <summary>
        /// Get all order records.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Order> GetAllOrders()
        {
            var orders = new List<Order>();
            using (var connection = new SqlConnection(GetConnection()))
            {
                const string sql =
                    @"SELECT [orderID]
                      ,[userID]
                      ,[firstName]
                      ,[lastName]
                      ,[payerEmail]
                      ,[paymentDate]
                      ,[paymentStatus]
                      ,[paymentType]
                      ,[mcGross]
                      ,[mcCurrency]
                      ,[dateCreated]
                      ,[paypalID]
                      ,[directdebitID]
                      ,[codeID]
                        FROM [finplanweb].[dbo].[orders]";
                var cmd = new SqlCommand(sql, connection);
                connection.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var order = new Order
                        {
                            Id = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            FirstName = reader.GetString(2),
                            LastName = reader.GetString(3),
                            PayerEmail = reader.GetString(4),
                            PaymentDate = reader.GetDateTime(5),
                            PaymentStatus = reader.GetString(6),
                            PaymentType = reader.GetString(7),
                            Gross = reader.GetSqlMoney(8).ToDecimal(),
                            Currency = reader.GetSqlMoney(9).ToDecimal(),
                            DateCreated = reader.GetDateTime(10),
                            PaypalId = reader.IsDBNull(11) ? null : reader.GetString(11),
                            DirectDebitId = reader.IsDBNull(12) ? null : reader.GetString(12),
                            CodeId = reader.IsDBNull(13) ? null : reader.GetString(13),
                        };

                        orders.Add(order);
                    }

                }

                return orders;
            }
        }

        /// <summary>
        ///Record PayPal transaction to the database
        /// </summary>
        /// <param name="checkout"></param>
        /// <param name="cart"></param>
        /// <param name="paypalid"></param>
        /// <param name="userid"></param>
        public static void RecordPayPalTransaction(Checkout checkout, List<CartItem> cart, string paypalid, int userid)
        {
            SqlTransaction transaction = null;
            try
            {
                var con = new SqlConnection(GetConnection());
                con.Open();
                transaction = con.BeginTransaction();
                var cmd = new SqlCommand
                {
                    Transaction = transaction,
                    Connection = con,
                    CommandType = CommandType.Text,
                    CommandText =
                        "INSERT INTO [dbo].[orders](userID, firstName, lastName, payerEmail, paymentDate, paymentStatus, paymentType, mcGross, mcCurrency, dateCreated, paypalID) " +
                        " OUTPUT INSERTED.orderID " +
                        "VALUES (@uID, @Firstname, @Surname, @email, @date, @status, @type, @gross, @net, @dateCreated, @pID)"
                };
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@uID", userid);
                cmd.Parameters.AddWithValue("@Firstname", checkout.BillingInfo.FirstName);
                cmd.Parameters.AddWithValue("@Surname", checkout.BillingInfo.SurName);
                cmd.Parameters.AddWithValue("@email", checkout.BillingInfo.Email);
                cmd.Parameters.AddWithValue("@date", DateTime.Now);
                cmd.Parameters.AddWithValue("@status", "Success");
                cmd.Parameters.AddWithValue("@type", checkout.PaymentInfo.IsDirectDebit ? "Direct Debit" : "Paypal");
                cmd.Parameters.AddWithValue("@gross", CalculateGross(cart));
                cmd.Parameters.AddWithValue("@net", CalculateNet(cart));
                cmd.Parameters.AddWithValue("@dateCreated", DateTime.Now);
                cmd.Parameters.AddWithValue("@pID", paypalid);
                var orderId = (Int32)cmd.ExecuteScalar();

                foreach (var item in cart)
                {
                    var insertOrderItemCmd = new SqlCommand
                    {
                        Transaction = transaction,
                        Connection = con,
                        CommandType = CommandType.Text,
                        CommandText =
                            "INSERT INTO [dbo].[orderItems](ProductID, qty, orderID) " +
                            "VALUES (@pID, @q, @oID)"
                    };
                    insertOrderItemCmd.Parameters.Clear();
                    insertOrderItemCmd.Parameters.AddWithValue("@pID", item.Id);
                    insertOrderItemCmd.Parameters.AddWithValue("@q", item.Quantity);
                    insertOrderItemCmd.Parameters.AddWithValue("@oID", orderId);
                    insertOrderItemCmd.ExecuteNonQuery();
                }

                transaction.Commit();

                if (con.State != ConnectionState.Closed) return;
                con.Close();
            }
            catch (SqlException ex)
            {
                transaction.Rollback();
                var msg = "Insert errors";
                msg += ex.Message;
                throw new Exception(msg);
            }

        }

        /// <summary>
        /// Record DirectDebit Transaction to the database
        /// </summary>
        /// <param name="checkout"></param>
        /// <param name="cart"></param>
        /// <param name="userid"></param>
        /// <param name="orderId"></param>
        public static void RecordDirectDebitTransaction(Checkout checkout, List<CartItem> cart, int userid, out int orderId)
        {
            SqlTransaction transaction = null;
            try
            {
                var con = new SqlConnection(GetConnection());
                con.Open();
                transaction = con.BeginTransaction();
                var cmd = new SqlCommand
                {
                    Transaction = transaction,
                    Connection = con,
                    CommandType = CommandType.Text,
                    CommandText =
                        "INSERT INTO [dbo].[orders](userID, firstName, lastName, payerEmail, paymentDate, paymentStatus, paymentType, mcGross, mcCurrency, dateCreated, directdebitID) " +
                        " OUTPUT INSERTED.orderID " +
                        "VALUES (@uID, @Firstname, @Surname, @email, @date, @status, @type, @gross, @net, @dateCreated, @ddID)"
                };
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@uID", userid);
                cmd.Parameters.AddWithValue("@Firstname", checkout.BillingInfo.FirstName);
                cmd.Parameters.AddWithValue("@Surname", checkout.BillingInfo.SurName);
                cmd.Parameters.AddWithValue("@email", checkout.BillingInfo.Email);
                cmd.Parameters.AddWithValue("@date", DateTime.Now);
                cmd.Parameters.AddWithValue("@status", "Success");
                cmd.Parameters.AddWithValue("@type", checkout.PaymentInfo.IsDirectDebit ? "Direct Debit" : "Paypal");
                cmd.Parameters.AddWithValue("@gross", CalculateGross(cart));
                cmd.Parameters.AddWithValue("@net", CalculateNet(cart));
                cmd.Parameters.AddWithValue("@dateCreated", DateTime.Now);
                cmd.Parameters.AddWithValue("@ddID", CreateDirectDebitId(17));//generate random id.
                orderId = (Int32)cmd.ExecuteScalar();

                foreach (var item in cart)
                {
                    var insertOrderItemCmd = new SqlCommand
                    {
                        Transaction = transaction,
                        Connection = con,
                        CommandType = CommandType.Text,
                        CommandText =
                            "INSERT INTO [dbo].[orderItems](ProductID, qty, orderID) " +
                            "VALUES (@pID, @q, @oID)"
                    };
                    insertOrderItemCmd.Parameters.Clear();
                    insertOrderItemCmd.Parameters.AddWithValue("@pID", item.Id);
                    insertOrderItemCmd.Parameters.AddWithValue("@q", item.Quantity);
                    insertOrderItemCmd.Parameters.AddWithValue("@oID", orderId);
                    insertOrderItemCmd.ExecuteNonQuery();
                }

                transaction.Commit();
                if (con.State != ConnectionState.Closed) return;
                con.Close();
            }
            catch (SqlException ex)
            {
                transaction.Rollback();
                var msg = "Insert errors";
                msg += ex.Message;
                throw new Exception(msg);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private static string CreateDirectDebitId(int length)
        {
            var valid = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var res = "";
            var rnd = new Random();
            while (0 < length--)
                res += valid[rnd.Next(valid.Length)];
            return res;
        }

        private static decimal CalculateNet(List<CartItem> cart)
        {
            var gross = Convert.ToDecimal(cart.Select(x => x.TotalPrice).Sum());
            return gross * 120 / 100;
        }

        private static decimal CalculateGross(List<CartItem> cart)
        {
            return Convert.ToDecimal(cart.Select(x => x.TotalPrice).Sum());
        }
    }
}