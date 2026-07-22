using MusicProject.data;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;


namespace MusicProject.Repositories.Concrete
{
    public class SongStatRepository : ISongStatRepository
    {
        private readonly AppDbContext _context;

        public SongStatRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public SongStat? GetBySongId(int songId)
        {
            return _context.SongStats
                .FirstOrDefault(songStat =>
                    songStat.SongId == songId
                );
        }

        public void Create(SongStat songStat)
        {
            _context.SongStats.Add(songStat);

            _context.SaveChanges();
        }

        public void Update(SongStat songStat)
        {
            _context.SongStats.Update(songStat);

            _context.SaveChanges();
        }
    }
}