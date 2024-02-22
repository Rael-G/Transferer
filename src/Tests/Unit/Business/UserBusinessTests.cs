using Api.Models.InputModel;
using Api.Models.ViewModels;
using Application.Dtos;
using Application.Interfaces.Services;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;
using Tests._Builder;

namespace Tests.Unit.Business
{
    public class UserBusinessTests
    {
        private Mock<IUserRepository> _repository;
        private IUserService _business;
        private readonly Mock<IArchiveService> _archiveService;

        private User user;
        private UserDto userDto;
        private UserViewModel userViewModel;
        UserInputModel userInputModel;

        public UserBusinessTests() 
        {
            _repository = new Mock<IUserRepository>();
            _archiveService = new Mock<IArchiveService>();
            var mapper = new Mock<IMapper>();
            _business = new UserService(_repository.Object, _archiveService.Object, mapper.Object);

            user = new UserBuilder().Build();
            userDto = new UserBuilder().BuildDto();
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

            result.Id.ShouldBe(user.Id);
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

            result.Id.ShouldBe(user.Id);
        }

        [Fact]
        public async void EditAsync_WhenSuccess_UpdateAsyncIsCalledOnce()
        {
            var result = await _business.EditAsync(userDto, userInputModel.OldPassword, userInputModel.NewPassword);

            _repository.Verify(r => r.UpdateAsync(It.IsAny<User>(), userInputModel.OldPassword, userInputModel.NewPassword), Times.Once);
            result.ShouldBeNull();
        }

        [Fact]
        public async void EditAsync_WhenUpdateFails_ReturnsUnsuccessfulResult()
        {
            var identityResult = new IdentityResult();
            identityResult.GetType().GetProperty("Successful").SetValue(identityResult, false);

            _repository.Setup(r => r.UpdateAsync(user, userInputModel.OldPassword, userInputModel.NewPassword)).ReturnsAsync(identityResult);

            var result = await _business.EditAsync(userDto, userInputModel.OldPassword, userInputModel.NewPassword);

            _repository.Verify(r => r.UpdateAsync(It.IsAny<User>(), userInputModel.OldPassword, userInputModel.NewPassword), Times.Once);
            result.Succeeded.ShouldBeFalse();
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

            user.Archives = ArchiveBuilder.BuildArchives(10);

            var result = await _business.RemoveAsync(user.Id);

            _archiveService.Verify(a => a.DeleteAsync(It.IsAny<ArchiveDto>()), Times.Exactly(10));
        }
    }
}
