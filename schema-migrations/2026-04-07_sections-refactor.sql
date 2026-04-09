-- ============================================================
-- Refactor sezioni simulazione: da enum fisso a entità CRUD
-- Data: 2026-04-07
-- ============================================================

-- 1. Drop tabella config precedente (sostituita dalla nuova entità)
DROP TABLE IF EXISTS DailySimulationSectionConfigs;

-- 2. Nuova tabella sezioni
CREATE TABLE DailySimulationSections (
    Id           UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Name         NVARCHAR(100)    NOT NULL,
    DisplayOrder INT              NOT NULL DEFAULT 0,
    IsActive     BIT              NOT NULL DEFAULT 1,
    CONSTRAINT PK_DailySimulationSections PRIMARY KEY (Id)
);

-- 3. Seed con GUID fissi per permettere il mapping degli entries esistenti
DECLARE @Id0 UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001' -- Colazione
DECLARE @Id1 UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000002' -- Pranzo
DECLARE @Id2 UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000003' -- Cena
DECLARE @Id3 UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000004' -- Spuntino
DECLARE @Id4 UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000005' -- Merenda
DECLARE @Id5 UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000006' -- Altro

INSERT INTO DailySimulationSections (Id, Name, DisplayOrder) VALUES
(@Id0, 'Colazione', 1),
(@Id1, 'Pranzo',    2),
(@Id2, 'Cena',      3),
(@Id3, 'Spuntino',  4),
(@Id4, 'Merenda',   5),
(@Id5, 'Altro',     6);

-- 4. Aggiunge colonna SectionId nullable (migration sicura)
ALTER TABLE DailySimulationEntries
    ADD SectionId UNIQUEIDENTIFIER NULL;

-- 5. Popola SectionId per gli entries esistenti basandosi sul vecchio SectionType
UPDATE DailySimulationEntries SET SectionId = @Id0 WHERE SectionType = 0;
UPDATE DailySimulationEntries SET SectionId = @Id1 WHERE SectionType = 1;
UPDATE DailySimulationEntries SET SectionId = @Id2 WHERE SectionType = 2;
UPDATE DailySimulationEntries SET SectionId = @Id3 WHERE SectionType = 3;
UPDATE DailySimulationEntries SET SectionId = @Id4 WHERE SectionType = 4;
UPDATE DailySimulationEntries SET SectionId = @Id5 WHERE SectionType = 5;

-- 6. Aggiunge FK verso DailySimulationSections
ALTER TABLE DailySimulationEntries
    ADD CONSTRAINT FK_DailySimulationEntries_Section
    FOREIGN KEY (SectionId) REFERENCES DailySimulationSections(Id);

-- 7. Rende SectionId NOT NULL
ALTER TABLE DailySimulationEntries
    ALTER COLUMN SectionId UNIQUEIDENTIFIER NOT NULL;

-- 8. Rimuove la colonna legacy SectionType
ALTER TABLE DailySimulationEntries DROP COLUMN SectionType;

-- DOWN:
-- ALTER TABLE DailySimulationEntries ADD SectionType TINYINT NOT NULL DEFAULT 0;
-- ALTER TABLE DailySimulationEntries DROP CONSTRAINT FK_DailySimulationEntries_Section;
-- ALTER TABLE DailySimulationEntries DROP COLUMN SectionId;
-- DROP TABLE DailySimulationSections;
