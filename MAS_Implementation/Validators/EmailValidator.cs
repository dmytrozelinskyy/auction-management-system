using System.Text.RegularExpressions;

namespace MAS_Implementation;

public static class EmailValidator
{
    private static readonly Regex Pattern =
        new(@"[a-zA-Z0-9._%+-]+@[a-zA-z0-9.-]+\.[a-zA-z]{2,}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);
    
    public static bool IsValidEmail(string email) =>
        !string.IsNullOrWhiteSpace(email) && Pattern.IsMatch(email);
}