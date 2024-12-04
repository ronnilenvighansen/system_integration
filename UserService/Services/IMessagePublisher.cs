using PostService.Models;

namespace UserService.Services;

public interface IMessagePublisher
{
    Task PublishUserCreatedMessage(UserCreatedMessage message);
}
