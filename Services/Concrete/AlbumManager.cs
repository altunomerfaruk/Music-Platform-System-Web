using MusicProject.DTOs;
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

        public AlbumDetailsDto? GetAlbumDetails(int albumId)
        {
            var album = _albumRepository.GetAlbumDetailsById(albumId);

            if (album == null)
            {
                return null;
            }

            var songs = album.Songs
                .OrderByDescending(song => song.SongStat?.PopularityScore ?? 0)
                .Select(song => new AlbumSongDto
                {
                    SongId = song.Id,
                    Title = song.Title,
                    TotalStreams = song.SongStat?.TotalStreams ?? 0,
                    TotalLikes = song.SongStat?.TotalLikes ?? 0,
                    PopularityScore = song.SongStat?.PopularityScore ?? 0
                })
                .ToList();

            return new AlbumDetailsDto
            {
                AlbumId = album.Id,
                Name = album.Name,
                Description = album.Description ?? "Albüm açıklaması bulunmuyor.",
                CoverImageUrl = album.CoverImageUrl,
                ReleaseDate = album.ReleaseDate,
                ArtistId = album.ArtistId,
                ArtistName = album.Artist.Name,
                Songs = songs
            };
        }
    }
}