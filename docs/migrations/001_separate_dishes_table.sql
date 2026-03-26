-- ============================================================
-- Migrazione: Separazione tabella Dishes da Foods
-- Data: 2026-03-26
-- Descrizione: Crea tabelle Dishes e Dishes_Nutrients separate,
--              migra i dati esistenti, ricollega DishIngredients.
-- ============================================================

-- 1. Nuova tabella Dishes (senza BrandId)
CREATE TABLE Dishes
(
    Id              UNIQUEIDENTIFIER NOT NULL,
    Name            NVARCHAR(50)     NOT NULL,
    Quantity        NUMERIC(6, 2)    NOT NULL,
    Calorie         NUMERIC(6, 2)    NOT NULL,
    UnitOfMeasureId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_Dishes PRIMARY KEY (Id),
    CONSTRAINT FK_Dishes_UnitsOfMeasures FOREIGN KEY (UnitOfMeasureId) REFERENCES UnitsOfMeasures (Id)
);

-- 2. Nuova tabella Dishes_Nutrients
CREATE TABLE Dishes_Nutrients
(
    DishId          UNIQUEIDENTIFIER NOT NULL,
    NutrientId      UNIQUEIDENTIFIER NOT NULL,
    UnitOfMeasureId UNIQUEIDENTIFIER NOT NULL,
    Quantity        NUMERIC(6, 2)    NOT NULL,
    CONSTRAINT PK_Dishes_Nutrients PRIMARY KEY (DishId, NutrientId),
    CONSTRAINT FK_Dishes_Nutrients_Dishes FOREIGN KEY (DishId) REFERENCES Dishes (Id),
    CONSTRAINT FK_Dishes_Nutrients_Nutrients FOREIGN KEY (NutrientId) REFERENCES Nutrients (Id),
    CONSTRAINT FK_Dishes_Nutrients_UnitsOfMeasures FOREIGN KEY (UnitOfMeasureId) REFERENCES UnitsOfMeasures (Id)
);

-- 3. Migra piatti esistenti in Dishes
INSERT INTO Dishes (Id, Name, Quantity, Calorie, UnitOfMeasureId)
SELECT Id, Name, Quantity, Calorie, UnitOfMeasureId
FROM Foods
WHERE IsDish = 1;

-- 4. Migra nutrienti dei piatti in Dishes_Nutrients
INSERT INTO Dishes_Nutrients (DishId, NutrientId, UnitOfMeasureId, Quantity)
SELECT FoodId, NutrientId, UnitOfMeasureId, Quantity
FROM Foods_Nutrients
WHERE FoodId IN (SELECT Id FROM Foods WHERE IsDish = 1);

-- 5. Ricollega DishIngredients.DishId → Dishes
ALTER TABLE DishIngredients
    DROP CONSTRAINT FK_DishIngredients_Dish;

ALTER TABLE DishIngredients
    ADD CONSTRAINT FK_DishIngredients_Dish
        FOREIGN KEY (DishId) REFERENCES Dishes (Id);

-- 6. Aggiorna la view PRIMA di droppare IsDish
--    (la colonna IsDish viene sostituita con CAST(0 AS BIT) poiché Foods contiene solo ingredienti)
CREATE OR ALTER VIEW Foods_Dashboard AS
SELECT f.[Id],
       f.[Name],
       f.[Barcode],
       f.[Quantity],
       b.Name            BrandDescription,
       f.[Calorie],
       uom.Name          UnitOfMeasureDescription,
       uom.Abbreviation,
       CAST(0 AS BIT)    IsDish
FROM [Foods] f
    LEFT JOIN Brands b             ON b.Id   = f.BrandId
    LEFT JOIN UnitsOfMeasures uom  ON uom.Id = f.UnitOfMeasureId;

-- 7. Rimuovi piatti dalla tabella Foods
DELETE FROM Foods_Nutrients WHERE FoodId IN (SELECT Id FROM Foods WHERE IsDish = 1);
DELETE FROM Foods WHERE IsDish = 1;

-- 8. Rimuovi colonna IsDish da Foods
ALTER TABLE Foods
    DROP COLUMN IsDish;
