using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Data
{
    public enum OrderStatusEnum
    {
        Completed = 1,
        Created = 2,
        Failed = 3,
        InProgress = 4
    }
}
