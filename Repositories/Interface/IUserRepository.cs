using MusicProject.Models.Concrete;

namespace MusicProject.Repositories.Interface
{
    public interface IUserRepository
    {
        void Create(User entity);

        User? GetById(int userId);

        User? GetByEmail(string email);

        User? GetByUsername(string username);

        bool UsernameExists(string username, int excludedUserId);

        bool EmailExists(string email, int excludedUserId);

        void Update(User entity);
    }
}