using MusicProject.Contracts.Responses;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;

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

        public bool UpdateArtistAlbum(int albumId,int artistId,string name,string? description,string? coverImageUrl,DateTime releaseDate)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException(
                    "Albüm adı boş bırakılamaz.");
            }

            var trimmedName = name.Trim();

            var duplicateAlbumExists = _albumRepository
                .GetAlbumsByArtistId(artistId)
                .Any(album =>
                    album.Id != albumId &&
                    album.Name.Equals(
                        trimmedName,
                        StringComparison.OrdinalIgnoreCase));

            if (duplicateAlbumExists)
            {
                throw new InvalidOperationException(
                    $"'{trimmedName}' adında başka bir albüm zaten mevcut.");
            }

            var trimmedDescription = string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim();

            var trimmedCoverImageUrl = string.IsNullOrWhiteSpace(coverImageUrl)
                ? null
                : coverImageUrl.Trim();

            return _albumRepository.UpdateArtistAlbum(
                albumId,
                artistId,
                trimmedName,
                trimmedDescription,
                trimmedCoverImageUrl,
                releaseDate);
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