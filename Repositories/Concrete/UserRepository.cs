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

        public User GetByEmail(string email)
        {
            // Veritabanında bu e-postaya sahip ilk kişiyi getir, yoksa null döndür
            return _context.users.FirstOrDefault(x => x.Email == email);
        }

        public User GetByUsername(string username)
        {
            return _context.users.FirstOrDefault(x => x.Username == username);
        }
    }
}