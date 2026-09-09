namespace MAS_Implementation;

public static class AmsConfig
{
    public static List<string> Categories { get; } = new()
    {
        "Fine Art", "Jewelry", "Wine", "Furniture", "Firearms",
        "Ivory", "Alcohol", "Antiques", "Watches", "Sculptures"
    };

    public static List<string> RegulatedCategories { get; } = new()
    {
        "Firearms", "Ivory", "Alcohol"
    };

    public static List<string> SanctionedCountries { get; } = new()
    {
        "Iran", "North Korea", "Syria", "Cuba"
    };

    public static double HighValueThreshold { get; } = 10000.0;
}