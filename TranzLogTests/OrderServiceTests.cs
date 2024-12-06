using AutoMapper;
using Moq;
using TranzLog.Data;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;
using TranzLog.Models;
using TranzLog.Services;
using Microsoft.AspNetCore.Http;
using TranzLog.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;

namespace TranzLogTests
{
    public class OrderServiceTests
    {
        [Fact]
        public async Task GetOrderInfoByTrackerAsyncTest_WhenOrderExists()
        {
            var repoContainerMock = new Mock<IRepositoryContainer>();
            var authServiceMock = new Mock<IAuthenticationService>();
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>(); 
            });
            IMapper mapper = new Mapper(configuration);
            var trackNumber = "track123";
            var fakeTransportOrder = new TransportOrder { Id = 1, TrackNumber = trackNumber };
            var expectedDTO = new UserOrderResponseDTO { Id = 1 };
            var orderRepoMock = new Mock<ITransportOrderRepository>();
            orderRepoMock.Setup(repo => repo.GetOrderInfoByTrackerAsync(trackNumber))
                .ReturnsAsync(fakeTransportOrder);
            repoContainerMock.Setup(container => container.OrderRepo)
                .Returns(orderRepoMock.Object);
            var service = new OrderService(null, mapper, repoContainerMock.Object, authServiceMock.Object);

            var result = await service.GetOrderInfoByTrackerAsync(trackNumber);

