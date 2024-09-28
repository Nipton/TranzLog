namespace TranzLog.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        T GetAsync(int id);
        IEnumerable<T> GetAll();

    }
}
