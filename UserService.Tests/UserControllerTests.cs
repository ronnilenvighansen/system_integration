using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using UserService.Controllers;
using Shared.Services;
using UserService.Commands;
using Microsoft.EntityFrameworkCore;

namespace UserService.Tests;

public class UserControllerTests
{
    private readonly UserDbContext _dbContext;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IMessagePublisher> _messagePublisherMock;
    private readonly UserController _controller;
    private readonly Mock<UserCommandHandler> _mockCommandHandler;


    public UserControllerTests()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;
            _dbContext = new UserDbContext(options);
            
        _userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        _messagePublisherMock = new Mock<IMessagePublisher>();

        _mockCommandHandler = new Mock<UserCommandHandler>(_dbContext, _messagePublisherMock.Object);


        _controller = new UserController(_userManagerMock.Object, _messagePublisherMock.Object, _mockCommandHandler.Object);
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

        var command = new CreateUserCommand
        {
            UserName = registerModel.UserName,
            Email = registerModel.Email,
            FullName = registerModel.FullName
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