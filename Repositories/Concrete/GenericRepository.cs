using Microsoft.EntityFrameworkCore;
using MusicProject.data;
using MusicProject.Models.Core;
using System.Linq.Expressions;

namespace MusicProject.Repositories.Interface
{
    public class GenericRepository<T> : IGenericRepository<T>
        where T : BaseEntities
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        public T? GetByID(
            int id,
            params Expression<Func<T, object>>[] includes
        )
        // DEĞİŞİKLİK:
        // public T GetByID yerine public T? GetByID yapıldı.
        // Çünkü kayıt bulunamazsa FirstOrDefault null dönebilir.
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query.FirstOrDefault(entity => entity.Id == id);
        }

        public void Delete(int id)
        {
            var entityToDelete = _dbSet.Find(id);

            if (entityToDelete != null)
            {
                entityToDelete.IsDeleted = true;

                _context.SaveChanges();
            }
        }

        public void Create(T entity)
        {
            _dbSet.Add(entity);

            _context.SaveChanges();
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);

            _context.SaveChanges();
        }
    }
}