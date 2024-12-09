using Polly;

namespace PostService.Services
{
    public class PolicyHandler : DelegatingHandler
    {
        private readonly IAsyncPolicy<HttpResponseMessage> _policy;

        public PolicyHandler(IAsyncPolicy<HttpResponseMessage> policy)
        {
            _policy = policy;
            Console.WriteLine("PolicyHandler is initialized.");
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"PolicyHandler is executing for request: {request.RequestUri}");
            return await _policy.ExecuteAsync(() => base.SendAsync(request, cancellationToken));
        }
    }
}
