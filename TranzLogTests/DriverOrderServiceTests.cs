using AutoMapper;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;
using TranzLog.Services;

namespace TranzLogTests
{
    public class DriverOrderServiceTests
    {
        [Fact]
        public async Task GetDriverAssignedOrdersAsync_ValidDriver()
        {
            var driverId = 1;
            var currentUser = new UserDTO { Id = 1 };
            var driver = new Driver { Id = driverId, UserId = currentUser.Id };
            var orders = new List<DriverOrderDTO> { new DriverOrderDTO { Id = 1, OrderStatus = OrderStatus.Pending }, new DriverOrderDTO { Id = 2, OrderStatus = OrderStatus.InProgress } };
            var httpContextMock = new Mock<HttpContext>();
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock.Setup(service => service.GetCurrentUserAsync(It.IsAny<HttpContext>())).ReturnsAsync(currentUser);

            var driverRepoMock = new Mock<IDriverRepository>();
            driverRepoMock.Setup(repo => repo.FindDriverByUserIdAsync(currentUser.Id)).ReturnsAsync(driver);

            var orderRepoMock = new Mock<ITransportOrderRepository>();
            orderRepoMock.Setup(repo => repo.GetOrdersForDriverAsync(driverId)).ReturnsAsync(orders);

            var service = new DriverOrderService(orderRepoMock.Object, authServiceMock.Object, driverRepoMock.Object, Mock.Of<IRepository<VehicleDTO>>());

            var result = await service.GetDriverAssignedOrdersAsync(httpContextMock.Object);

            Assert.NotNull(result);
            Assert.Equal(orders.Count, result.Count);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal(OrderStatus.Pending, result[0].OrderStatus);
            driverRepoMock.Verify(repo => repo.FindDriverByUserIdAsync(currentUser.Id), Times.Once);
            orderRepoMock.Verify(repo => repo.GetOrdersForDriverAsync(driverId), Times.Once);
        }
        [Fact]
        public async Task GetDriverAssignedOrdersAsync_WhenDriverNotFound()
        {
            var user = new UserDTO { Id = 1 };
            var httpContextMock = new Mock<HttpContext>();
            var authenticationServiceMock = new Mock<IAuthenticationService>();
            var driverRepositoryMock = new Mock<IDriverRepository>();

            authenticationServiceMock.Setup(service => service.GetCurrentUserAsync(It.IsAny<HttpContext>())).ReturnsAsync(user);

 
            driverRepositoryMock.Setup(repo => repo.FindDriverByUserIdAsync(user.Id)).ReturnsAsync((Driver)null);

            var driverOrderService = new DriverOrderService(
                Mock.Of<ITransportOrderRepository>(),
                authenticationServiceMock.Object,
                driverRepositoryMock.Object,
                Mock.Of<IRepository<VehicleDTO>>()
            );

            await Assert.ThrowsAsync<UserNotFoundException>(() => driverOrderService.GetDriverAssignedOrdersAsync(httpContextMock.Object));
        }
        [Fact]
        public async Task GetDriverAssignedOrdersAsync_WhenNoOrdersForDriver_ShouldReturnEmptyList()
        {
            var user = new UserDTO { Id = 1 };
            var driver = new Driver { Id = 1 };
            var orders = new List<DriverOrderDTO>(); 

            var httpContextMock = new Mock<HttpContext>();
            var authenticationServiceMock = new Mock<IAuthenticationService>();
            var driverRepositoryMock = new Mock<IDriverRepository>();
            var orderRepoMock = new Mock<ITransportOrderRepository>();

            authenticationServiceMock.Setup(service => service.GetCurrentUserAsync(It.IsAny<HttpContext>())).ReturnsAsync(user);

            driverRepositoryMock.Setup(repo => repo.FindDriverByUserIdAsync(user.Id)).ReturnsAsync(driver);

            orderRepoMock.Setup(repo => repo.GetOrdersForDriverAsync(driver.Id)).ReturnsAsync(orders);

            var driverOrderService = new DriverOrderService(
                orderRepoMock.Object,
                authenticationServiceMock.Object,
                driverRepositoryMock.Object,
                Mock.Of<IRepository<VehicleDTO>>()
            );

            var result = await driverOrderService.GetDriverAssignedOrdersAsync(httpContextMock.Object);

            Assert.Empty(result); 
        }
        [Fact]
        public async Task UpdateOrderDeliveryStatusAsync_InvalidStatus_ShouldThrowInvalidParameterException()
        {
            var invalidStatus = 999; 
            var service = new DriverOrderService(Mock.Of<ITransportOrderRepository>(), Mock.Of<IAuthenticationService>(), Mock.Of<IDriverRepository>(), Mock.Of<IRepository<VehicleDTO>>());

            await Assert.ThrowsAsync<InvalidParameterException>(() => service.UpdateOrderDeliveryStatusAsync(1, invalidStatus, new DefaultHttpContext()));
        }
        [Fact]
        public async Task UpdateOrderDeliveryStatusAsync_WhenStatusCannotBeChanged_ShouldThrowInvalidParameterException()
        {
            var orderId = 1;
            var invalidStatus = (int)OrderStatus.Pending; 
            var httpContextMock = new Mock<HttpContext>();

            var service = new DriverOrderService(Mock.Of<ITransportOrderRepository>(), Mock.Of<IAuthenticationService>(), Mock.Of<IDriverRepository>(), Mock.Of<IRepository<VehicleDTO>>());

            var exception = await Assert.ThrowsAsync<InvalidParameterException>(
                () => service.UpdateOrderDeliveryStatusAsync(orderId, invalidStatus, httpContextMock.Object)
            );
            Assert.Equal("Статус заказа можно изменить только на 'AcceptedByDriver' или 'Completed'", exception.Message);
        }
        [Fact]
        public async Task UpdateOrderDeliveryStatusAsync_WhenDriverNotFound()
        {
            var orderId = 1;
            var newStatus = (int)OrderStatus.AcceptedByDriver;
            var user = new UserDTO { Id = 1 };  
            var httpContextMock = new Mock<HttpContext>();
            var authenticationServiceMock = new Mock<IAuthenticationService>();
            var driverRepositoryMock = new Mock<IDriverRepository>();
            authenticationServiceMock.Setup(service => service.GetCurrentUserAsync(It.IsAny<HttpContext>())).ReturnsAsync(user);

            driverRepositoryMock.Setup(repo => repo.FindDriverByUserIdAsync(user.Id)).ReturnsAsync((Driver)null);
            var service = new DriverOrderService(Mock.Of<ITransportOrderRepository>(), authenticationServiceMock.Object,
        driverRepositoryMock.Object, Mock.Of<IRepository<VehicleDTO>>());

            await Assert.ThrowsAsync<UserNotFoundException>(
                () => service.UpdateOrderDeliveryStatusAsync(orderId, newStatus, httpContextMock.Object)
            );
        }
        [Fact]
        public async Task UpdateOrderDeliveryStatusAsync_ValidStatus_ShouldUpdateOrderStatus()
        {
            var orderId = 1;
            var validStatus = (int)OrderStatus.AcceptedByDriver;
            var currentUser = new UserDTO { Id = 1 };
            var driver = new Driver { Id = 1, UserId = currentUser.Id };
            var order = new TransportOrderDTO { Id = orderId, VehicleId = 1 };
            var vehicle = new VehicleDTO { Id = 1, DriverId = driver.Id };

            var httpContextMock = new Mock<HttpContext>();
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock.Setup(service => service.GetCurrentUserAsync(It.IsAny<HttpContext>())).ReturnsAsync(currentUser);

            var driverRepoMock = new Mock<IDriverRepository>();
            driverRepoMock.Setup(repo => repo.FindDriverByUserIdAsync(currentUser.Id)).ReturnsAsync(driver);

            var orderRepoMock = new Mock<ITransportOrderRepository>();
            orderRepoMock.Setup(repo => repo.GetAsync(orderId)).ReturnsAsync(order);

            var vehicleRepoMock = new Mock<IRepository<VehicleDTO>>();
            vehicleRepoMock.Setup(repo => repo.GetAsync((int)order.VehicleId)).ReturnsAsync(vehicle);

            var service = new DriverOrderService(orderRepoMock.Object, authServiceMock.Object, driverRepoMock.Object, vehicleRepoMock.Object);

            await service.UpdateOrderDeliveryStatusAsync(orderId, validStatus, httpContextMock.Object);

            orderRepoMock.Verify(repo => repo.UpdateOrderStatusAsync(orderId, (OrderStatus)validStatus), Times.Once);
        }
        [Fact]
        public async Task UpdateOrderDeliveryStatusAsync_WhenOrderNotFound_ShouldThrowEntityNotFoundException()
        {
            var orderId = 1;
            var newStatus = (int)OrderStatus.AcceptedByDriver;
            var user = new UserDTO { Id = 1 };  
            var httpContextMock = new Mock<HttpContext>();
            var authenticationServiceMock = new Mock<IAuthenticationService>();
            var driverRepositoryMock = new Mock<IDriverRepository>();
            var orderRepoMock = new Mock<ITransportOrderRepository>();

            authenticationServiceMock.Setup(service => service.GetCurrentUserAsync(It.IsAny<HttpContext>())).ReturnsAsync(user);

            driverRepositoryMock.Setup(repo => repo.FindDriverByUserIdAsync(user.Id)).ReturnsAsync(new Driver { Id = 1 });

            orderRepoMock.Setup(repo => repo.GetAsync(orderId)).ReturnsAsync((TransportOrderDTO)null);

            var driverOrderService = new DriverOrderService(orderRepoMock.Object, authenticationServiceMock.Object, driverRepositoryMock.Object, Mock.Of<IRepository<VehicleDTO>>()
            );

            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => driverOrderService.UpdateOrderDeliveryStatusAsync(orderId, newStatus, httpContextMock.Object)
            );
        }
        [Fact]
        public async Task UpdateOrderDeliveryStatusAsync_WhenVehicleNotFound_ShouldThrowEntityNotFoundException()
        {
            var orderId = 1;
            var newStatus = (int)OrderStatus.AcceptedByDriver;
            var user = new UserDTO { Id = 1 };  
            var order = new TransportOrderDTO { Id = orderId, VehicleId = 2 };  
            var httpContextMock = new Mock<HttpContext>();
            var authenticationServiceMock = new Mock<IAuthenticationService>();
            var driverRepositoryMock = new Mock<IDriverRepository>();
            var orderRepoMock = new Mock<ITransportOrderRepository>();
            var vehicleRepoMock = new Mock<IRepository<VehicleDTO>>();
            authenticationServiceMock.Setup(service => service.GetCurrentUserAsync(It.IsAny<HttpContext>())).ReturnsAsync(user);
            driverRepositoryMock.Setup(repo => repo.FindDriverByUserIdAsync(user.Id)).ReturnsAsync(new Driver { Id = 1 });
            orderRepoMock.Setup(repo => repo.GetAsync(orderId)).ReturnsAsync(order);
            vehicleRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync((VehicleDTO)null);

            var driverOrderService = new DriverOrderService(orderRepoMock.Object, authenticationServiceMock.Object, driverRepositoryMock.Object, vehicleRepoMock.Object);

            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => driverOrderService.UpdateOrderDeliveryStatusAsync(orderId, newStatus, httpContextMock.Object)
            );
        }
    }
}
