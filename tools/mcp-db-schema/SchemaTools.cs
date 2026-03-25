using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using ModelContextProtocol.Server;

namespace mcp_db_schema;

[McpServerToolType]
public class SchemaTools(ConnectionManager connectionManager)
{
    [McpServerTool]
    [Description("Elenca i nomi delle connection string reali trovate negli appsettings del progetto. Usare prima di get_schema se non si sa quale connection string è disponibile.")]
    public string list_connections()
    {
        var real = connectionManager.GetRealConnections();
        if (real.Count == 0)
            return "Nessuna connection string reale trovata negli appsettings.";

        var sb = new StringBuilder();
        sb.AppendLine("Connection string disponibili:");
        foreach (var (name, _) in real)
            sb.AppendLine($"  - {name}");

        if (connectionManager.ActiveConnectionName is not null)
            sb.AppendLine($"\nAttiva: {connectionManager.ActiveConnectionName}");

        return sb.ToString();
    }

    [McpServerTool]
    [Description("Cambia la connection string attiva per la sessione corrente. Usare list_connections per vedere i nomi disponibili. L'utente può anche dire 'cambia connection'.")]
    public string change_connection(
        [Description("Nome della connection string da attivare")] string connectionName)
    {
        var real = connectionManager.GetRealConnections();

        if (!real.TryGetValue(connectionName, out var cs))
            return $"Connection string '{connectionName}' non trovata. Disponibili: {string.Join(", ", real.Keys)}";

        connectionManager.SetActive(connectionName, cs);
        return $"Connection attiva cambiata in '{connectionName}'.";
    }

