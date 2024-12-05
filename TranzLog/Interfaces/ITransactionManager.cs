using Microsoft.EntityFrameworkCore.Storage;

namespace TranzLog.Interfaces
{
    public interface ITransactionManager
    {
        IDbContextTransaction BeginTransaction();
    }
}
