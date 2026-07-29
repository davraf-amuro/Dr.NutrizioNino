-- Migration: crea tabella NutrientExtractionCache per la cache dei risultati AI vision
-- Data: 2026-04-09

CREATE TABLE [dbo].[NutrientExtractionCache] (
    [Id]             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [ImageHash]      NVARCHAR(64)     NOT NULL,
    [ExtractedJson]  NVARCHAR(MAX)    NOT NULL,
    [ConfidenceScore] REAL            NOT NULL DEFAULT 0,
    [CreatedAt]      DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_NutrientExtractionCache] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_NutrientExtractionCache_ImageHash] UNIQUE ([ImageHash])
);
GO
