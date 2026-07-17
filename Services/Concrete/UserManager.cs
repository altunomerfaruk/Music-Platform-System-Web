using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class UserManager : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserManager(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User? Authenticate(string email, string password)
        {
            var user = _userRepository.GetByEmail(email);

            if (user != null && user.Password == password)
            {
                return user;
            }

            return null;
        }

        public bool Register(User user)
        {
            var existingUser = _userRepository.GetByEmail(user.Email);

            if (existingUser != null)
            {
                return false;
            }

            _userRepository.Create(user);

            return true;
        }
    }
}
