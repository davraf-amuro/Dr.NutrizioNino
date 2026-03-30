-- ============================================================
-- ALTER TABLE audit record
-- status    : PENDING
-- tabella   : dbo.Dishes
-- token     : 2000BBB321F3
-- timestamp : 2026-03-30 21:02:57 UTC
-- ============================================================

EXEC sp_rename 'dbo.Dishes.Quantity', 'WeightGrams', 'COLUMN';