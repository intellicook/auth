using IntelliCook.Auth.Contract;
using IntelliCook.Auth.Contract.Auth.Login;
using IntelliCook.Auth.Contract.Auth.Register;
using IntelliCook.Auth.Contract.Health;
using IntelliCook.Auth.Contract.User;
using System.Net;

namespace IntelliCook.Auth.Client;

public interface IAuthClient
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

    #region Auth

    /// <summary>
    ///     Logs in a user.
    /// </summary>
    public Task<Result<LoginPostResponseModel>> PostAuthLoginAsync(LoginPostRequestModel request);

    /// <summary>
    ///     Registers a new user.
    /// </summary>
    public Task<Result> PostAuthRegisterAsync(RegisterPostRequestModel request);

    #endregion

    #region User

    /// <summary>
    ///     Gets the current user.
    /// </summary>
    public Task<Result<UserGetResponseModel>> GetUserMeAsync();

    /// <summary>
    ///     Updates the current user.
    /// </summary>
    public Task<Result<UserPutResponseModel>> PutUserMeAsync(UserPutRequestModel request);

    /// <summary>
    ///     Updates the current user's password.
    /// </summary>
    public Task<Result> PutUserMePasswordAsync(UserPasswordPutRequestModel request);

    /// <summary>
    ///     Deletes the current user.
    /// </summary>
    public Task<Result> DeleteUserMeAsync();

    #endregion

    #region Health

    /// <summary>
    /// Checks the health of Auth and its components.
    /// </summary>
    public Task<Result<HealthGetResponseModel, HealthGetResponseModel>> GetHealthAsync();

    #endregion
}