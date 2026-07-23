using MusicProject.Models.Concrete;
using MusicProject.DTOs;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class LikedSongService : ILikedSongService
    {
        private readonly ILikedSongRepository _likedSongRepository;
        private readonly ISongStatService _songStatService;

        public LikedSongService(
            ILikedSongRepository likedSongRepository,
            ISongStatService songStatService)
        {
            _likedSongRepository = likedSongRepository;
            _songStatService = songStatService;
        }

        public bool ToggleLike(
            int userId,
            int songId)
        {
            var existingLike =
                _likedSongRepository.GetByUserAndSong(
                    userId,
                    songId
                );

            bool isNowLiked;

            if (existingLike == null)
            {
                var likedSong = new LikedSong
                {
                    UserId = userId,
                    SongId = songId,
                    LikedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _likedSongRepository.Create(likedSong);

                isNowLiked = true;
            }
            else
            {
                existingLike.IsActive =
                    !existingLike.IsActive;

                if (existingLike.IsActive)
                {
                    existingLike.LikedAt =
                        DateTime.UtcNow;
                }

                _likedSongRepository.Update(existingLike);

                isNowLiked = existingLike.IsActive;
            }

            var activeLikeCount =
                _likedSongRepository
                    .GetActiveLikeCountBySong(songId);

            _songStatService.UpdateLikeCount(
                songId,
                activeLikeCount
            );

            return isNowLiked;
        }

        public IEnumerable<int> GetActiveLikedSongIds(
            int userId)
        {
            return _likedSongRepository
                .GetActiveLikedSongIdsByUser(userId);
        }

        public IEnumerable<LikedSongDto> GetLikedSongsByUser(
            int userId)
        {
            var likedSongs =
                _likedSongRepository
                    .GetActiveLikedSongsByUser(userId);

            return likedSongs
                .Select(likedSong =>
                    new LikedSongDto
                    {
                        SongId = likedSong.SongId,

                        Title =
                            likedSong.Song.Title,

                        AlbumName =
                            likedSong.Song.Album?.Name
                            ?? "Albüm bilgisi yok",

                        ArtistName =
                            likedSong.Song
                                .SongArtists
                                .Select(songArtist =>
                                    songArtist.Artist.Name
                                )
                                .FirstOrDefault()
                            ?? "Sanatçı bilgisi yok",

                        TotalStreams =
                            likedSong.Song
                                .SongStat?
                                .TotalStreams
                            ?? 0,

                        LikedAt =
                            likedSong.LikedAt
                    }
                )
                .ToList();
        }
    }
}