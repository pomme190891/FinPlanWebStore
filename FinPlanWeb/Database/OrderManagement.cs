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
        public static string GetConnection()
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["standard"].ConnectionString;
        }

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
                cmd.Parameters.AddWithValue("@type", checkout.PaymentInfo.IsDirectDebit ?  "Direct Debit" : "Paypal");
                cmd.Parameters.AddWithValue("@gross", CalculateGross(cart) );
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

        public static void RecordDirectDebitTransaction(Checkout checkout, List<CartItem> cart, int userid)
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
                cmd.Parameters.AddWithValue("@ddID", CreateDirectDebitId(17));
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
            return gross*120/100;
        }

        private static decimal CalculateGross(List<CartItem> cart)
        {
            return Convert.ToDecimal(cart.Select(x => x.TotalPrice).Sum());
        }
    }
}