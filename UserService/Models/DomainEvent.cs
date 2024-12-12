namespace UserService.Models;
public class DomainEvent
{
    public Guid Id { get; set; }
    public DateTime OccurredOn { get; set; }
    public string EventType { get; set; }
    public string Data { get; set; }
}