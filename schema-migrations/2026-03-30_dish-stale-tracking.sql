-- ============================================================
-- Migrazione: dish stale tracking
-- Data      : 2026-03-30
-- Descrizione: aggiunge IsNutritionStale e NutrientsCalculatedAt
--              a Dishes; crea vista Dishes_Dashboard; aggiorna
--              Foods_Dashboard con le nuove colonne.
-- ============================================================

-- 1. Nuove colonne su Dishes
ALTER TABLE dbo.Dishes ADD
    IsNutritionStale    BIT      NOT NULL DEFAULT 0,
    NutrientsCalculatedAt DATETIME NULL;
GO

-- 2. Filtered index per query dashboard "piatti da aggiornare"
CREATE INDEX IX_Dishes_IsNutritionStale
    ON dbo.Dishes(IsNutritionStale)
    WHERE IsNutritionStale = 1;
GO

-- 3. Indice su DishIngredients.FoodId per propagazione stale bulk
CREATE INDEX IX_DishIngredients_FoodId
    ON dbo.DishIngredients(FoodId);
GO

-- 4. Vista Dishes_Dashboard speculare a Foods_Dashboard
CREATE VIEW dbo.Dishes_Dashboard AS
SELECT
    d.Id,
    d.Name,
    CAST(NULL AS NVARCHAR(50))  AS Barcode,
    d.Quantity,
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

-- 5. Aggiorna Foods_Dashboard aggiungendo le colonne stale
--    (sempre false/null per i cibi — il ricalcolo non li riguarda)
ALTER VIEW dbo.Foods_Dashboard AS
SELECT
    f.Id,
    f.Name,
    f.Barcode,
    f.Quantity,
    b.Name  AS BrandDescription,
    f.Calorie,
    u.Name  AS UnitOfMeasureDescription,
    u.Abbreviation,
    CAST(0 AS BIT)       AS IsDish,
    (
        SELECT STRING_AGG(s.Name, ', ') WITHIN GROUP (ORDER BY s.Name)
        FROM   dbo.FoodSupermarket fs
        JOIN   dbo.Supermarkets    s  ON s.Id = fs.SupermarketId
        WHERE  fs.FoodId = f.Id
    ) AS SupermarketsText,
    CAST(0 AS BIT)       AS IsNutritionStale,
    CAST(NULL AS DATETIME) AS NutrientsCalculatedAt
FROM dbo.Foods f
LEFT JOIN dbo.Brands         b ON b.Id = f.BrandId
LEFT JOIN dbo.UnitsOfMeasures u ON u.Id = f.UnitOfMeasureId;
GO
