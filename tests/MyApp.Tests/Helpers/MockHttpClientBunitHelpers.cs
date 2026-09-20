using Bunit;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

using static Microsoft.Extensions.Options.Options;

namespace MyApp.Tests.Helpers;

/// <summary>
/// Mocking HttpClient: https://bunit.dev/docs/test-doubles/mocking-httpclient.html
/// </summary>
public static class MockHttpClientBunitHelpers
{
    private static readonly JsonSerializerOptions defaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        RespectRequiredConstructorParameters = true,
    };

    public static MockHttpMessageHandler AddMockHttpClient(
        this BunitServiceProvider services, Uri baseAddress)
    {
        var mockHttpHandler = new MockHttpMessageHandler();

        services.AddHttpClient(DefaultName)
            .ConfigurePrimaryHttpMessageHandler(() => mockHttpHandler)
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = baseAddress;
            });

        return mockHttpHandler;
    }

    public static MockedRequest RespondJson<T>(
        this MockedRequest request,
        T content,
        Action<HttpRequestMessage>? callback = default,
        JsonSerializerOptions? options = default)
    {
        var jsonOptions = options ?? defaultOptions;

        request.Respond(req =>
        {
            callback?.Invoke(req); // 👈 ここで外側の変数にリクエストを渡す！

            var jsonContent = JsonSerializer.Serialize(content, jsonOptions);
            return CreateHttpResponse(jsonContent);
        });
        return request;
    }

    public static MockedRequest RespondJson<T>(
        this MockedRequest request,
        Func<T> contentProvider,
        Action<HttpRequestMessage>? callback = default,
        JsonSerializerOptions? options = default)
    {
        var jsonOptions = options ?? defaultOptions;

        request.Respond(req =>
        {
            callback?.Invoke(req);

            var jsonContent = JsonSerializer.Serialize(contentProvider(), jsonOptions);
            return CreateHttpResponse(jsonContent);
        });
        return request;
    }

    private static HttpResponseMessage CreateHttpResponse(string jsonContent)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            }
        };
    }
}
