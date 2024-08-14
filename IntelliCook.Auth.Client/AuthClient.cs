using IntelliCook.Auth.Contract;
using IntelliCook.Auth.Contract.Auth.Login;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntelliCook.Auth.Client;

public class AuthClient
{
    public class Result<T>
    {
        public T? Value { get; init; }

        public ProblemDetails? Error { get; init; }

        public bool IsSuccessful => Value is not null;

        public bool HasError => Error != null;

        public bool HasValidationError => Error is ValidationProblemDetails;

        public static Result<T> FromValue(T value)
        {
            return new Result<T>
            {
                Value = value
            };
        }

        public static Result<T> FromError(ProblemDetails? error = null)
        {
            return new Result<T>
            {
                Error = error
            };
        }
    }

    public HttpClient Client { get; set; } = new();

    public JsonSerializerOptions SerializerOptions { get; set; } = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public AuthClient()
    { }

    public AuthClient(Uri baseUrl)
    {
        Client.BaseAddress = baseUrl;
    }

    public AuthClient(Uri baseUrl, JsonSerializerOptions serializerOptions) : this(baseUrl)
    {
        SerializerOptions = serializerOptions;
    }

    #region Auth

    public async Task<Result<LoginPostResponseModel>> PostAuthLogin(LoginPostRequestModel request)
    {
        const string path = "/Auth/Login";
        var response = await Client.PostAsJsonAsync(path, request, SerializerOptions);
        return await CreateResult<LoginPostResponseModel>(response);
    }

    #endregion

    private async Task<Result<T>> CreateResult<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            ProblemDetails? error;

            try
            {
                error = JsonSerializer.Deserialize<ProblemDetails>(content, SerializerOptions);
            }
            catch (Exception)
            {
                error = JsonSerializer.Deserialize<ValidationProblemDetails>(content, SerializerOptions);
            }

            if (error == null)
            {
                throw new NullReferenceException($"Failed to deserialize {nameof(ProblemDetails)}, {nameof(error)} is null");
            }

            return Result<T>.FromError(error);
        }

        var value = JsonSerializer.Deserialize<T>(content, SerializerOptions);
        if (value == null)
        {
            throw new NullReferenceException($"Failed to deserialize {nameof(T)}, {nameof(value)} is null");
        }

        return Result<T>.FromValue(value);
    }
}