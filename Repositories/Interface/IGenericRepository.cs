using MusicProject.Models.Core;
using System.Linq.Expressions;

namespace MusicProject.Repositories.Interface
{
    public interface IGenericRepository<T> where T : BaseEntities
    {



        public IEnumerable<T> GetAll();
        T GetByID(int id, params Expression<Func<T, object>>[] includes);
        public void Delete(int id);
        public void Create(T entity);
        public void Update(T entity);



    }
}
