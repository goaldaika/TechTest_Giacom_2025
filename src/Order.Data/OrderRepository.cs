using Microsoft.EntityFrameworkCore;
using Order.Data.Entities;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Data
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync();
        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);
        Task<IEnumerable<OrderDetail>> GetOrderByStatus(int statusEnum);
        Task<OrderDetail> UpdateOrderStatus(Guid orderId, int updateStatus);
        Task<bool> CreateOrder(OrderDetail order);
        Task<IEnumerable<OrderProfit>> GetProfitOfCompletedOrderAsync(int? year = null);
        Task<IEnumerable<TotalProfit>> GetTotalProfitofEachMonth(int? year = null);
    }
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _orderContext;

        public OrderRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {

            var ltest = _orderContext.Database.IsInMemory();
            var orders = await _orderContext.Order
                .Include(x => x.Items)
                .Include(x => x.Status)
                .Select(x => new OrderSummary
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    ItemCount = x.Items.Count,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    CreatedDate = x.CreatedDate
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return orders;
        }
        public async Task<IEnumerable<OrderDetail>> GetOrderByStatus(int statusEnum)
        {
            if (!Enum.IsDefined(typeof(OrderStatusEnum), statusEnum))
            {
                throw new ArgumentException($"Invalid status enum value: {statusEnum}");
            }
            var status = (OrderStatusEnum)statusEnum;
            var statusName = status.ToString();

            var orders = await _orderContext.Order
                .Include(x => x.Items)
                    .ThenInclude(i => i.Product)
                .Include(x => x.Items)
                    .ThenInclude(i => i.Service)
                .Include(x => x.Status)
                .Where(x => x.Status != null && x.Status.Name == statusName)
                .Select(x => new OrderDetail
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    CreatedDate = x.CreatedDate,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost) ?? 0, 
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice) ?? 0, 
                    Items = x.Items.Select(i => new Model.OrderItem
                    {
                        Id = new Guid(i.Id),
                        OrderId = new Guid(i.OrderId),
                        ServiceId = new Guid(i.ServiceId),
                        ServiceName = i.Service.Name,
                        ProductId = new Guid(i.ProductId),
                        ProductName = i.Product.Name,
                        UnitCost = i.Product.UnitCost, 
                        UnitPrice = i.Product.UnitPrice, 
                        TotalCost = (i.Product.UnitCost) * (i.Quantity ?? 0),
                        TotalPrice = (i.Product.UnitPrice) * (i.Quantity ?? 0),
                        Quantity = i.Quantity ?? 0
                    })
                })
                .ToListAsync();

            return orders;
        }
        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var orderIdBytes = orderId.ToByteArray();
            
            var order = await _orderContext.Order
                .Where(x => _orderContext.Database.IsInMemory() ? x.Id.SequenceEqual(orderIdBytes) : x.Id == orderIdBytes)
                .Select(x => new OrderDetail
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    CreatedDate = x.CreatedDate,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    Items = x.Items.Select(i => new Model.OrderItem
                    {
                        Id = new Guid(i.Id),
                        OrderId = new Guid(i.OrderId),
                        ServiceId = new Guid(i.ServiceId),
                        ServiceName = i.Service.Name,
                        ProductId = new Guid(i.ProductId),
                        ProductName = i.Product.Name,
                        UnitCost = i.Product.UnitCost,
                        UnitPrice = i.Product.UnitPrice,
                        TotalCost = i.Product.UnitCost * i.Quantity.Value,
                        TotalPrice = i.Product.UnitPrice * i.Quantity.Value,
                        Quantity = i.Quantity.Value
                    })
                }).SingleOrDefaultAsync();
            return order;
        }
        public async Task<OrderDetail> UpdateOrderStatus(Guid orderId, int updateStatus)
        {

            var ltest = _orderContext.Database.IsInMemory();
            var orderIdBytes = orderId.ToByteArray();
            var orderToBeUpdated = _orderContext.Order
            .FirstOrDefault(x => _orderContext.Database.IsInMemory()
                            ? x.Id.SequenceEqual(orderIdBytes) : x.Id == orderIdBytes);
            if (orderToBeUpdated == null) return null;

            var status = GetStatusByEnum(updateStatus);

            orderToBeUpdated.StatusId = status.Id;

            _orderContext.Update(orderToBeUpdated);

            await _orderContext.SaveChangesAsync();

            return GetOrderByIdAsync(orderId).Result;
        }
        public async Task<bool> CreateOrder(OrderDetail order)
        {
            // Validate input
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order), "Order cannot be null.");
            }

            if (order.ResellerId == Guid.Empty)
            {
                throw new ArgumentException("ResellerId is required.", nameof(order.ResellerId));
            }

            if (order.CustomerId == Guid.Empty)
            {
                throw new ArgumentException("CustomerId is required.", nameof(order.CustomerId));
            }

            if (order.StatusId == Guid.Empty)
            {
                throw new ArgumentException("StatusId is required.", nameof(order.StatusId));
            }

            if (order.Items == null || !order.Items.Any())
            {
                throw new ArgumentException("Order must have at least one item.", nameof(order.Items));
            }

            // Validate StatusId
            var status = GetStatusByGuidAsync(order.StatusId);
            if (status == null)
            {
                throw new ArgumentException($"Status with ID {order.StatusId} not found.");
            }

            // Validate OrderItems
            foreach (var item in order.Items)
            {
                if (item.ProductId == Guid.Empty)
                {
                    throw new ArgumentException("ProductId is required for each order item.", nameof(item.ProductId));
                }

                if (item.ServiceId == Guid.Empty)
                {
                    throw new ArgumentException("ServiceId is required for each order item.", nameof(item.ServiceId));
                }

                if (item.Quantity <= 0)
                {
                    throw new ArgumentException("Quantity must be positive for each order item.", nameof(item.Quantity));
                }

                var product = await _orderContext.OrderProduct
                    .FirstOrDefaultAsync(p => p.Id.SequenceEqual(item.ProductId.ToByteArray()));
                if (product == null)
                {
                    throw new ArgumentException($"Product with ID {item.ProductId} not found.");
                }

                var service = await _orderContext.OrderService
                    .FirstOrDefaultAsync(s => s.Id.SequenceEqual(item.ServiceId.ToByteArray()));
                if (service == null)
                {
                    throw new ArgumentException($"Service with ID {item.ServiceId} not found.");
                }
            }

            var newOrder = new Entities.Order
            {
                Id = Guid.NewGuid().ToByteArray(),
                ResellerId = order.ResellerId.ToByteArray(),
                CustomerId = order.CustomerId.ToByteArray(),
                StatusId = order.StatusId.ToByteArray(),
                CreatedDate = DateTime.UtcNow,
                Items = order.Items.Select(i => new Entities.OrderItem
                {
                    Id = Guid.NewGuid().ToByteArray(),
                    OrderId = null, 
                    ProductId = i.ProductId.ToByteArray(),
                    ServiceId = i.ServiceId.ToByteArray(),
                    Quantity = i.Quantity
                }).ToList()
            };

            _orderContext.Order.Add(newOrder);

            foreach (var item in newOrder.Items)
            {
                item.OrderId = newOrder.Id;
            }

            await _orderContext.SaveChangesAsync();

            var savedOrder = await _orderContext.Order
                .Include(o => o.Status)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Service)
                .FirstOrDefaultAsync(o => o.Id.SequenceEqual(newOrder.Id));

            if(savedOrder != null) return true;

            return false;
        }
        public async Task<IEnumerable<OrderProfit>> GetProfitOfCompletedOrderAsync(int? year = null)
        {
            int targetYear = year ?? DateTime.UtcNow.Year;

            IEnumerable<Entities.Order> orderEntities = await _orderContext.Order
                .Include(o => o.Status)
                .ToListAsync();

            var completedOrderList = orderEntities
                .Where(o => o.Status != null && o.Status.Name == OrderStatusEnum.Completed.ToString() && o.CreatedDate.Year == targetYear);

            List<OrderDetail> completedOrderDetailList = new List<OrderDetail>();
            foreach (var order in completedOrderList)
            {
                var completedOrderDetail = await GetOrderByIdAsync(new Guid(order.Id));
                if (completedOrderDetail != null)
                {
                    completedOrderDetailList.Add(completedOrderDetail);
                }
            }

            var profits = completedOrderDetailList
                .SelectMany(o => o.Items, (o, i) => new
                {
                    OrderId = o.Id,
                    CreatedDate = o.CreatedDate,
                    Item = i
                })
                .GroupBy(
                    x => new { x.CreatedDate.Year, x.CreatedDate.Month },
                    x => new OrderProfit
                    {
                        Id = Guid.NewGuid(),
                        OrderId = x.OrderId,
                        ServiceId = x.Item.ServiceId,
                        ServiceName = x.Item.ServiceName,
                        ProductId = x.Item.ProductId,
                        ProductName = x.Item.ProductName,
                        TotalCost = x.Item.TotalCost,
                        TotalPrice = x.Item.TotalPrice,
                        CreatedDate = x.CreatedDate,
                        Profit = x.Item.TotalPrice - x.Item.TotalCost
                    })
                .SelectMany(g => g);

            return profits;
        }
        public async Task<IEnumerable<TotalProfit>> GetTotalProfitofEachMonth(int? year = null)
        {
            var orderProfits = await GetProfitOfCompletedOrderAsync(year);

            var totalProfits = orderProfits
                .GroupBy(p => p.CreatedDate.Month)
                .Select(g => new TotalProfit
                {
                    month = g.Key,
                    profit = g.Sum(p => p.Profit)
                })
                .OrderBy(t => t.month)
                .ToList();

            return totalProfits;
        }
        private OrderStatus GetStatusByEnum(int statusEnum)
        {
            if (!Enum.IsDefined(typeof(OrderStatusEnum), statusEnum))
            {
                throw new ArgumentException($"Invalid status enum value: {statusEnum}");
            }
            var status = (OrderStatusEnum)statusEnum;
            var statusName = status.ToString();
            var statusList = _orderContext.OrderStatus.ToList();
            var statusResult= statusList.FirstOrDefault(s => s.Name == statusName);
            return statusResult;
        }
        private OrderStatus GetStatusByGuidAsync(Guid statusGuid)
        {

            var statusIdBytes = statusGuid.ToByteArray();

            var statusList = _orderContext.OrderStatus.ToList();
            var statusResult = statusList.FirstOrDefault(x => x.Id.SequenceEqual(statusIdBytes));
            return statusResult;
        }

    }
}
