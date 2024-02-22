using Api.Models.InputModel;
using Api.Models.ViewModels;
using Application.Interfaces.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _business;
        private readonly IMapper _mapper;

        public UsersController(IUserService business)
        {
            _business = business;  
        }

        /// <summary>
        /// Retrieves user details by ID for admin users.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>Returns the details of the specified user for admin users.</returns>
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "admin")]
        [HttpGet("get")]
        [ProducesResponseType(typeof(UserViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(id);

            var user = await _business.GetAsync(id);
            if (user == null)
                return NotFound($"user not found. User Id: {id}");

            var userViewModel = UserViewModel.MapToViewModel(user);
            return Ok(userViewModel);
        }

        /// <summary>
        /// Searches for users by name for admin users.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>Returns a list of users matching the specified name for admin users.</returns>
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "admin")]
        [HttpGet("search")]
        [ProducesResponseType(typeof(UserViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Search(string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest(name);

            var user = await _business.SearchAsync(name);
            if (user == null)
                return NotFound($"User not found: {name}");

            var userViewModel = UserViewModel.MapToViewModel(user);

            return Ok(userViewModel);
        }

        /// <summary>
        /// Edits user details for the authenticated user.
        /// </summary>
        /// <param name="userInput">The updated user details.</param>
        /// <returns>Returns a success response if the update is successful for the authenticated user.</returns>
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("edit")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Edit(UserInputModel userInput)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var claimId = _business.GetUserIdFromClaims(User);

            var user = await _business.GetAsync(claimId);

            if (user == null)
                return NotFound($"User not found. User Id: {claimId}");

            user.UserName = userInput.UserName;

            var result = await _business.EditAsync(user, userInput.OldPassword, userInput.NewPassword);
            if (!result.Succeeded) 
                return BadRequest(result);

            return NoContent();
        }

        /// <summary>
        /// Removes a user by ID for admin users or removes the authenticated user.
        /// </summary>
        /// <param name="id">The ID of the user to remove.</param>
        /// <returns>Returns a success response if the removal is successful.</returns>
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpDelete("delete")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Remove(string id)
        {
            var claimId = _business.GetUserIdFromClaims(User);
            if (claimId != id && !_business.IsInRole("admin", User))
                return Unauthorized();

            var updatedUser = await _business.RemoveAsync(id);
            if (updatedUser == null)
                return NotFound($"User not found. User Id: {id}");

            return NoContent();
        }
    }
}
