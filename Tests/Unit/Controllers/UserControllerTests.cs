using Api.Business.Contracts;
using Api.Controllers;
using Api.Models;
using Api.Models.ViewModels;
using Bogus.DataSets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using System.Security.Claims;
using Tests._Builder;

namespace Tests.Unit.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserBusiness> _business;
        private readonly UsersController _controller;
    
        public UserControllerTests()
        {
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
            _business.Setup(b => b.SearchAsync(name)).ReturnsAsync(usersViewModels);

            var result = await _controller.Search(name);

            _business.Verify(r => r.SearchAsync(It.IsAny<string>()), Times.Once);

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
        public async void Edit_WhenUserIsNotInRepository_ReturnsNotFound()
        {
            var user = new UserBuilder().Build();
            var userViewModel = new UserViewModel(user);

            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(userViewModel.Id);
            _business.Setup(b => b.EditAsync(It.IsAny<UserViewModel>()))
                .ReturnsAsync(() => null);

            var result = await _controller.Edit(userViewModel);

            result.ShouldBeAssignableTo<NotFoundObjectResult>();
        }

        [Fact]
        public async void Edit_WhenSucess_ReturnsNoContent()
        {
            var user = new UserBuilder().Build();
            var userViewModel = new UserViewModel(user);

            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(userViewModel.Id);
            _business.Setup(b => b.EditAsync(It.IsAny<UserViewModel>()))
                .ReturnsAsync(userViewModel);

            var result = await _controller.Edit(userViewModel);

            result.ShouldBeAssignableTo<NoContentResult>();
        }

        [Fact]
        public async void Edit_WhenSucess_EditAsyncIsCalledOnce()
        {
            var user = new UserBuilder().Build();
            var userViewModel = new UserViewModel(user);

            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(userViewModel.Id);
            _business.Setup(b => b.EditAsync(It.IsAny<UserViewModel>()))
                .ReturnsAsync(userViewModel);

            var result = await _controller.Edit(userViewModel);

            _business.Verify(b => b.EditAsync(userViewModel), Times.Once());
        }

        [Fact]
        public async void Edit_WhenClaimIdAndUserIdAreDifferent_ReturnsUnauthorized()
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
            _business.Setup(b => b.IsInRole("admin", It.IsAny<ClaimsPrincipal>()))
                .Returns(true);
            _business.Setup(b => b.EditAsync(It.IsAny<UserViewModel>()))
                .ReturnsAsync(userViewModel);

            var result = await _controller.Edit(userViewModel);

            result.ShouldBeAssignableTo<NoContentResult>();
        }

        [Fact]
        public async void Remove_WhenUserIsNotInRepository_ReturnsNotFound()
        {
            var user = new UserBuilder().Build();
            var userViewModel = new UserViewModel(user);

            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(userViewModel.Id);
            _business.Setup(b => b.RemoveAsync(It.IsAny<string>()))
                .ReturnsAsync(() => null);

            var result = await _controller.Remove(userViewModel.Id);

            result.ShouldBeAssignableTo<NotFoundObjectResult>();
        }

        [Fact]
        public async void Remove_WhenSuccess_ReturnsNoContent()
        {
            var user = new UserBuilder().Build();
            var userViewModel = new UserViewModel(user);

            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(userViewModel.Id);
            _business.Setup(b => b.RemoveAsync(It.IsAny<string>()))
                .ReturnsAsync(userViewModel);

            var result = await _controller.Remove(userViewModel.Id);

            result.ShouldBeAssignableTo<NoContentResult>();
        }

        [Fact]
        public async void Remove_WhenSucess_RemoveAsyncIsCalledOnce()
        {
            var user = new UserBuilder().Build();
            var userViewModel = new UserViewModel(user);

            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(userViewModel.Id);
            _business.Setup(b => b.RemoveAsync(It.IsAny<string>()))
                .ReturnsAsync(userViewModel);

            var result = await _controller.Remove(userViewModel.Id);

            _business.Verify(b => b.RemoveAsync(userViewModel.Id), Times.Once());
        }

        [Fact]
        public async void Remove_WhenClaimIdAndUserIdAreDifferent_ReturnsUnauthorized()
        {
            var user = new UserBuilder().Build();
            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(Guid.NewGuid().ToString());

            var result = await _controller.Remove(user.Id);

            result.ShouldBeAssignableTo<UnauthorizedResult>();
        }

        [Fact]
        public async void Remove_WhenClientIsAdmin_Authorize()
        {
            var user = new UserBuilder().Build();
            var userViewModel = new UserViewModel(user);

            _business.Setup(b => b.IsInRole("admin", It.IsAny<ClaimsPrincipal>())).Returns(true);
            _business.Setup(b => b.RemoveAsync(It.IsAny<string>()))
                .ReturnsAsync(userViewModel);

            var result = await _controller.Remove(Guid.NewGuid().ToString());

            result.ShouldBeAssignableTo<NoContentResult>();
        }
    }
}
