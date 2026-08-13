using MusicProject.Contracts.Requests;
using MusicProject.Contracts.Responses.ArtistDashboard;
using MusicProject.Contracts.Responses.UserDashboard;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class ArtistManager : IArtistService
    {
        private readonly IArtistRepository _artistRepository;
        private readonly ISongRepository _songRepository;
        private readonly ICountryRepository _countryRepository;

        public ArtistManager(
            IArtistRepository artistRepository,
            ISongRepository songRepository,
            ICountryRepository countryRepository)
        {
            _artistRepository = artistRepository;
            _songRepository = songRepository;
            _countryRepository = countryRepository;
        }

        public IEnumerable<Artist> SearchArtists(ArtistSearchRequest request)
        {
            return _artistRepository.SearchArtists(request);
        }

        public int GetArtistCount()
        {
            return _artistRepository.GetArtistCount();
        }

        public IEnumerable<Artist> SearchArtistsByText(string query, int? maxResults)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return [];
            }

            return _artistRepository.SearchArtistsByText(query, maxResults);
        }

        public IEnumerable<Artist> GetFeaturedArtists(int count)
        {
            return count <= 0 ? [] : _artistRepository.GetFeaturedArtists(count);
        }

        public IEnumerable<string> GetUsedCountryNames()
        {
            return _artistRepository.GetUsedCountryNames();
        }

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
                .Count(song =>
                    song.SongArtists.Any(songArtist =>
                        songArtist.ArtistId == artistId));
        }

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
                    Description = album.Description
                        ?? "Albüm açıklaması bulunmuyor.",
                    ReleaseDate = album.ReleaseDate,
                    SongCount = album.Songs.Count
                })
                .ToList();

            var songs = artist.SongArtists
                .Select(songArtist => songArtist.Song)
                .DistinctBy(song => song.Id)
                .Select(song => new ArtistSongDto
                {
                    SongId = song.Id,
                    Title = song.Title,
                    AlbumName = song.Album?.Name ?? "Single",
                    TotalStreams = song.SongStat?.TotalStreams ?? 0,
                    TotalLikes = song.SongStat?.TotalLikes ?? 0,
                    PopularityScore =
                        song.SongStat?.PopularityScore ?? 0
                })
                .OrderByDescending(song => song.PopularityScore)
                .ToList();

            return new ArtistDetailsDto
            {
                ArtistId = artist.Id,
                Name = artist.Name,

                Country = artist.CountryEntity?.Name
                    ?? "Ülke bilgisi yok",

                DebutYear = artist.DebutYear,
                TotalFollowers = artist.Followers
                    .Count(follower => follower.IsActive),
                Albums = albums,
                Songs = songs
            };
        }

        public bool UpdateArtistProfile(
            int userId,
            string name,
            int? countryId,
            int? debutYear)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            if (countryId.HasValue &&
                !_countryRepository.Exists(countryId.Value))
            {
                return false;
            }

            var trimmedName = name.Trim();

            return _artistRepository.UpdateProfileByUserId(
                userId,
                trimmedName,
                countryId,
                debutYear);
        }

        public ArtistDashboardDto? GetArtistDashboard(int userId)
        {
            var artist =
                _artistRepository.GetArtistDashboardByUserId(userId);

            if (artist == null)
            {
                return null;
            }

            var songs = artist.SongArtists
                .Select(songArtist => songArtist.Song)
                .DistinctBy(song => song.Id)
                .ToList();

            var albumSongs = artist.Albums
                .SelectMany(album => album.Songs)
                .ToList();

            songs = songs
                .Concat(albumSongs)
                .DistinctBy(song => song.Id)
                .ToList();

            var totalStreams = songs
                .Sum(song => song.SongStat?.TotalStreams ?? 0);

            var totalLikes = songs
                .Sum(song => song.SongStat?.TotalLikes ?? 0);

            var popularSongs = songs
                .OrderByDescending(song =>
                    song.SongStat?.PopularityScore ?? 0)
                .ThenByDescending(song =>
                    song.SongStat?.TotalStreams ?? 0)
                .ThenBy(song => song.Title)
                .Take(5)
                .Select(song => new ArtistSongListItemDto
                {
                    SongId = song.Id,
                    Title = song.Title,
                    AlbumId = song.AlbumId,
                    AlbumName = song.Album?.Name ?? string.Empty,
                    TotalStreams = song.SongStat?.TotalStreams ?? 0,
                    TotalLikes = song.SongStat?.TotalLikes ?? 0,
                    PopularityScore = song.SongStat?.PopularityScore ?? 0,
                    PublicationStatus = song.PublicationStatus,
                    CreatedAt = song.CreatedAt,
                    IsAdminHidden = song.IsAdminHidden,
                    AdminHiddenReason = song.AdminHiddenReason,
                    IsHiddenByAlbum = song.Album?.IsAdminHidden ?? false,
                    AlbumAdminHiddenReason = song.Album?.AdminHiddenReason
                })
                .ToList();

            var recentAlbums = artist.Albums
                .OrderByDescending(album => album.ReleaseDate)
                .Take(4)
                .Select(album => new ArtistAlbumListItemDto
                {
                    AlbumId = album.Id,
                    Name = album.Name,
                    Description = album.Description,
                    CoverImageUrl = album.CoverImageUrl,
                    ReleaseDate = album.ReleaseDate,
                    SongCount = album.Songs.Count,
                    TotalStreams = album.Songs
                        .Sum(song => song.SongStat?.TotalStreams ?? 0),
                    IsAdminHidden = album.IsAdminHidden,
                    AdminHiddenReason = album.AdminHiddenReason,
                    AdminHiddenAtUtc = album.AdminHiddenAtUtc
                })
                .ToList();

            return new ArtistDashboardDto
            {
                Artist = new ArtistProfileDto
                {
                    ArtistId = artist.Id,
                    Name = artist.Name,
                    CountryId = artist.CountryId,
                    Country = artist.CountryEntity?.Name ?? string.Empty,
                    DebutYear = artist.DebutYear
                },
                PopularSongs = popularSongs,
                RecentAlbums = recentAlbums,
                TotalAlbums = artist.Albums.Count,
                TotalSongs = songs.Count,
                TotalStreams = totalStreams,
                TotalLikes = totalLikes,
                MonthlyListeners =
                    artist.ArtistStat?.MonthlyListeners ?? 0,
                TotalFollowers = artist.Followers
                    .Count(follower => follower.IsActive)
            };
        }
    }
}
