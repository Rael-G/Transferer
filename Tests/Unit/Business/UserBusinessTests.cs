using Api.Business;
using Api.Business.Implementation;
using Api.Data.Interfaces;
using Api.Models;
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

        private User user;
        private UserViewModel userViewModel;
        UserInputModel userInputModel;

        public UserBusinessTests() 
        {
            _repository = new Mock<IUserRepository>();
            var archiveBusiness = new Mock<IArchiveBusiness>();
            _business = new UserBusiness(_repository.Object, archiveBusiness.Object);

            user = new UserBuilder().Build();
            userViewModel = UserViewModel.MapToViewModel(user);
            userInputModel = new UserInputModel(user.UserName, "Batatinha1!", "Batatinha1!");
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
        public async void GetAsync_WhenSuccess_ReturnsUser()
        {
            _repository.Setup(r => r.GetByIdAsync(user.Id))
                .ReturnsAsync(user);

            var result = await _business.GetAsync(user.Id);

            result.ShouldBe(user);
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
        public async void SearchAsync_WhenSuccess_ReturnsUser()
        {
            var name = "Joaosinho77";

            _repository.Setup(r => r.GetByNameAsync(name))
                .ReturnsAsync(user);

            var result = await _business.SearchAsync(name);

            result.ShouldBe(user);
        }

        [Fact]
        public async void EditAsync_WhenSuccess_UpdateAsyncIsCalledOnce()
        {
            var result = await _business.EditAsync(user, userInputModel);

            _repository.Verify(r => r.UpdateAsync(user, userInputModel), Times.Once);
            result.ShouldBeNull();
        }

        [Fact]
        public async void EditAsync_WhenUpdateFails_ReturnsErrorMessage()
        {
            var errorMsg = "Update failed";
            _repository.Setup(r => r.UpdateAsync(user, userInputModel)).ReturnsAsync(errorMsg);

            var result = await _business.EditAsync(user, userInputModel);

            _repository.Verify(r => r.UpdateAsync(user, userInputModel), Times.Once);
            result.ShouldBe(errorMsg);
        }

        [Fact]
        public async void RemoveAsync_IfUserIsNotInRepository_ReturnsNull()
        {
            _repository.Setup(r => r.GetByIdAsync(user.Id))
                .ReturnsAsync(() => null);

            var result = await _business.RemoveAsync(user.Id);

            result.ShouldBeNull();
        }

        [Fact]
        public async void RemoveAsync_WhenSuccess_DeleteAsyncIsCalledOnce()
        {
            _repository.Setup(r => r.GetByIdAsync(userViewModel.Id))
                .ReturnsAsync(user);

            var result = await _business.RemoveAsync(userViewModel.Id);

            _repository.Verify(r => r.DeleteAsync(userViewModel.Id), Times.Once);
        }

        [Fact]
        public async void RemoveAsync_CallsDeleteAsyncOnArchiveBusinessForEachArchive()
        {
            _repository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

            var archive1 = new ArchiveBuilder().Build();
            var archive2 = new ArchiveBuilder().Build();
            user.Archives = new List<Archive> { archive1, archive2 };

            var archiveBusiness = new Mock<IArchiveBusiness>();
            _business = new UserBusiness(_repository.Object, archiveBusiness.Object);

            var result = await _business.RemoveAsync(user.Id);

            archiveBusiness.Verify(a => a.DeleteAsync(archive1), Times.Once);
            archiveBusiness.Verify(a => a.DeleteAsync(archive2), Times.Once);
        }
    }
}
