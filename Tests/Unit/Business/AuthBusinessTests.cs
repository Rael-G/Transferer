using Api.Models.InputModel;
using Api.Business.Implementation;
using Api.Data.Interfaces;
using Moq;
using Shouldly;
using Tests._Builder;
using Api.Services;

namespace Tests.Unit.Business
{
    public class AuthBusinessTests
    {
        private Mock<IUserRepository> _repository;
        private AuthBusiness _business;

        private LogInUser _logInUser = new LogInUser("batatinha", "Batata123!");

        public AuthBusinessTests()
        {
            _repository = new Mock<IUserRepository>();
            _business = new AuthBusiness(_repository.Object);
        }

        [Fact]
        public async void CreateAsync_WhenFailed_ReturnsResultToString()
        {
            var msg = "Failed";
            _repository.Setup(r => r.CreateAsync(_logInUser))
                .ReturnsAsync(msg);

            var result = await _business.CreateAsync(_logInUser);

            result.ShouldBe(msg);
        }

        [Fact]
        public async void CreateAsync_WhenSuccess_ReturnsNull()
        {
            _repository.Setup(r => r.CreateAsync(_logInUser))
                .ReturnsAsync(() => null);

            var result = await _business.CreateAsync(_logInUser);

            result.ShouldBeNull();
        }

        [Fact]
        public async void LoginAsync_WhenUserIsNull_ReturnsNull()
        {
            _repository.Setup(r => r.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(() => null);

            var result = await _business.LoginAsync(_logInUser); 
            
            result.ShouldBeNull();
        }

        [Fact]
        public async void LoginAsync_WhenPasswordFail_ReturnsLoggedUserWithTokenNull()
        {
            var user = new UserBuilder()
                .SetName(_logInUser.UserName).Build();
            
            _repository.Setup(r => r.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            var result = await _business.LoginAsync(_logInUser);

            result.ShouldNotBeNull();
            result.AccessToken.ShouldBeNull();
        }

        [Fact]
        public async void LoginAsync_WhenPasswordPass_ReturnsLoggedUser()
        {
            var user = new UserBuilder().SetName(_logInUser.UserName)
                .SetPassword(_logInUser.Password).Build();
            TokenService.SecretKey = Guid.NewGuid().ToString();

            _repository.Setup(r => r.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _repository.Setup(r => r.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            var result = await _business.LoginAsync(_logInUser);

            result.ShouldNotBeNull();
            result.AccessToken.ShouldNotBeNull();
        }
    }
}


