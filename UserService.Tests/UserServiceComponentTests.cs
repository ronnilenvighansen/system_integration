using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using PostService.Controllers;
using PostService.Models;

namespace UserService.Tests
{
    public class UserServiceComponentTests
    {
        private readonly UserDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly IMessagePublisher _messagePublisher;
        private readonly UserController _userController;

        public UserServiceComponentTests()
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;
            _dbContext = new UserDbContext(options);

            var store = new UserStore<User>(_dbContext);
            _userManager = new UserManager<User>(store, null, new PasswordHasher<User>(),
                null, null, null, null, null, null);

            _messagePublisher = new MockMessagePublisher();

            _userController = new UserController(_userManager, _messagePublisher);
        }

        [Fact]
        public async Task Register_CreatesUserAndPublishesMessage()
        {
            // Arrange
            var registerModel = new RegisterModel
            {
                UserName = "testUser",
                Email = "test@example.com",
                Password = "Password123!",
                FullName = "Test User"
            };

            // Act
            var result = await _userController.Register(registerModel);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);

            var createdUser = await _userManager.FindByNameAsync(registerModel.UserName);
            Assert.NotNull(createdUser);
            Assert.Equal(registerModel.Email, createdUser.Email);
            Assert.Equal(registerModel.FullName, createdUser.FullName);

            Assert.True(((MockMessagePublisher)_messagePublisher).MessagePublished);
        }

        [Fact]
        public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Act
            var result = await _userController.GetUser("nonExistentUserId");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }

    public class MockMessagePublisher : IMessagePublisher
    {
        public bool MessagePublished { get; private set; }

        public Task PublishUserCreatedMessage(UserCreatedMessage message)
        {
            MessagePublished = true;
            return Task.CompletedTask;
        }
    }
}
