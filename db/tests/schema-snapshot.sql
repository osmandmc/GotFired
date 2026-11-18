-- TEST HELPER: prints a normalized description of the dbo schema (one line per fact) for diffing two databases.
SET NOCOUNT ON;
SELECT line FROM (SELECT CAST(line AS nvarchar(4000)) COLLATE Latin1_General_BIN2 AS line FROM (
    SELECT 'COL ' + t.name + '.' + c.name + ' ' + ty.name
           + CASE WHEN ty.name IN ('nvarchar','varchar','varbinary')
                  THEN '(' + CASE c.max_length WHEN -1 THEN 'max' ELSE CAST(c.max_length / CASE WHEN ty.name = 'nvarchar' THEN 2 ELSE 1 END AS varchar) END + ')' ELSE '' END
           + CASE c.is_nullable WHEN 1 THEN ' NULL' ELSE ' NOT NULL' END
           + CASE c.is_identity WHEN 1 THEN ' IDENTITY' ELSE '' END AS line
    FROM sys.tables t JOIN sys.columns c ON c.object_id = t.object_id JOIN sys.types ty ON ty.user_type_id = c.user_type_id
    WHERE t.name NOT IN ('__EFMigrationsHistory', '__MigrationHistory')
    UNION ALL
    SELECT CASE WHEN i.is_primary_key = 1 THEN 'PK ' WHEN i.is_unique = 1 THEN 'UIX ' ELSE 'IX ' END
           + t.name + ' ' + i.name + ' (' + STUFF((SELECT ',' + c2.name FROM sys.index_columns ic2
                 JOIN sys.columns c2 ON c2.object_id = ic2.object_id AND c2.column_id = ic2.column_id
                 WHERE ic2.object_id = i.object_id AND ic2.index_id = i.index_id ORDER BY ic2.key_ordinal
                 FOR XML PATH('')), 1, 1, '') + ')'
           + ISNULL(' WHERE ' + CAST(i.filter_definition AS nvarchar(4000)) COLLATE DATABASE_DEFAULT, '')
    FROM sys.tables t JOIN sys.indexes i ON i.object_id = t.object_id
    WHERE t.name NOT IN ('__EFMigrationsHistory', '__MigrationHistory') AND i.index_id > 0
    UNION ALL
    SELECT 'FK ' + OBJECT_NAME(fk.parent_object_id) + ' ' + fk.name + ' (' + COL_NAME(fkc.parent_object_id, fkc.parent_column_id)
           + ') -> ' + OBJECT_NAME(fk.referenced_object_id) + ' ON DELETE ' + fk.delete_referential_action_desc
    FROM sys.foreign_keys fk JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
) y) x ORDER BY line;
