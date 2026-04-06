namespace Dr.NutrizioNino.Api.Helpers;

public static class Constants
{
    public static decimal GetDefaultQuantity() => 100;
    public static Guid GetDefaultUnitOfMeasure() => Guid.Parse("DCA98E63-9327-4AD7-8E01-04DDB5DDCF0E");
    public static Guid GetDefaultBrandId() => Guid.Parse("00000000-0000-0000-0000-000000000000");
    public static int GetDefaultCalories() => 0;

}
