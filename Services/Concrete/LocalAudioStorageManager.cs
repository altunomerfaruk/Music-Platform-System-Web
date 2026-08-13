using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class LocalAudioStorageManager : IAudioStorageService
    {
        private const long MaxFileSizeBytes = 30 * 1024 * 1024;

        private readonly string _audioStoragePath;

        public LocalAudioStorageManager(IWebHostEnvironment environment)
        {
            _audioStoragePath = Path.Combine(
                environment.ContentRootPath,
                "Storage",
                "Audio");

            Directory.CreateDirectory(_audioStoragePath);
        }

        public async Task<string> SaveMp3Async(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
                throw new InvalidOperationException("Bir MP3 dosyası seçmelisiniz.");

            if (audioFile.Length > MaxFileSizeBytes)
                throw new InvalidOperationException("MP3 dosyası en fazla 30 MB olabilir.");

            var extension = Path.GetExtension(audioFile.FileName);

            if (!string.Equals(extension, ".mp3", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Yalnızca MP3 dosyası yükleyebilirsiniz.");

            if (!await HasValidMp3SignatureAsync(audioFile))
                throw new InvalidOperationException("Yüklenen dosya geçerli bir MP3 dosyası değil.");

            var storedFileName = $"{Guid.NewGuid():N}.mp3";
            var fullPath = GetFullPath(storedFileName);

            await using var outputStream = new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true);

            await audioFile.CopyToAsync(outputStream);

            return storedFileName;
        }

        public void Delete(string? storedFileName)
        {
            if (string.IsNullOrWhiteSpace(storedFileName))
                return;

            var fullPath = GetFullPath(storedFileName);

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        public Stream? OpenRead(string? storedFileName)
        {
            if (string.IsNullOrWhiteSpace(storedFileName))
                return null;

            var fullPath = GetFullPath(storedFileName);

            if (!File.Exists(fullPath))
                return null;

            return new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                useAsync: true);
        }

        private string GetFullPath(string storedFileName)
        {
            var safeFileName = Path.GetFileName(storedFileName);

            if (!string.Equals(safeFileName, storedFileName, StringComparison.Ordinal))
                throw new InvalidOperationException("Geçersiz ses dosyası adı.");

            return Path.Combine(_audioStoragePath, safeFileName);
        }

        private static async Task<bool> HasValidMp3SignatureAsync(IFormFile audioFile)
        {
            await using var stream = audioFile.OpenReadStream();

            var header = new byte[3];
            var bytesRead = await stream.ReadAsync(header.AsMemory(0, header.Length));

            if (bytesRead < 2)
                return false;

            var hasId3Header =
                bytesRead >= 3 &&
                header[0] == 0x49 &&
                header[1] == 0x44 &&
                header[2] == 0x33;

            var hasMp3FrameHeader =
                header[0] == 0xFF &&
                (header[1] & 0xE0) == 0xE0;

            return hasId3Header || hasMp3FrameHeader;
        }
    }
}