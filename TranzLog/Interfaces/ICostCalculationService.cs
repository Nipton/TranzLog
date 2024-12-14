using TranzLog.Models;

namespace TranzLog.Interfaces
{
    public interface ICostCalculationService
    {
        double CalculateCost(double distance, IEnumerable<Cargo> cargos);
    }
}
