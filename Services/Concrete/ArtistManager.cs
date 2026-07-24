using MusicProject.DTOs;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class ArtistManager : IArtistService
    {
        private readonly IArtistRepository _artistRepository;
        private readonly ISongRepository _songRepository;

        public ArtistManager(IArtistRepository artistRepository, ISongRepository songRepository)
        {
            _artistRepository = artistRepository;
            _songRepository = songRepository;
        }

        // --- TEMEL CRUD İŞLEMLERİ ---

        public IEnumerable<Artist> GetAllArtists()
        {
            return _artistRepository.GetAll();
        }

        public Artist? GetArtistById(int id)
        {
            return _artistRepository.GetByID(id);
        }

        public void AddArtist(Artist artist)
        {
            _artistRepository.Create(artist);
        }

        public void UpdateArtist(Artist artist)
        {
            _artistRepository.Update(artist);
        }

        public void DeleteArtist(int id)
        {
            _artistRepository.Delete(id);
        }

        public int GetTotalSongCount(int artistId)
        {
            return _songRepository
                .GetAll()
                .Count(song => song.SongArtists.Any(songArtist => songArtist.ArtistId == artistId));
        }

        // Sanatçı detay sayfası için gereken bütün bilgileri DTO'ya çevirir.
        public ArtistDetailsDto? GetArtistDetails(int artistId)
        {
            var artist = _artistRepository.GetArtistDetailsById(artistId);

            if (artist == null)
            {
                return null;
            }

            var albums = artist.Albums
                .OrderByDescending(album => album.ReleaseDate)
                .Select(album => new ArtistAlbumDto
                {
                    AlbumId = album.Id,
                    Name = album.Name,
                    Description = album.Description ?? "Albüm açıklaması bulunmuyor.",
                    ReleaseDate = album.ReleaseDate,
                    SongCount = album.Songs.Count
                })
                .ToList();
            
            // Şarkılar Albums üzerinden değil SongArtists üzerinden alınıyor.
            var songs = artist.SongArtists
                .Select(songArtist => songArtist.Song)
                .DistinctBy(song => song.Id)
                .Select(song => new ArtistSongDto
                {
                    SongId = song.Id,
                    Title = song.Title,

                    // Şarkının albümü yoksa ekranda "Single" gösterilir.
                    AlbumName = song.Album?.Name ?? "Single",

                    TotalStreams = song.SongStat?.TotalStreams ?? 0,
                    TotalLikes = song.SongStat?.TotalLikes ?? 0,
                    PopularityScore = song.SongStat?.PopularityScore ?? 0
                })
                .OrderByDescending(song => song.PopularityScore)
                .ToList();

            return new ArtistDetailsDto
            {
                ArtistId = artist.Id,
                Name = artist.Name,
                Country = artist.Country ?? "Ülke bilgisi yok",
                DebutYear = artist.DebutYear,

                TotalFollowers = artist.Followers
                    .Count(follower => follower.IsActive),

                Albums = albums,
                Songs = songs
            };
        }
    }
}