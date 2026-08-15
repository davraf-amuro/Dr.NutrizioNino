-- ============================================================
-- 2026-08-15_dish-to-recipe-rename.sql
-- Rename del dominio Dish -> Recipe (Piatto -> Ricetta).
-- Rinomina in-place tabelle, colonne, vincoli e indici tramite sp_rename;
-- le viste vengono ricreate perche' sp_rename non riscrive il testo del modulo.
-- Idempotente: rieseguibile senza errori.
-- ============================================================

SET XACT_ABORT ON;
SET NOCOUNT ON;

BEGIN TRANSACTION;

-- ------------------------------------------------------------
-- 1. Tabelle
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.Dishes', 'U') IS NOT NULL
    EXEC sp_rename 'dbo.Dishes', 'Recipes';

IF OBJECT_ID('dbo.DishIngredients', 'U') IS NOT NULL
    EXEC sp_rename 'dbo.DishIngredients', 'RecipeIngredients';

IF OBJECT_ID('dbo.Dishes_Nutrients', 'U') IS NOT NULL
    EXEC sp_rename 'dbo.Dishes_Nutrients', 'Recipes_Nutrients';

-- ------------------------------------------------------------
-- 2. Colonne
-- ------------------------------------------------------------
IF COL_LENGTH('dbo.RecipeIngredients', 'DishId') IS NOT NULL
    EXEC sp_rename 'dbo.RecipeIngredients.DishId', 'RecipeId', 'COLUMN';

IF COL_LENGTH('dbo.Recipes_Nutrients', 'DishId') IS NOT NULL
    EXEC sp_rename 'dbo.Recipes_Nutrients.DishId', 'RecipeId', 'COLUMN';

