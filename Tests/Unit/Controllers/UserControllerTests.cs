using Api.Controllers;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace Tests.Unit.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private readonly UsersController _controller;
    
        public UserControllerTests()
        {
            _mockRoleManager =  new Mock<RoleManager<IdentityRole>>(
                         Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);
            _mockUserManager = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            _controller = new UsersController(_mockUserManager.Object, _mockRoleManager.Object);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async void Search_IfNameIsNullOrEmpty_ReturnsBadRequest(string name)
        {
            var result = await _controller.Search(name);

            result.ShouldBeAssignableTo(typeof(BadRequestObjectResult));
        }
        //Search if any found returns Ok with users

        //Edit if Success returns NoContent
        //Edit if client is not admin and trying to edit someone other
        //  than himself returns Unauthorized
        //Edit if Invalid Returns BadRequest

        //Role if success return NoContent

        //Remove if Success returns NoContent
        //Remvoe if client is not admin and trying to remove someone other
        //  than himself returns Unauthorized

    }
}
