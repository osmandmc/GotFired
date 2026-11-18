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
           + t.name + ' ' + i.name + ' (' + STRING_AGG(c.name, ',') WITHIN GROUP (ORDER BY ic.key_ordinal) + ')'
           + ISNULL(' WHERE ' + CAST(i.filter_definition AS nvarchar(4000)) COLLATE DATABASE_DEFAULT, '')
    FROM sys.tables t JOIN sys.indexes i ON i.object_id = t.object_id
    JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
    JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    WHERE t.name NOT IN ('__EFMigrationsHistory', '__MigrationHistory')
    GROUP BY t.name, i.name, i.is_primary_key, i.is_unique, i.filter_definition
    UNION ALL
    SELECT 'FK ' + OBJECT_NAME(fk.parent_object_id) + ' ' + fk.name + ' (' + COL_NAME(fkc.parent_object_id, fkc.parent_column_id)
           + ') -> ' + OBJECT_NAME(fk.referenced_object_id) + ' ON DELETE ' + fk.delete_referential_action_desc
    FROM sys.foreign_keys fk JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
) y) x ORDER BY line;
