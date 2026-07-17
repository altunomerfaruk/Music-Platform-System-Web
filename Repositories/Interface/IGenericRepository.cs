using MusicProject.Models.Core;
using System.Linq.Expressions;

namespace MusicProject.Repositories.Interface
{
    public interface IGenericRepository<T>
        where T : BaseEntities
    {
        IEnumerable<T> GetAll();

        T? GetByID(
            int id,
            params Expression<Func<T, object>>[] includes
        );

        void Delete(int id);

        void Create(T entity);

        void Update(T entity);
    }
}