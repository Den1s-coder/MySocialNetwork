using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.DTO.Auth;
using SocialNetwork.Application.DTO.Users;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities.Users;
using SocialNetwork.Domain.Enums;
using SocialNetwork.Domain.Interfaces;

namespace SocialNetwork.Application.Service
{
    public class UserService : IUserService
    {

        private readonly IUserRepository _userRepository;
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IFriendshipRepository friendshipRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _friendshipRepository = friendshipRepository;
            _mapper = mapper;
        }

        public async Task BanUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }
            user.IsBanned = true;
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        public async Task ChangeEmailAsync(Guid userId, ChangeEmailDto changeEmailDto, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
                throw new ArgumentException("User not found");

            var verifyResult = new PasswordHasher<User>()
                .VerifyHashedPassword(user, user.PasswordHash, changeEmailDto.Password);

            if (verifyResult != PasswordVerificationResult.Success)
                throw new UnauthorizedAccessException("Invalid password");

            user.Email = changeEmailDto.NewEmail;
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
                throw new ArgumentException("User not found");

            var verifyResult = new PasswordHasher<User>()
                .VerifyHashedPassword(user, user.PasswordHash, changePasswordDto.CurrentPassword);

            if (verifyResult != PasswordVerificationResult.Success)
                throw new UnauthorizedAccessException("Invalid password");

            user.PasswordHash = new PasswordHasher<User>()
                .HashPassword(user, changePasswordDto.NewPassword);
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        public async Task ChangeRoleAsync(Guid id, string newRole, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);

            if (user == null)
                throw new ArgumentException("User not found");

            if (!Enum.TryParse<UserRole>(newRole, out var role))
                throw new ArgumentException("Invalid role");

            user.Role = role;
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        public async Task ChangeUserRoleAsync(Guid userId, UserRole newRole, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
                throw new ArgumentException("User not found");

            user.Role = newRole;
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.GetAllAsync(cancellationToken);
            
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.GetAllAsync(cancellationToken);
            
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);

            return _mapper.Map<UserDto?>(user);
        }

        public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            return _mapper.Map<UserDto?>(user);
        }

        public async Task<UserDto?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByUserNameAsync(userName, cancellationToken);

            return _mapper.Map<UserDto?>(user);
        }

        public async Task<UserRole> GetUserRoleAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
                throw new ArgumentException("User not found");

            return user.Role;
        }

        public async Task<UserDto> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            return _mapper.Map<UserDto?>(user);
        }

        public async Task<UserDto> GetUserByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByUserNameAsync(name, cancellationToken);

            return _mapper.Map<UserDto?>(user);
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.GetUsersByRoleAsync(role, cancellationToken);
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<PaginetedResult<UserDto>> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query cannot be empty", nameof(query));

            var (items, total) = await _userRepository.SearchAsync(query, page, pageSize, cancellationToken);

            var result = new PaginetedResult<UserDto>
            {
                Items = _mapper.Map<IEnumerable<UserDto>>(items),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
            return result;
        }

        public async Task UpdateAsync(Guid id, UserDto userDto, CancellationToken cancellationToken = default)
        {
            var existingUser = await _userRepository.GetByIdAsync(id, cancellationToken);

            if (existingUser == null)
            {
                throw new ArgumentException("User not found");
            }

            existingUser.Name = userDto.Name;
            existingUser.Email = userDto.Email;
            existingUser.ProfilePictureUrl = userDto.ProfilePictureUrl;
            await _userRepository.UpdateAsync(existingUser, cancellationToken);
        }

        public async Task UpdateProfileAsync(UserDto updatedUserDto, CancellationToken cancellationToken = default)
        {
            var existingUser = await _userRepository.GetByIdAsync(updatedUserDto.Id, cancellationToken);

            if (existingUser == null)
            {
                throw new ArgumentException("User not found");
            }

            existingUser.Name = updatedUserDto.Name;
            existingUser.Email = updatedUserDto.Email;
            existingUser.ProfilePictureUrl = updatedUserDto.ProfilePictureUrl;
            await _userRepository.UpdateAsync(existingUser, cancellationToken);
        }
    }
}
