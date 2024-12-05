using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using TranzLog.Interfaces;

namespace TranzLog.Data
{
    public class TransactionManager : ITransactionManager
    {
        private readonly ShippingDbContext _dbContext;

        public TransactionManager(ShippingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IDbContextTransaction BeginTransaction()
        {
            return _dbContext.Database.BeginTransaction();
        }
    }
}
