-- ============================================================
-- 002_supermarkets.sql
-- Aggiunge la gestione dei supermercati collegati agli alimenti
-- Relazione M:N tramite tabella esplicita FoodSupermarket
-- ============================================================

-- 1. Tabella Supermarkets
CREATE TABLE Supermarkets (
    Id      UNIQUEIDENTIFIER NOT NULL,
    Name    NVARCHAR(50)     NOT NULL,
    CONSTRAINT PK_Supermarkets PRIMARY KEY (Id),
    CONSTRAINT UQ_Supermarkets_Name UNIQUE (Name)
);

-- 2. Tabella FoodSupermarket (join M:N)
CREATE TABLE FoodSupermarket (
    FoodId          UNIQUEIDENTIFIER NOT NULL,
    SupermarketId   UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_FoodSupermarket PRIMARY KEY (FoodId, SupermarketId),
    CONSTRAINT FK_FoodSupermarket_Foods         FOREIGN KEY (FoodId)        REFERENCES Foods(Id),
    CONSTRAINT FK_FoodSupermarket_Supermarkets  FOREIGN KEY (SupermarketId) REFERENCES Supermarkets(Id)
);

-- 3. Seed: catene italiane comuni
INSERT INTO Supermarkets (Id, Name) VALUES
    (NEWID(), 'Esselunga'),
    (NEWID(), 'Lidl'),
    (NEWID(), 'Conad'),
    (NEWID(), 'Carrefour'),
    (NEWID(), 'Coop'),
    (NEWID(), 'Eurospin'),
    (NEWID(), 'Pam'),
    (NEWID(), 'Penny Market'),
    (NEWID(), 'MD');

-- 4. Aggiorna la view Foods_Dashboard aggiungendo la colonna SupermarketsText
CREATE OR ALTER VIEW Foods_Dashboard AS
SELECT
    f.Id,
    f.Name,
    f.Barcode,
    f.Quantity,
    b.Name AS BrandDescription,
    f.Calorie,
    u.Name AS UnitOfMeasureDescription,
    u.Abbreviation,
    CAST(0 AS BIT) IsDish,
    (
        SELECT STRING_AGG(s.Name, ', ') WITHIN GROUP (ORDER BY s.Name)
        FROM FoodSupermarket fs
        JOIN Supermarkets s ON s.Id = fs.SupermarketId
        WHERE fs.FoodId = f.Id
    ) AS SupermarketsText
FROM Foods f
LEFT JOIN Brands b ON b.Id = f.BrandId
LEFT JOIN UnitsOfMeasures u ON u.Id = f.UnitOfMeasureId;
