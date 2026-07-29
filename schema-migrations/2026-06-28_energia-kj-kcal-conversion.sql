-- ============================================================
-- 2026-06-28_energia-kj-kcal-conversion.sql
-- Conversione kJ <-> kcal in UnitConversions (1 kcal = 4.184 kJ).
-- Fallback per etichette con SOLO kJ; quando l'etichetta riporta kcal
-- il valore stampato viene letto direttamente (nessuna conversione).
-- Idempotente.
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM dbo.UnitConversions WHERE FromUnit = 'kJ' AND ToUnit = 'kcal')
    INSERT INTO dbo.UnitConversions (Id, FromUnit, ToUnit, Factor)
    VALUES (NEWID(), 'kJ', 'kcal', 0.2390057361);

IF NOT EXISTS (SELECT 1 FROM dbo.UnitConversions WHERE FromUnit = 'kcal' AND ToUnit = 'kJ')
    INSERT INTO dbo.UnitConversions (Id, FromUnit, ToUnit, Factor)
    VALUES (NEWID(), 'kcal', 'kJ', 4.1840000000);

-- DOWN (ripristino):
-- DELETE FROM dbo.UnitConversions WHERE (FromUnit = 'kJ' AND ToUnit = 'kcal') OR (FromUnit = 'kcal' AND ToUnit = 'kJ');
