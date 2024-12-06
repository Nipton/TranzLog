using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TranzLog.Controllers;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;

namespace TranzLogTests
{
    public class UserOrderControllerTests
    {
        private readonly Mock<IOrderService> orderServiceMock;
        private readonly Mock<ILogger<UserOrderController>> loggerMock;
        private readonly UserOrderController controller;
        public UserOrderControllerTests()
        {
            orderServiceMock = new Mock<IOrderService>();
            loggerMock = new Mock<ILogger<UserOrderController>>();
            controller = new UserOrderController(orderServiceMock.Object, loggerMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }
        [Fact]
        public async Task CreateOrder_ReturnsOkResult()
        {
            var userOrderDTO = new UserOrderRequestDTO();
            var orderId = "12345";
            orderServiceMock.Setup(s => s.CreateOrderByUserAsync(userOrderDTO, It.IsAny<HttpContext>())).ReturnsAsync(orderId);

            var result = await controller.CreateOrder(userOrderDTO);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(orderId, okResult.Value);
            orderServiceMock.Verify(s => s.CreateOrderByUserAsync(userOrderDTO, It.IsAny<HttpContext>()), Times.Once);
        }
        [Fact]
        public async Task CreateOrder_ReturnsBadRequest()
        {
            var userOrderDTO = new UserOrderRequestDTO();
            orderServiceMock.Setup(s => s.CreateOrderByUserAsync(userOrderDTO, It.IsAny<HttpContext>())) .ThrowsAsync(new InvalidParameterException("Неполные данные для создания заказа."));

            var result = await controller.CreateOrder(userOrderDTO);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Неполные данные для создания заказа.", badRequestResult.Value);
            var logInvocations = loggerMock.Invocations;
            var logInvocation = logInvocations.FirstOrDefault(inv => inv.Method.Name == nameof(ILogger.Log));
            Assert.NotNull(logInvocation);
            Assert.Contains("Неполные данные для создания заказа.", logInvocation.Arguments[2]?.ToString());
        }
        [Fact]
        public async Task CreateOrder_ReturnsNotFound()
        {
            var userOrderDTO = new UserOrderRequestDTO();
            orderServiceMock.Setup(s => s.CreateOrderByUserAsync(userOrderDTO, It.IsAny<HttpContext>())).ThrowsAsync(new EntityNotFoundException("Заказ не найден"));

            var result = await controller.CreateOrder(userOrderDTO);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Заказ не найден", notFoundResult.Value);
            var logInvocations = loggerMock.Invocations;
            var logInvocation = logInvocations.FirstOrDefault(inv => inv.Method.Name == nameof(ILogger.Log));
            Assert.NotNull(logInvocation);
            Assert.Contains("Заказ не найден", logInvocation.Arguments[2]?.ToString());
        }
        [Fact]
        public async Task CreateOrder_ReturnsUnauthorized()
        {
            var userOrderDTO = new UserOrderRequestDTO();
            orderServiceMock.Setup(s => s.CreateOrderByUserAsync(userOrderDTO, It.IsAny<HttpContext>())).ThrowsAsync(new UnauthorizedAccessException("Ошибка аутентификации."));

            var result = await controller.CreateOrder(userOrderDTO);

            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(401, statusCodeResult.StatusCode);
            Assert.Equal("Ошибка аутентификации.", statusCodeResult.Value);
            var logInvocations = loggerMock.Invocations;
            var logInvocation = logInvocations.FirstOrDefault(inv => inv.Method.Name == nameof(ILogger.Log));
            Assert.NotNull(logInvocation);
            Assert.Contains("Ошибка аутентификации.", logInvocation.Arguments[2]?.ToString());
        }
        [Fact]
        public async Task CreateOrder_ReturnsServerError()
        {
            var userOrderDTO = new UserOrderRequestDTO();
            orderServiceMock.Setup(s => s.CreateOrderByUserAsync(userOrderDTO, It.IsAny<HttpContext>())).ThrowsAsync(new Exception("Ошибка сервера"));

            var result = await controller.CreateOrder(userOrderDTO);

            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Ошибка сервера", statusCodeResult.Value);
            var logInvocations = loggerMock.Invocations;
            var logInvocation = logInvocations.FirstOrDefault(inv => inv.Method.Name == nameof(ILogger.Log));
            Assert.NotNull(logInvocation);
            Assert.Contains("Ошибка сервера", logInvocation.Arguments[2]?.ToString());
        }
        [Fact]
        public async Task GetOrderByTracker_ReturnsOkResult()
        {
            var trackNumber = "123";
            var orderResponse = new UserOrderResponseDTO();
            orderServiceMock.Setup(s => s.GetOrderInfoByTrackerAsync(trackNumber)).ReturnsAsync(orderResponse);

            var result = await controller.GetOrderByTracker(trackNumber);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(orderResponse, okResult.Value);
        }
        [Fact]
        public async Task GetOrderByTracker_ReturnsBadRequest()
        {
            var result = await controller.GetOrderByTracker("");
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Не указан трек-номер", badRequestResult.Value);
        }
        [Fact]
        public async Task GetOrderByTracker_ReturnsNotFound_WhenOrderNotFound()
        {
            var trackNumber = "123";
            orderServiceMock.Setup(s => s.GetOrderInfoByTrackerAsync(trackNumber)).ReturnsAsync((UserOrderResponseDTO)null);

            var result = await controller.GetOrderByTracker(trackNumber);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Указанный трек-номер не найден.", notFoundResult.Value);
        }
        [Fact]
        public async Task GetOrderByTracker_ReturnsServerError()
        {
            var trackNumber = "123";
            orderServiceMock .Setup(s => s.GetOrderInfoByTrackerAsync(trackNumber)).ThrowsAsync(new Exception());

            var result = await controller.GetOrderByTracker(trackNumber);

            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Ошибка сервера", statusCodeResult.Value);
        }
        [Fact]
        public async Task GetAllUserOrders_ReturnsOkResult()
        {
            var orders = new List<UserOrderResponseDTO> { new UserOrderResponseDTO() };
            orderServiceMock .Setup(s => s.GetUserOrdersAsync(It.IsAny<HttpContext>())).ReturnsAsync(orders);

            var result = await controller.GetAllUserOrders();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(orders, okResult.Value);
        }
        [Fact]
        public async Task GetAllUserOrders_ReturnsUnauthorized()
        {
            orderServiceMock.Setup(s => s.GetUserOrdersAsync(It.IsAny<HttpContext>())).ThrowsAsync(new UnauthorizedAccessException());

            var result = await controller.GetAllUserOrders();

            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(401, statusCodeResult.StatusCode);
            Assert.Equal("Ошибка аутентификации.", statusCodeResult.Value);
        }
        [Fact]
        public async Task CancelOrderAsync_ReturnsOkResult()
        {
            var orderId = 123;
            orderServiceMock.Setup(s => s.CancelOrderAsync(orderId, It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
            var result = await controller.CancelOrderAsync(orderId);
            Assert.IsType<OkResult>(result);
        }
        [Fact]
        public async Task CancelOrderAsync_ReturnsNotFound()
        {
            var orderId = 123;
            orderServiceMock.Setup(s => s.CancelOrderAsync(orderId, It.IsAny<HttpContext>())).ThrowsAsync(new EntityNotFoundException("Заказ не найден"));
            var result = await controller.CancelOrderAsync(orderId);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Заказ не найден", notFoundResult.Value);
        }
        [Fact]
        public async Task CancelOrderAsync_ReturnsForbidden_WhenUnauthorizedAccessExceptionThrown()
        {
            var orderId = 123;
            orderServiceMock.Setup(s => s.CancelOrderAsync(orderId, It.IsAny<HttpContext>())).ThrowsAsync(new UnauthorizedAccessException("Forbidden"));
            var result = await controller.CancelOrderAsync(orderId);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusCodeResult.StatusCode);
            Assert.Equal("Forbidden", statusCodeResult.Value);
        }
    }
}
