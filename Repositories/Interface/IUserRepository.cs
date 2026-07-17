using MusicProject.Models.Concrete;

namespace MusicProject.Repositories.Interface
{
    public interface IUserRepository
    {
        void Create(User entity);

        User? GetByEmail(string email);

        User? GetByUsername(string username);

    }
}