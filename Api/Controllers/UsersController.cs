using Api.Business.Contracts;
using Api.Business.Implementation;
using Api.Data.Interfaces;
using Api.Models;
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
        [HttpPut("search")]
        public async Task<IActionResult> Search(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest(name);
            }
            var user = await _business.FindByNameAsync(name);
            return Ok(user);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("edit")]
        public async Task<IActionResult> Edit(UserViewModel user)
        {
            var claimId = _business.GetUserIdFromClaims(User);
            if (claimId != user.Id && !_business.IsInRole("admin", User))
            {
                return Unauthorized();
            }
            await _business.EditAsync(user);

            return NoContent();
        }

        [Authorize(AuthenticationSchemes = "Bearegitr")]
        [HttpDelete("delete")]
        public IActionResult Remove()
        {
            //If Claim.UserId == user.Id 
            // || Claim.Role == "admin" && Claim.UserId != user.Id 
            return NoContent();
        }
    }
}
