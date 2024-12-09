using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Services;
using PostService.Controllers;
using PostService.Services;

public class PostControllerTests
{
    private readonly Mock<IIDValidationService> _mockIdValidationService;
    private readonly DbContextOptions<PostDbContext> _dbOptions;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IUserServiceClient> _mockUserServiceClient;
    private readonly HttpClient _mockHttpClient;
    private readonly Mock<IMessagePublisher> _mockMessagePublisher;


    public PostControllerTests()
    {   
        _mockIdValidationService = new Mock<IIDValidationService>();
        _dbOptions = new DbContextOptionsBuilder<PostDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _mockHttpClientFactory = new Mock<IHttpClientFactory>();

        _mockUserServiceClient = new Mock<IUserServiceClient>();

        _mockHttpClient = new HttpClient(new MockHttpMessageHandler());

        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(_mockHttpClient);
        _mockMessagePublisher = new Mock<IMessagePublisher>();
    }

    [Fact]
    public async Task CreatePost_ReturnsBadRequest_WhenUserNotFound()
    {
        // Arrange
        var mockPost = new Post
        {
            UserId = "nonExistingUserId",
            Content = "This is a test post."
        };

        _mockUserServiceClient.Setup(client => client.GetUserByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((UserEntity)null); // No user found

        using var context = new PostDbContext(_dbOptions);
        var controller = new PostController(_mockIdValidationService.Object, context, _mockHttpClientFactory.Object, _mockUserServiceClient.Object, _mockMessagePublisher.Object);

        // Act
        var result = await controller.CreatePost(mockPost);

        // Assert
        var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid ID.", badRequestObjectResult.Value);
    }

    [Fact]
    public async Task CreatePost_ReturnsBadRequest_WhenUserInvalidDataReturned()
    {
        // Arrange
        var mockUserServiceResponse = new UserEntity
        {
            Id = "",
            UserName = "testUser",
            Email = "test@user.com"
        };

        var mockPost = new Post
        {
            UserId = "invalidUserId",
            Content = "This is a test post."
        };

        _mockUserServiceClient.Setup(client => client.GetUserByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(mockUserServiceResponse);

        using var context = new PostDbContext(_dbOptions);
        var controller = new PostController(_mockIdValidationService.Object, context, _mockHttpClientFactory.Object, _mockUserServiceClient.Object, _mockMessagePublisher.Object);

        // Act
        var result = await controller.CreatePost(mockPost);

        // Assert
        var badRequestResult = Assert.IsType<ActionResult<Post>>(result);
        var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(badRequestResult.Result);
        Assert.Equal("Invalid ID.", badRequestObjectResult.Value);
    }
}
