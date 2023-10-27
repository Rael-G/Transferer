using Api.Business;
using Api.Controllers;
using Api.Models;
using Api.Models.InputModel;
using Api.Models.ViewModels;
using Bogus.DataSets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using System.Security.Claims;
using System.Xml.Linq;
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
        public async void Get_IfIdIsNullOrEmpty_ReturnsBadRequest(string id)
        {
            var result = await _controller.Get(id);

            result.ShouldBeAssignableTo(typeof(BadRequestObjectResult));
        }

        [Fact]
        public async void Get_IfUserIsNull_Returns_NotFound()
        {
            var id = Guid.NewGuid().ToString();

            _business.Setup(b => b.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(() => null);

            var result = await _controller.Get(id);

            result.ShouldBeAssignableTo<NotFoundObjectResult>();
        }

        [Fact]
        public async void Get_IfFound_ReturnsOkWithUserViewModel()
        {
            var user = new UserBuilder().Build();
            var id = user.Id;
            var userViewModel = UserViewModel.MapToViewModel(user);
            _business.Setup(b => b.GetAsync(It.IsAny<string>())).ReturnsAsync(userViewModel);

            var result = await _controller.Get(id);

            result.ShouldBeAssignableTo<OkObjectResult>();
            if (result is OkObjectResult objResult)
            {
                objResult.Value.ShouldBeOfType(typeof(UserViewModel));
                if (objResult.Value is UserViewModel)
                {
                    objResult.Value.ShouldBe(userViewModel);
                }
            }
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
        public async void Search_IfUserIsNull_Returns_NotFound()
        {
            var name = "jislaine";

            _business.Setup(b => b.SearchAsync(It.IsAny<string>()))
                .ReturnsAsync(() => null);

            var result = await _controller.Search(name);

            result.ShouldBeAssignableTo<NotFoundObjectResult>();
        }

        [Fact]
        public async void Search_IfFound_ReturnsOkWithUserViewModel()
        {
            var user = new UserBuilder().Build();
            var name = user.UserName;
            var userViewModel = UserViewModel.MapToViewModel(user);
            _business.Setup(b => b.SearchAsync(It.IsAny<string>())).ReturnsAsync(userViewModel);

            var result = await _controller.Search(name);

            result.ShouldBeAssignableTo<OkObjectResult>();
            OkObjectResult objResult = result as OkObjectResult;
            objResult.Value.ShouldBe(userViewModel); 
        }

        [Fact]
        public async void Edit_WhenUserIsNotInRepository_ReturnsNotFound()
        {
            var user = new UserBuilder().Build();
            var userInputModel = new UserInputModel(user.Id, user.UserName);

            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(userInputModel.Id);
            _business.Setup(b => b.EditAsync(It.IsAny<UserInputModel>()))
                .ReturnsAsync(() => null);

            var result = await _controller.Edit(userInputModel);

            result.ShouldBeAssignableTo<NotFoundObjectResult>();
        }

        [Fact]
        public async void Edit_WhenSucess_ReturnsNoContent()
        {
            var user = new UserBuilder().Build();
            var userInputModel = new UserInputModel(user.Id, user.UserName);
            var userViewModel = UserViewModel.MapToViewModel(user);

            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(user.Id);
            _business.Setup(b => b.EditAsync(It.IsAny<UserInputModel>()))
                .ReturnsAsync(userViewModel);

            var result = await _controller.Edit(userInputModel);

            result.ShouldBeAssignableTo<NoContentResult>();
        }

        [Fact]
        public async void Edit_WhenSucess_EditAsyncIsCalledOnce()
        {
            var user = new UserBuilder().Build();
            var userInputModel = new UserInputModel(user.Id, user.UserName);
            var userViewModel = UserViewModel.MapToViewModel(user);

            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(user.Id);
            _business.Setup(b => b.EditAsync(It.IsAny<UserInputModel>()))
                .ReturnsAsync(userViewModel);

            var result = await _controller.Edit(userInputModel);

            _business.Verify(b => b.EditAsync(userInputModel), Times.Once());
        }

        [Fact]
        public async void Edit_WhenClaimIdAndUserIdAreDifferent_ReturnsUnauthorized()
        {
            var user = new UserBuilder().Build();
            var userInputModel = new UserInputModel(user.Id, user.UserName);

            var result = await _controller.Edit(userInputModel);

            result.ShouldBeAssignableTo<UnauthorizedResult>();
        }
        
        [Fact]
        public async void Edit_WhenClientIsAdmin_Authorize()
        {
            var user = new UserBuilder().Build();
            var userInputModel = new UserInputModel(user.Id, user.UserName);
            var userViewModel = UserViewModel.MapToViewModel(user);

            _business.Setup(b => b.IsInRole("admin", It.IsAny<ClaimsPrincipal>()))
                .Returns(true);
            _business.Setup(b => b.EditAsync(It.IsAny<UserInputModel>()))
                .ReturnsAsync(userViewModel);

            var result = await _controller.Edit(userInputModel);

            result.ShouldBeAssignableTo<NoContentResult>();
        }

        [Fact]
        public async void Remove_WhenUserIsNotInRepository_ReturnsNotFound()
        {
            var user = new UserBuilder().Build();
            var userViewModel = UserViewModel.MapToViewModel(user);

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
            var userViewModel = UserViewModel.MapToViewModel(user);

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
            var userViewModel = UserViewModel.MapToViewModel(user);

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
            var userViewModel = UserViewModel.MapToViewModel(user);

            _business.Setup(b => b.IsInRole("admin", It.IsAny<ClaimsPrincipal>())).Returns(true);
            _business.Setup(b => b.RemoveAsync(It.IsAny<string>()))
                .ReturnsAsync(userViewModel);

            var result = await _controller.Remove(Guid.NewGuid().ToString());

            result.ShouldBeAssignableTo<NoContentResult>();
        }
    }
}
