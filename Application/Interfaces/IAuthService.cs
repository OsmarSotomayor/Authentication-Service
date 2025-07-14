using AuthenticationSystem.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationSystem.Application.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Add a new user 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task RegisterAsync(RegisterUserDto dto);

        /// <summary>
        /// Began session of a new user 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);

        Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto);

        Task LogoutAsync(string token);

    }
}
