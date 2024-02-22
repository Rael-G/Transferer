using Api.Models.InputModel;
using Application.Dtos;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Moq;
using Shouldly;
using Tests._Builder;

namespace Tests.Unit.Business
{
    public class AuthBusinessTests
    {
        private Mock<IUserRepository> _repository;
        private AuthService _business;
        private LogInUser _logInUser = new LogInUser("batatinha", "Batata123!");
        private string pswd = "Password1!";

        private User user;
        private UserDto userDto;

        public AuthBusinessTests()
        {
            user = new UserBuilder().Build();
            userDto = new UserBuilder().BuildDto();

            var mapper = new Mock<IMapper>();
            _repository = new Mock<IUserRepository>();
            _business = new AuthService(_repository.Object, mapper.Object);
        }

        [Fact]
        public async void CreateAsync_WhenFailed_ReturnsResultToString()
        {
            var msg = "Failed";
            
            _repository.Setup(r => r.CreateAsync(It.IsAny<User>(), pswd))
                .ReturnsAsync(msg);

            var result = await _business.CreateAsync(userDto, pswd);

            result.ShouldBe(msg);
        }

        [Fact]
        public async void CreateAsync_WhenSuccess_ReturnsNull()
        {
            _repository.Setup(r => r.CreateAsync(It.IsAny<User>(), pswd))
                .ReturnsAsync(() => null);

            var result = await _business.CreateAsync(userDto, pswd);

            result.ShouldBeNull();
        }

        [Fact]
        public async void LoginAsync_WhenUserIsNull_ReturnsNull()
        {
            _repository.Setup(r => r.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(() => null);

            var result = await _business.LoginAsync(userDto, pswd); 
            
            result.ShouldBeNull();
        }

        [Fact]
        public async void LoginAsync_WhenPasswordFail_ReturnsNull()
        {
            var user = new UserBuilder()
                .SetName(_logInUser.UserName).Build();
            
            _repository.Setup(r => r.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            var result = await _business.LoginAsync(userDto, pswd);

            result.ShouldBeNull();
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

            var result = await _business.LoginAsync(userDto, pswd);

            result.ShouldNotBeNull();
            result.AccessToken.ShouldNotBeNull();
        }
    }
}


