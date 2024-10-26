namespace TranzLog.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> AddAsync(T entityDTO);
        Task<T> UpdateAsync(T entityDTO);
        Task DeleteAsync(int id);
        Task<T?> GetAsync(int id);
        IEnumerable<T> GetAll(int page = 1, int pageSize = 10);
    }
}
