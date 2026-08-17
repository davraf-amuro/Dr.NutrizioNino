-- ============================================================
-- Scheda Sciaudone — fabbisogno calcolato dalle misurazioni utente
-- Storicizzata: una riga per ogni misurazione di profilo
-- Data: 2026-08-16
-- ============================================================

-- Peso ideale dichiarato dall'utente: base di calcolo delle proteine della scheda.
-- Nullable perche' le misurazioni gia' registrate non lo hanno.
ALTER TABLE UserProfileEntries
    ADD IdealWeightKg NUMERIC(5,2) NULL;
GO

-- SciaudoneCards: valori calcolati a partire da una singola misurazione.
-- Formule (fonte unica in codice: Helpers/SciaudoneFormula.cs):
--   Kcal     = WeightKg * 22
--   ProteinG = IdealWeightKg * 2
--   FatG     = WeightKg * 0.66
--   FiberG   = (Kcal / 1000) * 15
--   CarbsG   = (Kcal - 990) / 4      -- 990 = 75 * 13.2, costante fissa da specifica
--
-- WeightKg e IdealWeightKg sono duplicati qui di proposito: la scheda resta leggibile
-- e verificabile anche se la misurazione di origine viene modificata.
CREATE TABLE SciaudoneCards (
    Id             UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    UserId         UNIQUEIDENTIFIER NOT NULL,
    ProfileEntryId UNIQUEIDENTIFIER NOT NULL,
    WeightKg       NUMERIC(5,2) NOT NULL,
    IdealWeightKg  NUMERIC(5,2) NOT NULL,
    Kcal           DECIMAL(6,1) NOT NULL,
    ProteinG       DECIMAL(6,1) NOT NULL,
    FatG           DECIMAL(6,1) NOT NULL,
    FiberG         DECIMAL(6,1) NOT NULL,
    CarbsG         DECIMAL(6,1) NOT NULL,
    ComputedAt     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_SciaudoneCards PRIMARY KEY (Id),
    -- NO ACTION obbligatorio: con CASCADE anche qui si creerebbe un secondo percorso
    -- di cancellazione verso AspNetUsers (SQL Server error 1785). La cancellazione
    -- dell'utente propaga comunque via UserProfileEntries.
    CONSTRAINT FK_SciaudoneCards_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_SciaudoneCards_ProfileEntry FOREIGN KEY (ProfileEntryId) REFERENCES UserProfileEntries(Id) ON DELETE CASCADE,
    -- Una sola scheda per misurazione
    CONSTRAINT UQ_SciaudoneCards_ProfileEntryId UNIQUE (ProfileEntryId)
);
GO

-- Lettura tipica: ultima scheda dell'utente / storico ordinato per data
CREATE INDEX IX_SciaudoneCards_UserId_ComputedAt
    ON SciaudoneCards (UserId, ComputedAt DESC);
GO

-- DOWN:
-- DROP INDEX IX_SciaudoneCards_UserId_ComputedAt ON SciaudoneCards;
-- DROP TABLE SciaudoneCards;
-- ALTER TABLE UserProfileEntries DROP COLUMN IdealWeightKg;
