using System.Text;
using CSharpCodeChallenge.Api.Models;
using CSharpCodeChallenge.Api.Services;
using CSharpCodeChallenge.Api.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CSharpCodeChallenge.Api.Controllers;

[ApiController]
[Route("api/published-items")]
public sealed class PublishedItemController : ControllerBase
{
    private readonly IPublishRequestValidator _validator;
    private readonly IPublishedItemXmlGenerator _xmlGenerator;

    public PublishedItemController(
        IPublishRequestValidator validator,
        IPublishedItemXmlGenerator xmlGenerator)
    {
        _validator = validator;
        _xmlGenerator = xmlGenerator;
    }

    [HttpPost("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult Export([FromBody] PublishRequest request)
    {
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid request",
                Detail = validationResult.ErrorMessage
            });
        }

        var xml = _xmlGenerator.Generate(request);
        var fileName = $"PublishedItem_{request.Id}.xml";

        return File(Encoding.UTF8.GetBytes(xml), "application/xml", fileName);
    }
}
