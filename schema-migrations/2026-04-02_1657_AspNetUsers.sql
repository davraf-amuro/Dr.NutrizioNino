-- ============================================================
-- ALTER TABLE audit record
-- status    : EXECUTED
-- tabella   : AspNetUsers
-- token     : 2A0BFE4219E9
-- timestamp : 2026-04-02 16:57:33 UTC
-- ============================================================

CREATE UNIQUE INDEX IX_AspNetUsers_NormalizedEmail_Unique ON AspNetUsers (NormalizedEmail) WHERE NormalizedEmail IS NOT NULL;