using BCryptNet = BCrypt.Net.BCrypt;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using WebApi.Authorization;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace WebApi.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        IEnumerable<User> GetAll();
        User GetById(int id);
        void Register(RegisterRequest model);
        void Update(int id, UpdateRequest model);
        void Delete(int id);
    }

    public class UserService : IUserService
    {
        private DataContext _context;
        private IJwtUtils _jwtUtils;
        private readonly IMapper _mapper;

        public UserService(
            DataContext context,
            IJwtUtils jwtUtils,
            IMapper mapper
            )
        {
            _context = context;
            _jwtUtils = jwtUtils;
            _mapper = mapper;
        }


        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var user = _context.Users.SingleOrDefault(x => x.Username == model.Username);

            // validate
            if (user == null || !BCryptNet.Verify(model.Password, user.PasswordHash))
            {
                throw new AppException("Username or password is incorrect", model);
            }

            // authentication successful so generate jwt token
            var jwtToken = _jwtUtils.GenerateJwtToken(user);

            return new AuthenticateResponse(user, jwtToken);
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        public User GetById(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            return user;
        }

        public void Register(RegisterRequest model)
        {
            // Validate
            if (_context.Users.Any(x => x.Username == model.Username))
            {
                throw new AppException("Username '" + model.Username + "' is already taken");
            }

            // Map model to new user object
            var user = _mapper.Map<User>(model);

            // Hash password
            user.PasswordHash = BCryptNet.HashPassword(model.Password);

            // Save user
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Update(int id, UpdateRequest model)
        {
            var user = _getUser(id);

            // Validate
            if (model.Username != user.Username && _context.Users.Any(x => x.Username == model.Username))
            {
                throw new AppException("Username '" + model.Username + "' is already taken");
            }

            // Hash password if was entered
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = BCryptNet.HashPassword(model.Password);
            }

            // Copy model to user and save
            _mapper.Map(model, user);
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = _getUser(id);

            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        #region Private methods
        private User _getUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) throw new KeyNotFoundException("User not found");
            return user;
        }
        #endregion
    }
}