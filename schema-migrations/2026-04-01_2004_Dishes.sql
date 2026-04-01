-- ============================================================
-- ALTER TABLE audit record
-- status    : EXECUTED
-- tabella   : Dishes
-- token     : ED59507A0529
-- timestamp : 2026-04-01 20:04:51 UTC
-- ============================================================

ALTER TABLE [dbo].[Dishes] ADD [OwnerId] UNIQUEIDENTIFIER NULL CONSTRAINT [FK_Dishes_Owner] FOREIGN KEY REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE SET NULL