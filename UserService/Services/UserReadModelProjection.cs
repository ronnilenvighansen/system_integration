using Newtonsoft.Json;
using Shared.Models;

public class UserReadModelProjection
{
    private readonly UserDbContext _eventStore;
    private readonly UserDbContext _readModelDbContext;

    public UserReadModelProjection(UserDbContext eventStore, UserDbContext readModelDbContext)
    {
        _eventStore = eventStore;
        _readModelDbContext = readModelDbContext;
    }

    public void Project()
    {
        var events = _eventStore.DomainEvents.ToList();

        foreach (var @event in events)
        {
            switch (@event.EventType)
            {
                case nameof(UserCreatedMessage):
                    var userCreatedEvent = JsonConvert.DeserializeObject<UserCreatedMessage>(@event.Data);
                    _readModelDbContext.Users.Add(new User
                    {
                        Id = userCreatedEvent.UserId,
                        UserName = userCreatedEvent.UserName,
                        Email = userCreatedEvent.Email,
                        FullName = userCreatedEvent.FullName
                    });
                break;

                case nameof(UserDeletedMessage):
                    var userDeletedEvent = JsonConvert.DeserializeObject<UserDeletedMessage>(@event.Data);
                    var userToDelete = _readModelDbContext.Users.FirstOrDefault(u => u.Id == userDeletedEvent.UserId);
                    if (userToDelete != null)
                    {
                        _readModelDbContext.Users.Remove(userToDelete);
                    }
                    break;

            }
        }

        _readModelDbContext.SaveChanges();
    }
}