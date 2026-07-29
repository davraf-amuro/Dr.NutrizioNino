-- Migration: aggiunge colonna UnitaMisura (unità canonica) alla tabella Nutrient
-- Data: 2026-04-09

ALTER TABLE [Nutrients] ADD [UnitaMisura] NVARCHAR(20) NULL;
GO

-- Popola con l'abbreviazione dell'unità di misura default
UPDATE n SET n.[UnitaMisura] = uom.[Abbreviation]
FROM [Nutrients] n
JOIN [UnitsOfMeasures] uom ON n.[DefaultUnitOfMeasureId] = uom.[Id]
WHERE n.[UnitaMisura] IS NULL;
GO

-- Rende la colonna NOT NULL dopo il populate
ALTER TABLE [Nutrients] ALTER COLUMN [UnitaMisura] NVARCHAR(20) NOT NULL;
GO
