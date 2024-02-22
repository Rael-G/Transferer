using Api.Controllers;
using Api.Models.InputModel;
using Api.Models.ViewModels;
using Application.Dtos;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using System.Security.Claims;
using Tests._Builder;

namespace Tests.Unit.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _business;
        private readonly UsersController _controller;

        private User _user;
        private UserDto _userDto;
        private UserInputModel _userInputModel;

        public UserControllerTests()
        {
            _business = new Mock<IUserService>();
            _controller = new UsersController(_business.Object);

            _user = new UserBuilder().Build();
            _userDto = new UserBuilder().BuildDto();
            _userInputModel = new UserInputModel(_user.UserName, "Batatinha1!", "Batatinha1!");
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
            _business.Setup(b => b.GetAsync(It.IsAny<string>())).ReturnsAsync(_userDto);

            var result = await _controller.Get(_userDto.Id);

            result.ShouldBeAssignableTo<OkObjectResult>()
                .ShouldNotBeNull()
                .Value.ShouldBeAssignableTo<UserViewModel>()
                .ShouldNotBeNull()
                .Id.ShouldBe(_userDto.Id);
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
            _business.Setup(b => b.SearchAsync(It.IsAny<string>())).ReturnsAsync(_userDto);

            var result = await _controller.Search(_userDto.UserName);

            result.ShouldBeAssignableTo<OkObjectResult>()
                .ShouldNotBeNull()
                .Value.ShouldBeAssignableTo<UserViewModel>()
                .ShouldNotBeNull()
                .ShouldBeAssignableTo<UserViewModel>()
                .ShouldNotBeNull()
                .Id.ShouldBe(_userDto.Id);
        }

        [Fact]
        public async void Edit_WhenUserIsNotInRepository_ReturnsNotFound()
        {
            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(_user.Id);
            _business.Setup(b => b.EditAsync(It.IsAny<UserDto>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(() => null);

            var result = await _controller.Edit(_userInputModel);

            result.ShouldBeAssignableTo<NotFoundObjectResult>();
        }

        [Fact]
        public async void Edit_WhenIdentityValidationsFails_ReturnsBadRequest()
        {
            var identityResult = new IdentityResult();
            identityResult.GetType().GetProperty("Successful").SetValue(identityResult, false);

            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(_user.Id);
            _business.Setup(b => b.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(_userDto);
            _business.Setup(b => b.EditAsync(It.IsAny<UserDto>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(identityResult);

            var result = await _controller.Edit(_userInputModel);

            result.ShouldBeAssignableTo<BadRequestObjectResult>();
        }

        [Fact]
        public async void Edit_WhenSucess_ReturnsNoContent()
        {
            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(_user.Id);
            _business.Setup(b => b.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(_userDto);
            _business.Setup(b => b.EditAsync(It.IsAny<UserDto>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(() => null);

            var result = await _controller.Edit(_userInputModel);

            result.ShouldBeAssignableTo<NoContentResult>();
        }

        [Fact]
        public async void Edit_WhenSucess_EditAsyncIsCalledOnce()
        {
            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(_user.Id);
            _business.Setup(b => b.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(_userDto);
            _business.Setup(b => b.EditAsync(It.IsAny<UserDto>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(() => null);

            var result = await _controller.Edit(_userInputModel);

            _business.Verify(b => b.EditAsync(_userDto, _userInputModel.OldPassword, _userInputModel.NewPassword), Times.Once());
        }

        [Fact]
        public async void Remove_WhenUserIsNotInRepository_ReturnsNotFound()
        {
            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(_user.Id);
            _business.Setup(b => b.RemoveAsync(It.IsAny<string>()))
                .ReturnsAsync(() => null);

            var result = await _controller.Remove(_user.Id);

            result.ShouldBeAssignableTo<NotFoundObjectResult>();
        }

        [Fact]
        public async void Remove_WhenSuccess_ReturnsNoContent()
        {
            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(_user.Id);
            _business.Setup(b => b.RemoveAsync(It.IsAny<string>()))
                .ReturnsAsync(_userDto);

            var result = await _controller.Remove(_user.Id);

            result.ShouldBeAssignableTo<NoContentResult>();
        }

        [Fact]
        public async void Remove_WhenSucess_RemoveAsyncIsCalledOnce()
        {
            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(_user.Id);
            _business.Setup(b => b.RemoveAsync(It.IsAny<string>()))
                .ReturnsAsync(_userDto);

            var result = await _controller.Remove(_user.Id);

            _business.Verify(b => b.RemoveAsync(_user.Id), Times.Once());
        }

        [Fact]
        public async void Remove_WhenClaimIdAndUserIdAreDifferent_ReturnsUnauthorized()
        {
            _business.Setup(b => b.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(Guid.NewGuid().ToString());

            var result = await _controller.Remove(_user.Id);

            result.ShouldBeAssignableTo<UnauthorizedResult>();
        }

        [Fact]
        public async void Remove_WhenClientIsAdmin_Authorize()
        {
            _business.Setup(b => b.IsInRole("admin", It.IsAny<ClaimsPrincipal>())).Returns(true);
            _business.Setup(b => b.RemoveAsync(It.IsAny<string>()))
                .ReturnsAsync(_userDto);

            var result = await _controller.Remove(Guid.NewGuid().ToString());

            result.ShouldBeAssignableTo<NoContentResult>();
        }
    }
}
