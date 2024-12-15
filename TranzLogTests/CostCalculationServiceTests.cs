using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TranzLog.Exceptions;
using TranzLog.Models;
using TranzLog.Services;

namespace TranzLogTests
{
    public class CostCalculationServiceTests
    {
        [Fact]
        public void CalculateCost_NullOrEmptyCargo()
        {
            var service = new CostCalculationService();
            var exception = Assert.Throws<InvalidParameterException>(() => service.CalculateCost(10, null));
            Assert.Equal("Не передан список грузов.", exception.Message);
            var exception2 = Assert.Throws<InvalidParameterException>(() => service.CalculateCost(10, new List<Cargo> { }));
            Assert.Equal("Не передан список грузов.", exception2.Message);
        }
        [Fact]
        public void CalculateCost_NegativeDistance()
        {
            var cargo = new List<Cargo>() { new Cargo { Id = 1} };
            var service = new CostCalculationService();
            var exception = Assert.Throws<InvalidParameterException>(() => service.CalculateCost(-10, cargo));
            Assert.Equal("Дистанция не может быть отрицательной или равнятся нулю.", exception.Message);
            var exception2 = Assert.Throws<InvalidParameterException>(() => service.CalculateCost(0, cargo));
            Assert.Equal("Дистанция не может быть отрицательной или равнятся нулю.", exception2.Message);
        }
        [Fact]
        public void CalculateCost_ReturnsCorrectCost()
        {
            var cargos = new List<Cargo>{new Cargo { Id = 1, Weight = 10, Volume = 5 }, new Cargo { Id = 2, Weight = 20, Volume = 10 }};
            var service = new CostCalculationService();
            var cost = service.CalculateCost(100, cargos);
            Assert.Equal(1605, cost);
        }
        [Fact]
        public void CalculateCost_InvalidCargoValues()
        {
            var cargos = new List<Cargo> { new Cargo { Id = 1, Weight = -10, Volume = 5 } };
            var cargos2 = new List<Cargo> { new Cargo { Id = 1, Weight = 10, Volume = -5 } };
            var service = new CostCalculationService();
            var exception = Assert.Throws<InvalidParameterException>(() => service.CalculateCost(10, cargos));
            Assert.Equal("Некорректные данные о грузе: отрицательный вес или объём.", exception.Message);
            var exception2 = Assert.Throws<InvalidParameterException>(() => service.CalculateCost(10, cargos2));
            Assert.Equal("Некорректные данные о грузе: отрицательный вес или объём.", exception2.Message);
        }
    }
}
