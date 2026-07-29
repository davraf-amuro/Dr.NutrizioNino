-- Tabella conversioni unità di misura (sostituisce logica hardcoded in UnitConversionService)
CREATE TABLE dbo.UnitConversions (
    Id       UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    FromUnit NVARCHAR(10)     NOT NULL,
    ToUnit   NVARCHAR(10)     NOT NULL,
    Factor   DECIMAL(20, 10)  NOT NULL,
    CONSTRAINT PK_UnitConversions       PRIMARY KEY (Id),
    CONSTRAINT UQ_UnitConversions_Pair  UNIQUE (FromUnit, ToUnit)
);

-- Seed: tutte le coppie SI standard (mg / g / gr / µg / mcg)
INSERT INTO dbo.UnitConversions (Id, FromUnit, ToUnit, Factor) VALUES
-- µg ↔ mcg (alias della stessa unità)
(NEWID(), N'µg',  N'mcg', 1),
(NEWID(), N'mcg', N'µg',  1),
-- mg ↔ µg / mcg
(NEWID(), N'mg',  N'µg',  1000),
(NEWID(), N'mg',  N'mcg', 1000),
(NEWID(), N'µg',  N'mg',  0.001),
(NEWID(), N'mcg', N'mg',  0.001),
-- g / gr ↔ mg
(NEWID(), N'g',   N'mg',  1000),
(NEWID(), N'gr',  N'mg',  1000),
(NEWID(), N'mg',  N'g',   0.001),
(NEWID(), N'mg',  N'gr',  0.001),
-- g ↔ gr (alias)
(NEWID(), N'g',   N'gr',  1),
(NEWID(), N'gr',  N'g',   1),
-- g / gr ↔ µg / mcg
(NEWID(), N'g',   N'µg',  1000000),
(NEWID(), N'g',   N'mcg', 1000000),
(NEWID(), N'gr',  N'µg',  1000000),
(NEWID(), N'gr',  N'mcg', 1000000),
(NEWID(), N'µg',  N'g',   0.000001),
(NEWID(), N'mcg', N'g',   0.000001),
(NEWID(), N'µg',  N'gr',  0.000001),
(NEWID(), N'mcg', N'gr',  0.000001);
