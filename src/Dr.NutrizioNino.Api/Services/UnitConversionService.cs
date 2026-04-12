namespace Dr.NutrizioNino.Api.Services;

public static class UnitConversionService
{
    public static decimal Convert(decimal value, string fromUnit, string toUnit) =>
        (fromUnit, toUnit) switch
        {
            ("mg", "µg") => value * 1000,
            ("µg", "mg") => value / 1000,
            ("g", "mg")  => value * 1000,
            ("mg", "g")  => value / 1000,
            _ when fromUnit == toUnit => value,
            _ => throw new NotSupportedException($"Conversione {fromUnit}→{toUnit} non supportata")
        };
}
