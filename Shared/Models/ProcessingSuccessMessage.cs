public class ProcessingSuccessMessage
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string? StatusMessage { get; set; }
}
