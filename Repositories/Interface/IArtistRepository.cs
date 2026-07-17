using MusicProject.Models.Concrete;
using System.Collections.Generic;

namespace MusicProject.Repositories.Interface
{
    public interface IArtistRepository
    {
        IEnumerable<Artist> GetAll();
        Artist? GetByID(int id);
        void Create(Artist entity);
        void Update(Artist entity);
        void Delete(int id);
    }
}