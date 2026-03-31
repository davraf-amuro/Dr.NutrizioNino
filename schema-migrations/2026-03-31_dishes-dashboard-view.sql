-- ============================================================
-- 2026-03-31_dishes-dashboard-view.sql
-- View dedicata al dashboard piatti, separata da Foods_Dashboard
-- Campi: Id, Name, Quantity (WeightGrams), Calorie,
--        UnitOfMeasureDescription, Abbreviation,
--        IsNutritionStale, NutrientsCalculatedAt
-- ============================================================

CREATE OR ALTER VIEW Dishes_Dashboard AS
SELECT
    d.Id,
    d.Name,
    d.WeightGrams                               AS Quantity,
    d.Calorie,
    u.Name                                      AS UnitOfMeasureDescription,
    u.Abbreviation,
    d.IsNutritionStale,
    d.NutrientsCalculatedAt
FROM Dishes d
LEFT JOIN UnitsOfMeasures u ON u.Id = d.UnitOfMeasureId;
