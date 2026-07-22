using MusicProject.Models.Concrete;

namespace MusicProject.Repositories.Interface
{
    public interface ISongStatRepository
    {
        SongStat? GetBySongId(int songId);

        void Create(SongStat songStat);

        void Update(SongStat songStat);
    }
}