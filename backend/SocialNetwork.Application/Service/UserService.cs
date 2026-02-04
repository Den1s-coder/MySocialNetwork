using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SocialNetwork.Application.DTO.Auth;
using SocialNetwork.Application.DTO.Users;
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

        public async Task BanUser(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }
            user.IsBanned = true;
            await _userRepository.UpdateAsync(user);
        }

        public async Task ChangeEmailAsync(Guid userId, ChangeEmailDto changeEmailDto, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new ArgumentException("User not found");

            var verifyResult = new PasswordHasher<User>()
                .VerifyHashedPassword(user, user.PasswordHash, changeEmailDto.Password);

            if (verifyResult != PasswordVerificationResult.Success)
                throw new UnauthorizedAccessException("Invalid password");

            user.Email = changeEmailDto.NewEmail;
            await _userRepository.UpdateAsync(user);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new ArgumentException("User not found");

            var verifyResult = new PasswordHasher<User>()
                .VerifyHashedPassword(user, user.PasswordHash, changePasswordDto.CurrentPassword);

            if (verifyResult != PasswordVerificationResult.Success)
                throw new UnauthorizedAccessException("Invalid password");

            user.PasswordHash = new PasswordHasher<User>()
                .HashPassword(user, changePasswordDto.NewPassword);
            await _userRepository.UpdateAsync(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.GetAllAsync();
            
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(id);

            return _mapper.Map<UserDto?>(user);
        }

        public async Task<UserDto> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            return _mapper.Map<UserDto?>(user);
        }

        public async Task<UserDto> GetUserByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByUserNameAsync(name);

            return _mapper.Map<UserDto?>(user);
        }

        public async Task UpdateProfileAsync(UserDto updatedUserDto, CancellationToken cancellationToken = default)
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
