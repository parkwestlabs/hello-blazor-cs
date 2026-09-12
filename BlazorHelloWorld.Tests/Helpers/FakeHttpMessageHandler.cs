namespace BlazorHelloWorld.Tests.Helpers;

public class FakeHttpMessageHandler(HttpResponseMessage responseMessage) : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request; // 検証用にリクエストを保持
        return Task.FromResult(responseMessage);
    }
}
