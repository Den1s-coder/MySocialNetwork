using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SocialNetwork.Application.DTO.Auth;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Entities.Users;
using SocialNetwork.Domain.Interfaces;

namespace SocialNetwork.Application.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IJwtProvider _jwtProvider;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly string _googleClientId;

        public AuthService(IUserRepository userRepository,
            IMapper mapper, 
            IJwtProvider jwtProvider, 
            IRefreshTokenRepository refreshTokenRepository,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _jwtProvider = jwtProvider;
            _refreshTokenRepository = refreshTokenRepository;
            _googleClientId = configuration["Authentication:GoogleClientId"];
        }

        public async Task<LoginResponce> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
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

        public async Task<LoginResponce> LoginWithGoogleAsync(string idToken, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrEmpty(idToken))
                throw new ArgumentException("ID token is null or empty", nameof(idToken));

            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings();
                if(!string.IsNullOrEmpty(_googleClientId))
                {
                    settings.Audience = new[] { _googleClientId };
                }
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("Invalid Google ID token", ex);
            }

            if(string.IsNullOrWhiteSpace(payload.Email) || (payload.EmailVerified is not true))
                throw new UnauthorizedAccessException("Google ID token does not contain email or Email not Verified");

            var user = await _userRepository.GetByEmailAsync(payload.Email);

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Name = payload.Name ?? payload.Email,
                    Email = payload.Email,
                    ProfilePictureUrl = payload.Picture
                };
                await _userRepository.CreateAsync(user);
            }

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

        public async Task RegisterAsync(RegisterDto userDto, CancellationToken cancellationToken = default)
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
