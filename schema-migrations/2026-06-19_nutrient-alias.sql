-- Migration: crea tabella NutrientAlias per mappare nomi AI a nutrienti canonici (alias globali)
-- Data: 2026-06-19

CREATE TABLE [dbo].[NutrientAlias] (
    [Id]         UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [AiName]     NVARCHAR(200)    NOT NULL,
    [NutrientId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt]  DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_NutrientAlias] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_NutrientAlias_AiName] UNIQUE ([AiName]),
    CONSTRAINT [FK_NutrientAlias_Nutrient] FOREIGN KEY ([NutrientId]) REFERENCES [dbo].[Nutrients]([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_NutrientAlias_AiName] ON [dbo].[NutrientAlias] ([AiName]);
GO
