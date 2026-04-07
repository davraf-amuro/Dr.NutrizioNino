-- ============================================================
-- Configurazione ordine sezioni simulazione giornaliera
-- Data: 2026-04-07
-- ============================================================

CREATE TABLE DailySimulationSectionConfigs (
    SectionType  tinyint      NOT NULL PRIMARY KEY,
    DisplayName  nvarchar(50) NOT NULL,
    DisplayOrder int          NOT NULL DEFAULT 0
);

INSERT INTO DailySimulationSectionConfigs (SectionType, DisplayName, DisplayOrder) VALUES
(0, 'Colazione', 1),
(1, 'Pranzo',    2),
(2, 'Cena',      3),
(3, 'Spuntino',  4),
(4, 'Merenda',   5),
(5, 'Altro',     6);

-- DOWN:
-- DROP TABLE DailySimulationSectionConfigs;
