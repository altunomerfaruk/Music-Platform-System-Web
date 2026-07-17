using MusicProject.data;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;
using System.Linq;

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
    }
}