using ProjectCore.Data;

namespace ProjectCore.Shared.Repositories
{
    public class BaseRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;

        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            T? t = await _context.Set<T>().FindAsync(id);
            return t;
        }

    }
}
