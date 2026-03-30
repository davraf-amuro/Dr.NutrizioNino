-- ============================================================
-- ALTER TABLE audit record
-- status    : EXECUTED
-- tabella   : dbo.Dishes
-- token     : AD3414DAAA1C
-- timestamp : 2026-03-30 21:03:09 UTC
-- ============================================================

CREATE INDEX IX_Dishes_IsNutritionStale ON dbo.Dishes(IsNutritionStale) WHERE IsNutritionStale = 1;