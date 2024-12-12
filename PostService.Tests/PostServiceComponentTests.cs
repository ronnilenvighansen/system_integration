using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostService.Controllers;
using Shared.Services;
using PostService.Services;

namespace PostService.Tests
{
    public class PostControllerTests
    {
        private readonly Mock<IIDValidationService> _mockIdValidationService;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<IMessagePublisher> _mockMessagePublisher;
        private readonly PostController _postController;
        private readonly PostDbContext _context;

        public PostControllerTests()
        {
            _mockIdValidationService = new Mock<IIDValidationService>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockMessagePublisher = new Mock<IMessagePublisher>();

            var options = new DbContextOptionsBuilder<PostDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;
            _context = new PostDbContext(options);

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://userservice")
            };
            _mockHttpClientFactory
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

            _postController = new PostController(
                _mockIdValidationService.Object,
                _context, 
                _mockHttpClientFactory.Object, 
                _mockMessagePublisher.Object
            );
        }

        [Fact]
        public void HealthCheck_ShouldReturnOk()
        {
            // Act
            var result = _postController.HealthCheck();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Service is running", actionResult.Value);
        }

        [Fact]
        public async Task UpdatePost_ShouldUpdateExistingPost_AndReturnNoContent()
        {
            // Arrange
            var existingPost = new Post
            {
                Id = Guid.NewGuid(),
                Content = "Existing content",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow
            };
            _context.Posts.Add(existingPost);
            await _context.SaveChangesAsync();

            var updatedPost = new Post
            {
                Id = existingPost.Id,
                Content = "Updated content",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _postController.UpdatePost(existingPost.Id, updatedPost);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var postInDb = await _context.Posts.FindAsync(existingPost.Id);
            Assert.Equal(updatedPost.Content, postInDb.Content);
        }

        [Fact]
        public async Task UpdatePost_ShouldReturnNotFound_WhenPostDoesNotExist()
        {
            // Arrange
            var updatedPost = new Post
            {
                Id = Guid.NewGuid(),
                Content = "Updated content",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _postController.UpdatePost(updatedPost.Id, updatedPost);

            // Assert
            var actionResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Post not found.", actionResult.Value);
        }

        [Fact]
        public async Task DeletePost_ShouldRemovePost_AndReturnNoContent()
        {
            // Arrange
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Content = "Test content",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow
            };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postController.DeletePost(post.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var postInDb = await _context.Posts.FindAsync(post.Id);
            Assert.Null(postInDb);
        }

        [Fact]
        public async Task DeletePost_ShouldReturnNotFound_WhenPostDoesNotExist()
        {
            // Act
            var result = await _postController.DeletePost(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
