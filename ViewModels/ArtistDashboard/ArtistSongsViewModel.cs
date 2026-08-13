namespace MusicProject.ViewModels.ArtistDashboard;
using MusicProject.Contracts.Responses.ArtistDashboard;


public class ArtistSongsViewModel: ArtistLayoutViewModel
    {
     public IEnumerable<ArtistSongListItemDto> Songs { get; set; }
         = new List<ArtistSongListItemDto>();

     public int TotalStreams {  get; set; }

     public int TotalLikes { get; set; }

    }
