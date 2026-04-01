-- ============================================================
-- ALTER TABLE audit record
-- status    : EXECUTED
-- tabella   : Foods
-- token     : 0C7C2CDDCC0E
-- timestamp : 2026-04-01 20:04:43 UTC
-- ============================================================

ALTER TABLE [dbo].[Foods] ADD [OwnerId] UNIQUEIDENTIFIER NULL CONSTRAINT [FK_Foods_Owner] FOREIGN KEY REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE SET NULL