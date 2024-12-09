namespace PostService.Services
{
    public interface IIDValidationService
    {
        Task<bool> ValidateIDAsync(string userId);
    }
}
