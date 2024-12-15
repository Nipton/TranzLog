using TranzLog.Exceptions;
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
            if (cargos == null || cargos.Count() == 0)
                throw new InvalidParameterException("Не передан список грузов.");
            if(distance <= 0)
                throw new InvalidParameterException("Дистанция не может быть отрицательной или равнятся нулю.");
            double totalWeight = 0;
            double totalVolume = 0;
            foreach (Cargo cargo in cargos)
            {
                if (cargo.TotalSize < 0 || cargo.Volume < 0 || cargo.Weight < 0)
                    throw new InvalidParameterException("Некорректные данные о грузе: отрицательный вес или объём.");
                totalWeight += cargo.Weight;
                if (cargo.Volume > 0)
                    totalVolume += cargo.Volume;
                else totalVolume += cargo.TotalSize;
            }
            var cost = baseRate + (ratePerKm * distance) + (ratePerKg * totalWeight) + (ratePerCubicMeter * totalVolume);
            if (cost < baseRate)
                throw new InvalidOperationException("Ошибка во время операции подсчета стоимости. Расчетная стоимость доставки не может быть меньше базовой ставки.");
            if (double.IsInfinity(cost))
            {
                throw new OverflowException("Стоимость превышает максимально допустимое значение.");
            }
            return cost;
        }
    }
}
