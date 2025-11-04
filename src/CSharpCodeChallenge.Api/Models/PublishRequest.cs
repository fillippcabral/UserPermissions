using System.Text.Json.Serialization;

namespace CSharpCodeChallenge.Api.Models;

public sealed record class PublishRequest
{
    [JsonPropertyName("Id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("ReportMetadata")]
    public ReportMetadata? ReportMetadata { get; init; } = null;

    [JsonPropertyName("CountryIds")]
    public IReadOnlyList<string> CountryIds { get; init; } = Array.Empty<string>();

    [JsonPropertyName("Title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("PublishDate")]
    public DateTimeOffset PublishDate { get; init; } = DateTimeOffset.MinValue;

    [JsonPropertyName("Status")]
    public int Status { get; init; } = 0;

    [JsonPropertyName("TestRun")]
    public bool TestRun { get; init; } = false;
}

public sealed record class ReportMetadata
{
    [JsonPropertyName("Title")]
    public string? Title { get; init; } = null;

    [JsonPropertyName("ContactSection")]
    public IReadOnlyList<ContactSection> ContactSection { get; init; } = Array.Empty<ContactSection>();
}

public sealed record class ContactSection
{
    [JsonPropertyName("ContactInformation")]
    public IReadOnlyList<ContactInformation> ContactInformation { get; init; } = Array.Empty<ContactInformation>();
}

public sealed record class ContactInformation
{
    [JsonPropertyName("ContactHeader")]
    public string? ContactHeader { get; init; } = null;

    [JsonPropertyName("Contacts")]
    public IReadOnlyList<Contact> Contacts { get; init; } = Array.Empty<Contact>();
}

public sealed record class Contact
{
    [JsonPropertyName("FirstName")]
    public string? FirstName { get; init; } = null;

    [JsonPropertyName("LastName")]
    public string? LastName { get; init; } = null;

    [JsonPropertyName("Email")]
    public string? Email { get; init; } = null;

    [JsonPropertyName("Title")]
    public string? Title { get; init; } = null;

    [JsonPropertyName("PhoneNumber")]
    public string? PhoneNumber { get; init; } = null;

    [JsonPropertyName("Accreditation")]
    public string? Accreditation { get; init; } = null;
}
