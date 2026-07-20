-- ============================================================
-- Fabbisogno nutrizionale personale utente
-- Tabella dedicata, valore corrente (nessuno storico)
-- Data: 2026-07-21
-- ============================================================

-- NutritionalTargets: fabbisogno dichiarato dall'utente (kcal/carbo/proteine/grassi)
-- Un solo record per utente (UNIQUE su UserId) — nessuna storicizzazione
CREATE TABLE NutritionalTargets (
    Id            UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    UserId        UNIQUEIDENTIFIER NOT NULL,
    KcalTarget    DECIMAL(6,1) NULL,
    CarbsTarget   DECIMAL(6,1) NULL,
    ProteinTarget DECIMAL(6,1) NULL,
    FatTarget     DECIMAL(6,1) NULL,
    UpdatedAt     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_NutritionalTargets PRIMARY KEY (Id),
    CONSTRAINT FK_NutritionalTargets_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_NutritionalTargets_UserId UNIQUE (UserId)
);

-- DOWN:
-- DROP TABLE NutritionalTargets;
