using Api.Data.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public interface IUserRepository
    {
        List<User> GetAllAsync();
        User GetByIdAsync(string id);
        User GetByNameAsync(string name);
        User UpdateAsync(string id);
        User DeleteAsync(string id);
    }

    [ApiController]
    [Route("[controller]")]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<User> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "admin")]
        [HttpPut("list")]
        public IActionResult ListAll()
        {
            return Ok();
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "admin")]
        [HttpPut("search")]
        public IActionResult Search()
        {
            return Ok();
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("edit")]
        public IActionResult Edit()
        {
            //If Claim.UserId == user.Id
            // || Claim.Role == "admin"
            return NoContent();
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "admin")]
        [HttpPut("role")]
        public IActionResult Role()
        {
            return NoContent();
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpDelete("delete")]
        public IActionResult Remove()
        {
            //If Claim.UserId == user.Id 
            // || Claim.Role == "admin"
            return NoContent();
        }



    }
}
