namespace Dr.NutrizioNino.Api.Helpers;

/// <summary>
/// Fonte unica delle formule della scheda Sciaudone (TODO/20260816-02.md).
/// Calcolo puro e senza stato: nessun accesso a DB, nessuna dipendenza.
/// </summary>
public static class SciaudoneFormula
{
    /// <summary>Calorie giornaliere per ogni kg di peso attuale.</summary>
    public const decimal KcalPerKg = 22m;

    /// <summary>Grammi di proteine per ogni kg di peso ideale.</summary>
    public const decimal ProteinGramsPerIdealKg = 2m;

    /// <summary>Grammi di grassi per ogni kg di peso attuale.</summary>
    public const decimal FatGramsPerKg = 0.66m;

    /// <summary>Blocco calorico di riferimento per il calcolo delle fibre.</summary>
    public const decimal FiberKcalBlock = 1000m;

    /// <summary>Grammi di fibre per ogni blocco calorico di riferimento.</summary>
    public const decimal FiberGramsPerBlock = 15m;

    /// <summary>
    /// Quota calorica fissa sottratta prima di ricavare i carboidrati (75 × 13.2).
    /// Costante per scelta esplicita del committente: non viene riparametrata sul peso
    /// dell'utente, quindi il bilancio kcal = 4P + 9F + 4C chiude solo intorno ai 75 kg.
    /// </summary>
    public const decimal ProteinFatKcalAllowance = 990m;

    /// <summary>Calorie per grammo di carboidrati.</summary>
    public const decimal KcalPerCarbGram = 4m;

    /// <summary>Cifre decimali dei valori persistiti (colonne decimal(6,1)).</summary>
    private const int Decimals = 1;

    /// <summary>Calorie giornaliere per il dimagrimento: peso attuale × 22.</summary>
    public static decimal Kcal(decimal weightKg) =>
        Math.Round(weightKg * KcalPerKg, Decimals);

    /// <summary>Proteine giornaliere in grammi: peso ideale × 2.</summary>
    public static decimal ProteinGrams(decimal idealWeightKg) =>
        Math.Round(idealWeightKg * ProteinGramsPerIdealKg, Decimals);

    /// <summary>Grassi giornalieri in grammi: peso attuale × 0.66.</summary>
    public static decimal FatGrams(decimal weightKg) =>
        Math.Round(weightKg * FatGramsPerKg, Decimals);

    /// <summary>Fibre giornaliere in grammi: (calorie / 1000) × 15.</summary>
    public static decimal FiberGrams(decimal kcal) =>
        Math.Round(kcal / FiberKcalBlock * FiberGramsPerBlock, Decimals);

    /// <summary>Carboidrati giornalieri in grammi: (calorie − 990) / 4.</summary>
    public static decimal CarbGrams(decimal kcal) =>
        Math.Round((kcal - ProteinFatKcalAllowance) / KcalPerCarbGram, Decimals);
}
