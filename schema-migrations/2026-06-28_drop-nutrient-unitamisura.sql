-- Migration: rimuove la ridondanza Nutrients.UnitaMisura (viola 3NF: duplica UnitsOfMeasures.Abbreviation
--            raggiungibile via DefaultUnitOfMeasureId) e l'indice non-unique ridondante su NutrientAlias.AiName
--            (già coperto dal vincolo UNIQUE UQ_NutrientAlias_AiName).
-- Data: 2026-06-28

-- 1) Drop colonna denormalizzata UnitaMisura: single source of truth = FK DefaultUnitOfMeasureId
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[Nutrients]') AND name = N'UnitaMisura'
)
BEGIN
    ALTER TABLE [dbo].[Nutrients] DROP COLUMN [UnitaMisura];
END
GO

-- 2) Drop indice non-unique ridondante: l'unicità è già garantita da UQ_NutrientAlias_AiName
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_NutrientAlias_AiName' AND object_id = OBJECT_ID(N'[dbo].[NutrientAlias]')
)
BEGIN
    DROP INDEX [IX_NutrientAlias_AiName] ON [dbo].[NutrientAlias];
END
GO
