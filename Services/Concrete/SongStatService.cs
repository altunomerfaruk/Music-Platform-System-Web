using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class SongStatService : ISongStatService
    {
        private readonly ISongStatRepository _songStatRepository;

        public SongStatService(
            ISongStatRepository songStatRepository)
        {
            _songStatRepository = songStatRepository;
        }

        public void UpdateLikeCount(
            int songId,
            int totalLikes)
        {
            var songStat =
                _songStatRepository.GetBySongId(songId);

            if (songStat == null)
            {
                songStat = new SongStat
                {
                    SongId = songId,
                    TotalStreams = 0,
                    TotalLikes = totalLikes
                };

                CalculatePopularityScore(songStat);

                _songStatRepository.Create(songStat);

                return;
            }

            songStat.TotalLikes = totalLikes;

            CalculatePopularityScore(songStat);

            _songStatRepository.Update(songStat);
        }

        private void CalculatePopularityScore(
            SongStat songStat)
        {
            songStat.PopularityScore =
                songStat.TotalStreams
                + (songStat.TotalLikes * 3);

        }
    }
}