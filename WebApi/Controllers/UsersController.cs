using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApi.Authorization;
using WebApi.Domain.Entities;
using WebApi.Models.Users;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public IActionResult Authenticate(AuthenticateRequest model)
        {
            _logger.LogInformation("Authenticating User: {username}", model.Username);
            var response = _userService.Authenticate(model);

            return Ok(response);
        }

        [Authorize(Role.Admin)]
        [HttpPost("[action]")]
        public IActionResult Register(RegisterRequest model)
        {
            _logger.LogInformation("Registering a user: {username}", model.Username);
            _userService.Register(model);

            return Ok(new { message = "Registration successful" });
        }

        [Authorize(Role.Admin)]
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, UpdateRequest model)
        {
            _logger.LogInformation("Updating a user: {username}", model.Username);
            _userService.Update(id, model);

            return Ok(new { message = "User updated successfully" });
        }

        [Authorize(Role.Admin)]
        [HttpGet]
        public IActionResult GetAll()
        {
            _logger.LogInformation("Getting all users");
            var users = _userService.GetAll();

            return Ok(users);
        }

        [Authorize(Role.Admin)]
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            _logger.LogInformation("Delete a user with ID: {id}", id);
            
            var currentUser = (User)HttpContext.Items["User"];
            if (id == currentUser.Id)
            {
                return Unauthorized(new { message = "You can't delete your own user" });
            }

            _userService.Delete(id);

            return Ok(new { message = $"User with ID:{id} deleted" });
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            _logger.LogInformation("Getting user with ID: {id}", id);

            // only admins can access other user records
            var currentUser = (User)HttpContext.Items["User"];
            if (id != currentUser.Id && currentUser.Role != Role.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            var user = _userService.GetById(id);
            return Ok(user);
        }
    }
}
