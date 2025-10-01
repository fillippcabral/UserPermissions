
using System.Text.RegularExpressions;

namespace UserPermissions.Application.Services;

public static class Validation
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public static void Require(bool cond, string message)
    {
        if (!cond) throw new ArgumentException(message);
    }

    public static void ValidateEmail(string email)
    {
        Require(!string.IsNullOrWhiteSpace(email), "Email is required.");
        Require(EmailRegex.IsMatch(email), "Invalid email format.");
    }

    public static void ValidateName(string name)
    {
        Require(!string.IsNullOrWhiteSpace(name), "Name is required.");
        Require(name.Trim().Length >= 2, "Name must have at least 2 characters.");
    }

    public static void ValidatePassword(string password)
    {
        Require(!string.IsNullOrWhiteSpace(password), "Password is required.");
        Require(password.Length >= 6, "Password must be at least 6 characters.");
    }
}
