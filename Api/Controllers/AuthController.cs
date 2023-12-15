using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Models.InputModel;
using Api.Business;
using Api.Models.ViewModels;

namespace Api.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthBusiness _business;
        public AuthController(IAuthBusiness business)
        {
            _business = business;
        }

        /// <summary>
        /// Logs in a user with provided credentials.
        /// </summary>
        /// <param name="logInUser">The user credentials for login.</param>
        /// <returns>Returns the logged-in user information, including a token.</returns>
        [HttpPost()]
        [Route("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserViewModel), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> LogIn(LogInUser logInUser)
        {
            var tokenViewModel = await _business.LoginAsync(logInUser);

            if (tokenViewModel is null)
            {
                return NotFound("User not found.");
            }

            if (tokenViewModel.AccessToken is null)
            {
                return BadRequest("Incorrect password.");
            }

            return Ok(tokenViewModel);
        }

        /// <summary>
        /// Creates a new user with provided credentials.
        /// </summary>
        /// <param name="logInUser">The user credentials for creating a new account.</param>
        /// <returns>Returns the newly created user information, including a token.</returns>
        [HttpPost()]
        [Route("signin")]
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

        
    }
}