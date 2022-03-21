﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TicTacToe.Server.Services;
using TicTacToe.Server.Controllers;
using TicTacToe.Server.DTO;
using TicTacToe.Server.Exceptions;
using Xunit;

namespace ServerTests.ControllerTests
{
    public class AccountControllerTest
    {
        private static readonly Mock<ILogger<AccountController>> _loggerMock = new();

        private static readonly Mock<IAccountService> _serviceMock = new();

        private readonly AccountController _accountController = ConfigureControllerContext();

        private static AccountController ConfigureControllerContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Custom-Header"] = "account-test-methods";

            return new AccountController(_loggerMock.Object, _serviceMock.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                },
            };
        }

        public static readonly object[] ValidUsersLogin =
        {
            new object[] {"qwerty", "111111" },
            new object[] {"qwerty123", "string" },
        };

        [Theory, MemberData(nameof(ValidUsersLogin))]
        public async Task LoginPressValidDataShouldReturnOk(string login, string password)
        {
            //Arrange
            var account = new UserAccountDto(login, password);
            _ = _serviceMock.Setup(x => x.InvokeLoginAsync(account));

            //Act
            var result = await _accountController.LoginAsync(account);

            //Assert
            Assert.NotNull(result);

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);

            var content = okResult?.Value as string;
            Assert.NotNull(content);

            Assert.Equal(login, content);
        }

        [Theory, MemberData(nameof(ValidUsersLogin))]
        public async Task LoginAccountShouldReturnNotFoundIfInputLoginNotExist(string login, string password)
        {
            //Arrange
            var account = new UserAccountDto(login, password);
            _ = _serviceMock.Setup(x => x.InvokeLoginAsync(account))
                .Throws<AccountException>();

            //Act
            var result = await _accountController.LoginAsync(account);

            //Assert
            Assert.NotNull(result);
            _ = Assert.IsType<NotFoundObjectResult>(result);
        }

        [Theory, MemberData(nameof(ValidUsersLogin))]
        public async Task LoginAccountShouldReturnConflictIfUserTryLoginThreeTimes(string login,
            string password)
        {
            //Arrange
            var account = new UserAccountDto(login, password);
            _ = _serviceMock.Setup(x => x.InvokeLoginAsync(account))
                .Throws<TimeoutException>();

            //Act
            var result = await _accountController.LoginAsync(account);

            //Assert
            Assert.NotNull(result);
            _ = Assert.IsType<ConflictObjectResult>(result);
        }

        [Theory, MemberData(nameof(ValidUsersLogin))]
        public async Task RegistrationAsyncShouldReturnOkIfInputValidData(string login, string password)
        {
            //Arrange
            var account = new UserAccountDto(login, password);
            _ = _serviceMock.Setup(x => x.InvokeRegistrationAsync(account));

            //Act
            var result = await _accountController.RegistrationAsync(account);

            //Assert
            Assert.NotNull(result);

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);

            var content = okResult?.Value as string;
            Assert.NotNull(content);

            Assert.Equal(login, content);
        }

        [Theory, MemberData(nameof(ValidUsersLogin))]
        public async Task RegistrationAsyncShouldReturnConflictIfUserAlreadyExit(string login, string password)
        {
            //Arrange
            var account = new UserAccountDto(login, password);
            _ = _serviceMock.Setup(x => x.InvokeRegistrationAsync(account))
                .Throws<AccountException>();

            //Act
            var result = await _accountController.RegistrationAsync(account);

            //Assert
            Assert.NotNull(result);
            _ = Assert.IsType<ConflictObjectResult>(result);
        }

        [Theory]
        [InlineData("qwerty")]
        [InlineData("qwerty123")]
        public async Task LogoutAsyncShouldReturnOk(string login)
        {
            //Arrange
            _ = _serviceMock.Setup(x => x.RemoveActiveAccountByLogin(login));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Login"] = login;
            var accountController = new AccountController(_loggerMock.Object, _serviceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };

            //Act
            var result = await accountController.LogoutAsync();

            //Assert
            Assert.NotNull(result);
            _ = Assert.IsType<OkObjectResult>(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task LogoutAsyncShouldReturnUnAuthorizedIfUserNonAuthorized(string login)
        {
            //Arrange
            _ = _serviceMock.Setup(x => x.RemoveActiveAccountByLogin(login))
                .Throws<AccountException>();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Login"] = login;
            var accountController = new AccountController(_loggerMock.Object, _serviceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };

            //Act
            var result = await accountController.LogoutAsync();

            //Assert
            Assert.NotNull(result);
            _ = Assert.IsType<UnauthorizedResult>(result);
        }
    }
}
