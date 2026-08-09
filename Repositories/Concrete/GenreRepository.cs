using Microsoft.EntityFrameworkCore;
using MusicProject.Data;
using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;
using MusicProject.Repositories.Interface;

namespace MusicProject.Repositories.Concrete
{
    public class GenreRepository : IGenreRepository
    {
        private readonly AppDbContext _context;

        public GenreRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Genre> GetAllGenres()
        {
            return _context.Genres
                .AsNoTracking()
                .Include(genre => genre.SongGenres)
                .OrderBy(genre => genre.Name)
                .ToList();
        }

        public Genre? GetGenreDetailsById(int genreId)
        {
            return _context.Genres
                .AsNoTracking()
                .Include(genre => genre.SongGenres
                    .Where(songGenre =>
                        songGenre.Song.PublicationStatus == PublicationStatus.Published &&
                        (songGenre.Song.AlbumId == null ||
                         songGenre.Song.Album!.PublicationStatus == PublicationStatus.Published)))
                    .ThenInclude(songGenre => songGenre.Song)
                        .ThenInclude(song => song.Album)
                            .ThenInclude(album => album!.Artist)
                .Include(genre => genre.SongGenres
                    .Where(songGenre =>
                        songGenre.Song.PublicationStatus == PublicationStatus.Published &&
                        (songGenre.Song.AlbumId == null ||
                         songGenre.Song.Album!.PublicationStatus == PublicationStatus.Published)))
                    .ThenInclude(songGenre => songGenre.Song)
                        .ThenInclude(song => song.SongArtists)
                            .ThenInclude(songArtist => songArtist.Artist)
                .Include(genre => genre.SongGenres
                    .Where(songGenre =>
                        songGenre.Song.PublicationStatus == PublicationStatus.Published &&
                        (songGenre.Song.AlbumId == null ||
                         songGenre.Song.Album!.PublicationStatus == PublicationStatus.Published)))
                    .ThenInclude(songGenre => songGenre.Song)
                        .ThenInclude(song => song.SongStat)
                .FirstOrDefault(genre => genre.Id == genreId);
        }
    }
}