-- ============================================================
-- ALTER TABLE audit record
-- status    : EXECUTED
-- tabella   : AspNetUsers
-- token     : DA4C0EDE8E46
-- timestamp : 2026-04-01 20:05:06 UTC
-- ============================================================

ALTER TABLE [dbo].[AspNetUsers] ADD CONSTRAINT [UQ_AspNetUsers_NormalizedUserName] UNIQUE ([NormalizedUserName])