    [McpServerTool]
    [Description("Legge lo schema del database: tabelle, viste e le relative colonne con tipo, nullabilità e vincoli. Se ci sono più connection string disponibili e nessuna è attiva, chiede all'utente quale usare.")]
    public async Task<string> get_schema(
        [Description("Filtra per nome esatto di una tabella o vista (opzionale)")] string? tableName = null,
        CancellationToken cancellationToken = default)
    {
        if (connectionManager.ActiveConnectionString is null)
        {
            var real = connectionManager.GetRealConnections();

            if (real.Count == 0)
                return "Nessuna connection string reale trovata negli appsettings. Verificare MCP_PROJECT_PATH.";

            if (real.Count == 1)
            {
                var (name, cs) = real.First();
                connectionManager.SetActive(name, cs);
            }
            else
            {
                return $"Trovate {real.Count} connection string. Usare change_connection con uno di questi nomi: {string.Join(", ", real.Keys)}";
            }
        }

        try
        {
            return await QuerySchemaAsync(connectionManager.ActiveConnectionString!, tableName, cancellationToken);
        }
        catch (SqlException ex)
        {
            return $"Errore SQL (connection: {connectionManager.ActiveConnectionName}): {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("""
        Esegue DDL sul database con tre livelli di autorizzazione:
        - 🟢 CREATE nuovo oggetto: mostra anteprima informativa, l'utente può confermare
        - 🟡 Rischio medio (ADD/DROP CONSTRAINT, DROP VIEW/INDEX, CREATE OR ALTER su oggetto esistente): richiedi conferma esplicita
        - 🔴 Rischio alto (DROP TABLE, DROP COLUMN, ALTER COLUMN): NON chiamare confirmed=true senza che l'utente abbia letto i rischi e scritto esplicitamente di voler procedere
        REGOLA: chiamare SEMPRE prima con confirmed=false, poi confirmed=true SOLO dopo approvazione esplicita dell'utente.
        """)]
    public async Task<string> execute_ddl(
        [Description("Statement DDL da eseguire")] string sql,
        [Description("false = anteprima senza eseguire (obbligatorio come primo passo), true = esegue solo dopo approvazione esplicita dell'utente")] bool confirmed = false,
        CancellationToken cancellationToken = default)
    {
        if (connectionManager.ActiveConnectionString is null)
        {
            var real = connectionManager.GetRealConnections();
            if (real.Count == 0)
                return "Nessuna connection string reale trovata. Usare list_connections e change_connection prima.";
            if (real.Count == 1)
            {
                var (name, cs) = real.First();
                connectionManager.SetActive(name, cs);
            }
            else
            {
                return $"Trovate {real.Count} connection string attive. Usare change_connection prima.";
            }
        }

        var (summary, riskLevel, riskDetail) = DescribeDdl(sql.Trim());

        // Per CREATE OR ALTER VIEW/PROCEDURE: controlla se l'oggetto esiste già e promuovi il rischio
        if (riskLevel == RiskLevel.Low && IsCreateOrAlter(sql))
        {
            var objectName = ExtractCreateOrAlterName(sql);
            if (objectName is not null && await ObjectExistsAsync(objectName, connectionManager.ActiveConnectionString!, cancellationToken))
            {
                riskLevel = RiskLevel.Medium;
                riskDetail = $"L'oggetto '{objectName}' esiste già: questa operazione lo sovrascriverà.\n" + riskDetail;
            }
        }

        if (!confirmed)
        {
            return riskLevel switch
            {
                RiskLevel.Low => $"""
                    ℹ️  ANTEPRIMA — nessuna modifica eseguita
                    ══════════════════════════════════════════════════════
                    Operazione : {summary}
                    Database   : {connectionManager.ActiveConnectionName}

                    {riskDetail}

                    SQL:
                    ──────────────────────────────────────────────────────
                    {sql.Trim()}
                    ──────────────────────────────────────────────────────

                    Stai per creare un nuovo oggetto. Conferma per procedere.
                    ══════════════════════════════════════════════════════
                    """,

                RiskLevel.Medium => $"""
                    ⚠️  RICHIESTA CONFERMA
                    ══════════════════════════════════════════════════════
                    Operazione : {summary}
                    Database   : {connectionManager.ActiveConnectionName}
                    Rischio    : 🟡 MEDIO

                    {riskDetail}

                    SQL:
                    ──────────────────────────────────────────────────────
                    {sql.Trim()}
                    ──────────────────────────────────────────────────────

                    Questa operazione modifica struttura esistente.
                    Mostra questo messaggio all'utente e attendi conferma prima di procedere.
                    ══════════════════════════════════════════════════════
                    """,

                RiskLevel.High => $"""
                    🛑  PERMESSO ESPLICITO RICHIESTO
                    ══════════════════════════════════════════════════════
                    Operazione : {summary}
                    Database   : {connectionManager.ActiveConnectionName}
                    Rischio    : 🔴 ALTO — OPERAZIONE POTENZIALMENTE IRREVERSIBILE

                    {riskDetail}

                    SQL:
                    ──────────────────────────────────────────────────────
                    {sql.Trim()}
                    ──────────────────────────────────────────────────────

                    ⚠️  ISTRUZIONE PER L'AI: NON chiamare confirmed=true finché l'utente
                    non ha scritto esplicitamente di voler procedere, dopo aver letto i rischi.
                    Mostra TUTTO questo messaggio e attendi risposta testuale.
                    ══════════════════════════════════════════════════════
                    """,

                _ => $"""
                    ⚪ OPERAZIONE NON RICONOSCIUTA — verificare il SQL manualmente
                    ══════════════════════════════════════════════════════
                    Operazione : {summary}
                    Database   : {connectionManager.ActiveConnectionName}

                    {riskDetail}

                    SQL:
                    ──────────────────────────────────────────────────────
                    {sql.Trim()}
                    ──────────────────────────────────────────────────────

                    Verificare il SQL prima di confermare.
                    ══════════════════════════════════════════════════════
                    """
            };
        }

        var riskBadge = riskLevel switch
        {
            RiskLevel.High    => "🔴",
            RiskLevel.Medium  => "🟡",
            RiskLevel.Unknown => "⚪",
            _                 => "🟢"
        };

        try
        {
            await using var conn = new SqlConnection(connectionManager.ActiveConnectionString);
            await conn.OpenAsync(cancellationToken);
            await using var cmd = new SqlCommand(sql.Trim(), conn) { CommandTimeout = 30 };
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return $"✅ {riskBadge} Eseguito su '{connectionManager.ActiveConnectionName}'.\n{summary}";
        }
        catch (SqlException ex)
        {
            return $"❌ Errore SQL (connection: {connectionManager.ActiveConnectionName}): {ex.Message}";
        }
    }

    private static bool IsCreateOrAlter(string sql)
        => Regex.IsMatch(sql, @"CREATE\s+OR\s+ALTER\s+(VIEW|PROCEDURE|PROC|FUNCTION)", RegexOptions.IgnoreCase);

    private static string? ExtractCreateOrAlterName(string sql)
    {
        var m = Regex.Match(sql, @"CREATE\s+OR\s+ALTER\s+(?:VIEW|PROCEDURE|PROC|FUNCTION)\s+(?:\[?\w+\]?\.)?\[?(\w+)\]?", RegexOptions.IgnoreCase);
        return m.Success ? m.Groups[1].Value : null;
    }

    private static async Task<bool> ObjectExistsAsync(string name, string connectionString, CancellationToken ct)
    {
        try
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(
                "SELECT 1 FROM sys.objects WHERE name = @name AND type IN ('V','P','FN','TF','IF')", conn);
            cmd.Parameters.AddWithValue("@name", name);
            var result = await cmd.ExecuteScalarAsync(ct);
            return result is not null;
        }
        catch
        {
            return false; // in caso di errore di connessione, non bloccare il flusso
        }
    }

    private enum RiskLevel { Low, Medium, High, Unknown }

    /// <summary>
    /// Analizza uno statement DDL e restituisce (sommario breve, livello di rischio, dettaglio esteso).
    /// </summary>
    private static (string Summary, RiskLevel Risk, string Detail) DescribeDdl(string sql)
    {
        var normalized = Regex.Replace(sql, @"\s+", " ").Trim();

        // ── CREATE TABLE ──────────────────────────────────────────────────────
        var m = Regex.Match(normalized, @"CREATE\s+TABLE\s+(?:\[?\w+\]?\.)?\[?(\w+)\]?", RegexOptions.IgnoreCase);
        if (m.Success)
        {
            var t = m.Groups[1].Value;
            return (
                $"CREATE TABLE '{t}'",
                RiskLevel.Low,
                $"Creerà la nuova tabella '{t}'. Nessun dato esistente viene toccato.\n" +
                $"Se la tabella esiste già l'operazione fallirà con errore (usare IF NOT EXISTS per evitarlo)."
            );
        }

        // ── DROP TABLE ────────────────────────────────────────────────────────
        m = Regex.Match(normalized, @"DROP\s+TABLE\s+(?:IF\s+EXISTS\s+)?(?:\[?\w+\]?\.)?\[?(\w+)\]?", RegexOptions.IgnoreCase);
        if (m.Success)
        {
            var t = m.Groups[1].Value;
            return (
                $"DROP TABLE '{t}'",
                RiskLevel.High,
                $"ELIMINERÀ DEFINITIVAMENTE la tabella '{t}' e TUTTI i dati in essa contenuti.\n" +
                $"L'operazione è IRREVERSIBILE: non esiste un rollback automatico.\n" +
                $"Se esistono chiavi esterne che puntano a questa tabella, l'operazione fallirà " +
                $"con un errore di vincolo referenziale — verificare prima con get_schema."
            );
        }

        // ── ALTER TABLE ───────────────────────────────────────────────────────
        m = Regex.Match(normalized, @"ALTER\s+TABLE\s+(?:\[?\w+\]?\.)?\[?(\w+)\]?\s+(.+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (m.Success)
        {
            var tableName = m.Groups[1].Value;
            var rest = m.Groups[2].Value.Trim();
            var restUpper = rest.ToUpperInvariant();

            // ADD CONSTRAINT
            var mc = Regex.Match(rest, @"ADD\s+CONSTRAINT\s+\[?(\w+)\]?\s*(.*)", RegexOptions.IgnoreCase);
            if (mc.Success)
            {
                var cName = mc.Groups[1].Value;
                var cDef  = mc.Groups[2].Value.Trim();
                var isFk  = cDef.ToUpperInvariant().Contains("FOREIGN KEY");
                var isPk  = cDef.ToUpperInvariant().Contains("PRIMARY KEY");
                var detail = isFk
                    ? $"Aggiungerà la chiave esterna '{cName}' sulla tabella '{tableName}'.\n" +
                      $"Se esistono righe che violano il vincolo FK, l'operazione fallirà con errore referenziale.\n" +
                      $"Verificare la coerenza dei dati prima di procedere."
                    : isPk
                    ? $"Aggiungerà la chiave primaria '{cName}' sulla tabella '{tableName}'.\n" +
                      $"Richiede che la colonna/e indicata/e non contenga valori NULL o duplicati."
                    : $"Aggiungerà il vincolo '{cName}' ({cDef[..Math.Min(80, cDef.Length)]}) sulla tabella '{tableName}'.\n" +
                      $"Se i dati esistenti violano il vincolo, l'operazione fallirà.";
                return ($"ALTER TABLE '{tableName}' ADD CONSTRAINT '{cName}'", RiskLevel.Medium, detail);
            }

            // ADD COLUMN
            mc = Regex.Match(rest, @"ADD\s+(?:COLUMN\s+)?\[?(\w+)\]?\s+(\S.*)", RegexOptions.IgnoreCase);
            if (mc.Success && !restUpper.TrimStart().StartsWith("ADD CONSTRAINT"))
            {
                var colName = mc.Groups[1].Value;
                var colDef  = mc.Groups[2].Value.Trim();
                var isNotNull  = Regex.IsMatch(colDef, @"\bNOT\s+NULL\b", RegexOptions.IgnoreCase);
                var hasDefault = Regex.IsMatch(colDef, @"\bDEFAULT\b", RegexOptions.IgnoreCase);
                string detail;
                RiskLevel risk;
                if (isNotNull && !hasDefault)
                {
                    risk   = RiskLevel.Medium;
                    detail = $"Aggiungerà la colonna '{colName}' ({colDef}) alla tabella '{tableName}'.\n" +
                             $"ATTENZIONE: la colonna è NOT NULL senza DEFAULT.\n" +
                             $"Se la tabella contiene già righe, SQL Server rifiuterà l'operazione con errore.\n" +
                             $"Soluzione: aggiungere un DEFAULT oppure aggiungere prima la colonna come NULL " +
                             $"e poi popolarla con un UPDATE.";
                }
                else
                {
                    risk   = RiskLevel.Low;
                    detail = $"Aggiungerà la colonna '{colName}' ({colDef}) alla tabella '{tableName}'.\n" +
                             $"Le righe esistenti riceveranno il valore DEFAULT (o NULL se non specificato).\n" +
                             $"Nessun dato esistente viene modificato o rimosso.";
                }
                return ($"ALTER TABLE '{tableName}' ADD COLUMN '{colName}'", risk, detail);
            }

            // DROP COLUMN
            mc = Regex.Match(rest, @"DROP\s+COLUMN\s+\[?(\w+)\]?", RegexOptions.IgnoreCase);
            if (mc.Success)
            {
                var colName = mc.Groups[1].Value;
                return (
                    $"ALTER TABLE '{tableName}' DROP COLUMN '{colName}'",
                    RiskLevel.High,
                    $"ELIMINERÀ DEFINITIVAMENTE la colonna '{colName}' dalla tabella '{tableName}'.\n" +
                    $"Tutti i dati contenuti in quella colonna andranno PERSI in modo IRREVERSIBILE.\n" +
                    $"Se la colonna è referenziata da un indice, un vincolo o una chiave esterna, " +
                    $"l'operazione fallirà — rimuovere prima i vincoli dipendenti."
                );
            }

            // DROP CONSTRAINT
            mc = Regex.Match(rest, @"DROP\s+CONSTRAINT\s+\[?(\w+)\]?", RegexOptions.IgnoreCase);
            if (mc.Success)
            {
                var cName = mc.Groups[1].Value;
                return (
                    $"ALTER TABLE '{tableName}' DROP CONSTRAINT '{cName}'",
                    RiskLevel.Medium,
                    $"Rimuoverà il vincolo '{cName}' dalla tabella '{tableName}'.\n" +
                    $"Dopo la rimozione non sarà più garantita l'integrità referenziale o le regole " +
                    $"che quel vincolo imponeva. I dati esistenti non vengono modificati.\n" +
                    $"Verificare che nessun altro oggetto dipenda da questo vincolo."
                );
            }

            // ALTER COLUMN
            mc = Regex.Match(rest, @"ALTER\s+COLUMN\s+\[?(\w+)\]?\s+(.+)", RegexOptions.IgnoreCase);
            if (mc.Success)
            {
                var colName = mc.Groups[1].Value;
                var newDef  = mc.Groups[2].Value.Trim();
                return (
                    $"ALTER TABLE '{tableName}' ALTER COLUMN '{colName}'",
                    RiskLevel.High,
                    $"Cambierà la definizione della colonna '{colName}' nella tabella '{tableName}' in: {newDef}\n" +
                    $"RISCHI:\n" +
                    $"  • Se il nuovo tipo è più piccolo (es. VARCHAR(100) → VARCHAR(10)), " +
                    $"i valori più lunghi verranno TRONCATI o l'operazione fallirà.\n" +
                    $"  • Se si aggiunge NOT NULL e ci sono valori NULL presenti, l'operazione fallirà.\n" +
                    $"  • Se il tipo è incompatibile (es. stringa → int), l'operazione fallirà con errore di conversione.\n" +
                    $"Eseguire prima una verifica dei dati con SELECT."
                );
            }

            return (
                $"ALTER TABLE '{tableName}'",
                RiskLevel.Unknown,
                $"Operazione ALTER TABLE sulla tabella '{tableName}': {rest[..Math.Min(120, rest.Length)]}.\n" +
                $"Tipo di operazione non riconosciuto dal parser — verificare manualmente il SQL prima di procedere."
            );
        }

        // ── CREATE/ALTER VIEW ─────────────────────────────────────────────────
        m = Regex.Match(normalized, @"(CREATE\s+OR\s+ALTER|CREATE|ALTER)\s+VIEW\s+(?:\[?\w+\]?\.)?\[?(\w+)\]?", RegexOptions.IgnoreCase);
        if (m.Success)
        {
            var op      = m.Groups[1].Value;
            var vName   = m.Groups[2].Value;
            var isAlter = op.ToUpperInvariant().Contains("ALTER");
            return (
                $"{op.ToUpperInvariant()} VIEW '{vName}'",
                RiskLevel.Low,
                isAlter
                    ? $"Modificherà la definizione della vista '{vName}'.\n" +
                      $"Le query che usano questa vista potrebbero restituire risultati diversi dopo la modifica.\n" +
                      $"Verificare che nessun componente dipendente si rompa."
                    : $"Creerà la nuova vista '{vName}'. Nessun dato esistente viene toccato."
            );
        }

        // ── DROP VIEW ─────────────────────────────────────────────────────────
        m = Regex.Match(normalized, @"DROP\s+VIEW\s+(?:IF\s+EXISTS\s+)?(?:\[?\w+\]?\.)?\[?(\w+)\]?", RegexOptions.IgnoreCase);
        if (m.Success)
        {
            var vName = m.Groups[1].Value;
            return (
                $"DROP VIEW '{vName}'",
                RiskLevel.Medium,
                $"Eliminerà la vista '{vName}'.\n" +
                $"Qualsiasi query, report o componente che utilizza questa vista smetterà di funzionare.\n" +
                $"Verificare le dipendenze prima di procedere (sp_depends '{vName}')."
            );
        }

        // ── CREATE INDEX ──────────────────────────────────────────────────────
        m = Regex.Match(normalized, @"CREATE\s+(?:UNIQUE\s+)?(?:CLUSTERED\s+|NONCLUSTERED\s+)?INDEX\s+\[?(\w+)\]?\s+ON\s+(?:\[?\w+\]?\.)?\[?(\w+)\]?", RegexOptions.IgnoreCase);
        if (m.Success)
        {
            var iName = m.Groups[1].Value;
            var tName = m.Groups[2].Value;
            var isUnique = normalized.ToUpperInvariant().Contains("UNIQUE");
            var detail = $"Creerà l'indice '{iName}' sulla tabella '{tName}'.\n" +
                         $"Durante la creazione la tabella potrebbe essere bloccata in lettura/scrittura " +
                         $"(a seconda delle opzioni usate).\n";
            if (isUnique)
                detail += $"Indice UNIQUE: se esistono valori duplicati nelle colonne indicate, " +
                          $"la creazione fallirà con errore.";
            return ($"CREATE INDEX '{iName}' su '{tName}'", RiskLevel.Low, detail);
        }

        // ── DROP INDEX ────────────────────────────────────────────────────────
        m = Regex.Match(normalized, @"DROP\s+INDEX\s+(?:IF\s+EXISTS\s+)?\[?(\w+)\]?", RegexOptions.IgnoreCase);
        if (m.Success)
        {
            var iName = m.Groups[1].Value;
            return (
                $"DROP INDEX '{iName}'",
                RiskLevel.Medium,
                $"Eliminerà l'indice '{iName}'.\n" +
                $"Le query che dipendono da questo indice per le prestazioni diventeranno più lente.\n" +
                $"I dati non vengono modificati."
            );
        }

        // ── fallback ──────────────────────────────────────────────────────────
        return (
            "Operazione DDL non riconosciuta",
            RiskLevel.Unknown,
            $"Il parser non ha riconosciuto il tipo di operazione DDL.\n" +
            $"Verificare manualmente il SQL prima di procedere.\n" +
            $"Estratto: {normalized[..Math.Min(200, normalized.Length)]}"
        );
    }

    private static async Task<string> QuerySchemaAsync(string connectionString, string? tableFilter, CancellationToken ct)
    {
        const string sql = """
            SELECT
                t.TABLE_SCHEMA,
                t.TABLE_NAME,
                t.TABLE_TYPE,
                c.COLUMN_NAME,
                c.ORDINAL_POSITION,
                c.IS_NULLABLE,
                c.DATA_TYPE,
                c.CHARACTER_MAXIMUM_LENGTH,
                c.NUMERIC_PRECISION,
                c.NUMERIC_SCALE
            FROM INFORMATION_SCHEMA.TABLES t
            JOIN INFORMATION_SCHEMA.COLUMNS c
                ON t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
            WHERE (@TableName IS NULL OR t.TABLE_NAME = @TableName)
            ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME, c.ORDINAL_POSITION
            """;

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@TableName", (object?)tableFilter ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var sb = new StringBuilder();
        string? currentTable = null;

        while (await reader.ReadAsync(ct))
        {
            var schema = reader.GetString(0);
            var table = reader.GetString(1);
            var tableType = reader.GetString(2) == "VIEW" ? "VIEW" : "TABLE";
            var column = reader.GetString(3);
            var nullable = reader.GetString(5) == "YES";
            var dataType = reader.GetString(6);
            var maxLen = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7);
            var precision = reader.IsDBNull(8) ? (int?)null : (int)reader.GetByte(8);
            var scale = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9);

            var fullName = $"{schema}.{table}";
            if (fullName != currentTable)
            {
                if (currentTable is not null) sb.AppendLine();
                sb.AppendLine($"[{tableType}] {fullName}");
                currentTable = fullName;
            }

            var typeDesc = dataType;
            if (maxLen.HasValue)
                typeDesc += maxLen == -1 ? "(MAX)" : $"({maxLen})";
            else if (precision.HasValue)
                typeDesc += scale.HasValue ? $"({precision},{scale})" : $"({precision})";

            sb.AppendLine($"  {column} {typeDesc}{(nullable ? "" : " NOT NULL")}");
        }

        return sb.Length == 0 ? "Nessuna tabella o vista trovata." : sb.ToString();
    }
}
