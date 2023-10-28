using Api.Business;
using Api.Controllers;
using Api.Models;
using Api.Models.InputModel;
using Api.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace Tests.Unit.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthBusiness> _business;
        private readonly AuthController _controller;

        private readonly LogInUser _logInUser = new("Batatinha", "Batata123!");
        private readonly LoggedUser _loggedUser = new() { UserName = "Batatinha", Token = "token" };

        public AuthControllerTests()
        {
            _business = new Mock<IAuthBusiness>();
            _controller = new AuthController(_business.Object);
        }

        [Fact]
        public async void Login_WhenLogedUserIsNull_ReturnsNotFound()
        {
            _business.Setup(b => b.LoginAsync(_logInUser))
                .ReturnsAsync(() => null);

            var result = await _controller.LogIn(_logInUser);

            result.ShouldBeAssignableTo<NotFoundObjectResult>();
        }

        [Fact]
        public async void Login_WhenTokenIsNull_ReturnsBadRequest()
        {
            var loggedUserWithNullToken = _loggedUser;
            loggedUserWithNullToken.Token = null;

            _business.Setup(b => b.LoginAsync(_logInUser))
                .ReturnsAsync(loggedUserWithNullToken);

            var result = await _controller.LogIn(_logInUser);

            result.ShouldBeAssignableTo<BadRequestObjectResult>();
        }

        [Fact]
        public async void Login_Sucess_ReturnsOkWithLogedUser()
        {
            _business.Setup(b => b.LoginAsync(_logInUser))
                .ReturnsAsync(_loggedUser);

            var result = await _controller.LogIn(_logInUser);

            result.ShouldBeAssignableTo<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.ShouldBe(_loggedUser);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async void Signin_WhenModelStateInvalid_ReturnsBadRequestResult(string userName)
        {
            var emptyNameUser = new LogInUser(userName, _logInUser.Password);

            var controller = new AuthController(_business.Object);
            controller.ModelState.AddModelError("UserName", "The field is required");

            var result = await controller.SignIn(emptyNameUser);

            result.ShouldBeAssignableTo<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.ShouldBeAssignableTo<SerializableError>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async void Signin_WhenCreateFails_ReturnsBadRequestWithResult(string password)
        {
            var msg = "Failed";
            LogInUser emptyPasswordUser = new(_logInUser.UserName, password);

            _business.Setup(b => b.CreateAsync(emptyPasswordUser))
                .ReturnsAsync(msg);

            IActionResult result = await _controller.SignIn(emptyPasswordUser);

            result.ShouldBeAssignableTo<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.ShouldBe(msg);
        }

        [Fact]
        public async void Signin_WhenSucess_ReturnsOkWithLogedUser()
        {
            IdentityResult createResult = IdentityResult.Success;

            _business.Setup(b => b.CreateAsync(_logInUser))
                .ReturnsAsync(() => null);
            _business.Setup(b => b.LoginAsync(_logInUser))
                .ReturnsAsync(_loggedUser);

            var result = await _controller.SignIn(_logInUser);

            result.ShouldBeAssignableTo<OkObjectResult>();
            var createdResult = result as OkObjectResult;
            createdResult.Value.ShouldBe(_loggedUser);
        }
    }
}
