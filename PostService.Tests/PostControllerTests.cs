using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class PostControllerTests
{
    private readonly DbContextOptions<PostDbContext> _dbOptions;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IUserServiceClient> _mockUserServiceClient;
    private readonly HttpClient _mockHttpClient;

    public PostControllerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<PostDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _mockHttpClientFactory = new Mock<IHttpClientFactory>();

        _mockUserServiceClient = new Mock<IUserServiceClient>();

        _mockHttpClient = new HttpClient(new MockHttpMessageHandler());

        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(_mockHttpClient);
    }

    [Fact]
    public async Task CreatePost_ReturnsCreatedAtAction_WhenUserIsValid()
    {
        // Arrange
        var mockUserServiceResponse = new UserEntity
        {
            Id = "validUserId",
            UserName = "testUser",
            Email = "test@user.com"
        };

        var mockPost = new Post
        {
            Id = Guid.NewGuid(),
            UserId = "validUserId",
            Content = "This is a test post."
        };

        _mockUserServiceClient.Setup(client => client.GetUserByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(mockUserServiceResponse);

        // Use an actual PostDbContext here or mock it depending on your setup
        using var context = new PostDbContext(_dbOptions);
        var controller = new PostController(context, _mockHttpClientFactory.Object, _mockUserServiceClient.Object);

        // Act
        var result = await controller.CreatePost(mockPost);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Post>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var createdPost = Assert.IsType<Post>(createdAtActionResult.Value);
        
        Assert.Equal(mockPost.UserId, createdPost.UserId);
        Assert.Equal(mockPost.Content, createdPost.Content);
        Assert.Equal(201, createdAtActionResult.StatusCode);
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
        var controller = new PostController(context, _mockHttpClientFactory.Object, _mockUserServiceClient.Object);

        // Act
        var result = await controller.CreatePost(mockPost);

        // Assert
        var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid or non-existent user.", badRequestObjectResult.Value);
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
        var controller = new PostController(context, _mockHttpClientFactory.Object, _mockUserServiceClient.Object);

        // Act
        var result = await controller.CreatePost(mockPost);

        // Assert
        var badRequestResult = Assert.IsType<ActionResult<Post>>(result);
        var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(badRequestResult.Result);
        Assert.Equal("Invalid or non-existent user.", badRequestObjectResult.Value);
    }

    [Fact]
    public async Task CreatePost_ReturnsInternalServerError_WhenUserServiceFails()
    {
        // Arrange
        var mockPost = new Post
        {
            UserId = "validUserId",
            Content = "This is a test post."
        };

        _mockUserServiceClient.Setup(client => client.GetUserByIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new System.Exception("User service communication error."));

        using var context = new PostDbContext(_dbOptions);
        var controller = new PostController(context, _mockHttpClientFactory.Object, _mockUserServiceClient.Object);

        // Act
        var result = await controller.CreatePost(mockPost);

        // Assert
        var statusCodeResult = Assert.IsType<ActionResult<Post>>(result);
        var objectResult = Assert.IsType<ObjectResult>(statusCodeResult.Result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("User service communication error.", objectResult.Value);
    }
}
