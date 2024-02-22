using Application.Dtos;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IArchiveService _archiveBusiness;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IArchiveService archiveBusiness, IMapper mapper)
        {
            _repository = repository;
            _archiveBusiness = archiveBusiness;
            _mapper = mapper;
        }

        public async Task<UserDto?> GetAsync(string id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> SearchAsync(string name)
        {
            var user = await _repository.GetByNameAsync(name);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<IdentityResult> EditAsync(UserDto userDto, string oldPassword, string newPassword)
        {
            var user = await _repository.GetByIdAsync(userDto.Id);
            user.UserName = userDto.UserName;

            return await _repository.UpdateAsync(user, oldPassword, newPassword);
        }

        public async Task<UserDto?> RemoveAsync(string id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
                return null;

            // Delete all user archives.
            foreach (var archive in user.Archives)
            {
                var archiveDto = _mapper.Map<ArchiveDto>(archive);
                await _archiveBusiness.DeleteAsync(archiveDto);
            }

            await _repository.DeleteAsync(user.Id);

            return _mapper.Map<UserDto>(user);
        }

        public string? GetUserIdFromClaims(ClaimsPrincipal user)
        {
            var claim = user.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (claim != null)
            {
                return claim.Value;
            }

            return null;
        }

        public bool IsInRole(string role, ClaimsPrincipal user)
        {
            return user.IsInRole(role);
        }
    }
}
