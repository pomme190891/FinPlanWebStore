using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using FinPlanWeb.DTOs;
using FinPlanWeb.Database;

namespace FinPlanWeb.Controllers
{
    public class OrderManagementController : BaseController
    {
        private const int pageSize = 10;

        /// <summary>
        /// Searching for Order Records
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="paymentType"></param>
        /// <param name="firm"></param>
        /// <returns></returns>
        public ActionResult Search(string from, string to, string paymentType, string firm)
        {
            var users = UserManagement.GetAllUserList();
            var orders = OrderManagement.GetAllOrders();
            var filteredOrders = SearchFunction(@from, to, paymentType, firm, orders, users, 1);

            var orderList = PopulateOrderList(filteredOrders, users);
            var totalPage = (int)Math.Ceiling(((double)orders.Count() / (double)pageSize));
            return Json(new { orderList, totalPage }, JsonRequestBehavior.AllowGet); //Allow JSON for $.get
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="paymentType"></param>
        /// <param name="firm"></param>
        /// <param name="orders"></param>
        /// <param name="users"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        private static IEnumerable<OrderManagement.Order> SearchFunction(string @from, string to, string paymentType, string firm, IEnumerable<OrderManagement.Order> orders,
                                                  IEnumerable<UserManagement.User> users, int pageNum)
        {
            if (!string.IsNullOrEmpty(@from))
            {
                var fromDate = DateTime.ParseExact(@from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                orders = orders.Where(x => x.DateCreated >= fromDate);
            }
            if (!string.IsNullOrEmpty(to))
            {
                var toDate = DateTime.ParseExact(to, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                orders = orders.Where(x => x.DateCreated <= toDate);
            }
            if (!string.IsNullOrEmpty(paymentType))
            {
                orders = orders.Where(x => x.PaymentType == paymentType);
            }
            if (!string.IsNullOrEmpty(firm))
            {
                var firmUser = users.Where(x => x.FirmName == firm).Select(x => x.Id);
                orders = orders.Where(x => firmUser.Contains(x.UserId));
            }

            return orders.Skip((pageNum - 1) * pageSize)
                .Take(pageSize);
        }


        /// <summary>
        /// Get all the objects that convert(serilalise) into
        /// JSON Object String 
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderHistory()
        {
            var serializer = new JavaScriptSerializer();
            var users = UserManagement.GetAllUserList();
            var orders = OrderManagement.GetAllOrders();
            var filteredOrders = SearchFunction(null, null, null, null, orders, users, 1);
            var orderList = PopulateOrderList(filteredOrders, users);
            ViewBag.Orders = serializer.Serialize(orderList);
            ViewBag.TotalOrdersPage = (int)Math.Ceiling(((double)orders.Count() / (double)pageSize)); ;
            ViewBag.OrderDetail = serializer.Serialize(new OrderDetailDTO());
            return View();
        }
        

        /// <summary>
        /// Return Order details that matches the orderID
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public ActionResult GetFullOrderDetail(int orderId)
        {
            var order = OrderManagement.GetAllOrders().Single(x => x.Id == orderId); //find a Single record that matches orderID 
            var orderItem = OrderManagement.GetOrderItems(orderId);
            var user = UserManagement.GetAllUserList().Single(x => x.Id == order.UserId);
            var products = ProductManagement.GetProducts(ProductManagement.ProductType.All)
                .Where(x => IdInList(x.Id, orderItem.Select(o => o.ProductId).ToArray())); //get products within in Orderitems collection.

            var orderDetail = new OrderDetailDTO
                {
                    Id = order.Id,
                    BuyerFirmName = user.FirmName,
                    BuyerUsername = user.UserName,
                    Currency = order.Currency,
                    DirectDebitId = order.DirectDebitId,
                    FirstName = order.FirstName,
                    Gross = order.Gross,
                    LastName = order.LastName,
                    Email = order.PayerEmail,
                    PaymentStatus = order.PaymentStatus,
                    PaymentType = order.PaymentType,
                    PaypalId = order.PaypalId,
                    PromotionCodeId = order.CodeId,
                    TransactionDate = order.DateCreated.ToString("dd/MM/yyyy")
                };

            foreach (var item in orderItem)
            {
                var product = products.SingleOrDefault(x => x.Id == item.ProductId);
                orderDetail.OrderItems.Add(new OrderItemDTO
                    {
                        ProductName = product.Name,
                        ProductCode = product.Code,
                        Quantity = item.Quantity
                    });
            }
            return Json(new {orderDetail}, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// This method checks whether the id is in the collection of ids.
        /// If yes, return true or another return false.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        private bool IdInList(int id, IEnumerable<int> ids)
        {
            return ids.Any(i => i == id);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="users"></param>
        /// <returns></returns>
        private static List<OrderDTO> PopulateOrderList(IEnumerable<OrderManagement.Order> orders, IEnumerable<UserManagement.User> users)
        {
            var orderList = new List<OrderDTO>();

            foreach (var order in orders)
            {
                var user = users.Single(x => x.Id == order.UserId);
                orderList.Add(new OrderDTO
                    {
                        Id = order.Id,
                        BuyerFirmName = user.FirmName,
                        BuyerUsername = user.UserName,
                        Currency = order.Currency,
                        PaymentType = order.PaymentType,
                        Name = user.FirstName + " " + user.SurName,
                        TransactionDate = order.DateCreated.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                    });
            }
            return orderList;
        }

        /// <summary>
        /// Pagination
        /// </summary>
        /// <param name="pageNum"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="paymentType"></param>
        /// <param name="firm"></param>
        /// <returns></returns>
        public ActionResult Paging(int pageNum, string from, string to, string paymentType, string firm)
        {
            var users = UserManagement.GetAllUserList();
            var orders = OrderManagement.GetAllOrders();
            var filteredOrders = SearchFunction(@from, to, paymentType, firm, orders, users, pageNum);

            var orderList = PopulateOrderList(filteredOrders, users);
            return Json(new { orderList }, JsonRequestBehavior.AllowGet); //Return JSON object.
        }
    }
}
