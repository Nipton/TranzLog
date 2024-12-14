using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;
using TranzLog.Interfaces;

namespace TranzLog.Services
{
    public class DistanceCalculationService : IDistanceCalculationService
    {
        private const string GeoapifyBaseUrl = "https://api.geoapify.com/v1/routing";
        private readonly string apiKey;
        private readonly HttpClient httpClient;
        public DistanceCalculationService(IConfiguration configuration, HttpClient httpClient)
        {
            apiKey = configuration["Geoapify:ApiKey"] ?? throw new Exception("API ключ не найден в конфигурации.");
            this.httpClient = httpClient;
        }
        public async Task<(double Distance, TimeSpan Duration)> CalculateDistanceAsync(Models.Route route)
        {
            if (route == null)
            {
                throw new ArgumentNullException("Маршрут не может быть null.");
            }
            string url = $"{GeoapifyBaseUrl}?waypoints=" +$"{route.OriginLatitude.ToString(CultureInfo.InvariantCulture)}," + $"{route.OriginLongitude.ToString(CultureInfo.InvariantCulture)}|" + $"{route.DestinationLatitude.ToString(CultureInfo.InvariantCulture)}," + $"{route.DestinationLongitude.ToString(CultureInfo.InvariantCulture)}" + $"&mode=drive&apiKey={apiKey}";
            HttpResponseMessage response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Ошибка при запросе к Geoapify API.");
            }
            string jsonResponse = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(jsonResponse);
            double distance = json["features"]?[0]?["properties"]?["distance"]?.Value<double>() ?? throw new Exception("Расстояние не найдено.");
            double duration = json["features"]?[0]?["properties"]?["time"]?.Value<double>() ?? throw new Exception("Продолжительность не найдена.");
            return (distance / 1000, TimeSpan.FromSeconds(duration));
        }
    }
}
