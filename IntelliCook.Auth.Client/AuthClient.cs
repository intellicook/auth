using IntelliCook.Auth.Contract;
using IntelliCook.Auth.Contract.Auth.Login;
using IntelliCook.Auth.Contract.Auth.Register;
using IntelliCook.Auth.Contract.Health;
using IntelliCook.Auth.Contract.User;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntelliCook.Auth.Client;

public class AuthClient : IDisposable
{
    public class Result<TValue, TError>
        where TValue : class
        where TError : class
    {
        public TValue? TryValue { get; init; }

        public TValue Value => TryValue ?? throw new NullReferenceException($"{nameof(Value)} is null");

        public TError? TryError { get; init; }

        public TError Error => TryError ?? throw new NullReferenceException($"{nameof(Error)} is null");

        public HttpStatusCode StatusCode { get; init; }

        public bool IsSuccessful => (int)StatusCode >= 200 && (int)StatusCode < 300;

        public bool HasError => TryError != null;

        public ValidationProblemDetails? TryValidationError => TryError as ValidationProblemDetails;

        public ValidationProblemDetails ValidationError => Error as ValidationProblemDetails ??
                                                           throw new NullReferenceException(
                                                               $"{nameof(ValidationError)} is null");

        public static Result<TValue, TError> FromValue(HttpStatusCode statusCode, TValue value)
        {
            return new Result<TValue, TError>
            {
                TryValue = value,
                StatusCode = statusCode
            };
        }

        public static Result<TValue, TError> FromError(HttpStatusCode statusCode, TError? error = null)
        {
            return new Result<TValue, TError>
            {
                TryError = error,
                StatusCode = statusCode
            };
        }
    }

    public class Result<T> : Result<T, ProblemDetails> where T : class
    {
        public bool HasValidationError => TryError is ValidationProblemDetails;

        public static new Result<T> FromValue(HttpStatusCode statusCode, T value)
        {
            return new Result<T>
            {
                TryValue = value,
                StatusCode = statusCode
            };
        }

        public static new Result<T> FromError(HttpStatusCode statusCode, ProblemDetails? error = null)
        {
            return new Result<T>
            {
                TryError = error,
                StatusCode = statusCode
            };
        }
    }

    public class Result : Result<object>
    {
        public static Result FromValue(HttpStatusCode statusCode)
        {
            return new Result
            {
                StatusCode = statusCode
            };
        }

        public static new Result FromError(HttpStatusCode statusCode, ProblemDetails? error = null)
        {
            return new Result
            {
                TryError = error,
                StatusCode = statusCode
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

    public HttpRequestHeaders RequestHeaders => Client.DefaultRequestHeaders;

    public AuthClient()
    {
    }

    public AuthClient(Uri baseUrl)
    {
        Client.BaseAddress = baseUrl;
    }

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Auth

    /// <summary>
    ///     Logs in a user.
    /// </summary>
    public async Task<Result<LoginPostResponseModel>> PostAuthLogin(LoginPostRequestModel request)
    {
        var response = await Client.PostAsJsonAsync("/Auth/Login", request, SerializerOptions);
        return await CreateResult<LoginPostResponseModel>(response);
    }

    /// <summary>
    ///     Registers a new user.
    /// </summary>
    public async Task<Result> PostAuthRegister(RegisterPostRequestModel request)
    {
        var response = await Client.PostAsJsonAsync("/Auth/Register", request, SerializerOptions);
        return await CreateResult(response);
    }

    #endregion

    #region User

    /// <summary>
    ///     Gets the current user.
    /// </summary>
    public async Task<Result<UserGetResponseModel>> GetUserMe()
    {
        var response = await Client.GetAsync("/User/Me");
        return await CreateResult<UserGetResponseModel>(response);
    }

    /// <summary>
    ///     Deletes the current user.
    /// </summary>
    public async Task<Result> DeleteUserMe()
    {
        var response = await Client.DeleteAsync("/User/Me");
        return await CreateResult(response);
    }

    #endregion

    #region Health

    /// <summary>
    /// Checks the health of Auth and its components.
    /// </summary>
    public async Task<Result<HealthGetResponseModel, HealthGetResponseModel>> GetHealth()
    {
        var response = await Client.GetAsync("/Health");
        return await CreateResult<HealthGetResponseModel, HealthGetResponseModel>(response);
    }

    #endregion

    #region Helpers

    private async Task<Result> CreateResult(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return Result.FromValue(response.StatusCode);
        }

        var error = DeserializeProblemDetails(content);

        return Result.FromError(response.StatusCode, error);
    }

    private async Task<Result<T>> CreateResult<T>(HttpResponseMessage response) where T : class
    {
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var value = JsonSerializer.Deserialize<T>(content, SerializerOptions);
            if (value == null)
            {
                throw new NullReferenceException($"Failed to deserialize {nameof(T)}, {nameof(value)} is null");
            }

            return Result<T>.FromValue(response.StatusCode, value);
        }

        var error = DeserializeProblemDetails(content);

        return Result<T>.FromError(response.StatusCode, error);
    }

    private async Task<Result<TValue, TError>> CreateResult<TValue, TError>(HttpResponseMessage response)
        where TValue : class
        where TError : class
    {
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var value = JsonSerializer.Deserialize<TValue>(content, SerializerOptions);
            if (value == null)
            {
                throw new NullReferenceException($"Failed to deserialize {nameof(TValue)}, {nameof(value)} is null");
            }

            return Result<TValue, TError>.FromValue(response.StatusCode, value);
        }

        var error = JsonSerializer.Deserialize<TError>(content, SerializerOptions);
        if (error == null)
        {
            throw new NullReferenceException($"Failed to deserialize {nameof(TError)}, {nameof(error)} is null");
        }

        return Result<TValue, TError>.FromError(response.StatusCode, error);
    }

    private ProblemDetails? DeserializeProblemDetails(string content)
    {
        try
        {
            return JsonSerializer.Deserialize<ProblemDetails>(content, SerializerOptions);
        }
        catch (Exception)
        {
            try
            {
                return JsonSerializer.Deserialize<ValidationProblemDetails>(content, SerializerOptions);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    #endregion
}