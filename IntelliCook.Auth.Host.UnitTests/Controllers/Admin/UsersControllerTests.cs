using FluentAssertions;
using IntelliCook.Auth.Contract.User;
using IntelliCook.Auth.Host.Controllers.Admin;
using IntelliCook.Auth.Host.UnitTests.Utils;
using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IntelliCook.Auth.Host.UnitTests.Controllers.Admin;

public class UsersControllerTests
{
    private readonly UsersController _usersController;
    private readonly Mock<UserManager<IntelliCookUser>> _userManagerMock;

    public UsersControllerTests()
    {
        _userManagerMock = new Mock<UserManager<IntelliCookUser>>(
            Mock.Of<IUserStore<IntelliCookUser>>(),
            null,
            null,
            null,
            null,
            null,
            new IdentityErrorDescriber(),
            null,
            null
        );
        _usersController = new UsersController(_userManagerMock.Object);
    }

    #region Get

    [Fact]
    public async void Get_Success_ReturnsOkObjectResult()
    {
        // Arrange
        var users = new TestAsyncEnumerable<IntelliCookUser>(new List<IntelliCookUser>
        {
            new()
            {
                Name = "Test User",
                Role = UserRoleModel.User,
                UserName = "testuser",
                Email = "User@Example.com"
            },
            new()
            {
                Name = "Test Admin",
                Role = UserRoleModel.Admin,
                UserName = "testadmin",
                Email = "Admin@Example.com"
            }
        });

        _userManagerMock.Setup(x => x.Users).Returns(users.AsQueryable());

        // Act
        var result = await _usersController.Get();

        // Assert
        result.Should().BeOfType<OkObjectResult>().Which
            .Value.Should().BeEquivalentTo(users.Select(user => new UserGetResponseModel
            {
                Name = user.Name,
                Role = user.Role,
                Username = user.UserName,
                Email = user.Email
            }));
    }

    #endregion
}