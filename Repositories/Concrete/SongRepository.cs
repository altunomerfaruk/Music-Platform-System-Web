using Microsoft.EntityFrameworkCore;
using MusicProject.data;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;

namespace MusicProject.Repositories.Concrete
{
    public class SongRepository : GenericRepository<Song>, ISongRepository
    {
        public SongRepository(AppDbContext context)
            : base(context)
        {
        }

        public IEnumerable<Song> GetSongsSortedByAlphabet()
        {
            return _dbSet
                .AsNoTracking()

                // DEĞİŞİKLİK:
                // Albümle beraber albümün sanatçısı da yükleniyor.
                .Include(song => song.Album)
                    .ThenInclude(album => album!.Artist)

                // DEĞİŞİKLİK:
                // Şarkıya doğrudan bağlı sanatçılar filtreleme için yükleniyor.
                .Include(song => song.SongArtists)
                    .ThenInclude(songArtist => songArtist.Artist)

                // DEĞİŞİKLİK:
                // Tür filtresinin çalışabilmesi için şarkı-tür ilişkileri yükleniyor.
                .Include(song => song.SongGenres)
                    .ThenInclude(songGenre => songGenre.Genre)

                .Include(song => song.SongStat)

                .OrderBy(song => song.Title)
                .ThenByDescending(song => song.AlbumId)
                .ToList();
        }

        public List<Song> GetSongsByAlbum(int albumId)
        {
            return _dbSet
                .AsNoTracking()
                .Where(song => song.AlbumId == albumId)

                // DEĞİŞİKLİK:
                // Albüm şarkıları gösterilirken detay bilgilerinin de hazır gelmesi sağlandı.
                .Include(song => song.Album)
                    .ThenInclude(album => album!.Artist)

                .Include(song => song.SongArtists)
                    .ThenInclude(songArtist => songArtist.Artist)

                .Include(song => song.SongGenres)
                    .ThenInclude(songGenre => songGenre.Genre)

                .Include(song => song.SongStat)

                .OrderBy(song => song.Title)
                .ToList();
        }

        public List<Song> GetPopularSongs()
        {
            return _context.Songs
                .AsNoTracking()
                .Include(song => song.SongStat)
                .Include(song => song.Album)
                    .ThenInclude(album => album!.Artist)

                .Include(song => song.SongArtists)
                    .ThenInclude(songArtist => songArtist.Artist)

                .OrderByDescending(song =>
                    song.SongStat != null
                        ? song.SongStat.PopularityScore
                        : 0
                )
                .ThenBy(song => song.Title)
                .Take(5)
                .ToList();
        }

        public Song? GetSongDetailsById(int songId)
        {
            return _context.Songs
                .AsNoTracking()
                .Include(song => song.Album)
                    .ThenInclude(album => album!.Artist)
                .Include(song => song.SongArtists)
                    .ThenInclude(songArtist => songArtist.Artist)
                .Include(song => song.SongGenres)
                    .ThenInclude(songGenre => songGenre.Genre)
                .Include(song => song.SongStat)
                .FirstOrDefault(song => song.Id == songId);
        }

        public Song? GetSongForListening(int songId)
        {
            return _context.Songs
                .Include(song => song.SongStat)
                .Include(song => song.Album)
                    .ThenInclude(album => album!.Artist)
                .Include(song => song.SongArtists)
                    .ThenInclude(songArtist => songArtist.Artist)
                .Include(song => song.SongGenres)
                    .ThenInclude(songGenre => songGenre.Genre)
                .FirstOrDefault(song => song.Id == songId);
        }
    }
}