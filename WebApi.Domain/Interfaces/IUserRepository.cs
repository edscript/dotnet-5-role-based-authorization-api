using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WebApi.Domain.Entities;

namespace WebApi.Domain.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        IEnumerable<User> GetAdminUsers(int count);
        User GetByUsername(string username);
        bool IsUsernameAlreadyRegistered(string username);
    }
}
