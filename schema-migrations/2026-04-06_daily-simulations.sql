-- ============================================================
-- Simulazione Piano Giornaliero
-- Tabelle per la registrazione di giornate tipo con analisi
-- e confronto dei nutrienti.
-- Data: 2026-04-06
-- ============================================================

-- DailySimulations: una "giornata tipo" per utente
CREATE TABLE DailySimulations (
    Id          UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    UserId      UNIQUEIDENTIFIER NOT NULL,
    Name        NVARCHAR(100) NOT NULL,
    CreatedAt   DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt   DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_DailySimulations PRIMARY KEY (Id),
    CONSTRAINT FK_DailySimulations_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
CREATE INDEX IX_DailySimulations_UserId ON DailySimulations(UserId);

-- DailySimulationEntries: singola voce in una sezione (Food o Dish)
-- SectionType: 0=Colazione 1=Pranzo 2=Cena 3=Spuntino 4=Merenda 5=Altro
-- SourceType:  0=Food 1=Dish
-- SourceId: nullable — per ricalcolo quantità; null se sorgente eliminata
CREATE TABLE DailySimulationEntries (
    Id              UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    SimulationId    UNIQUEIDENTIFIER NOT NULL,
    SectionType     TINYINT NOT NULL,
    SourceType      TINYINT NOT NULL,
    SourceId        UNIQUEIDENTIFIER NULL,
    SourceName      NVARCHAR(200) NOT NULL,
    QuantityGrams   DECIMAL(8,2) NOT NULL,
    SnapshotAt      DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_DailySimulationEntries PRIMARY KEY (Id),
    CONSTRAINT FK_DailySimulationEntries_Sim FOREIGN KEY (SimulationId) REFERENCES DailySimulations(Id) ON DELETE CASCADE
);
CREATE INDEX IX_DailySimulationEntries_SimId ON DailySimulationEntries(SimulationId);

-- DailySimulationEntryNutrients: snapshot nutrienti scalati per la quantità scelta
-- Tutti i valori sono snapshot al momento dell'inserimento (immutabili)
CREATE TABLE DailySimulationEntryNutrients (
    EntryId          UNIQUEIDENTIFIER NOT NULL,
    NutrientName     NVARCHAR(100) NOT NULL,
    PositionOrder    INT NOT NULL,
    Quantity         DECIMAL(10,4) NOT NULL,
    UnitAbbreviation NVARCHAR(20) NOT NULL,
    CONSTRAINT PK_DSEntryNutrients PRIMARY KEY (EntryId, NutrientName),
    CONSTRAINT FK_DSEntryNutrients_Entry FOREIGN KEY (EntryId) REFERENCES DailySimulationEntries(Id) ON DELETE CASCADE
);
