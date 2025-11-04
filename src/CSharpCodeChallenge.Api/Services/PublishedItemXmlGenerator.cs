using System.Xml.Linq;
using CSharpCodeChallenge.Api.Models;

namespace CSharpCodeChallenge.Api.Services;

public sealed class PublishedItemXmlGenerator : IPublishedItemXmlGenerator
{
    public string Generate(PublishRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("PublishedItem",
                new XElement("Title", request.Title),
                BuildCountriesElement(request.CountryIds),
                new XElement("PublishedDate", request.PublishDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffff'Z'")),
                BuildContactInformationElement(request.ReportMetadata)
            )
        );

        return document.ToString();
    }

    private static XElement BuildCountriesElement(IReadOnlyList<string> countryIds)
    {
        var countryElements = countryIds?.Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => new XElement("Country", id))
            .ToArray() ?? Array.Empty<XElement>();

        return new XElement("Countries", countryElements);
    }

    private static XElement BuildContactInformationElement(ReportMetadata? reportMetadata)
    {
        var personGroups = new List<XElement>();
        if (reportMetadata?.ContactSection is not null)
        {
            var sequence = 1;
            foreach (var section in reportMetadata.ContactSection)
            {
                if (section.ContactInformation is null)
                {
                    continue;
                }

                foreach (var contactInfo in section.ContactInformation)
                {
                    var members = BuildPersonGroupMembers(contactInfo).ToArray();
                    if (members.Length == 0)
                    {
                        continue;
                    }

                    var personGroup = new XElement(
                        "PersonGroup",
                        new XAttribute("sequence", sequence++),
                        new XElement("Name", GetContactHeader(contactInfo)),
                        members
                    );

                    personGroups.Add(personGroup);
                }
            }
        }

        return new XElement("ContactInformation", personGroups);
    }

    private static IEnumerable<XElement> BuildPersonGroupMembers(ContactInformation contactInformation)
    {
        if (contactInformation.Contacts is null)
        {
            yield break;
        }

        foreach (var contact in contactInformation.Contacts)
        {
            if (IsContactEmpty(contact))
            {
                continue;
            }

            var person = new XElement(
                "Person",
                new XElement("FamilyName", contact.LastName ?? string.Empty),
                new XElement("GivenName", contact.FirstName ?? string.Empty),
                new XElement("DisplayName", BuildDisplayName(contact)),
                new XElement("JobTitle", contact.Title ?? string.Empty)
            );

            var contactInfoElement = BuildContactInfoElement(contact);
            if (contactInfoElement is not null)
            {
                person.Add(contactInfoElement);
            }

            var accreditationElement = BuildAccreditationElement(contact);
            if (accreditationElement is not null)
            {
                person.Add(accreditationElement);
            }

            yield return new XElement("PersonGroupMember", person);
        }
    }

    private static bool IsContactEmpty(Contact contact) =>
        string.IsNullOrWhiteSpace(contact.FirstName)
        && string.IsNullOrWhiteSpace(contact.LastName)
        && string.IsNullOrWhiteSpace(contact.Email)
        && string.IsNullOrWhiteSpace(contact.Title)
        && string.IsNullOrWhiteSpace(contact.PhoneNumber)
        && string.IsNullOrWhiteSpace(contact.Accreditation);

    private static string GetContactHeader(ContactInformation contactInformation)
    {
        if (!string.IsNullOrWhiteSpace(contactInformation.ContactHeader))
        {
            return contactInformation.ContactHeader;
        }

        return "Contacts";
    }

    private static string BuildDisplayName(Contact contact)
    {
        var nameParts = new[] { contact.FirstName, contact.LastName }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToArray();

        if (nameParts.Length > 0)
        {
            return string.Join(" ", nameParts);
        }

        if (!string.IsNullOrWhiteSpace(contact.Email))
        {
            return contact.Email!;
        }

        return contact.Title ?? string.Empty;
    }

    private static XElement? BuildContactInfoElement(Contact contact)
    {
        var infoChildren = new List<XElement>();

        var normalizedPhone = NormalizePhoneNumber(contact.PhoneNumber);
        if (!string.IsNullOrWhiteSpace(normalizedPhone))
        {
            infoChildren.Add(new XElement("Phone",
                new XElement("Number", normalizedPhone)));
        }

        if (!string.IsNullOrWhiteSpace(contact.Email))
        {
            infoChildren.Add(new XElement("Email", contact.Email));
        }

        if (infoChildren.Count == 0)
        {
            return null;
        }

        return new XElement("ContactInfo", infoChildren);
    }

    private static XElement? BuildAccreditationElement(Contact contact)
    {
        return string.IsNullOrWhiteSpace(contact.Accreditation)
            ? null
            : new XElement("Accreditation", contact.Accreditation);
    }

    private static string? NormalizePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return null;
        }

        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        if (digits.Length == 11)
        {
            return string.Create(14, digits, static (span, value) =>
            {
                span[0] = value[0];
                span[1] = '-';
                span[2] = value[1];
                span[3] = value[2];
                span[4] = value[3];
                span[5] = '-';
                span[6] = value[4];
                span[7] = value[5];
                span[8] = value[6];
                span[9] = '-';
                span[10] = value[7];
                span[11] = value[8];
                span[12] = value[9];
                span[13] = value[10];
            });
        }

        if (digits.Length == 10)
        {
            return string.Create(12, digits, static (span, value) =>
            {
                span[0] = value[0];
                span[1] = value[1];
                span[2] = value[2];
                span[3] = '-';
                span[4] = value[3];
                span[5] = value[4];
                span[6] = value[5];
                span[7] = '-';
                span[8] = value[6];
                span[9] = value[7];
                span[10] = value[8];
                span[11] = value[9];
            });
        }

        return digits;
    }
}
