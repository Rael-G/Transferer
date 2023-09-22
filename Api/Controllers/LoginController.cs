using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Authentication.ViewModel;
using Authentication.Services;

namespace Api.Controllers
{
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        public LoginController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet()]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(SignInUser signInUser)
        {
            var user = await _userManager.FindByNameAsync(signInUser.UserName);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var passwordHasher = new PasswordHasher<IdentityUser>();
            var result = passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, signInUser.Password);

            if (result != PasswordVerificationResult.Success)
            {
                return BadRequest("Incorrect password.");
            }

            var token = TokenService.GenerateToken(user);

            var logedUser = new LogedUser { UserName = user.UserName, Token = token };

            return new OkObjectResult(logedUser);
        }

        [HttpPost()]
        [Route("signin")]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn(SignInUser signInUser)
        {
            var existing = await _userManager.Users.FirstOrDefaultAsync(
                u => u.UserName == signInUser.UserName);

            if (existing != null)
            {
                return BadRequest($"Username unavailable: {signInUser.UserName}");
            }

            IdentityUser user = new IdentityUser()
            {
                UserName = signInUser.UserName
            };

            await _userManager.CreateAsync(user, signInUser.Password);

            var token = TokenService.GenerateToken(user);
            var logedUser = new LogedUser { UserName = user.UserName, Token = token };

            return CreatedAtAction(nameof(Login), new { logedUser });
        }
    }
}