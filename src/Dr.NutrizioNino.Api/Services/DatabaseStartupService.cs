using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Services;

/// <summary>
/// Singleton che incapsula lo stato di disponibilità del database e la sua inizializzazione
/// (verifica connessione + seed ruoli). Invocato all'avvio in modo resiliente (mai rilancia) e
/// on-demand dagli endpoint di stato/retry quando il DB torna raggiungibile.
/// </summary>
public class DatabaseStartupService(
    IServiceScopeFactory scopeFactory,
    IDbContextFactory<DrNutrizioNinoContext> contextFactory,
    ILogger<DatabaseStartupService> logger)
{
    /// <summary>Vero se l'ultimo controllo/inizializzazione ha trovato il database pronto.</summary>
    public bool IsDatabaseReady { get; private set; }

    /// <summary>Messaggio dell'ultimo errore di connessione/inizializzazione, null se nessuno.</summary>
    public string? LastError { get; private set; }

    /// <summary>Istante UTC dell'ultimo controllo eseguito.</summary>
    public DateTimeOffset? LastCheckedUtc { get; private set; }

    /// <summary>
    /// Verifica la connessione al database e, se raggiungibile, esegue il seed dei ruoli.
    /// Aggiorna lo stato interno e non rilancia mai: ritorna false su qualsiasi errore così
    /// che l'avvio dell'API non venga interrotto.
    /// </summary>
    public async Task<bool> TryInitializeAsync(CancellationToken ct)
    {
        LastCheckedUtc = DateTimeOffset.UtcNow;
        try
        {
            // verifica raggiungibilità del database prima di qualsiasi operazione
            await using var db = await contextFactory.CreateDbContextAsync(ct);
            if (!await db.Database.CanConnectAsync(ct))
            {
                IsDatabaseReady = false;
                LastError = "Il database non è raggiungibile.";
                logger.LogWarning("Database non raggiungibile durante l'inizializzazione");
                return false;
            }

            // seed ruoli: AdminUserService è scoped, serve uno scope dedicato
            using var scope = scopeFactory.CreateScope();
            var adminService = scope.ServiceProvider.GetRequiredService<AdminUserService>();
            await adminService.EnsureRolesExistAsync();

            IsDatabaseReady = true;
            LastError = null;
            logger.LogInformation("Inizializzazione database completata: connessione ok e ruoli garantiti");
            return true;
        }
        catch (Exception ex)
        {
            IsDatabaseReady = false;
            LastError = ex.Message;
            logger.LogError(ex, "Errore durante l'inizializzazione del database");
            return false;
        }
    }

    /// <summary>
    /// Verifica la reale leggibilità del database (senza seed) e aggiorna lo stato interno.
    /// Usato dall'endpoint di stato per fornire un valore live al frontend.
    /// Oltre a CanConnect esegue una lettura reale su una tabella: un DB che accetta
    /// connessioni ma con file dati illeggibile (es. errore 823) deve risultare NON pronto.
    /// </summary>
    public async Task<bool> CheckConnectionAsync(CancellationToken ct)
    {
        LastCheckedUtc = DateTimeOffset.UtcNow;
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(ct);
            // probe leggero: connessione + lettura reale di una pagina dati (sorvola CanConnect-only)
            if (!await db.Database.CanConnectAsync(ct))
            {
                IsDatabaseReady = false;
                LastError = "Il database non è raggiungibile.";
                return false;
            }

            await db.Roles.AsNoTracking().AnyAsync(ct);
            IsDatabaseReady = true;
            LastError = null;
            return true;
        }
        catch (Exception ex)
        {
            IsDatabaseReady = false;
            LastError = ex.Message;
            logger.LogWarning(ex, "Verifica connessione database fallita");
            return false;
        }
    }
}
