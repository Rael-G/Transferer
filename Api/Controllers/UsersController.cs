using Api.Business;
using Api.Business.Implementation;
using Api.Data.Interfaces;
using Api.Models;
using Api.Models.InputModel;
using Api.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

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
        [HttpPut("get")]
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
            return Ok(user);
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "admin")]
        [HttpPut("search")]
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
            return Ok(user);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("edit")]
        public async Task<IActionResult> Edit(UserInputModel user)
        {
            var claimId = _business.GetUserIdFromClaims(User);
            if (claimId != user.Id && !_business.IsInRole("admin", User))
            {
                return Unauthorized();
            }

            var updatedUser = await _business.EditAsync(user);
            if (updatedUser == null)
            {
                return NotFound($"User not found. User Id: {user.Id}");
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
            return NoContent();
        }
    }
}
