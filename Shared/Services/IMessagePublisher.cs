using Shared.Models;

namespace Shared.Services;

public interface IMessagePublisher
{
    Task PublishUserCreatedMessage(UserCreatedMessage message);
    Task PublishUserDeletedMessage(UserDeletedMessage message);
}
