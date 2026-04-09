-- ============================================================
-- Preferenza tema UI per utente
-- Data: 2026-04-06
-- ============================================================

-- UP: aggiunge colonna ThemePreference ad AspNetUsers
ALTER TABLE AspNetUsers
    ADD ThemePreference nvarchar(10) NOT NULL DEFAULT 'light';

-- Valori accettati: 'light', 'dark', 'system'

-- DOWN:
-- ALTER TABLE AspNetUsers DROP COLUMN ThemePreference;
