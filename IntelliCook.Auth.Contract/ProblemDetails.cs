namespace IntelliCook.Auth.Contract;

public class ProblemDetails
{
    public string? Type { get; set; }

    public string? Title { get; set; }

    public int? Status { get; set; }

    public string? Detail { get; set; }

    public string? Instance { get; set; }
}

public class ValidationProblemDetails : ProblemDetails
{
    public IDictionary<string, ICollection<string>>? Errors { get; set; }
}