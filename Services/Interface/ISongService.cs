using MusicProject.Models.Concrete;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicProject.Services.Interface
{
    public interface ISongService
    {
        // Temel CRUD işlemleri
        IEnumerable<Song> GetAllSongs();
        Song GetSongById(int id);
        void AddSong(Song song);
        void UpdateSong(Song song);
        void DeleteSong(int id);

        List<Song> GetSongsByAlbum(int albumId);
        IEnumerable<Song> GetSongsSortedByAlphabet();
    }
}