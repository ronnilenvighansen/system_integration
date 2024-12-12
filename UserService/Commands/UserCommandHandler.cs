using Shared.Services;
using UserService.Commands;
using Shared.Models;
using Newtonsoft.Json;
using UserService.Models;

public class UserCommandHandler
{
    private readonly UserDbContext _userDbContext;
    private readonly IMessagePublisher _messagePublisher;

    public UserCommandHandler(UserDbContext userDbContext, IMessagePublisher messagePublisher)
    {
        _userDbContext = userDbContext;
        _messagePublisher = messagePublisher;
    }

    public async Task Handle(CreateUserCommand command)
    {
        var userCreatedEvent = new UserCreatedMessage
        {
            UserId = Guid.NewGuid().ToString(),
            UserName = command.UserName,
            Email = command.Email,
            FullName = command.FullName
        };

        _userDbContext.DomainEvents.Add(new DomainEvent
        {
            Id = Guid.NewGuid(),
            OccurredOn = DateTime.UtcNow,
            EventType = typeof(UserCreatedMessage).FullName,
            Data = JsonConvert.SerializeObject(userCreatedEvent)
        });

        _userDbContext.SaveChanges();

        await _messagePublisher.PublishUserCreatedMessage(userCreatedEvent);
    }

    public async Task Handle(DeleteUserCommand command)
    {
        var userDeletedEvent = new UserDeletedMessage
        {
            UserId = command.UserId,
            UserName = command.UserName
        };

        _userDbContext.DomainEvents.Add(new DomainEvent
        {
            Id = Guid.NewGuid(),
            OccurredOn = DateTime.UtcNow,
            EventType = typeof(UserDeletedMessage).FullName,
            Data = JsonConvert.SerializeObject(userDeletedEvent)
        });

        _userDbContext.SaveChanges();

        await _messagePublisher.PublishUserDeletedMessage(userDeletedEvent);
    }
}
