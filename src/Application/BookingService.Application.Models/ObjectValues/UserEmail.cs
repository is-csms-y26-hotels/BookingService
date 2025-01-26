using System.Text.RegularExpressions;

namespace BookingService.Application.Models.ObjectValues;

public readonly partial record struct UserEmail
{
    public string Email { get; private init; }

    public UserEmail(string value)
    {
        Match match = EmailRegex().Match(value);

        if (!match.Success) throw new ArgumentException($"Invalid email format: {value}");

        Email = match.Value;
    }

    [GeneratedRegex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$")]
    private static partial Regex EmailRegex();
}