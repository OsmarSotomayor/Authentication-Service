using AuthenticationSystem.Application.Dtos;
using AuthenticationSystem.Application.Interfaces;
using Domain.Interfaces;
using Domain.Models;
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
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            this._userRepository = userRepository;
            this._configuration = configuration;
        }
        public async Task RegisterAsync(RegisterUserDto dto)
        {
            var userExist = await _userRepository.GetByUsernameAsync(dto.Username, false);
            if (userExist != null)
                throw new Exception("El usuario ya existe");

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = _passwordHasher.HashPassword(null!, dto.Password)
            };

            await _userRepository.CreateUser(user);
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userRepository.GetByUsernameAsync(dto.Username, false);

            if (user == null)
                throw new Exception("Usuario no válido.");

            if (user.IsLocked)
                throw new Exception("Cuenta bloqueada por múltiples intentos fallidos.");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            var isSuccess = result == PasswordVerificationResult.Success;

            await _userRepository.AddLoginAttemptAsync(new LoginAttempt
            {
                UserId = user.Id,
                IsSuccess = isSuccess
            });

            if (!isSuccess)
            {
                user.FailedLoginAttempts += 1;
                if (user.FailedLoginAttempts >= 3)
                    user.IsLocked = true;

                await _userRepository.UpdateAsync(user);
                throw new Exception("Credenciales inválidas.");
            }

            // Reset attempts
            user.FailedLoginAttempts = 0;
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var token = GenerateJwtToken(user);

            return token;
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
    }
}
