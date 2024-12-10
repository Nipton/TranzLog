using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TranzLog.Data;
using TranzLog.Models.DTO;
using TranzLog.Models;
using TranzLog.Repositories;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using TranzLog.Exceptions;

namespace TranzLogTests
{
    public class TransportOrderRepositoryTests
    {
        private readonly IMapper mapper;
        private readonly DbContextOptions<ShippingDbContext> dbContextOptions;

        public TransportOrderRepositoryTests()
        {
            dbContextOptions = new DbContextOptionsBuilder<ShippingDbContext>()
                .UseInMemoryDatabase("dbTest")
                .Options;
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TransportOrder, TransportOrderDTO>().ReverseMap();
            });
            mapper = configuration.CreateMapper();
        }
        [Fact]
        public async Task AddAsync_AddsOrderToDatabaseAndReturnsDTO()
        {

            var dbContext = new ShippingDbContext(dbContextOptions);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var repository = new TransportOrderRepository(dbContext, mapper, cache);
            var dto = new TransportOrderDTO { Id = 1, TrackNumber = "123" };
            
            var result = await repository.AddAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.TrackNumber, result.TrackNumber);
            var orderInDb = await dbContext.TransportOrders.FindAsync(result.Id);
            Assert.NotNull(orderInDb);
            Assert.Equal(dto.TrackNumber, orderInDb.TrackNumber);
            dbContext.Dispose();
        }
        [Fact]
        public async Task UpdateAsync_UpdatesOrderAndReturnsUpdatedDTO()
        {
            var dbContext = new ShippingDbContext(dbContextOptions);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var repository = new TransportOrderRepository(dbContext, mapper, cache);

            var order = new TransportOrder { TrackNumber = "123", OrderStatus = OrderStatus.Pending };
            dbContext.TransportOrders.Add(order);
            await dbContext.SaveChangesAsync();

            var dto = new TransportOrderDTO
            {
                Id = order.Id,
                TrackNumber = "5",
                OrderStatus = OrderStatus.Completed
            };

            var result = await repository.UpdateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.TrackNumber, result.TrackNumber);
            Assert.Equal(dto.OrderStatus, result.OrderStatus);

            var orderInDb = await dbContext.TransportOrders.FindAsync(order.Id);
            Assert.NotNull(orderInDb);
            Assert.Equal(dto.TrackNumber, orderInDb.TrackNumber);
            Assert.Equal(dto.OrderStatus, orderInDb.OrderStatus);
        }
        [Fact]
        public async Task DeleteAsync_DeletesOrderFromDatabase()
        {
            var dbContext = new ShippingDbContext(dbContextOptions);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var repository = new TransportOrderRepository(dbContext, mapper, cache);

            var order = new TransportOrder { TrackNumber = "123", OrderStatus = OrderStatus.Pending };
            dbContext.TransportOrders.Add(order);
            await dbContext.SaveChangesAsync();

            await repository.DeleteAsync(order.Id);

            var orderInDb = await dbContext.TransportOrders.FindAsync(order.Id);
            Assert.Null(orderInDb); 
        }
        [Fact]
        public async Task InvalidatesCacheByVersionChange()
        {
            var dbContext = new ShippingDbContext(dbContextOptions);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var repository = new TransportOrderRepository(dbContext, mapper, cache);

            var order = new TransportOrder { Id = 1, };
            var order2 = new TransportOrder { Id = 2, };
            await dbContext.AddAsync(order);
            await dbContext.AddAsync(order2);
            await dbContext.SaveChangesAsync();           
            var initialPage = repository.GetAll();

            await repository.DeleteAsync(1);

            var newPage = repository.GetAll();
            Assert.NotEqual(initialPage, newPage); 
        }
        [Fact]
        public async Task GetAsync_Returns()
        {
            var dbContext = new ShippingDbContext(dbContextOptions);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var repository = new TransportOrderRepository(dbContext, mapper, cache);

            var order = new TransportOrder { TrackNumber = "123", OrderStatus = OrderStatus.Pending };
            dbContext.TransportOrders.Add(order);
            await dbContext.SaveChangesAsync();

            var result = await repository.GetAsync(order.Id);
            var result2 = await repository.GetAsync(999);

            Assert.NotNull(result);
            Assert.Equal(order.TrackNumber, result.TrackNumber);
            Assert.Null(result2);
        }
        [Fact]
        public void GetAll_ReturnsOrdersWithPagination()
        {
            var dbContext = new ShippingDbContext(dbContextOptions);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var repository = new TransportOrderRepository(dbContext, mapper, cache);

            var orders = new List<TransportOrder>
        {
            new TransportOrder { TrackNumber = "123", OrderStatus = OrderStatus.Pending },
            new TransportOrder { TrackNumber = "124", OrderStatus = OrderStatus.Completed },
        };

            dbContext.TransportOrders.AddRange(orders);
            dbContext.SaveChanges();

            var result = repository.GetAll(1, 2); 

            Assert.Equal(2, result.Count());
            Assert.Equal("123", result.First().TrackNumber);
            Assert.Equal("124", result.Last().TrackNumber);
        }
        [Fact]
        public void GetAll_ThrowsExceptionWhenPageDoesNotExist()
        {
            var dbContext = new ShippingDbContext(dbContextOptions);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var repository = new TransportOrderRepository(dbContext, mapper, cache);

            Assert.Throws<InvalidParameterException>(() => repository.GetAll(10, 2)); 
        }
        [Fact]
        public async Task UpdateOrderStatusAsync_UpdatesStatusSuccessfully()
        {
            using var dbContext = new ShippingDbContext(dbContextOptions);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var repository = new TransportOrderRepository(dbContext, mapper, cache);
            var order = new TransportOrder
            {
                Id = 1,
                OrderStatus = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.TransportOrders.Add(order);
            await dbContext.SaveChangesAsync();

            await repository.UpdateOrderStatusAsync(order.Id, OrderStatus.Confirmed);

            var updatedOrder = await dbContext.TransportOrders.FindAsync(order.Id);
            Assert.NotNull(updatedOrder);
            Assert.Equal(OrderStatus.Confirmed, updatedOrder.OrderStatus);
        }
        [Fact]
        public async Task UpdateOrderStatusAsync_SetsCompletionTime()
        {
            using var dbContext = new ShippingDbContext(dbContextOptions);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var repository = new TransportOrderRepository(dbContext, mapper, cache);
            var order = new TransportOrder
            {
                Id = 1,
                OrderStatus = OrderStatus.Confirmed,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.TransportOrders.Add(order);
            await dbContext.SaveChangesAsync();

            await repository.UpdateOrderStatusAsync(order.Id, OrderStatus.Completed);

            var updatedOrder = await dbContext.TransportOrders.FindAsync(order.Id);
            Assert.NotNull(updatedOrder);
            Assert.Equal(OrderStatus.Completed, updatedOrder.OrderStatus);
            Assert.NotNull(updatedOrder.CompletionTime);
            Assert.True((DateTime.UtcNow - updatedOrder.CompletionTime.Value).TotalSeconds < 1);
        }
        [Fact]
        public async Task UpdateOrderStatusAsync_SetsStartTransportTime()
        {
            using var dbContext = new ShippingDbContext(dbContextOptions);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var repository = new TransportOrderRepository(dbContext, mapper, cache);
            var order = new TransportOrder
            {
                Id = 3,
                OrderStatus = OrderStatus.Confirmed,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.TransportOrders.Add(order);
            await dbContext.SaveChangesAsync();

            await repository.UpdateOrderStatusAsync(order.Id, OrderStatus.AcceptedByDriver);

            var updatedOrder = await dbContext.TransportOrders.FindAsync(order.Id);
            Assert.NotNull(updatedOrder);
            Assert.Equal(OrderStatus.AcceptedByDriver, updatedOrder.OrderStatus);
            Assert.NotNull(updatedOrder.StartTransportTime);
            Assert.True((DateTime.UtcNow - updatedOrder.StartTransportTime.Value).TotalSeconds < 1);
        }
        [Fact]
        public async Task UpdateOrderStatusAsync_ThrowsEntityNotFoundException_WhenOrderDoesNotExist()
        {
            using var dbContext = new ShippingDbContext(dbContextOptions);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var repository = new TransportOrderRepository(dbContext, mapper, cache);
            int nonExistentOrderId = 999;

            var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
                () => repository.UpdateOrderStatusAsync(nonExistentOrderId, OrderStatus.Confirmed));

            Assert.Equal($"Заказ с ID {nonExistentOrderId} не найден.", exception.Message);
        }
    }
}
