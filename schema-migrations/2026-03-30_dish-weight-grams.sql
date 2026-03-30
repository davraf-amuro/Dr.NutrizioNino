-- ============================================================
-- Migrazione: dish weight grams
-- Data      : 2026-03-30
-- Descrizione: rinomina Dishes.Quantity in WeightGrams;
--              aggiorna la vista Dishes_Dashboard di conseguenza.
--              I calcoli dei nutrienti diventano assoluti (peso reale)
--              anziché normalizzati a 100g.
--              I piatti esistenti vanno cancellati e reinseriti.
-- ============================================================

-- 1. Rinomina colonna
EXEC sp_rename 'dbo.Dishes.Quantity', 'WeightGrams', 'COLUMN';
GO

-- 2. Aggiorna vista Dishes_Dashboard
ALTER VIEW dbo.Dishes_Dashboard AS
SELECT
    d.Id,
    d.Name,
    CAST(NULL AS NVARCHAR(50))  AS Barcode,
    d.WeightGrams               AS Quantity,
    CAST(NULL AS NVARCHAR(50))  AS BrandDescription,
    d.Calorie,
    u.Name                      AS UnitOfMeasureDescription,
    u.Abbreviation,
    CAST(1 AS BIT)              AS IsDish,
    CAST(NULL AS NVARCHAR(MAX)) AS SupermarketsText,
    d.IsNutritionStale,
    d.NutrientsCalculatedAt
FROM dbo.Dishes d
LEFT JOIN dbo.UnitsOfMeasures u ON u.Id = d.UnitOfMeasureId;
GO
