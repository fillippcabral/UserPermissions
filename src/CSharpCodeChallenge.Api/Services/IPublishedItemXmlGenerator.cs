using CSharpCodeChallenge.Api.Models;

namespace CSharpCodeChallenge.Api.Services;

public interface IPublishedItemXmlGenerator
{
    string Generate(PublishRequest request);
}
