-- ============================================================
-- ALTER TABLE audit record
-- status    : PENDING
-- tabella   : NutrientAlias
-- token     : B11DFD99EB85
-- timestamp : 2026-06-28 13:35:35 UTC
-- ============================================================

ALTER TABLE NutrientAlias ADD CONSTRAINT UQ_NutrientAlias_AiName UNIQUE (AiName)