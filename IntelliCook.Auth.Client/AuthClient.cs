using IntelliCook.Auth.Contract;
using IntelliCook.Auth.Contract.Auth.Login;
using IntelliCook.Auth.Contract.Auth.Register;
using IntelliCook.Auth.Contract.Health;
using IntelliCook.Auth.Contract.User;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntelliCook.Auth.Client;

public class AuthClient<TAuthOptions> : IAuthClient, IDisposable where TAuthOptions : class, IAuthOptions
{
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

    [ActivatorUtilitiesConstructor]
    public AuthClient(IHttpContextAccessor httpContextAccessor, IOptions<TAuthOptions> options)
    {
        var auth = httpContextAccessor.HttpContext?.Request.Headers.Authorization;
        if (!StringValues.IsNullOrEmpty(auth.GetValueOrDefault("")))
        {
            RequestHeaders.Add("Authorization", auth.ToString());
        }

        Client.BaseAddress = options.Value.BaseUrl;
    }

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Auth

    public async Task<IAuthClient.Result<LoginPostResponseModel>> PostAuthLoginAsync(LoginPostRequestModel request)
    {
        var response = await Client.PostAsJsonAsync("/Auth/Login", request, SerializerOptions);
        return await CreateResultAsync<LoginPostResponseModel>(response);
    }

    public async Task<IAuthClient.Result> PostAuthRegisterAsync(RegisterPostRequestModel request)
    {
        var response = await Client.PostAsJsonAsync("/Auth/Register", request, SerializerOptions);
        return await CreateResultAsync(response);
    }

    #endregion

    #region User

    public async Task<IAuthClient.Result<UserGetResponseModel>> GetUserMeAsync()
    {
        var response = await Client.GetAsync("/User/Me");
        return await CreateResultAsync<UserGetResponseModel>(response);
    }

    public async Task<IAuthClient.Result<UserPutResponseModel>> PutUserMeAsync(UserPutRequestModel request)
    {
        var response = await Client.PutAsJsonAsync("/User/Me", request, SerializerOptions);
        return await CreateResultAsync<UserPutResponseModel>(response);
    }

    public async Task<IAuthClient.Result> PutUserMePasswordAsync(UserPasswordPutRequestModel request)
    {
        var response = await Client.PutAsJsonAsync("/User/Me/Password", request, SerializerOptions);
        return await CreateResultAsync(response);
    }

    public async Task<IAuthClient.Result> DeleteUserMeAsync()
    {
        var response = await Client.DeleteAsync("/User/Me");
        return await CreateResultAsync(response);
    }

    #endregion

    #region Health

    public async Task<IAuthClient.Result<HealthGetResponseModel, HealthGetResponseModel>> GetHealthAsync()
    {
        var response = await Client.GetAsync("/Health");
        return await CreateResultAsync<HealthGetResponseModel, HealthGetResponseModel>(response);
    }

    #endregion

    #region Helpers

    private async Task<IAuthClient.Result> CreateResultAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return IAuthClient.Result.FromValue(response.StatusCode);
        }

        var error = DeserializeProblemDetails(content);

        return IAuthClient.Result.FromError(response.StatusCode, error);
    }

    private async Task<IAuthClient.Result<T>> CreateResultAsync<T>(HttpResponseMessage response) where T : class
    {
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var value = JsonSerializer.Deserialize<T>(content, SerializerOptions);
            if (value == null)
            {
                throw new NullReferenceException($"Failed to deserialize {nameof(T)}, {nameof(value)} is null");
            }

            return IAuthClient.Result<T>.FromValue(response.StatusCode, value);
        }

        var error = DeserializeProblemDetails(content);

        return IAuthClient.Result<T>.FromError(response.StatusCode, error);
    }

    private async Task<IAuthClient.Result<TValue, TError>> CreateResultAsync<TValue, TError>(
        HttpResponseMessage response)
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

            return IAuthClient.Result<TValue, TError>.FromValue(response.StatusCode, value);
        }

        var error = JsonSerializer.Deserialize<TError>(content, SerializerOptions);
        if (error == null)
        {
            throw new NullReferenceException($"Failed to deserialize {nameof(TError)}, {nameof(error)} is null");
        }

        return IAuthClient.Result<TValue, TError>.FromError(response.StatusCode, error);
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