using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NUnit.Framework;
using Order.Data;
using Order.Data.Entities;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Service.Tests
{
    public class OrderServiceTests
    {
        private IOrderService _orderService;
        private IOrderRepository _orderRepository;
        private OrderContext _orderContext;
        private DbConnection _connection;

        private readonly byte[] _orderStatusCompletedId = Guid.NewGuid().ToByteArray();
        private readonly byte[] _orderStatusCreatedId = Guid.NewGuid().ToByteArray();
        private readonly byte[] _orderStatusFailedId = Guid.NewGuid().ToByteArray();
        private readonly byte[] _orderStatusInProgressId = Guid.NewGuid().ToByteArray();
        private readonly byte[] _orderServiceEmailId = Guid.NewGuid().ToByteArray();
        private readonly byte[] _orderProductEmailId = Guid.NewGuid().ToByteArray();


        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<OrderContext>()
                .UseSqlite(CreateInMemoryDatabase())
                .EnableDetailedErrors(true)
                .EnableSensitiveDataLogging(true)
                .Options;

            _connection = RelationalOptionsExtension.Extract(options).Connection;

            _orderContext = new OrderContext(options);
            _orderContext.Database.EnsureDeleted();
            _orderContext.Database.EnsureCreated();
            _orderRepository = new OrderRepository(_orderContext);
            _orderService = new OrderService(_orderRepository);
           

            await AddReferenceDataAsync(_orderContext);
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Dispose();
            _orderContext.Dispose();
        }


        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            return connection;
        }
        #region Task 1
        [Test]
        public async Task GetOrderByStatus_ReturnCorrectOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            await AddOrder(orderId, 1);
            var statusEnum = (int)OrderStatusEnum.Completed;

            // Act
            var orders = await _orderService.GetOrderByStatus(statusEnum);

            // Assert
            Assert.AreEqual(1, orders.Count());
            var order = orders.First();
            Assert.AreEqual(orderId, order.Id);
            Assert.AreEqual(OrderStatusEnum.Completed.ToString(), order.StatusName);
            Assert.AreEqual(0.8m, order.TotalCost);
            Assert.AreEqual(0.9m, order.TotalPrice);
            Assert.AreEqual(1, order.Items.Count());
        }

        [Test]
        public async Task GetOrderByStatus_UndefinedStatus_ThrowException()
        {
            // Arrange
            int invalidStatusEnum = 999; // Non-existent status

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _orderService.GetOrderByStatus(invalidStatusEnum));
            Assert.That(exception.Message, Does.Contain("Invalid status enum value"));
        }

        [Test]
        public async Task GetOrderByStatus_NoUseStatus_ReturnEmpty()
        {
            // Arrange
            var statusEnum = (int)OrderStatusEnum.Completed; // No orders with Completed status

            // Act
            var orders = await _orderService.GetOrderByStatus(statusEnum);

            // Assert
            Assert.IsEmpty(orders);
        }
        #endregion

        #region Task2
        [Test]
        public async Task UpdateOrderStatus_ReturnCorrectResult()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            await AddOrder(orderId, 1);
            var newStatusEnum = (int)OrderStatusEnum.Completed;
            var orders = await _orderService.GetOrdersAsync();
            var order = orders.FirstOrDefault();
            // Act
            var updatedOrder = await _orderService.UpdateOrderStatus(order.Id, newStatusEnum);

            // Assert
            Assert.IsNotNull(updatedOrder);
            Assert.AreEqual(orderId, updatedOrder.Id);
            Assert.AreEqual("Completed", updatedOrder.StatusName);
            Assert.AreEqual(0.8m, updatedOrder.TotalCost);
            Assert.AreEqual(0.9m, updatedOrder.TotalPrice);
        }

        [Test]
        public async Task UpdateOrderStatus_UndefinedStatus_ThrowException()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            await AddOrder(orderId, 1);
            int invalidStatusEnum = 999; // Non-existent status

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _orderService.UpdateOrderStatus(orderId, invalidStatusEnum));
            Assert.That(exception.Message, Does.Contain("Invalid status enum value"));
        }

        [Test]
        public async Task UpdateOrderStatus_WrongProvidedOrder_ThrowException()
        {
            // Arrange
            var nonExistentOrderId = Guid.NewGuid();
            var newStatusEnum = (int)OrderStatusEnum.Completed;

            // Act & Assert
            var result = await _orderService.UpdateOrderStatus(nonExistentOrderId, newStatusEnum);
            Assert.IsNull(result); // Repository returns null for non-existent order
        }
        #endregion


        #region Task3
        [Test]
        public async Task CreateOrder_ReturnCorrectResult()
        {
            // Arrange
            var order = new Order.Model.OrderDetail
            {
                Id = Guid.NewGuid(),
                ResellerId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                StatusId = new Guid(_orderStatusCreatedId),
                CreatedDate = DateTime.UtcNow,
                Items = new[]
                {
                new Order.Model.OrderItem
                {
                    Id = Guid.NewGuid(),
                    ServiceId = new Guid(_orderServiceEmailId),
                    ProductId = new Guid(_orderProductEmailId),
                    Quantity = 2
                }
            }
            };

            // Act
            var result = await _orderService.CreateOrder(order);

            // Assert
            Assert.IsTrue(result);
            var createdOrder = _orderService.GetOrdersAsync().Result.FirstOrDefault();
            var savedOrder = await _orderService.GetOrderByIdAsync(createdOrder.Id);
            Assert.IsNotNull(savedOrder);
            Assert.AreEqual(order.ResellerId, savedOrder.ResellerId);
            Assert.AreEqual(order.CustomerId, savedOrder.CustomerId);
            Assert.AreEqual("Created", savedOrder.StatusName);
            Assert.AreEqual(1.6m, savedOrder.TotalCost);
            Assert.AreEqual(1.8m, savedOrder.TotalPrice);
            Assert.AreEqual(1, savedOrder.Items.Count());
            Assert.AreEqual(2, savedOrder.Items.First().Quantity);
        }

        [Test]
        public async Task CreateOrder_InvalidParameters_ThrowException()
        {
            // Arrange
            var invalidOrder = new Order.Model.OrderDetail
            {
                Id = Guid.NewGuid(),
                ResellerId = Guid.Empty, // Invalid ResellerId
                CustomerId = Guid.NewGuid(),
                StatusId = new Guid(_orderStatusCreatedId),
                CreatedDate = DateTime.UtcNow,
                Items = new[]
                {
                new Order.Model.OrderItem
                {
                    Id = Guid.NewGuid(),
                    ServiceId = new Guid(_orderServiceEmailId),
                    ProductId = new Guid(_orderProductEmailId),
                    Quantity = 2
                }
            }
            };

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _orderService.CreateOrder(invalidOrder));
            Assert.That(exception.Message, Does.Contain("ResellerId is required"));
        }
        #endregion

        #region Task4
        [Test]
        public async Task GetTotalProfitofEachMonth_ReturnCorrectResult()
        {
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);
            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2);
            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 1);
            // Act
            var profits = await _orderService.GetTotalProfitofEachMonth(2025);

            // Assert
            Assert.AreEqual(1, profits.Count());
            var Profit = profits.Single(p => p.month == 7);
            Assert.AreEqual(0.4, Profit.profit); 
        }

        [Test]
        public async Task GetTotalProfitofEachMonth_YearWithNoProfit_ReturnEmpty()
        {

            // Act
            var profits = await _orderService.GetTotalProfitofEachMonth(2025);

            // Assert
            Assert.IsEmpty(profits);
        }
        [Test]
        public async Task GetOrdersAsync_ReturnsCorrectNumberOfOrders()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3);

            // Act
            var orders = await _orderService.GetOrdersAsync();

            // Assert
            Assert.AreEqual(3, orders.Count());
        }
        [Test]
        public async Task GetTotalProfitofEachMonth_NonExistYearInDatabase_ReturnEmpty()
        {

            // Act
            var profits = await _orderService.GetTotalProfitofEachMonth(2025);

            // Assert
            Assert.IsEmpty(profits);
        }

        [Test]
        public async Task GetTotalProfitofEachMonth_InvalidYear_ReturnEmpty()
        {
            // Act
            var profits = await _orderService.GetTotalProfitofEachMonth(-1); // Invalid year

            // Assert
            Assert.IsEmpty(profits);
        }
        #endregion
        [Test]
        public async Task GetOrdersAsync_ReturnsOrdersWithCorrectTotals()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3);

            // Act
            var orders = await _orderService.GetOrdersAsync();

            // Assert
            var order1 = orders.SingleOrDefault(x => x.Id == orderId1);
            var order2 = orders.SingleOrDefault(x => x.Id == orderId2);
            var order3 = orders.SingleOrDefault(x => x.Id == orderId3);

            Assert.AreEqual(0.8m, order1.TotalCost);
            Assert.AreEqual(0.9m, order1.TotalPrice);

            Assert.AreEqual(1.6m, order2.TotalCost);
            Assert.AreEqual(1.8m, order2.TotalPrice);

            Assert.AreEqual(2.4m, order3.TotalCost);
            Assert.AreEqual(2.7m, order3.TotalPrice);
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsCorrectOrder()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(orderId1, order.Id);
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsCorrectOrderItemCount()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(1, order.Items.Count());
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsOrderWithCorrectTotals()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 2);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(1.6m, order.TotalCost);
            Assert.AreEqual(1.8m, order.TotalPrice);
        }

        private async Task AddOrder(Guid orderId, int quantity)
        {
            var orderIdBytes = orderId.ToByteArray();
            _orderContext.Order.Add(new Data.Entities.Order
            {
                Id = orderIdBytes,
                ResellerId = Guid.NewGuid().ToByteArray(),
                CustomerId = Guid.NewGuid().ToByteArray(),
                CreatedDate = DateTime.Now,
                StatusId = _orderStatusCompletedId,
            });

            _orderContext.OrderItem.Add(new OrderItem
            {
                Id = Guid.NewGuid().ToByteArray(),
                OrderId = orderIdBytes,
                ServiceId = _orderServiceEmailId,
                ProductId = _orderProductEmailId,
                Quantity = quantity
            });

            await _orderContext.SaveChangesAsync();
        }

        private async Task AddReferenceDataAsync(OrderContext orderContext)
        {
            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusCompletedId,
                Name = "Completed",
            });
            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusCreatedId,
                Name = "Created",
            });
            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusFailedId,
                Name = "Failed",
            });
            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusInProgressId,
                Name = "InProgress",
            });

            orderContext.OrderService.Add(new Data.Entities.OrderService
            {
                Id = _orderServiceEmailId,
                Name = "Email"
            });

            orderContext.OrderProduct.Add(new OrderProduct
            {
                Id = _orderProductEmailId,
                Name = "100GB Mailbox",
                UnitCost = 0.8m,
                UnitPrice = 0.9m,
                ServiceId = _orderServiceEmailId
            });

            await orderContext.SaveChangesAsync();
        }
    }
}
