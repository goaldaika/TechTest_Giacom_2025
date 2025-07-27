using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Model
{
    public class OrderProfit
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal Profit { get; set; }
    }
}
