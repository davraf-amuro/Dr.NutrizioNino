-- ============================================================
-- 2026-04-03_categories.sql
-- Crea le tabelle Categories e FoodCategory (M:N Food-Category)
-- ESEGUIRE in ordine: prima Categories, poi FoodCategory
-- ============================================================

CREATE TABLE [dbo].[Categories] (
    [Id]   UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_Categories] PRIMARY KEY,
    [Name] NVARCHAR(50)     NOT NULL
);

CREATE TABLE [dbo].[FoodCategory] (
    [FoodId]     UNIQUEIDENTIFIER NOT NULL,
    [CategoryId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_FoodCategory]          PRIMARY KEY ([FoodId], [CategoryId]),
    CONSTRAINT [FK_FoodCategory_Foods]       FOREIGN KEY ([FoodId])     REFERENCES [dbo].[Foods]([Id])      ON DELETE CASCADE,
    CONSTRAINT [FK_FoodCategory_Categories]  FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories]([Id]) ON DELETE CASCADE
);

-- DOWN (ripristino):
-- DROP TABLE [dbo].[FoodCategory];
-- DROP TABLE [dbo].[Categories];
