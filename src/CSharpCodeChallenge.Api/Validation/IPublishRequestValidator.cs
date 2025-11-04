using CSharpCodeChallenge.Api.Models;

namespace CSharpCodeChallenge.Api.Validation;

public interface IPublishRequestValidator
{
    PublishValidationResult Validate(PublishRequest? request);
}
