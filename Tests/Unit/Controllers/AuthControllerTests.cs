using Api.Business;
using Api.Controllers;
using Api.Models.InputModel;
using Api.Models.ViewModels;
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
        private readonly TokenViewModel _tokenModel = new() 
        { AccessToken = Guid.NewGuid().ToString(), RefreshToken = Guid.NewGuid().ToString() };

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
            _business.Setup(b => b.LoginAsync(_logInUser))
            .ReturnsAsync(new TokenViewModel { AccessToken = null });

            var result = await _controller.LogIn(_logInUser);

            result.ShouldBeAssignableTo<BadRequestObjectResult>();
        }

        [Fact]
        public async void Login_Sucess_ReturnsOkWithLogedUser()
        {
            _business.Setup(b => b.LoginAsync(_logInUser))
            .ReturnsAsync(_tokenModel);

            var result = await _controller.LogIn(_logInUser);

            result.ShouldBeAssignableTo<OkObjectResult>()
                .ShouldNotBeNull()
                .Value.ShouldBe(_tokenModel);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async void Signin_WhenModelStateInvalid_ReturnsBadRequestResult(string userName)
        {
            var emptyNameUser = new LogInUser(userName, _logInUser.Password);
            _controller.ModelState.AddModelError("UserName", "The field is required");

            var result = await _controller.SignIn(emptyNameUser);

            result.ShouldBeAssignableTo<BadRequestObjectResult>()
                .ShouldNotBeNull()
                .Value.ShouldBeAssignableTo<SerializableError>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async void Signin_WhenCreateFails_ReturnsBadRequestWithResult(string password)
        {
            var errorMsg = "Failed";
            var emptyPasswordUser = new LogInUser(_logInUser.UserName, password);

            _business.Setup(b => b.CreateAsync(emptyPasswordUser))
                .ReturnsAsync(errorMsg);

            var result = await _controller.SignIn(emptyPasswordUser);

            result.ShouldBeAssignableTo<BadRequestObjectResult>()
                .ShouldNotBeNull()
                .Value.ShouldBe(errorMsg);
        }

        [Fact]
        public async void Signin_WhenSucess_ReturnsOkWithLogedUser()
        {
            _business.Setup(b => b.CreateAsync(_logInUser)).ReturnsAsync(() => null);
            _business.Setup(b => b.LoginAsync(_logInUser)).ReturnsAsync(_tokenModel);

            var result = await _controller.SignIn(_logInUser);

            result.ShouldBeAssignableTo<OkObjectResult>()
                .ShouldNotBeNull()
                .Value.ShouldBe(_tokenModel);
        }
    }
}
