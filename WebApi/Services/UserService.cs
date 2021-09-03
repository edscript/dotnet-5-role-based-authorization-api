using BCryptNet = BCrypt.Net.BCrypt;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using WebApi.Authorization;
using WebApi.Domain.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;
using AutoMapper;
using Microsoft.Extensions.Logging;
using WebApi.Domain.Interfaces;

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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtUtils _jwtUtils;
        private readonly IMapper _mapper;

        public UserService(
            IJwtUtils jwtUtils,
            IMapper mapper,
            IUnitOfWork unitOfWork
            )
        {
            _jwtUtils = jwtUtils;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }


        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var user = _unitOfWork.Users.GetByUsername(model.Username);

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
            return _unitOfWork.Users.GetAll();
        }

        public User GetById(int id)
        {
            var user = _unitOfWork.Users.GetById(id);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            return user;
        }

        public void Register(RegisterRequest model)
        {
            // Validate
            if (_unitOfWork.Users.IsUsernameAlreadyRegistered(model.Username))
            {
                throw new AppException("Username '" + model.Username + "' is already taken");
            }

            // Map model to new user object
            var user = _mapper.Map<User>(model);

            // Hash password
            user.PasswordHash = BCryptNet.HashPassword(model.Password);

            // Save user
            _unitOfWork.Users.Add(user);
            _unitOfWork.Complete();
        }

        public void Update(int id, UpdateRequest model)
        {
            var user = _getUser(id);

            // Validate
            if (model.Username != user.Username && _unitOfWork.Users.IsUsernameAlreadyRegistered(model.Username))
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
            _unitOfWork.Complete();
        }

        public void Delete(int id)
        {
            var user = _getUser(id);

            _unitOfWork.Users.Remove(user);
            _unitOfWork.Complete();
        }

        #region Private methods
        private User _getUser(int id)
        {
            var user = _unitOfWork.Users.GetById(id);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            return user;
        }
        #endregion
    }
}