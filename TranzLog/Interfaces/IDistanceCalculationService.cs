namespace TranzLog.Interfaces
{
    public interface IDistanceCalculationService
    {
        public Task<(double Distance, TimeSpan Duration)> CalculateDistanceAsync(Models.Route route);
    }
}
