using MusicProject.Contracts.Responses.UserDashboard;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;
using MusicProject.Contracts.Requests;

namespace MusicProject.Services.Concrete
{
    public class AlbumManager : IAlbumService
    {
        private readonly IAlbumRepository _albumRepository;

        public AlbumManager(IAlbumRepository albumRepository)
        {
            _albumRepository = albumRepository;
        }

        public IEnumerable<Album> GetAllAlbums()
        {
            return _albumRepository.GetAllAlbums();
        }

        public IEnumerable<Album> SearchAlbumsByText(string query, int? maxResults)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return [];
            }

            return _albumRepository.SearchVisibleAlbumsByText(query, maxResults);
        }

        public IEnumerable<Album> GetAlbumsByArtistId(int artistId)
        {
            if (artistId <= 0)
            {
                return new List<Album>();
            }

            return _albumRepository.GetAlbumsByArtistId(artistId);
        }

        public Album? GetArtistAlbumDetails(int albumId, int artistId)
        {
            if (albumId <= 0 || artistId <= 0)
            {
                return null;
            }

            return _albumRepository.GetArtistAlbumDetails(albumId, artistId);
        }

        public void AddAlbum(Album album)
        {
            if (string.IsNullOrWhiteSpace(album.Name))
            {
                throw new InvalidOperationException(
                    "Albüm adı boş bırakılamaz."
                );
            }

            var albumExists = _albumRepository
                .GetAlbumsByArtistId(album.ArtistId)
                .Any(existingAlbum =>
                    existingAlbum.Name.Equals(
                        album.Name,
                        StringComparison.OrdinalIgnoreCase
                    ));

            if (albumExists)
            {
                throw new InvalidOperationException(
                    $"'{album.Name}' adında bir albüm zaten mevcut."
                );
            }

            _albumRepository.Create(album);
        }

        public bool UpdateArtistAlbum(UpdateAlbumRequest request)
        {
            if (request.AlbumId <= 0 || request.ArtistId <= 0)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Albüm adı boş bırakılamaz.");
            }

            var trimmedName = request.Name.Trim();

            var duplicateAlbumExists = _albumRepository
                .GetAlbumsByArtistId(request.ArtistId)
                .Any(album =>
                    album.Id != request.AlbumId &&
                    album.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase));

            if (duplicateAlbumExists)
            {
                throw new InvalidOperationException(
                    $"'{trimmedName}' adında başka bir albüm zaten mevcut.");
            }

            request.Name = trimmedName;

            request.Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();

            request.CoverImageUrl = string.IsNullOrWhiteSpace(request.CoverImageUrl)
                ? null
                : request.CoverImageUrl.Trim();

            return _albumRepository.UpdateArtistAlbum(request);
        
        
        }

        public Album? GetAlbumById(int albumId)
        {
            if (albumId <= 0)
            {
                return null;
            }

            return _albumRepository.GetAlbumById(albumId);
        }

        public bool UpdatePublication(Album album)
        {
            if (album.Id <= 0)
            {
                return false;
            }

            return _albumRepository.UpdatePublication(album);
        }



        public bool DeleteArtistAlbum(int albumId, int artistId)
        {
            return _albumRepository.DeleteArtistAlbum(albumId, artistId);
        }

        public AlbumDetailsDto? GetAlbumDetails(int albumId)
        {
            var album = _albumRepository.GetAlbumDetailsById(albumId);

            if (album == null)
            {
                return null;
            }

            var songs = album.Songs
                .OrderByDescending(song =>
                    song.SongStat?.PopularityScore ?? 0)
                .Select(song => new AlbumSongDto
                {
                    SongId = song.Id,
                    Title = song.Title,
                    TotalStreams = song.SongStat?.TotalStreams ?? 0,
                    TotalLikes = song.SongStat?.TotalLikes ?? 0,
                    PopularityScore =
                        song.SongStat?.PopularityScore ?? 0
                })
                .ToList();

            return new AlbumDetailsDto
            {
                AlbumId = album.Id,
                Name = album.Name,
                Description = album.Description ??
                              "Albüm açıklaması bulunmuyor.",
                CoverImageUrl = album.CoverImageUrl,
                ReleaseDate = album.ReleaseDate,
                ArtistId = album.ArtistId,
                ArtistName = album.Artist.Name,
                Songs = songs
            };
        }
    }
}