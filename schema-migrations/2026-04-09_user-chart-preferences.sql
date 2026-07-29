-- Migration: aggiunge colonna NutrientChartPreferences (JSON) alla tabella AspNetUsers
-- Data: 2026-04-09

ALTER TABLE [AspNetUsers] ADD [NutrientChartPreferences] NVARCHAR(MAX) NULL;
GO
