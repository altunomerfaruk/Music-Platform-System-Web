using MusicProject.data;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;
using System.Collections.Generic;
using System.Linq;

namespace MusicProject.Repositories.Concrete
{
    public class ArtistRepository : IArtistRepository
    {
        private readonly AppDbContext _context;

        public ArtistRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Artist> GetAll()
        {
            return _context.Artists.ToList();
        }

        public Artist? GetByID(int id)
        {
            return _context.Artists.Find(id);

        }

        public void Create(Artist entity)
        {
            _context.Artists.Add(entity);
            _context.SaveChanges();
        }

        public void Update(Artist entity)
        {
            _context.Artists.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var artist = _context.Artists.Find(id);

            if (artist != null)
            {
                _context.Artists.Remove(artist);
                _context.SaveChanges();
            }
        }
    }
}