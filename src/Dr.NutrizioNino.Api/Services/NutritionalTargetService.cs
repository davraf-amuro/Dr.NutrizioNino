using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Services;

/// <summary>Application service for the user's declared nutritional target (current value, no history).</summary>
public class NutritionalTargetService(DrNutrizioNinoContext context)
{
    /// <summary>Returns the user's current target, or null if never set.</summary>
    public async Task<NutritionalTargetDto?> GetAsync(Guid userId, CancellationToken ct = default)
    {
        // Leggo da DB l'eventuale riga unica del fabbisogno utente
        return await context.NutritionalTargets!
            .Where(t => t.UserId == userId)
            .Select(t => new NutritionalTargetDto(t.KcalTarget, t.CarbsTarget, t.ProteinTarget, t.FatTarget, t.UpdatedAt))
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>Creates or updates the user's unique target row (upsert).</summary>
    public async Task<NutritionalTargetDto> UpsertAsync(Guid userId, SetNutritionalTargetDto request, CancellationToken ct = default)
    {
        // Cerco la riga esistente per aggiornarla, altrimenti ne creo una nuova
        var existing = await context.NutritionalTargets!
            .FirstOrDefaultAsync(t => t.UserId == userId, ct);

        if (existing is null)
        {
            existing = new NutritionalTarget { Id = Guid.NewGuid(), UserId = userId };
            context.NutritionalTargets!.Add(existing);
        }

        existing.KcalTarget = request.KcalTarget;
        existing.CarbsTarget = request.CarbsTarget;
        existing.ProteinTarget = request.ProteinTarget;
        existing.FatTarget = request.FatTarget;
        existing.UpdatedAt = DateTime.UtcNow;

        // Scrivo su DB la riga creata/aggiornata
        await context.SaveChangesAsync(ct);

        return new NutritionalTargetDto(existing.KcalTarget, existing.CarbsTarget, existing.ProteinTarget, existing.FatTarget, existing.UpdatedAt);
    }
}
