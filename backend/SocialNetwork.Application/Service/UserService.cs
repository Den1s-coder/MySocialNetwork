using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Interfaces;

namespace SocialNetwork.Application.Service
{
    public class UserService : IUserService
    {

        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task BanUser(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }
            user.IsBanned = true;
            await _userRepository.UpdateAsync(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            return _mapper.Map<UserDto?>(user);
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            return _mapper.Map<UserDto?>(user);
        }

        public async Task<UserDto> GetUserByNameAsync(string name)
        {
            var user = await _userRepository.GetByUserNameAsync(name);

            return _mapper.Map<UserDto?>(user);
        }

        public async Task UpdateProfileAsync(UserDto updatedUserDto)
        {
            var existingUser = await _userRepository.GetByIdAsync(updatedUserDto.Id);

            if (existingUser == null)
            {
                throw new ArgumentException("User not found");
            }

            existingUser.Name = updatedUserDto.Name;
            existingUser.Email = updatedUserDto.Email;
            existingUser.ProfilePictureUrl = updatedUserDto.ProfilePictureUrl;
            await _userRepository.UpdateAsync(existingUser);
        }
    }
}
