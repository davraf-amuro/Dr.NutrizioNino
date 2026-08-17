using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto.Auth;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Services;

public class UserProfileService(DrNutrizioNinoContext context, SciaudoneCardService sciaudoneCards)
{
    public async Task<IList<ProfileEntryResponse>> GetHistoryAsync(Guid userId, CancellationToken ct = default)
    {
        return await context.UserProfileEntries
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.RecordedAt)
            .Select(e => new ProfileEntryResponse(e.Id, e.RecordedAt, e.WeightKg, e.IdealWeightKg, e.HeightCm, e.Sex, e.Job))
            .ToListAsync(ct);
    }

    public async Task<ProfileEntryResponse?> GetCurrentAsync(Guid userId, CancellationToken ct = default)
    {
        return await context.UserProfileEntries
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.RecordedAt)
            .Select(e => new ProfileEntryResponse(e.Id, e.RecordedAt, e.WeightKg, e.IdealWeightKg, e.HeightCm, e.Sex, e.Job))
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>Registra una misurazione e, se i dati bastano, la relativa scheda Sciaudone nella stessa transazione.</summary>
    public async Task<ProfileEntryResponse> AddEntryAsync(Guid userId, AddProfileEntryRequest request, CancellationToken ct = default)
    {
        var entry = new UserProfileEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RecordedAt = DateTime.UtcNow,
            WeightKg = request.WeightKg,
            IdealWeightKg = request.IdealWeightKg,
            HeightCm = request.HeightCm,
            Sex = request.Sex,
            Job = request.Job
        };

        context.UserProfileEntries.Add(entry);

        // Accodo anche la scheda calcolata: resta null se peso o peso ideale mancano
        sciaudoneCards.StageForEntry(entry);

        // Scrivo su DB misurazione e scheda in un'unica SaveChanges
        await context.SaveChangesAsync(ct);

        return new ProfileEntryResponse(entry.Id, entry.RecordedAt, entry.WeightKg, entry.IdealWeightKg, entry.HeightCm, entry.Sex, entry.Job);
    }
}
