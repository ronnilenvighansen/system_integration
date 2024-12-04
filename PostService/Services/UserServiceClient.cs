public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;

    public UserServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserEntity?> GetUserByIdAsync(string userId)
    {
        var response = await _httpClient.GetAsync($"byid/{userId}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<UserEntity>();
    }
}
