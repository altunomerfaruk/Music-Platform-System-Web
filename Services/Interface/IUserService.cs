using MusicProject.Models.Concrete;

namespace MusicProject.Services.Interface
{
    public interface IUserService
    {
        // Kullanıcı bulunamazsa null dönebilir
        User? Authenticate(string email, string password);

        // Kayıt başarılıysa true, başarısızsa false döner
        bool Register(User user);
    }
}