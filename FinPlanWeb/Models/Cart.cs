using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinPlanWeb.Models
{
    public class CartItem
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice
        {
            get
            {
                return UnitPrice * Quantity;
            }
        }
        public string UnitPriceInStr
        {
            get { return string.Format("{0:0.00}", UnitPrice);  }
        }
        public string TotalPriceInStr
        {
            get { return string.Format("{0:0.00}", TotalPrice); }
        }
    }
}