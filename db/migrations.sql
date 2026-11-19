BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118235530_AddApplicantAccessToken'
)
BEGIN
    ALTER TABLE [DismissalCases] ADD [AccessTokenExpiresAt] datetime NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118235530_AddApplicantAccessToken'
)
BEGIN
    ALTER TABLE [DismissalCases] ADD [AccessTokenHash] varbinary(32) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118235530_AddApplicantAccessToken'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_AccessTokenHash] ON [DismissalCases] ([AccessTokenHash]) WHERE [AccessTokenHash] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118235530_AddApplicantAccessToken'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261118235530_AddApplicantAccessToken', N'10.0.15');
END;

COMMIT;
GO

