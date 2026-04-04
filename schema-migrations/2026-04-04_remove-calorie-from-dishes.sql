-- ============================================================
-- 2026-04-04_remove-calorie-from-dishes.sql
-- Rimuove il campo Calorie dalla tabella Dishes.
-- Il valore delle calorie rimane disponibile come nutriente "Energia"
-- in Dishes_Nutrients. La view Dishes_Dashboard viene aggiornata
-- per calcolare Calorie tramite join su Dishes_Nutrients.
-- ============================================================

ALTER TABLE Dishes DROP COLUMN Calorie;
GO

CREATE OR ALTER VIEW Dishes_Dashboard AS
SELECT
    d.Id,
    d.Name,
    d.WeightGrams                                   AS Quantity,
    CAST(ISNULL((
        SELECT dn.Quantity
        FROM Dishes_Nutrients dn
        JOIN Nutrients n ON n.Id = dn.NutrientId
        WHERE dn.DishId = d.Id AND n.Name = 'Energia'
    ), 0) AS NUMERIC(6,2))                          AS Calorie,
    u.Name                                          AS UnitOfMeasureDescription,
    u.Abbreviation,
    d.IsNutritionStale,
    d.NutrientsCalculatedAt,
    d.OwnerId
FROM Dishes d
LEFT JOIN UnitsOfMeasures u ON u.Id = d.UnitOfMeasureId;
