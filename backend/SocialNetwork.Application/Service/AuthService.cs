using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IJwtProvider _jwtProvider;

        public AuthService(IUserRepository userRepository, IMapper mapper, IJwtProvider jwtProvider)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _jwtProvider = jwtProvider;
        }

        public async Task<string> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByUserNameAsync(loginDto.Username);

            if (user == null)
                throw new ArgumentNullException("User not found");

            var verifyResult = new PasswordHasher<User>()
                .VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

            if (verifyResult == PasswordVerificationResult.Success)
            {
                return _jwtProvider.Generate(user);
            }
            else
            {
                throw new UnauthorizedAccessException("Invalid password");
            }
        }

        public async Task RegisterAsync(RegisterDto userDto)
        {
            if (userDto == null)
                throw new ArgumentNullException("userDTO is null");

            var user = _mapper.Map<User>(userDto);

            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, user.PasswordHash);

            if (user == null)
                throw new InvalidOperationException("Mapping failed");

            await _userRepository.CreateAsync(user);
        }
    }
}
