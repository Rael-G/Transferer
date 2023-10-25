using Api.Business.Contracts;
using Api.Business.Implementation;
using Api.Data.Interfaces;
using Api.Models.ViewModels;
using Bogus.DataSets;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public async void SearchAsync_WhenSuccess_ReturnsUsersViewModel()
        {
            var name = "Joaosinho77";
            var users = UserBuilder.BuildUsers(5);
            var userViewModels = UserViewModel.MapUsersToViewModel(users);

            _repository.Setup(r => r.GetByNameAsync(name))
                .ReturnsAsync(users);

            var result = await _business.SearchAsync(name);

            userViewModels.ShouldAllBe(u => result.Contains(u));
        }

        [Fact]
        public async void EditAsync_IfUserIsNotInRepository_ReturnsNull()
        {
            var user = new UserBuilder().Build();
            var userViewModel = new UserViewModel(user);

            _repository.Setup(r => r.GetByIdAsync(userViewModel.Id))
                .ReturnsAsync(() => null);

            var result = await _business.EditAsync(userViewModel);

            result.ShouldBeNull();
        }

        [Fact]
        public async void EditAsync_WhenSuccess_UpdateAsyncIsCalledOnce()
        {
            var user = new UserBuilder().Build();
            var userViewModel = new UserViewModel(user);

            _repository.Setup(r => r.GetByIdAsync(userViewModel.Id))
                .ReturnsAsync(user);

            var result = await _business.EditAsync(userViewModel);

            _repository.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async void RemoveAsync_IfUserIsNotInRepository_ReturnsNull()
        {
            var user = new UserBuilder().Build();
            var userViewModel = new UserViewModel(user);

            _repository.Setup(r => r.GetByIdAsync(userViewModel.Id))
                .ReturnsAsync(() => null);

            var result = await _business.RemoveAsync(userViewModel.Id);

            result.ShouldBeNull();
        }

        [Fact]
        public async void RemoveAsync_WhenSuccess_DeleteAsyncIsCalledOnce()
        {
            var user = new UserBuilder().Build();
            var userViewModel = new UserViewModel(user);

            _repository.Setup(r => r.GetByIdAsync(userViewModel.Id))
                .ReturnsAsync(user);

            var result = await _business.RemoveAsync(userViewModel.Id);

            _repository.Verify(r => r.DeleteAsync(userViewModel.Id), Times.Once);
        }
    }
}
