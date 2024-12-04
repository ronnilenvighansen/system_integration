using Moq;
using Microsoft.AspNetCore.Mvc;
using PostService.Models;
using Microsoft.AspNetCore.Identity;
using PostService.Controllers;
using UserService.Services;

namespace UserService.Tests;

public class UserControllerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IMessagePublisher> _messagePublisherMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        _messagePublisherMock = new Mock<IMessagePublisher>();

        _controller = new UserController(_userManagerMock.Object, _messagePublisherMock.Object);
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenUserCreationIsSuccessful()
    {
        // Arrange
        var registerModel = new RegisterModel
        {
            UserName = "testuser",
            Email = "test@example.com",
            FullName = "Test User",
            Password = "Test@1234"
        };

        var user = new User
        {
            UserName = registerModel.UserName,
            Email = registerModel.Email,
            FullName = registerModel.FullName
        };

        _userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<User>(), registerModel.Password))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.Register(registerModel) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(user.UserName, (result.Value as User)?.UserName);

        _messagePublisherMock.Verify(mp => mp.PublishUserCreatedMessage(
            It.Is<UserCreatedMessage>(msg => 
                msg.UserName == registerModel.UserName && 
                msg.Email == registerModel.Email)
        ), Times.Once);

    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenUserCreationFails()
    {
        // Arrange
        var registerModel = new RegisterModel
        {
            UserName = "testuser",
            Email = "test@example.com",
            FullName = "Test User",
            Password = "Test@1234"
        };

        _userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<User>(), registerModel.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User creation failed" }));

        // Act
        var result = await _controller.Register(registerModel) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.IsType<List<IdentityError>>(result.Value);
    }
}