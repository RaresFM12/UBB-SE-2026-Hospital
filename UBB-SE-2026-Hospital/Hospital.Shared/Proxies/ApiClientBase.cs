using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Hospital.Shared.Proxies;

public abstract class ApiClientBase
{
    protected readonly HttpClient HttpClient;
    protected readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    protected static string QueryDate(DateTime value) => Uri.EscapeDataString(value.ToString("O", System.Globalization.CultureInfo.InvariantCulture));


    protected ApiClientBase(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    protected Task<T?> GetAsync<T>(string uri, CancellationToken cancellationToken = default) =>
        SendAsync<T>(HttpMethod.Get, uri, null, cancellationToken);

    protected Task<TResponse?> PostAsync<TRequest, TResponse>(
        string uri,
        TRequest payload,
        CancellationToken cancellationToken = default) =>
        SendAsync<TResponse>(HttpMethod.Post, uri, payload, cancellationToken);

    protected async Task PostAsync<TRequest>(
        string uri,
        TRequest payload,
        CancellationToken cancellationToken = default)
    {
        _ = await SendAsync<object>(HttpMethod.Post, uri, payload, cancellationToken);
    }

    protected async Task PutAsync<TRequest>(
        string uri,
        TRequest payload,
        CancellationToken cancellationToken = default)
    {
        _ = await SendAsync<object>(HttpMethod.Put, uri, payload, cancellationToken);
    }

    protected async Task DeleteAsync(string uri, CancellationToken cancellationToken = default)
    {
        _ = await SendAsync<object>(HttpMethod.Delete, uri, null, cancellationToken);
    }

    private async Task<T?> SendAsync<T>(
        HttpMethod method,
        string uri,
        object? payload,
        CancellationToken cancellationToken)
    {
        var requestUri = new Uri(uri, UriKind.RelativeOrAbsolute);
        using var request = new HttpRequestMessage(method, requestUri);

        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload, options: JsonOptions);
        }

        using HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessWithMessageAsync(response, cancellationToken);

        if (response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(responseBody, JsonOptions);
    }

    private static async Task EnsureSuccessWithMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string errorMessage = await ReadErrorMessageAsync(response, cancellationToken);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException("Please sign in again to continue.");
        }

        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Conflict)
        {
            throw new ArgumentException(errorMessage);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException(errorMessage);
        }

        throw new InvalidOperationException(
            response.StatusCode is HttpStatusCode.InternalServerError or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable
                ? "The server could not complete the request. Please try again."
                : errorMessage);
    }

    private static async Task<string> ReadErrorMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return $"Request failed with status code {(int)response.StatusCode} ({response.StatusCode}).";
        }

        try
        {
            if (responseBody.TrimStart().StartsWith('"'))
            {
                return JsonSerializer.Deserialize<string>(responseBody) ?? "Request failed.";
            }

            using JsonDocument document = JsonDocument.Parse(responseBody);
            JsonElement root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                return string.Join(Environment.NewLine, root.EnumerateArray().Select(item => item.ToString()));
            }

            if (root.TryGetProperty("detail", out JsonElement detail))
            {
                return detail.GetString() ?? "Request failed.";
            }

            if (root.TryGetProperty("errors", out JsonElement errors))
            {
                return FlattenValidationErrors(errors);
            }

            if (root.TryGetProperty("title", out JsonElement title))
            {
                return title.GetString() ?? "Request failed.";
            }
        }
        catch (JsonException)
        {
            return responseBody;
        }

        return responseBody;
    }

    private static string FlattenValidationErrors(JsonElement errors)
    {
        if (errors.ValueKind != JsonValueKind.Object)
        {
            return errors.ToString();
        }

        List<string> messages = new();
        foreach (JsonProperty property in errors.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (JsonElement item in property.Value.EnumerateArray())
            {
                string? message = item.GetString();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    messages.Add(message);
                }
            }
        }

        return messages.Count > 0 ? string.Join(Environment.NewLine, messages) : "Request failed.";
    }
}

