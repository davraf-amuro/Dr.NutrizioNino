using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto.Auth;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Services;

/// <summary>Application service della scheda Sciaudone: la genera dalle misurazioni e ne espone corrente e storico.</summary>
public class SciaudoneCardService(DrNutrizioNinoContext context, ILogger<SciaudoneCardService> logger)
{
    /// <summary>Ultima scheda calcolata dell'utente, null se non ne esiste nessuna.</summary>
    public async Task<SciaudoneCardDto?> GetCurrentAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Leggo da DB la scheda più recente dell'utente
        return await context.SciaudoneCards!
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.ComputedAt)
            .Select(c => new SciaudoneCardDto(c.Id, c.ProfileEntryId, c.ComputedAt, c.WeightKg, c.IdealWeightKg, c.Kcal, c.ProteinG, c.FatG, c.FiberG, c.CarbsG))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>Storico completo delle schede dell'utente, dalla più recente.</summary>
    public async Task<IList<SciaudoneCardDto>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Leggo da DB tutte le schede dell'utente in ordine cronologico inverso
        return await context.SciaudoneCards!
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.ComputedAt)
            .Select(c => new SciaudoneCardDto(c.Id, c.ProfileEntryId, c.ComputedAt, c.WeightKg, c.IdealWeightKg, c.Kcal, c.ProteinG, c.FatG, c.FiberG, c.CarbsG))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Calcola la scheda per la misurazione e la accoda al context, senza salvare:
    /// il <c>SaveChanges</c> resta al chiamante, così misurazione e scheda sono atomiche.
    /// Ritorna null se la misurazione non ha peso attuale o peso ideale.
    /// </summary>
    public SciaudoneCard? StageForEntry(UserProfileEntry entry)
    {
        if (entry.WeightKg is not { } weightKg || entry.IdealWeightKg is not { } idealWeightKg)
        {
            logger.LogInformation("Scheda Sciaudone non generata per la misurazione {ProfileEntryId}: peso o peso ideale mancante", entry.Id);
            return null;
        }

        var kcal = SciaudoneFormula.Kcal(weightKg);

        var card = new SciaudoneCard
        {
            Id = Guid.NewGuid(),
            UserId = entry.UserId,
            ProfileEntryId = entry.Id,
            WeightKg = weightKg,
            IdealWeightKg = idealWeightKg,
            Kcal = kcal,
            ProteinG = SciaudoneFormula.ProteinGrams(idealWeightKg),
            FatG = SciaudoneFormula.FatGrams(weightKg),
            FiberG = SciaudoneFormula.FiberGrams(kcal),
            CarbsG = SciaudoneFormula.CarbGrams(kcal),
            ComputedAt = DateTime.UtcNow
        };

        // Accodo la scheda al context: viene scritta a DB dal SaveChanges del chiamante
        context.SciaudoneCards!.Add(card);

        return card;
    }

    /// <summary>Converte una scheda già calcolata nel DTO di risposta.</summary>
    public static SciaudoneCardDto ToDto(SciaudoneCard card) =>
        new(card.Id, card.ProfileEntryId, card.ComputedAt, card.WeightKg, card.IdealWeightKg, card.Kcal, card.ProteinG, card.FatG, card.FiberG, card.CarbsG);
}
