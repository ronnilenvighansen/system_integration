using EasyNetQ;
using Shared.Models;

namespace Shared.Services;

public class MessagePublisher : IMessagePublisher
{
    private readonly IBus _bus;

    public MessagePublisher(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishUserCreatedMessage(UserCreatedMessage message)
    {
        await _bus.PubSub.PublishAsync(message);
    }
}
