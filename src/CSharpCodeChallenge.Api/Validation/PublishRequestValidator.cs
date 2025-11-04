using CSharpCodeChallenge.Api.Models;

namespace CSharpCodeChallenge.Api.Validation;

public sealed class PublishRequestValidator : IPublishRequestValidator
{
    private static readonly DateTimeOffset PublishDateThreshold = new(2024, 8, 24, 0, 0, 0, TimeSpan.Zero);

    public PublishValidationResult Validate(PublishRequest? request)
    {
        if (request is null)
        {
            return PublishValidationResult.Failure("Request body cannot be null.");
        }

        if (request.Status != 3)
        {
            return PublishValidationResult.Failure("Status must be equal to 3.");
        }

        if (request.PublishDate < PublishDateThreshold)
        {
            return PublishValidationResult.Failure("Publish date must be on or after 2024-08-24.");
        }

        if (!request.TestRun)
        {
            return PublishValidationResult.Failure("TestRun must be true in order to generate the XML file.");
        }

        return PublishValidationResult.Success();
    }
}
