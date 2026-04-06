using Dr.NutrizioNino.Api.Models;

namespace Dr.NutrizioNino.Api.Helpers;

public static class DailySimulationExtensions
{
    public static string ToDisplayName(this DailySimulationSectionType sectionType) => sectionType switch
    {
        DailySimulationSectionType.Colazione => "Colazione",
        DailySimulationSectionType.Pranzo    => "Pranzo",
        DailySimulationSectionType.Cena      => "Cena",
        DailySimulationSectionType.Spuntino  => "Spuntino",
        DailySimulationSectionType.Merenda   => "Merenda",
        DailySimulationSectionType.Altro     => "Altro",
        _                                    => "Sconosciuto"
    };
}
