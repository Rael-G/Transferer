using Api.Business.Contracts;
using Api.Controllers;
using Api.Models;
using Api.Models.ViewModels;
using Bogus.DataSets;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using System.Security.Claims;
using Tests._Builder;

namespace Tests.Unit.Controllers
{
    public class UserControllerTests
    {
        //private readonly Mock<UserManager<User>> _mockUserManager;
        //private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private readonly Mock<IUserBusiness> _business;
        private readonly UsersController _controller;
    
        public UserControllerTests()
        {
            //_mockRoleManager =  new Mock<RoleManager<IdentityRole>>(
            //             Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);
            //_mockUserManager = new Mock<UserManager<User>>(
            //    Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            _business = new Mock<IUserBusiness>();
            _controller = new UsersController(_business.Object);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async void Search_IfNameIsNullOrEmpty_ReturnsBadRequest(string name)
        {
            var result = await _controller.Search(name);

            result.ShouldBeAssignableTo(typeof(BadRequestObjectResult));
        }

        [Fact]
        public async void Search_AnyFound_ReturnsOkWithUsersViewModel()
        {
            var user = new UserBuilder().Build();
            var name = user.UserName;
            var users = UserBuilder.BuildUsers(3);
            var usersViewModels = UserViewModel.MapUsersToViewModel(users);
            _business.Setup(b => b.FindByNameAsync(name)).ReturnsAsync(usersViewModels);

            var result = await _controller.Search(name);

            _business.Verify(r => r.FindByNameAsync(It.IsAny<string>()), Times.Once);

            result.ShouldBeAssignableTo<OkObjectResult>();
            if (result is OkObjectResult objResult)
            {
                objResult.Value.ShouldNotBeNull();
                objResult.Value.ShouldBeOfType(typeof(List<UserViewModel>));
                if (objResult.Value is List<UserViewModel>)
                {
                    objResult.Value.ShouldBe(usersViewModels);
                }
            }
        }
        
        [Fact]
        public async void Edit_WhenSucess_ReturnsNoContent()
        {
            var user = new UserBuilder().Build();
            var userViewModel = new UserViewModel(user);

            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(userViewModel.Id);

            var result = await _controller.Edit(userViewModel);

            result.ShouldBeAssignableTo<NoContentResult>();
        }

        [Fact]
        public async void Edit_WhenUnauthorized_ReturnsUnauthorized()
        {
            var user = new UserBuilder().Build();
            var userViewModel = new UserViewModel(user);

            var result = await _controller.Edit(userViewModel);

            result.ShouldBeAssignableTo<UnauthorizedResult>();
        }
        
        [Fact]
        public async void Edit_WhenClientIsAdmin_Authorize()
        {
            var user = new UserBuilder().Build();
            var userViewModel = new UserViewModel(user);
            _business.Setup(b => b.IsInRole("admin", It.IsAny<ClaimsPrincipal>())).Returns(true);

            var result = await _controller.Edit(userViewModel);

            result.ShouldBeAssignableTo<NoContentResult>();
        }
        //Edit if client is not admin and trying to edit someone else
        //  than himself returns Unauthorized
        //Edit if Invalid Returns BadRequest

        //Role if success return NoContent

        //Remove if Success returns NoContent
        //Remvoe if client is not admin and trying to remove someone other
        //  than himself returns Unauthorized

        //Auxiliary




    }
}
