using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using RichardSzalay.MockHttp;
using TranzLog.Models;
using TranzLog.Services;


namespace TranzLogTests
{
    public class DistanceCalculationServiceTests 
    {
        [Fact]
        public void CalculateDistanceAsync_NoApi()
        {
            var inMemorySettings = new Dictionary<string, string>(); 
            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
            var httpClient = new HttpClient();

            var exception = Assert.Throws<Exception>(() => new DistanceCalculationService(configuration, httpClient));
            Assert.Equal("API ключ не найден в конфигурации.", exception.Message);
        }
        [Fact]
        public async void CalculateDistanceAsync_NullRoute()
        {
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c["Geoapify:ApiKey"]).Returns("YOUR_API_KEY");
            var httpClient = new HttpClient();
            var service = new DistanceCalculationService(configurationMock.Object, httpClient);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await service.CalculateDistanceAsync(null);
            });
            Assert.Contains("Маршрут не может быть null.", exception.Message);
        }
        //[Fact]
        [Fact(Skip = "Используется для ручного тестирования с реальным API")]
        public async Task TestRealApiRequest()
        {
            //пример запроса
            string test = "https://api.geoapify.com/v1/routing?waypoints=59.90404799204936,30.36357758034933|55.56597269988714,37.77273480261431&mode=drive&apiKey=YOUR_API_KEY";
            double distance = 749396;
            TimeSpan time = TimeSpan.FromSeconds(26426.347);
            Route route = new() { OriginLatitude = 59.90404799204936, OriginLongitude = 30.36357758034933, DestinationLatitude = 55.56597269988714,DestinationLongitude = 37.77273480261431 };
            using var httpClient = new HttpClient();
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c["Geoapify:ApiKey"]).Returns("YOUR_API_KEY");
            var service = new DistanceCalculationService(configurationMock.Object, httpClient);

            var result = await service.CalculateDistanceAsync(route);

            Assert.Equal(distance / 1000, result.Distance);
            Assert.Equal(time, result.Duration);
        }
        [Fact]
        public async Task CalculateDistanceAsync_ReturnsCorrectResult()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When("https://api.geoapify.com/v1/routing*").Respond("application/json", @"{
            ""features"": [
                {
                    ""properties"": {
                        ""distance"": 749396,
                        ""time"": 26426.347
                    }
                }]}");
            var mockHttpClient = new HttpClient(mockHttp);
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c["Geoapify:ApiKey"]).Returns("YOUR_API_KEY");

            var service = new DistanceCalculationService(configurationMock.Object, mockHttpClient);
            var route = new Route
            {
                OriginLatitude = 59.9,
                OriginLongitude = 30.3,
                DestinationLatitude = 55.5,
                DestinationLongitude = 37.7
            };

            var result = await service.CalculateDistanceAsync(route);

            Assert.Equal(749.396, result.Distance, 3); 
            Assert.Equal(TimeSpan.FromSeconds(26426.347), result.Duration);
        }
        [Fact]
        public async Task CalculateDistanceAsync_ThrowsException()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When("https://api.geoapify.com/v1/routing*")
                .Respond(System.Net.HttpStatusCode.BadRequest); // Симуляция ошибки 400 Bad Request

            var mockHttpClient = new HttpClient(mockHttp);
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c["Geoapify:ApiKey"]).Returns("YOUR_API_KEY");

            var service = new DistanceCalculationService(configurationMock.Object, mockHttpClient);
            var route = new Route
            {
                OriginLatitude = 59.9,
                OriginLongitude = 30.3,
                DestinationLatitude = 55.5,
                DestinationLongitude = 37.7
            };

            var exception = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await service.CalculateDistanceAsync(route);
            });

            Assert.Equal("Ошибка при запросе к Geoapify API.", exception.Message);
        }
    }

}
