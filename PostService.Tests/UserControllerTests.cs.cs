using Microsoft.AspNetCore.Mvc;
using Moq;
using PostService.Models;
using UserService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PostService.Controllers;

namespace PostService.Tests
{
    public class UserControllerTests
    {
        private readonly UserController _controller;
        private readonly Mock<IMessagePublisher> _mockMessagePublisher;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly UserDbContext _dbContext;

        public UserControllerTests()
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;
            _dbContext = new UserDbContext(options);

            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null);

            _mockMessagePublisher = new Mock<IMessagePublisher>();

            _controller = new UserController(_mockUserManager.Object, _mockMessagePublisher.Object);
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

            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), registerModel.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Register(registerModel);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);

            _mockMessagePublisher.Verify(m => m.PublishUserCreatedMessage(It.IsAny<UserCreatedMessage>()), Times.Once);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUserCreationFails()
        {
            // Arrange
            var registerModel = new RegisterModel
            {
                UserName = "testUser",
                Email = "test@example.com",
                Password = "Password123!",
                FullName = "Test User"
            };

            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), registerModel.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

            // Act
            var result = await _controller.Register(registerModel);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, actionResult.StatusCode);
        }

        [Fact]
        public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "nonExistentUserId";

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetUserById_ReturnsOk_WhenUserExists()
        {
            // Arrange
            var userId = "existingUserId";

            var user = new User { Id = userId, UserName = "testUser", Email = "test@example.com" };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic returnValue = okResult.Value;
            Assert.Equal(userId, returnValue.Id);
        }

    }
}
