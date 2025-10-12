using System.Text;
using System.Text.Json;
using Domain.Common;

namespace Application.Http;

public interface IHttpService
{
    Task<T> GetAsync<T>(string url, Dictionary<string, string>? headers = null);
    Task<T> PostAsync<T>(string url, object data, Dictionary<string, string>? headers = null);
    Task<T> PutAsync<T>(string url, object data, Dictionary<string, string>? headers = null);
    Task<bool> DeleteAsync(string url, Dictionary<string, string>? headers = null);
}

public class HttpService : IHttpService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<T> GetAsync<T>(string url, Dictionary<string, string>? headers = null)
    {
        var request = CreateHttpRequest(HttpMethod.Get, url, headers);
        return await SendRequestAsync<T>(request);
    }

    public async Task<T> PostAsync<T>(string url, object data, Dictionary<string, string>? headers = null)
    {
        var request = CreateHttpRequest(HttpMethod.Post, url, headers);
        request.Content = CreateJsonContent(data);
        return await SendRequestAsync<T>(request);
    }

    public async Task<T> PutAsync<T>(string url, object data, Dictionary<string, string>? headers = null)
    {
        var request = CreateHttpRequest(HttpMethod.Put, url, headers);
        request.Content = CreateJsonContent(data);
        return await SendRequestAsync<T>(request);
    }

    public async Task<bool> DeleteAsync(string url, Dictionary<string, string>? headers = null)
    {
        var request = CreateHttpRequest(HttpMethod.Delete, url, headers);
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    private HttpRequestMessage CreateHttpRequest(HttpMethod method, string url, Dictionary<string, string>? headers)
    {
        var request = new HttpRequestMessage(method, url);
        
        request.Headers.Add("X-Trace-Id", TraceId.Current);
        
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        return request;
    }

    private StringContent CreateJsonContent(object data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private async Task<T> SendRequestAsync<T>(HttpRequestMessage request)
    {
        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"HTTP request failed with status code {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();
        
        if (string.IsNullOrEmpty(content))
        {
            return default(T)!;
        }

        return JsonSerializer.Deserialize<T>(content, _jsonOptions) ?? 
               throw new InvalidOperationException("Failed to deserialize response");
    }
}