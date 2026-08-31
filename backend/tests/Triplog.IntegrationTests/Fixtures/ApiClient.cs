using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Triplog.IntegrationTests.Fixtures;

public class ApiClient(HttpClient httpClient, string jwt)
{
    // Match the API's System.Text.Json config — camelCase properties, enum-as-string
    private static readonly JsonSerializerOptions jsonSerializerOptions =
        new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() },
        };

    public async Task<HttpResponseMessage> PostRawAsync(string path, object? body = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        if (body is not null)
            request.Content = JsonContent.Create(body, options: jsonSerializerOptions);

        return await httpClient.SendAsync(request);
    }

    public async Task<T> PostAsync<T>(string path, object body)
    {
        var response = await PostRawAsync(path, body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(jsonSerializerOptions))!;
    }

    public async Task PostVoidAsync(string path, object? body = null)
    {
        var response = await PostRawAsync(path, body);
        response.EnsureSuccessStatusCode();
    }

    public async Task<T> GetAsync<T>(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(jsonSerializerOptions))!;
    }

    public async Task PutVoidAsync(string path, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        request.Content = JsonContent.Create(body, options: jsonSerializerOptions);

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