-- ------------------------------------------------------------
-- 3. Primary key (il rename del vincolo rinomina anche l'indice associato)
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.PK_Dishes', 'PK') IS NOT NULL
    EXEC sp_rename 'dbo.PK_Dishes', 'PK_Recipes', 'OBJECT';

IF OBJECT_ID('dbo.PK_DishIngredients', 'PK') IS NOT NULL
    EXEC sp_rename 'dbo.PK_DishIngredients', 'PK_RecipeIngredients', 'OBJECT';

IF OBJECT_ID('dbo.PK_Dishes_Nutrients', 'PK') IS NOT NULL
    EXEC sp_rename 'dbo.PK_Dishes_Nutrients', 'PK_Recipes_Nutrients', 'OBJECT';

-- ------------------------------------------------------------
-- 4. Foreign key
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.FK_Dishes_Owner', 'F') IS NOT NULL
    EXEC sp_rename 'dbo.FK_Dishes_Owner', 'FK_Recipes_Owner', 'OBJECT';

IF OBJECT_ID('dbo.FK_Dishes_UnitsOfMeasures', 'F') IS NOT NULL
    EXEC sp_rename 'dbo.FK_Dishes_UnitsOfMeasures', 'FK_Recipes_UnitsOfMeasures', 'OBJECT';

IF OBJECT_ID('dbo.FK_Dishes_Nutrients_Dishes', 'F') IS NOT NULL
    EXEC sp_rename 'dbo.FK_Dishes_Nutrients_Dishes', 'FK_Recipes_Nutrients_Recipes', 'OBJECT';

IF OBJECT_ID('dbo.FK_Dishes_Nutrients_Nutrients', 'F') IS NOT NULL
    EXEC sp_rename 'dbo.FK_Dishes_Nutrients_Nutrients', 'FK_Recipes_Nutrients_Nutrients', 'OBJECT';

IF OBJECT_ID('dbo.FK_Dishes_Nutrients_UnitsOfMeasures', 'F') IS NOT NULL
    EXEC sp_rename 'dbo.FK_Dishes_Nutrients_UnitsOfMeasures', 'FK_Recipes_Nutrients_UnitsOfMeasures', 'OBJECT';

IF OBJECT_ID('dbo.FK_DishIngredients_Dish', 'F') IS NOT NULL
    EXEC sp_rename 'dbo.FK_DishIngredients_Dish', 'FK_RecipeIngredients_Recipe', 'OBJECT';

IF OBJECT_ID('dbo.FK_DishIngredients_Food', 'F') IS NOT NULL
    EXEC sp_rename 'dbo.FK_DishIngredients_Food', 'FK_RecipeIngredients_Food', 'OBJECT';

-- ------------------------------------------------------------
-- 5. Indici non clusterizzati
-- ------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Recipes') AND name = 'IX_Dishes_IsNutritionStale')
    EXEC sp_rename 'dbo.Recipes.IX_Dishes_IsNutritionStale', 'IX_Recipes_IsNutritionStale', 'INDEX';

IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.RecipeIngredients') AND name = 'IX_DishIngredients_FoodId')
    EXEC sp_rename 'dbo.RecipeIngredients.IX_DishIngredients_FoodId', 'IX_RecipeIngredients_FoodId', 'INDEX';

-- ------------------------------------------------------------
-- 6. Default constraint su Recipes.IsNutritionStale
--    Il nome originale e' auto-generato (DF__Dishes__...), lo risolvo a runtime.
-- ------------------------------------------------------------
DECLARE @defaultConstraintName sysname;

SELECT @defaultConstraintName = dc.name
FROM sys.default_constraints dc
JOIN sys.columns c
  ON c.object_id = dc.parent_object_id
 AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID('dbo.Recipes')
  AND c.name = 'IsNutritionStale';

IF @defaultConstraintName IS NOT NULL AND @defaultConstraintName <> 'DF_Recipes_IsNutritionStale'
    EXEC sp_rename @objname = @defaultConstraintName, @newname = 'DF_Recipes_IsNutritionStale', @objtype = 'OBJECT';

-- ------------------------------------------------------------
-- 7. Vista Dishes_Dashboard -> Recipes_Dashboard
--    CREATE VIEW deve essere il primo statement del batch: uso EXEC per
--    restare dentro la transazione senza separatori GO.
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.Dishes_Dashboard', 'V') IS NOT NULL
    DROP VIEW dbo.Dishes_Dashboard;

IF OBJECT_ID('dbo.Recipes_Dashboard', 'V') IS NOT NULL
    DROP VIEW dbo.Recipes_Dashboard;

EXEC('
CREATE VIEW Recipes_Dashboard AS
SELECT
    r.Id,
    r.Name,
    r.WeightGrams                                   AS Quantity,
    CAST(ISNULL((
        SELECT rn.Quantity
        FROM Recipes_Nutrients rn
        JOIN Nutrients n ON n.Id = rn.NutrientId
        WHERE rn.RecipeId = r.Id AND n.Name = ''Energia''
    ), 0) AS NUMERIC(6,2))                          AS Calorie,
    u.Name                                          AS UnitOfMeasureDescription,
    u.Abbreviation,
    r.IsNutritionStale,
    r.NutrientsCalculatedAt,
    r.OwnerId
FROM Recipes r
LEFT JOIN UnitsOfMeasures u ON u.Id = r.UnitOfMeasureId;
');

-- ------------------------------------------------------------
-- 8. Vista Foods_Dashboard: colonna IsDish -> IsRecipe
--    Ricreata identica alla definizione corrente, con la sola colonna rinominata.
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.Foods_Dashboard', 'V') IS NOT NULL
    DROP VIEW dbo.Foods_Dashboard;

EXEC('
CREATE VIEW Foods_Dashboard AS
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
        WHERE fn.FoodId = f.Id AND n.Name = ''Energia''
    ), 0) AS NUMERIC(6,2))                          AS Calorie,
    u.Name                                          AS UnitOfMeasureDescription,
    u.Abbreviation,
    CAST(0 AS BIT)                                  AS IsRecipe,
    (
        SELECT STRING_AGG(s.Name, '', '') WITHIN GROUP (ORDER BY s.Name)
        FROM FoodSupermarket fs
        JOIN Supermarkets s ON s.Id = fs.SupermarketId
        WHERE fs.FoodId = f.Id
    )                                               AS SupermarketsText,
    CAST(0 AS BIT)                                  AS IsNutritionStale,
    CAST(NULL AS DATETIME2)                         AS NutrientsCalculatedAt,
    f.OwnerId,
    (
        SELECT STRING_AGG(c.Name, '', '') WITHIN GROUP (ORDER BY c.Name)
        FROM FoodCategory fc
        JOIN Categories c ON c.Id = fc.CategoryId
        WHERE fc.FoodId = f.Id
    )                                               AS CategoriesText
FROM Foods f
LEFT JOIN Brands b ON b.Id = f.BrandId
LEFT JOIN UnitsOfMeasures u ON u.Id = f.UnitOfMeasureId;
');

COMMIT TRANSACTION;

PRINT 'Rename Dish -> Recipe completato.';
