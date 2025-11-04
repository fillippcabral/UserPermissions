namespace CSharpCodeChallenge.Api.Validation;

public sealed record class PublishValidationResult(bool IsValid, string? ErrorMessage)
{
    public static PublishValidationResult Success() => new(true, null);

    public static PublishValidationResult Failure(string message) => new(false, message);
}
