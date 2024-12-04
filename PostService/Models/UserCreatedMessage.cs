namespace PostService.Models
{
    public class UserCreatedMessage
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }
}
