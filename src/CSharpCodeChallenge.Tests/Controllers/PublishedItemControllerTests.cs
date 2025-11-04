using System.Text;
using CSharpCodeChallenge.Api.Controllers;
using CSharpCodeChallenge.Api.Models;
using CSharpCodeChallenge.Api.Services;
using CSharpCodeChallenge.Api.Validation;
using Microsoft.AspNetCore.Mvc;

namespace CSharpCodeChallenge.Tests.Controllers;

public class PublishedItemControllerTests
{
    private static readonly PublishRequest ValidRequest = new()
    {
        Id = "Sample123",
        Title = "Document for Publication > Best Report Yet!!",
        CountryIds = new[] { "US" },
        PublishDate = new DateTimeOffset(2024, 8, 26, 18, 19, 59, TimeSpan.Zero),
        Status = 3,
        TestRun = true,
        ReportMetadata = new ReportMetadata
        {
            Title = "Document for Publication > Best Report Yet!!",
            ContactSection = new[]
            {
                new ContactSection
                {
                    ContactInformation = new[]
                    {
                        new ContactInformation
                        {
                            ContactHeader = "Media Contact",
                            Contacts = new[]
                            {
                                new Contact
                                {
                                    FirstName = "Mike",
                                    LastName = "Johnsen",
                                    Email = "mike.johnsen@kbra.com",
                                    Title = "Director of Communications & Marketing",
                                    PhoneNumber = "+1 646-731-1347",
                                    Accreditation = "CM&AA"
                                }
                            }
                        }
                    }
                }
            }
        }
    };

    [Fact]
    public void Export_WithInvalidStatus_ReturnsBadRequest()
    {
        var request = ValidRequest with { Status = 2 };
        var controller = CreateController();

        var actionResult = controller.Export(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Status must be equal to 3.", problem.Detail);
    }

    [Fact]
    public void Export_WithValidRequest_ReturnsXmlFile()
    {
        var controller = CreateController();

        var actionResult = controller.Export(ValidRequest);

        var fileResult = Assert.IsType<FileContentResult>(actionResult);
        Assert.Equal("application/xml", fileResult.ContentType);
        Assert.Equal("PublishedItem_Sample123.xml", fileResult.FileDownloadName);

        var xml = Encoding.UTF8.GetString(fileResult.FileContents);
        Assert.Contains("<PublishedItem>", xml);
        Assert.Contains("<Title>Document for Publication &gt; Best Report Yet!!</Title>", xml);
        Assert.Contains("<Name>Media Contact</Name>", xml);
    }

    private static PublishedItemController CreateController() =>
        new(new PublishRequestValidator(), new PublishedItemXmlGenerator());
}
