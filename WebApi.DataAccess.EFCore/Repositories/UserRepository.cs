using System.Collections.Generic;
using System.Linq;
using WebApi.Domain.Entities;
using WebApi.Domain.Interfaces;

namespace WebApi.DataAccess.EFCore.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationContext context) : base(context) { }
        public IEnumerable<User> GetAdminUsers(int count)
        {
            return _context.Users.Where(u => u.Role == Role.Admin).Take(count).ToList();
        }

        public User GetByUsername(string username)
        {
            return _context.Users.SingleOrDefault(u => u.Username == username);
        }

        public bool IsUsernameAlreadyRegistered(string username)
        {
            return _context.Users.Any(u => u.Username == username);
        }
    }
}
