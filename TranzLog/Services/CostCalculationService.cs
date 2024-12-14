using TranzLog.Interfaces;
using TranzLog.Models;

namespace TranzLog.Services
{
    public class CostCalculationService : ICostCalculationService
    {
        const double baseRate = 500; // Базовая ставка
        const double ratePerKm = 10; // Цена за километр
        const double ratePerKg = 2;  // Цена за килограмм
        const double ratePerCubicMeter = 3; // Цена за кубический метр
        public double CalculateCost(double distance, IEnumerable<Cargo> cargos)
        {
            double totalWeight = 0;
            double totalVolume = 0;
            foreach (Cargo cargo in cargos)
            {
                totalWeight += cargo.Weight;
                if (cargo.Volume > 0)
                    totalVolume += cargo.Volume;
                else totalVolume += cargo.TotalSize;
            }
            return baseRate + (ratePerKm * distance) + (ratePerKg * totalWeight) + (ratePerCubicMeter * totalVolume);
        }
    }
}
