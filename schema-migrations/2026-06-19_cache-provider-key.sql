-- Migration: aggiunge ProviderKey a NutrientExtractionCache, rende la cache provider-aware
-- Data: 2026-06-19

-- Rimuovi vincolo univoco su solo ImageHash (ora la chiave è hash+provider)
ALTER TABLE [dbo].[NutrientExtractionCache]
    DROP CONSTRAINT [UQ_NutrientExtractionCache_ImageHash];
GO

-- Aggiungi colonna ProviderKey con default 'ollama' per retrocompatibilità righe esistenti
ALTER TABLE [dbo].[NutrientExtractionCache]
    ADD [ProviderKey] NVARCHAR(100) NOT NULL DEFAULT 'ollama';
GO

-- Nuovo vincolo univoco su (ImageHash, ProviderKey)
ALTER TABLE [dbo].[NutrientExtractionCache]
    ADD CONSTRAINT [UQ_Cache_Hash_Provider] UNIQUE ([ImageHash], [ProviderKey]);
GO

-- Index su ImageHash per lookup veloce prima di filtrare per provider
CREATE INDEX [IX_Cache_ImageHash] ON [dbo].[NutrientExtractionCache] ([ImageHash]);
GO
