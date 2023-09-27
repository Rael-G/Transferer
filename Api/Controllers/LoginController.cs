using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Api.Models.ViewModels;
using Api.Services;
using Api.Models;

namespace Api.Controllers
{
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        public LoginController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost()]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(SignInUser signInUser)
        {
            var user = await _userManager.FindByNameAsync(signInUser.UserName);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, signInUser.Password);

            if (result != PasswordVerificationResult.Success)
            {
                return BadRequest("Incorrect password.");
            }

            var token = TokenService.GenerateToken(user, _userManager);

            var logedUser = new LogedUser { UserName = user.UserName, Token = token };

            return new OkObjectResult(logedUser);
        }

        [HttpPost()]
        [Route("signin")]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn(SignInUser signInUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            User user = new User()
            {
                UserName = signInUser.UserName
            };

            var result = await _userManager.CreateAsync(user, signInUser.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            var token = TokenService.GenerateToken(user, _userManager);
            var logedUser = new LogedUser { UserName = user.UserName, Token = token };

            return CreatedAtAction(nameof(Login), new { logedUser });
        }
    }
}