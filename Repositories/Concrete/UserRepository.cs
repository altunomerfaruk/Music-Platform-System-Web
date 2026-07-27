using MusicProject.data;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;

namespace MusicProject.Repositories.Concrete
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Create(User entity)
        {
            _context.users.Add(entity);
            _context.SaveChanges();
        }

        public User? GetById(int userId)
        {
            return _context.users
                .FirstOrDefault(user => user.Id == userId);
        }

        public User? GetByEmail(string email)
        {
            return _context.users
                .FirstOrDefault(user => user.Email == email);
        }

        public User? GetByUsername(string username)
        {
            return _context.users
                .FirstOrDefault(user => user.Username == username);
        }

        public bool UsernameExists(string username, int excludedUserId)
        {
            return _context.users.Any(user =>
                user.Username == username &&
                user.Id != excludedUserId);
        }

        public bool EmailExists(string email, int excludedUserId)
        {
            return _context.users.Any(user =>
                user.Email == email &&
                user.Id != excludedUserId);
        }

        public void Update(User entity)
        {
            _context.SaveChanges();
        }
    }
}