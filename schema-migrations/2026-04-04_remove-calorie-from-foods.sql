-- ============================================================
-- 2026-04-04_remove-calorie-from-foods.sql
-- Rimuove il campo Calorie dalla tabella Foods.
-- Il valore delle calorie rimane disponibile come nutriente "Energia"
-- in Foods_Nutrients. La view Foods_Dashboard viene aggiornata
-- per calcolare Calorie tramite join su Foods_Nutrients.
-- ============================================================

ALTER TABLE Foods DROP COLUMN Calorie;
GO

CREATE OR ALTER VIEW Foods_Dashboard AS
SELECT
    f.Id,
    f.Name,
    f.Barcode,
    f.Quantity,
    b.Name                                          AS BrandDescription,
    CAST(ISNULL((
        SELECT fn.Quantity
        FROM Foods_Nutrients fn
        JOIN Nutrients n ON n.Id = fn.NutrientId
        WHERE fn.FoodId = f.Id AND n.Name = 'Energia'
    ), 0) AS NUMERIC(6,2))                          AS Calorie,
    u.Name                                          AS UnitOfMeasureDescription,
    u.Abbreviation,
    CAST(0 AS BIT)                                  AS IsDish,
    (
        SELECT STRING_AGG(s.Name, ', ') WITHIN GROUP (ORDER BY s.Name)
        FROM FoodSupermarket fs
        JOIN Supermarkets s ON s.Id = fs.SupermarketId
        WHERE fs.FoodId = f.Id
    )                                               AS SupermarketsText,
    CAST(0 AS BIT)                                  AS IsNutritionStale,
    CAST(NULL AS DATETIME2)                         AS NutrientsCalculatedAt,
    f.OwnerId
FROM Foods f
LEFT JOIN Brands b ON b.Id = f.BrandId
LEFT JOIN UnitsOfMeasures u ON u.Id = f.UnitOfMeasureId;
