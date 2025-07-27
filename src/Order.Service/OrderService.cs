using Order.Data;
using Order.Data.Entities;
using Order.Model;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Service
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync();

        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);
        Task<IEnumerable<OrderDetail>> GetOrderByStatus(int statusEnum);
        Task<OrderDetail> UpdateOrderStatus(Guid orderId, int updateStatusEnum);
        Task<bool> CreateOrder(OrderDetail order);
        Task<IEnumerable<OrderProfit>> GetProfitOfCompletedOrderAsync(int? year = null);
        Task<IEnumerable<TotalProfit>> GetTotalProfitofEachMonth(int? year = null);
    }
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {
            var orders = await _orderRepository.GetOrdersAsync();
            return orders;
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            return order;
        }

        public async Task<IEnumerable<OrderDetail>> GetOrderByStatus(int statusEnum)
        {
            var order = await _orderRepository.GetOrderByStatus(statusEnum);
            return order;
        }

        public async Task<OrderDetail> UpdateOrderStatus(Guid orderId, int updateStatusEnum)
        {
            var order = await _orderRepository.UpdateOrderStatus(orderId, updateStatusEnum);
            return order;
        }
        public async Task<bool> CreateOrder(OrderDetail order)
        {
            var orderResult = await _orderRepository.CreateOrder(order);
            return orderResult;
        }

        public async Task<IEnumerable<OrderProfit>> GetProfitOfCompletedOrderAsync(int? year = null)
        {
            var orderResult = await _orderRepository.GetProfitOfCompletedOrderAsync(year);
            return orderResult;
        }
        public async Task<IEnumerable<TotalProfit>> GetTotalProfitofEachMonth(int? year = null)
        {
            var orderResult = await _orderRepository.GetTotalProfitofEachMonth(year);
            return orderResult;
        }
    }
}
