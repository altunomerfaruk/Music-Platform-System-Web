namespace MusicProject.Models.ViewModels;
using MusicProject.Models.Concrete;


public class ArtistSongsViewModel: ArtistLayoutViewModel
    {
     public IEnumerable<Song> Songs { get; set; } = new List<Song>();

     public int TotalStreams {  get; set; }

     public int TotalLikes { get; set; }

    }

