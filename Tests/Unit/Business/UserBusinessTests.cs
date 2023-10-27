using Api.Business.Implementation;
using Api.Data.Interfaces;
using Api.Models.InputModel;
using Api.Models.ViewModels;
using Moq;
using Shouldly;
using Tests._Builder;

namespace Tests.Unit.Business
{
    public class UserBusinessTests
    {
        private Mock<IUserRepository> _repository;
        private UserBusiness _business;

        public UserBusinessTests() 
        {
            _repository = new Mock<IUserRepository>();
            _business = new UserBusiness(_repository.Object);
        }

        [Fact]
        public async void GetAsync_IfUserIsNotInRepository_ReturnsNull()
        {
            var id = Guid.NewGuid().ToString();

            _repository.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(() => null);

            var result = await _business.GetAsync(id);

            result.ShouldBeNull();
        }

        [Fact]
        public async void GetAsync_WhenSuccess_ReturnsUsersViewModel()
        {
            var id = Guid.NewGuid().ToString();
            var user = UserBuilder.BuildUser();
            var userViewModel = UserViewModel.MapToViewModel(user);

            _repository.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(user);

            var result = await _business.GetAsync(id);

            result.ShouldBe(userViewModel);
        }

        [Fact]
        public async void SearchAsync_IfUserIsNotInRepository_ReturnsNull()
        {
            var name = "jorisvaldo";

            _repository.Setup(r => r.GetByNameAsync(name))
                .ReturnsAsync(() => null);

            var result = await _business.SearchAsync(name);

            result.ShouldBeNull();
        }

        [Fact]
        public async void SearchAsync_WhenSuccess_ReturnsUsersViewModel()
        {
            var name = "Joaosinho77";
            var user = UserBuilder.BuildUser();
            var userViewModel = UserViewModel.MapToViewModel(user);

            _repository.Setup(r => r.GetByNameAsync(name))
                .ReturnsAsync(user);

            var result = await _business.SearchAsync(name);

            result.ShouldBe(userViewModel);
        }

        [Fact]
        public async void EditAsync_IfUserIsNotInRepository_ReturnsNull()
        {
            var user = new UserBuilder().Build();
            var userInputModel = new UserInputModel(user.Id, user.UserName);

            _repository.Setup(r => r.GetByIdAsync(userInputModel.Id))
                .ReturnsAsync(() => null);

            var result = await _business.EditAsync(userInputModel);

            result.ShouldBeNull();
        }

        [Fact]
        public async void EditAsync_WhenSuccess_UpdateAsyncIsCalledOnce()
        {
            var user = new UserBuilder().Build();
            var userInputModel = new UserInputModel(user.Id, user.UserName);

            _repository.Setup(r => r.GetByIdAsync(userInputModel.Id))
                .ReturnsAsync(user);

            var result = await _business.EditAsync(userInputModel);

            _repository.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async void RemoveAsync_IfUserIsNotInRepository_ReturnsNull()
        {
            var user = new UserBuilder().Build();
            var userInputModel = new UserInputModel(user.Id, user.UserName);

            _repository.Setup(r => r.GetByIdAsync(userInputModel.Id))
                .ReturnsAsync(() => null);

            var result = await _business.RemoveAsync(userInputModel.Id);

            result.ShouldBeNull();
        }

        [Fact]
        public async void RemoveAsync_WhenSuccess_DeleteAsyncIsCalledOnce()
        {
            var user = new UserBuilder().Build();
            var userViewModel = UserViewModel.MapToViewModel(user);

            _repository.Setup(r => r.GetByIdAsync(userViewModel.Id))
                .ReturnsAsync(user);

            var result = await _business.RemoveAsync(userViewModel.Id);

            _repository.Verify(r => r.DeleteAsync(userViewModel.Id), Times.Once);
        }
    }
}