            Assert.NotNull(result);
            Assert.IsType<UserOrderResponseDTO>(result);
            Assert.Equal(expectedDTO.Id, result.Id);
            Assert.Equal(fakeTransportOrder.Id, result.Id);
            orderRepoMock.Verify(repo => repo.GetOrderInfoByTrackerAsync(trackNumber), Times.Once);
        }
        [Fact]
        public async Task GetOrderInfoByTrackerAsync_WhenOrderDoesNotExist()
        {
            var repoContainerMock = new Mock<IRepositoryContainer>();
            var mapperMock = new Mock<IMapper>();
            var authServiceMock = new Mock<IAuthenticationService>();
            var trackNumber = "track123";
            var orderRepoMock = new Mock<ITransportOrderRepository>();
            orderRepoMock
                .Setup(repo => repo.GetOrderInfoByTrackerAsync(trackNumber))
                .ReturnsAsync((TransportOrder?)null);

            repoContainerMock
                .Setup(container => container.OrderRepo)
                .Returns(orderRepoMock.Object);

            var service = new OrderService(null, mapperMock.Object, repoContainerMock.Object, authServiceMock.Object);

            var result = await service.GetOrderInfoByTrackerAsync(trackNumber);

            Assert.Null(result);
            orderRepoMock.Verify(repo => repo.GetOrderInfoByTrackerAsync(trackNumber), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<UserOrderResponseDTO>(It.IsAny<TransportOrder>()), Times.Never);
        }
        [Fact]
        public async Task CancelOrderAsync_ShouldCancelOrder()
        {

            var orderId = 1;
            var trackNumber = "track123";
            var order = new TransportOrderDTO
            {
                Id = orderId,
                TrackNumber = trackNumber,
                UserId = 123,  
                OrderStatus = OrderStatus.Pending
            };
            var user = new UserDTO
            {
                Id = 123,
                UserName = "Иван"
            };

            var orderRepoMock = new Mock<ITransportOrderRepository>();
            var authServiceMock = new Mock<IAuthenticationService>();
            var repoContainerMock = new Mock<IRepositoryContainer>();
            var httpContextMock = new Mock<HttpContext>();
            orderRepoMock.Setup(repo => repo.GetAsync(orderId)).ReturnsAsync(order);
            orderRepoMock.Setup(repo => repo.UpdateAsync(It.IsAny<TransportOrderDTO>())).ReturnsAsync(new TransportOrderDTO());
            authServiceMock.Setup(service => service.GetCurrentUserAsync(It.IsAny<HttpContext>())).ReturnsAsync(user);
            repoContainerMock.Setup(container => container.OrderRepo).Returns(orderRepoMock.Object);
            var orderService = new OrderService(null, null, repoContainerMock.Object, authServiceMock.Object);

            await orderService.CancelOrderAsync(orderId, httpContextMock.Object);

            Assert.Equal(OrderStatus.Cancelled, order.OrderStatus); 
            orderRepoMock.Verify(repo => repo.UpdateAsync(It.Is<TransportOrderDTO>(o => o.Id == orderId && o.OrderStatus == OrderStatus.Cancelled)), Times.Once);
        }
        [Fact]
        public async Task CancelOrderAsync_WhenOrderNotFound()
        {
            var orderId = 1;
            var httpContextMock = new Mock<HttpContext>();
            var orderRepoMock = new Mock<ITransportOrderRepository>();
            var authServiceMock = new Mock<IAuthenticationService>();
            var repoContainerMock = new Mock<IRepositoryContainer>();
            orderRepoMock.Setup(repo => repo.GetAsync(orderId)).ReturnsAsync((TransportOrderDTO?)null);
            repoContainerMock.Setup(container => container.OrderRepo).Returns(orderRepoMock.Object);
            var orderService = new OrderService(null, null, repoContainerMock.Object, authServiceMock.Object);

            var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => orderService.CancelOrderAsync(orderId, httpContextMock.Object));
            Assert.Equal($"Заказ с ID {orderId} не найден.", exception.Message);
        }
        [Fact]
        public async Task CancelOrderAsync_WhenUserIsNotAuthorized_ShouldThrowUnauthorizedAccessException()
        {
            var orderId = 1;
            var trackNumber = "track123";
            var order = new TransportOrderDTO
            {
                Id = orderId,
                TrackNumber = trackNumber,
                UserId = 123,  
                OrderStatus = OrderStatus.Pending
            };
            var user = new UserDTO
            {
                Id = 124,
                UserName = "Иван"
            };

            var orderRepoMock = new Mock<ITransportOrderRepository>();
            var authServiceMock = new Mock<IAuthenticationService>();
            var repoContainerMock = new Mock<IRepositoryContainer>();
            var httpContextMock = new Mock<HttpContext>();

            orderRepoMock.Setup(repo => repo.GetAsync(orderId)).ReturnsAsync(order);
            authServiceMock.Setup(service => service.GetCurrentUserAsync(It.IsAny<HttpContext>())).ReturnsAsync(user);
            repoContainerMock.Setup(container => container.OrderRepo).Returns(orderRepoMock.Object);

            var orderService = new OrderService(null, null, repoContainerMock.Object, authServiceMock.Object);

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => orderService.CancelOrderAsync(orderId, httpContextMock.Object));
            Assert.Equal("Нет прав для данного действия.", exception.Message);
        }
        [Fact]
        public async Task CreateOrderByUserAsync_WhenInvalidParameterException()
        {
            var userOrderDTO = new UserOrderRequestDTO
            {
                Consignee = null, 
                Shipper = new ShipperDTO(),
                RouteId = 1,
                CargoList = new List<CargoDTO> { new CargoDTO() }
            };
            var userOrderDTO2 = new UserOrderRequestDTO
            {
                Consignee = new ConsigneeDTO(),
                Shipper = null,
                RouteId = 1,
                CargoList = new List<CargoDTO> { new CargoDTO() }
            };
            var userOrderDTO3 = new UserOrderRequestDTO
            {
                Consignee = new ConsigneeDTO(),
                Shipper = new ShipperDTO(),
                RouteId = 1,
                CargoList = new List<CargoDTO> { }
            };
            var userOrderDTO4 = new UserOrderRequestDTO
            {
                Consignee = new ConsigneeDTO(),
                Shipper = new ShipperDTO(),
                RouteId = 1,
                CargoList = new List<CargoDTO> { new CargoDTO() }
            };

            var httpContextMock = new Mock<HttpContext>();
            var repoContainerMock = new Mock<IRepositoryContainer>();
            var authServiceMock = new Mock<IAuthenticationService>();
            repoContainerMock.Setup(repo => repo.RouteRepo.RouteExistsAsync(It.IsAny<int>())).ReturnsAsync(false);


            var orderService = new OrderService(null, null, repoContainerMock.Object, authServiceMock.Object);

            await Assert.ThrowsAsync<InvalidParameterException>(() => orderService.CreateOrderByUserAsync(userOrderDTO, httpContextMock.Object));
            await Assert.ThrowsAsync<InvalidParameterException>(() => orderService.CreateOrderByUserAsync(userOrderDTO2, httpContextMock.Object));
            await Assert.ThrowsAsync<InvalidParameterException>(() => orderService.CreateOrderByUserAsync(userOrderDTO3, httpContextMock.Object));
            await Assert.ThrowsAsync<EntityNotFoundException>(() => orderService.CreateOrderByUserAsync(userOrderDTO4, httpContextMock.Object));
        }
        [Fact]
        public async Task CreateOrderByUserAsync_WhenErrorTransaction()
        {
            var userOrderDTO = new UserOrderRequestDTO
            {
                Consignee = new ConsigneeDTO(),
                Shipper = new ShipperDTO(),
                RouteId = 1,
                CargoList = new List<CargoDTO> { new CargoDTO() }
            };

            var httpContextMock = new Mock<HttpContext>();

            var transactionMock = new Mock<IDbContextTransaction>();
            var transactionManagerMock = new Mock<ITransactionManager>();
            transactionManagerMock.Setup(tm => tm.BeginTransaction()).Returns(transactionMock.Object);

            var repoContainerMock = new Mock<IRepositoryContainer>();
            repoContainerMock.Setup(repo => repo.RouteRepo.RouteExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            repoContainerMock.Setup(repo => repo.ConsigneeRepo.AddAsync(It.IsAny<ConsigneeDTO>())).ThrowsAsync(new Exception());

            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock.Setup(service => service.GetCurrentUserAsync(It.IsAny<HttpContext>())).ReturnsAsync(new UserDTO { Id = 1 });

            var orderService = new OrderService(transactionManagerMock.Object, null, repoContainerMock.Object, authServiceMock.Object);


            await Assert.ThrowsAsync<Exception>(() => orderService.CreateOrderByUserAsync(userOrderDTO, httpContextMock.Object));

            transactionMock.Verify(t => t.Rollback(), Times.Once);
        }
        [Fact]
        public async Task CreateOrderByUserAsync_WhenDataIsValid()
        {
            var userOrderDTO = new UserOrderRequestDTO
            {
                Consignee = new ConsigneeDTO(),
                Shipper = new ShipperDTO(),
                RouteId = 1,
                CargoList = new List<CargoDTO> { new CargoDTO() }
            };

            var httpContextMock = new Mock<HttpContext>();

            var consigneeResult = new ConsigneeDTO { Id = 1 };
            var shipperResult = new ShipperDTO { Id = 1 };
            var transportOrderResult = new TransportOrderDTO { Id = 1, TrackNumber = "track123" };

            var repoContainerMock = new Mock<IRepositoryContainer>();
            repoContainerMock.Setup(repo => repo.RouteRepo.RouteExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            repoContainerMock.Setup(repo => repo.ConsigneeRepo.AddAsync(It.IsAny<ConsigneeDTO>())).ReturnsAsync(consigneeResult);
            repoContainerMock.Setup(repo => repo.ShipperRepo.AddAsync(It.IsAny<ShipperDTO>())).ReturnsAsync(shipperResult);
            repoContainerMock.Setup(repo => repo.OrderRepo.AddAsync(It.IsAny<TransportOrderDTO>())).ReturnsAsync(transportOrderResult);
            repoContainerMock.Setup(repo => repo.CargoRepo.AddAsync(It.IsAny<CargoDTO>())).ReturnsAsync(new CargoDTO());

            var authServiceMock = new Mock<IAuthenticationService>();
            var currentUser = new UserDTO { Id = 1 };
            authServiceMock.Setup(service => service.GetCurrentUserAsync(It.IsAny<HttpContext>())).ReturnsAsync(currentUser);

            var transactionMock = new Mock<IDbContextTransaction>();
            var transactionManagerMock = new Mock<ITransactionManager>();
            transactionManagerMock.Setup(tm => tm.BeginTransaction()).Returns(transactionMock.Object);

            var orderService = new OrderService(transactionManagerMock.Object, null, repoContainerMock.Object, authServiceMock.Object);

            var result = await orderService.CreateOrderByUserAsync(userOrderDTO, httpContextMock.Object);

            Assert.Equal("track123", result);
            transactionMock.Verify(t => t.Commit(), Times.Once); 
            transactionMock.Verify(t => t.Rollback(), Times.Never);
            repoContainerMock.Verify(repo => repo.ConsigneeRepo.AddAsync(It.IsAny<ConsigneeDTO>()), Times.Once);
            repoContainerMock.Verify(repo => repo.ShipperRepo.AddAsync(It.IsAny<ShipperDTO>()), Times.Once);
            repoContainerMock.Verify(repo => repo.OrderRepo.AddAsync(It.IsAny<TransportOrderDTO>()), Times.Once);
            repoContainerMock.Verify(repo => repo.CargoRepo.AddAsync(It.IsAny<CargoDTO>()), Times.Exactly(userOrderDTO.CargoList.Count));
        }
    }   
}