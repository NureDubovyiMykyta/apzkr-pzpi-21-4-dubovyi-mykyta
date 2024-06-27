using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using SmartOrangeryApi.Models;
using SmartOrangeryApi.Services;
using System.Security.Claims;

namespace SmartOrangeryApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            var user = new User
            {
                Name = registerModel.Name,
                Email = registerModel.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerModel.Password),
                Role = "user"
            };
            await _userService.CreateAsync(user);
            return Ok();
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var user = await _userService.GetByEmailAsync(loginModel.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginModel.Password, user.PasswordHash))
            {
                return Unauthorized();
            }

            var token = await _userService.GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> Get() =>
            Ok(await _userService.GetAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> Get(string id)
        {
            var user = await _userService.GetAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] User updatedUser)
        {
            var userId = new ObjectId(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _userService.UpdateAsync(id, updatedUser, userId);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = new ObjectId(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _userService.RemoveAsync(id, userId);

            return NoContent();
        }
    }

}