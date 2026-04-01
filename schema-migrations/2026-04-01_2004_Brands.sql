-- ============================================================
-- ALTER TABLE audit record
-- status    : EXECUTED
-- tabella   : Brands
-- token     : B14228B806B0
-- timestamp : 2026-04-01 20:04:58 UTC
-- ============================================================

ALTER TABLE [dbo].[Brands] ADD [OwnerId] UNIQUEIDENTIFIER NULL CONSTRAINT [FK_Brands_Owner] FOREIGN KEY REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE SET NULL