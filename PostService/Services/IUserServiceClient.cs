public interface IUserServiceClient
{
    Task<UserEntity?> GetUserByIdAsync(string userId);
}
