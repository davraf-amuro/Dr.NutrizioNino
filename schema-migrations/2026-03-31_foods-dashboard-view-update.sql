-- ============================================================
-- 2026-03-31_foods-dashboard-view-update.sql
-- Aggiunge IsNutritionStale e NutrientsCalculatedAt alla view
-- Foods_Dashboard per allineamento con il modello FoodDashboardInfo
-- ============================================================

CREATE OR ALTER VIEW Foods_Dashboard AS
SELECT
    f.Id,
    f.Name,
    f.Barcode,
    f.Quantity,
    b.Name                                      AS BrandDescription,
    f.Calorie,
    u.Name                                      AS UnitOfMeasureDescription,
    u.Abbreviation,
    CAST(0 AS BIT)                              AS IsDish,
    (
        SELECT STRING_AGG(s.Name, ', ') WITHIN GROUP (ORDER BY s.Name)
        FROM FoodSupermarket fs
        JOIN Supermarkets s ON s.Id = fs.SupermarketId
        WHERE fs.FoodId = f.Id
    )                                           AS SupermarketsText,
    CAST(0 AS BIT)                              AS IsNutritionStale,
    CAST(NULL AS DATETIME2)                     AS NutrientsCalculatedAt
FROM Foods f
LEFT JOIN Brands b ON b.Id = f.BrandId
LEFT JOIN UnitsOfMeasures u ON u.Id = f.UnitOfMeasureId;
