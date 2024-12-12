using Polly;

namespace PostService.Services
{
    public class IDValidationService : IIDValidationService
    {
        private readonly HttpClient _httpClient;
        private readonly IAsyncPolicy<HttpResponseMessage> _policy;

        public IDValidationService(HttpClient httpClient, 
            IAsyncPolicy<HttpResponseMessage> policy)
        {
            _httpClient = httpClient;
            _policy = policy;
        }

        public async Task<bool> ValidateIDAsync(string userId)
        {
            try
            {
                Console.WriteLine($"Making request to {_httpClient.BaseAddress}validate-id/{userId}");

                var response = await _policy.ExecuteAsync(() =>
                    _httpClient.GetAsync($"validate-id/{userId}"));

                Console.WriteLine($"Response Status Code: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Content: {content}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating ID: {ex.Message}");
                return false;
            }
        }
    }
}
