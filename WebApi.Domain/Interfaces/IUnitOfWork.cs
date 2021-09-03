using System;

namespace WebApi.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users {  get; }
        int Complete();
    }
}
