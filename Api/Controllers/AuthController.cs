using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Models.InputModel;
using Api.Models.ViewModels;
using Api.Interfaces.Services;

namespace Api.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _business;
        public AuthController(IAuthService business)
        {
            _business = business;
        }

        /// <summary>
        /// Logs in a user with provided credentials.
        /// </summary>
        /// <param name="logInUser">The user credentials for login.</param>
        /// <returns>Returns the logged-in user information, including a token.</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserViewModel), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> LogIn(LogInUser logInUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tokenViewModel = await _business.LoginAsync(logInUser);

            if (tokenViewModel is null)
            {
                return BadRequest("Wrong User or Password.");
            }

            return Ok(tokenViewModel);
        }

        /// <summary>
        /// Creates a new user with provided credentials.
        /// </summary>
        /// <param name="logInUser">The user credentials for creating a new account.</param>
        /// <returns>Returns the newly created user information, including a token.</returns>
        [HttpPost("signin")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserViewModel), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SignIn(LogInUser logInUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _business.CreateAsync(logInUser);

            if (!string.IsNullOrEmpty(result))
            {
                return BadRequest(result);
            }

            var tokenVM = await _business.LoginAsync(logInUser);

            return Ok(tokenVM);
        }

        /// <summary>
        /// Controller endpoint for regenerating a new token based on the provided input model.
        /// </summary>
        /// <param name="tokenInput">The input model containing the access and refresh tokens.</param>
        /// <returns>Returns the regenerated token information.</returns>
        [HttpPost("regen-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Token), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RegenerateToken(TokenInputModel tokenInput)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tokenView = await _business.RegenarateTokenAsync(tokenInput);

            if (tokenView is null)
            {
                return BadRequest("Invalid Token");
            }

            return Ok(tokenView);
        }
    }
}