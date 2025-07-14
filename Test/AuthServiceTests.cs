using AuthenticationSystem.Application.Dtos;
using AuthenticationSystem.Application.Services;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class AuthServiceTests
    {
        //Declaramos lo que vamos a revisar con mock
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<IBlacklistRepository> _blacklistRepoMock = new();
        private readonly IConfiguration _config;

        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "TestSecretKeyTestSecretKey" },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" }
        };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _authService = new AuthService(
                _userRepoMock.Object,
                _config,
                _blacklistRepoMock.Object
            );
        }

        [Fact]
        public async Task RegisterAsync_Should_Register_New_User()
        {
            //Indicamos que metodos del user repo vamos a probar 
            _userRepoMock.Setup(r => r.GetByUsernameAsync("test", false))
                .ReturnsAsync((User?)null);

            _userRepoMock.Setup(r => r.CreateUser(It.IsAny<User>())) //IsAny indica queacepta cualquier objeto de ese metodo
                .Returns(Task.CompletedTask);

            var dto = new RegisterUserDto { Username = "test", Password = "1234" };
            await _authService.RegisterAsync(dto); //probamos el metodo RegisterAsync

            _userRepoMock.Verify(r => r.CreateUser(It.Is<User>(u => u.Username == "test")), Times.Once);
        }

        [Fact]
        public async Task RegisterAsyncShould_Throw_If_User_Exists()
        {
            _userRepoMock.Setup(r => r.GetByUsernameAsync("admin", false))
                .ReturnsAsync(new User { Username = "admin" });

            var dto = new RegisterUserDto { Username = "admin", Password = "pass" };

            await Assert.ThrowsAsync<Exception>(() => _authService.RegisterAsync(dto));
        }

        [Fact]
        public async Task LoginAsync_Should_Throw_When_User_Locked()
        {
            var user = new User
            {
                Id =1,
                Username = "admin",
                PasswordHash = new PasswordHasher<User>().HashPassword(null!, "password"),
                IsLocked = true
            };

            _userRepoMock.Setup(r => r.GetByUsernameAsync("admin", false)).ReturnsAsync(user);
            var dto = new LoginRequestDto { Username = "admin", Password = "password" };

            await Assert.ThrowsAsync<Exception>(() => _authService.LoginAsync(dto));
        }
    }
}
