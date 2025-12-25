using AutoMapper;
using Azure.Core;
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
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthService(IUserRepository userRepository, IMapper mapper, IJwtProvider jwtProvider, IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _jwtProvider = jwtProvider;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<LoginResponce> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByUserNameAsync(loginDto.Username);

            if (user == null)
                throw new ArgumentNullException("User not found");

            var verifyResult = new PasswordHasher<User>()
                .VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

            if (verifyResult == PasswordVerificationResult.Success)
            {
                var accessToken = _jwtProvider.GenerateAccessToken(user);

                var refreshToken = _jwtProvider.GenerateRefreshToken();

                var refreshTokenEntity = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    Token = refreshToken,
                    UserId = user.Id,
                    ExpiresOn = DateTime.UtcNow.AddDays(7)
                };

                await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

                return new LoginResponce(accessToken, refreshToken);
            }
            else
            {
                throw new UnauthorizedAccessException("Invalid password");
            }
        }

        public async Task<LoginResponce> LoginWithRefreshTokenAsync(string refreshToken)
        {
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (existingToken == null || existingToken.ExpiresOn < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid or expired refresh token");

            var user = await _userRepository.GetByIdAsync(existingToken.UserId);
            if (user == null)
                throw new ArgumentNullException("User not found");

            var newAccessToken = _jwtProvider.GenerateAccessToken(user);
            var newRefreshToken = _jwtProvider.GenerateRefreshToken();

            existingToken.Token = newRefreshToken;
            existingToken.ExpiresOn = DateTime.UtcNow.AddDays(7);

            await _refreshTokenRepository.UpdateAsync(existingToken);
            return new LoginResponce(newAccessToken, newRefreshToken);
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
