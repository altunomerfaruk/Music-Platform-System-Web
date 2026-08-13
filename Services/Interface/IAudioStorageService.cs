using Microsoft.AspNetCore.Http;

namespace MusicProject.Services.Interface
{
    public interface IAudioStorageService
    {
        Task<string> SaveMp3Async(IFormFile audioFile);
        void Delete(string? storedFileName);
        Stream? OpenRead(string? storedFileName);
    }
}