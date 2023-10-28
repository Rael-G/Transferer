using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Models.InputModel;
using Api.Business;

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

        [HttpPost()]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LogIn(LogInUser logInUser)
        {
            var loggedUser = await _business.LoginAsync(logInUser);

            if (loggedUser == null)
            {
                return NotFound("User not found.");
            }

            if (loggedUser.Token == null)
            {
                return BadRequest("Incorrect password.");
            }

            return Ok(loggedUser);
        }

        [HttpPost()]
        [Route("signin")]
        [AllowAnonymous]
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

            var loggedUser = await _business.LoginAsync(logInUser);

            return Ok(loggedUser);
        }
    }
}