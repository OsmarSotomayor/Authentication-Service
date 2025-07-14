using AuthenticationSystem.Application.Dtos;
using AuthenticationSystem.Application.Interfaces;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationSystem.Application.Services
{
    public class AuthService:IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IBlacklistRepository _blacklistRepository;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthService(IUserRepository userRepository, IConfiguration configuration, IBlacklistRepository _blacklistRepository)
        {
            this._userRepository = userRepository;
            this._configuration = configuration;
            this._blacklistRepository = _blacklistRepository;
        }
        public async Task RegisterAsync(RegisterUserDto dto)
        {
            var userExist = await _userRepository.GetByUsernameAsync(dto.Username, false);
            if (userExist != null)
                throw new Exception("User already exist");

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = _passwordHasher.HashPassword(null!, dto.Password)
            };

            await _userRepository.CreateUser(user);
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userRepository.GetByUsernameAsync(dto.Username,false);

            if (user == null)
                throw new Exception("Usuario no válido.");

            if (user.IsLocked)
                throw new Exception("Cuenta bloqueada por múltiples intentos fallidos.");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            var isSuccess = result == PasswordVerificationResult.Success;

            // Guardar intento de login
            await _userRepository.AddLoginAttemptAsync(new LoginAttempt
            {
                UserId = user.Id,
                IsSuccess = isSuccess,
                AttemptedAt = DateTime.UtcNow
            });

            if (!isSuccess)
            {
                user.FailedLoginAttempts += 1;
                if (user.FailedLoginAttempts >= 3)
                    user.IsLocked = true;

                await _userRepository.UpdateAsync(user);
                throw new Exception("Credenciales inválidas.");
            }

            // Resetear intentos
            user.FailedLoginAttempts = 0;
            user.LastLoginAt = DateTime.UtcNow;

            // Generar refresh token
            var refreshToken = Guid.NewGuid().ToString();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userRepository.UpdateAsync(user);

            // Generar token JWT
            var jwt = GenerateJwtToken(user);

            return new LoginResponseDto
            {
                Token = jwt.Token,
                ExpiresAt = jwt.ExpiresAt,
                RefreshToken = refreshToken
            };
        }

        private LoginResponseDto GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddHours(1);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("username", user.Username)
                },
                expires: expires,
                signingCredentials: creds
            );

            return new LoginResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = expires
            };
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            var user = await _userRepository.GetByUsernameAsync(dto.Username, false);

            if (user == null ||
                user.RefreshToken != dto.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new Exception("Invalid token or expired");
            }

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = Guid.NewGuid().ToString();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userRepository.UpdateAsync(user);

            return new LoginResponseDto
            {
                Token = newAccessToken.Token,
                ExpiresAt = newAccessToken.ExpiresAt,
                RefreshToken = newRefreshToken
            };
        }

        public async Task LogoutAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new Exception("Token no proporcionado.");

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
                throw new Exception("Token inválido.");
            var jwt = handler.ReadJwtToken(token);
            var expiration = jwt.ValidTo;

            if (expiration <= DateTime.UtcNow)
                throw new Exception("El token ya expiró");

            await _blacklistRepository.AddToBlacklistAsync(token, expiration);
        }

    }
}
