using Moq;
public class MockHttpMessageHandler : HttpMessageHandler
{
    public Mock<Func<HttpRequestMessage, Task<HttpResponseMessage>>> SendAsyncMock { get; } = new Mock<Func<HttpRequestMessage, Task<HttpResponseMessage>>>();

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return SendAsyncMock.Object.Invoke(request);
    }
}
