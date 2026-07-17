using MusicProject.Models.Concrete;

namespace MusicProject.Repositories.Interface
{
    public interface IUserRepository
    {
        void Create(User entity);
        User GetByEmail(string email); // Giriş yaparken e-postayı bulmak için
        User GetByUsername(string username); // Kayıt olurken bu isimde biri var mı diye bakmak için
    }
}