using Api.Business;
using Api.Models.InputModel;
using Api.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserBusiness _business;

        public UsersController(IUserBusiness business)
        {
            _business = business;  
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "admin")]
        [HttpGet("get")]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(id);
            }
            var user = await _business.GetAsync(id);
            if (user == null)
            {
                return NotFound($"user not found. User Id: {id}");
            }
            var userVM = UserViewModel.MapToViewModel(user);
            return Ok(userVM);
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "admin")]
        [HttpGet("search")]
        public async Task<IActionResult> Search(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest(name);
            }
            var user = await _business.SearchAsync(name);
            if (user == null)
            {
                return NotFound($"User not found: {name}");
            }
            var userVM = UserViewModel.MapToViewModel(user);
            return Ok(userVM);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("edit")]
        public async Task<IActionResult> Edit(UserInputModel userInput)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var claimId = _business.GetUserIdFromClaims(User);

            var user = await _business.GetAsync(claimId);

            if (user == null)
            {
                return NotFound($"User not found. User Id: {claimId}");
            }

            var result = await _business.EditAsync(user, userInput);
            if (!result.IsNullOrEmpty()) 
            { 
                return BadRequest(result);
            }

            return NoContent();
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpDelete("delete")]
        public async Task<IActionResult> Remove(string id)
        {
            var claimId = _business.GetUserIdFromClaims(User);
            if (claimId != id && !_business.IsInRole("admin", User))
            {
                return Unauthorized();
            }
            var updatedUser = await _business.RemoveAsync(id);
            if (updatedUser == null)
            {
                return NotFound($"User not found. User Id: {id}");
            }

            //TODO:
            //Remover todos os arquivos do usuario
            return NoContent();
        }
    }
}
