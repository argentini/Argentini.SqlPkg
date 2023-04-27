using System.Data;
using Microsoft.Data.SqlClient;

namespace Argentini.SqlPkg.Extensions;

/// <summary>
/// Various tools to make using SqlConnection and SqlDataReader more bearable. 
/// </summary>
public static class SqlTools
{
	#region Data Helpers

	public static async Task PurgeDatabase(Settings settings)
	{
		var builder = new SqlConnectionStringBuilder(settings.TargetConnectionString)
		{
			InitialCatalog = "master"
		};

		using (var sqlReader = new SqlReader(new SqlReaderConfiguration
		       {
			       ConnectionString = builder.ToString(),
			       CommandText = @$"
if not exists (
    select [name]
        from sys.databases
        where [name] = N'{settings.TargetDatabaseName}'
)
    select 0
else
    select 1
"
		       }))
		{
			await using (await sqlReader.ExecuteReaderAsync())
			{
				sqlReader.Read();
				
				if (await sqlReader.SafeGetIntAsync(0) == 0)
				{
					// Create Database

					Console.WriteLine(
						$"=> Creating Database [{settings.TargetDatabaseName}] on {settings.TargetServerName}...");
					
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = builder.ToString(),
						CommandText = $@"
if not exists (
    select [name]
        from sys.databases
        where [name] = N'{settings.TargetDatabaseName}'
)
	create database [{settings.TargetDatabaseName}]
"
					});
					
					Console.WriteLine("=> Database Created");
				}

				else
				{
					// Purge Existing Database

					Console.WriteLine(
						$"=> Purging Database [{settings.TargetDatabaseName}] on {settings.TargetServerName}...");

					Console.WriteLine("=> Setting Single User Mode...");
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = settings.TargetConnectionString,
						CommandText = @"
-- Switch to single user mode

DECLARE @sqlprep NVARCHAR(max)
SET @sqlprep = 'ALTER DATABASE ' + quotename(db_name()) + ' SET SINGLE_USER WITH ROLLBACK IMMEDIATE'
EXEC sp_executesql @sqlprep
"
					});

					Console.WriteLine("=> Dropping Extended Properties...");
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = settings.TargetConnectionString,
						CommandText = @"
-- Drop all extended properties

DECLARE @rows int
SET @rows = (SELECT COUNT(*) FROM sys.extended_properties WHERE class = 0)

IF @rows > 0
BEGIN
	DECLARE @sql NVARCHAR(max)
	SET @sql = ''
	SELECT @sql += 'sp_dropextendedproperty @name = ' + QUOTENAME(ep.name, '''') + ';'
	FROM sys.extended_properties AS ep
	WHERE class=0

	EXEC sp_executesql @sql
END
"
					});

					Console.WriteLine("=> Dropping Triggers...");
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = settings.TargetConnectionString,
						CommandText = @"
-- Drop all triggers

DECLARE @dynsql NVARCHAR(max)
SET @dynsql = ''
SELECT @dynsql += ' DROP TRIGGER ' + 
	CASE t.parent_class_desc
		WHEN 'DATABASE'
			THEN QUOTENAME(t.NAME) + ' ON DATABASE; '
		ELSE QUOTENAME(s.NAME) + '.' + QUOTENAME(t.NAME) + '; '
	END
FROM sys.triggers t
LEFT OUTER JOIN sys.tables tables
ON tables.object_id = t.parent_id
LEFT OUTER JOIN sys.schemas s
ON tables.[schema_id] = s.[schema_id]
ORDER BY t.parent_id

EXEC sp_executesql @dynsql
"
					});

					Console.WriteLine("=> Dropping Foreign Keys...");
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = settings.TargetConnectionString,
						CommandText = @"
DECLARE @sql NVARCHAR(max)
SET @sql = ''
SELECT @sql += ' ALTER TABLE ' + QUOTENAME(s.NAME) + '.' + QUOTENAME(t.NAME) + ' DROP CONSTRAINT ' + QUOTENAME(tc.CONSTRAINT_NAME) + ';'
FROM sys.tables t
JOIN sys.schemas s
ON t.[schema_id] = s.[schema_id]
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS as tc
ON tc.TABLE_SCHEMA = s.name AND tc.TABLE_NAME = t.name
WHERE t.type = 'U' AND CONSTRAINT_TYPE = 'FOREIGN KEY'

EXEC sp_executesql @sql
"
					});

					Console.WriteLine("=> Dropping Fulltext Catalogs...");
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = settings.TargetConnectionString,
						CommandText = @"
-- Drop fulltext indexes and catalogs

DECLARE @Catalog NVARCHAR(128),
		@SQL NVARCHAR(MAX),
		@SQL2 NVARCHAR(MAX),
		@COLS NVARCHAR(4000),
		@Owner NVARCHAR(128),
		@Table NVARCHAR(128),
		@ObjectID INT,
		@AccentOn BIT,
		@CatalogID INT,
		@IndexID INT,
		@Max_objectId INT,
		@NL CHAR(2),
		@i int

SET @i = 1;

-- Cursor to fetch the name of catalogs one by one for the current database

declare FTCur cursor for SELECT Name
FROM sys.fulltext_catalogs
	ORDER BY NAME

OPEN FTCur
 

FETCH FTCur INTO @Catalog

WHILE @@FETCH_status >= 0

BEGIN

SET @i = @i + 1;

SELECT
	@NL = CHAR(13) + CHAR(10)

-- Check catalog exists
IF EXISTS
(
	SELECT Name
	FROM sys.fulltext_catalogs
	WHERE Name = @Catalog
) BEGIN
		-- Store the catalog details
		SELECT
			@CatalogID = i.fulltext_catalog_id
			,@ObjectID = 0
			,@Max_objectId = MAX(object_id)
			,@AccentOn = is_accent_sensitivity_on
		FROM sys.fulltext_index_catalog_usages AS i
		JOIN sys.fulltext_catalogs c
			ON i.fulltext_catalog_id = c.fulltext_catalog_id
		WHERE c.Name = @Catalog
		GROUP BY	i.fulltext_catalog_id
					,is_accent_sensitivity_on

		-- Script out catalog
		SET @SQL2 = 'DROP FULLTEXT CATALOG ' + QUOTENAME(@Catalog) + @NL

		END

		DECLARE FTObject CURSOR FOR SELECT	MIN(i.object_id) objectId
									,u.name AS schemaName
									,t.Name
									,unique_index_id
									,c.name as catalogueName
		FROM sys.tables AS t
		JOIN sys.schemas AS u
			ON u.schema_id = t.schema_id
		JOIN sys.fulltext_indexes i
			ON t.object_id = i.object_id
		JOIN sys.fulltext_catalogs c
			ON i.fulltext_catalog_id = c.fulltext_catalog_id
		WHERE 1 = 1 
		AND c.Name = @Catalog
		--AND i.object_id > @ObjectID
		GROUP BY	u.name
					,t.Name
					,unique_index_id
					,c.name

		OPEN FTObject

		FETCH FTObject INTO @ObjectID, @Owner, @Table, @IndexID, @Catalog
		-- Loop through all fulltext indexes within catalog

				WHILE @@FETCH_status >= 0 
				BEGIN
		
					-- Script Fulltext Index
					SELECT
						@COLS = NULL
						,@SQL = 'DROP FULLTEXT INDEX ON ' + QUOTENAME(@Owner) + '.' + QUOTENAME(@Table) + @NL

					-- SELECT @SQL
EXEC sp_executesql @SQL

					FETCH FTObject INTO @ObjectID, @Owner, @Table, @IndexID, @Catalog
				END

EXEC sp_executesql @SQL2

		CLOSE FTObject;
		DEALLOCATE FTObject;
FETCH FTCur INTO @catalog
END
CLOSE FTCur
DEALLOCATE FTCur
"
					});

					Console.WriteLine("=> Dropping Table Indexes...");
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = settings.TargetConnectionString,
						CommandText = @"
-- Drop all table indexes

DECLARE @SchemaName VARCHAR(256)
DECLARE @TableName VARCHAR(256)
DECLARE @IndexName VARCHAR(256)
DECLARE @TSQLDropIndex NVARCHAR(MAX)

DECLARE CursorIndexes CURSOR FOR
    SELECT
        (CASE WHEN schema_id IS NULL THEN SCHEMA_NAME(VIEW_SCHEMA_ID) ELSE SCHEMA_NAME(schema_id) END) AS SCHEMA_NAME,
        (CASE WHEN schema_id IS NULL THEN VIEW_NAME ELSE TABLE_NAME END) AS OBJECT_NAME,
        final.name AS INDEX_NAME
    FROM (
        SELECT I.*, T.schema_id, T.name AS 'TABLE_NAME', V.schema_id AS 'VIEW_SCHEMA_ID', V.name AS 'VIEW_NAME', X.xml_index_type, X.secondary_type_desc, X2.name AS 'XML_PARENT_NAME', ST.no_recompute, DS.name AS 'DS_NAME', IncludedColumns
        FROM sys.indexes I
        LEFT JOIN sys.objects SO ON I.object_id = SO.object_id
        LEFT JOIN sys.tables T ON  T.object_id = I.object_id
        LEFT JOIN sys.views V ON  V.object_id = I.object_id
        LEFT JOIN sys.xml_indexes X ON I.index_id = X.index_id AND X.object_id = I.object_id
        LEFT JOIN sys.xml_indexes X2 ON X2.index_id = X.using_xml_index_id AND X2.object_id = I.object_id
        LEFT JOIN sys.stats ST ON  ST.object_id = I.object_id AND ST.stats_id = I.index_id
        LEFT JOIN sys.data_spaces DS ON  I.data_space_id = DS.data_space_id
        LEFT JOIN sys.filegroups FG ON  I.data_space_id = FG.data_space_id
        LEFT JOIN (
            SELECT *
            FROM
                (SELECT IC2.object_id, IC2.index_id,
                    STUFF((SELECT ', ' + C.name
                    FROM sys.index_columns IC1 INNER JOIN
                        sys.columns C ON C.object_id = IC1.object_id
                            AND C.column_id = IC1.column_id
                            AND IC1.is_included_column = 1
                    WHERE  IC1.object_id = IC2.object_id AND IC1.index_id = IC2.index_id
                    GROUP BY IC1.object_id, C.name, index_id
                    FOR XML PATH('')
                ), 1, 2, '') as IncludedColumns
            FROM sys.index_columns IC2
            GROUP BY IC2.object_id, IC2.index_id) tmp1
            WHERE IncludedColumns IS NOT NULL
        ) tmp2 ON tmp2.object_id = I.object_id AND tmp2.index_id = I.index_id
        WHERE I.is_unique_constraint = 0 AND I.is_primary_key = 0 AND I.type <> 0 AND SO.type <> 'IT' AND SO.type <> 'S' AND SO.is_ms_shipped = 0 --AND SCHEMA_NAME(T.schema_id) <> 'sys'
    ) final
    ORDER BY final.name, final.xml_index_type DESC

OPEN CursorIndexes
FETCH NEXT FROM CursorIndexes INTO @SchemaName,@TableName,@IndexName

SET @TSQLDropIndex = '';

WHILE @@fetch_status = 0
BEGIN
	SET @TSQLDropIndex = @TSQLDropIndex + 'IF EXISTS (SELECT * FROM sys.indexes WHERE name = ''' + @IndexName + ''')' + CHAR(13) + CHAR(10) + '  DROP INDEX ' + QUOTENAME(@IndexName) + ' ON ' + QUOTENAME(@SchemaName)+ '.' + QUOTENAME(@TableName) + ';' + CHAR(13)
	--PRINT @TSQLDropIndex
	FETCH NEXT FROM CursorIndexes INTO @SchemaName,@TableName,@IndexName
END

CLOSE CursorIndexes
DEALLOCATE CursorIndexes 

IF @TSQLDropIndex <> '' BEGIN
	--PRINT @TSQLDropIndex
	EXEC sp_executesql @TSQLDropIndex
END
"
					});

					Console.WriteLine("=> Dropping Table Check Constraints...");
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = settings.TargetConnectionString,
						CommandText = @"
-- Drop all table check constraints

DECLARE @sql NVARCHAR(max)
SET @sql = ''
SELECT @sql += ' ALTER TABLE ' + QUOTENAME(s.NAME) + '.' + QUOTENAME(t.NAME) + ' DROP CONSTRAINT ' + tc.CONSTRAINT_NAME + ';'
FROM sys.tables t
JOIN sys.schemas s
ON t.[schema_id] = s.[schema_id]
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS as tc
ON tc.TABLE_SCHEMA = s.name AND tc.TABLE_NAME = t.name
WHERE t.type = 'U' AND CONSTRAINT_TYPE = 'CHECK'

EXEC sp_executesql @sql
"
					});

					Console.WriteLine("=> Dropping Views...");
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = settings.TargetConnectionString,
						CommandText = @"
-- Drop all views

DECLARE @dynsql NVARCHAR(max)
SET @dynsql = ''
SELECT @dynsql += ' DROP VIEW ' + QUOTENAME(s.NAME) + '.' + QUOTENAME(t.NAME) + '; '
FROM sys.views t
JOIN sys.schemas s
ON t.[schema_id] = s.[schema_id]
WHERE s.NAME <> 'sys'

EXEC sp_executesql @dynsql
"
					});

					Console.WriteLine("=> Dropping Stored Procedures...");
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = settings.TargetConnectionString,
						CommandText = @"
-- Drop all stored procedures

DECLARE @dynsql NVARCHAR(max)
SET @dynsql = ''
SELECT @dynsql += ' DROP PROCEDURE ' + QUOTENAME(s.NAME) + '.' + QUOTENAME(t.NAME) + '; '
FROM sys.procedures t
JOIN sys.schemas s
ON t.[schema_id] = s.[schema_id]

EXEC sp_executesql @dynsql
"
					});

					Console.WriteLine("=> Dropping Table Primary Keys...");
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = settings.TargetConnectionString,
						CommandText = @"
-- Drop all table primary keys

DECLARE @sql NVARCHAR(max)
SET @sql = ''
SELECT @sql += ' ALTER TABLE ' + QUOTENAME(s.NAME) + '.' + QUOTENAME(t.NAME) + ' DROP CONSTRAINT ' + QUOTENAME(tc.CONSTRAINT_NAME) + ';'
FROM sys.tables t
JOIN sys.schemas s
ON t.[schema_id] = s.[schema_id]
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS as tc
ON tc.TABLE_SCHEMA = s.name AND tc.TABLE_NAME = t.name
WHERE t.type = 'U' AND CONSTRAINT_TYPE = 'PRIMARY KEY'

EXEC sp_executesql @sql
"
					});

					Console.WriteLine("=> Dropping Tables...");
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = settings.TargetConnectionString,
						CommandText = @"
-- Drop all tables

DECLARE @sql NVARCHAR(max)
SET @sql = ''
SELECT @sql += ' DROP TABLE ' + QUOTENAME(s.NAME) + '.' + QUOTENAME(t.NAME) + '; ' + CHAR(13) + CHAR(10)
FROM sys.tables t
JOIN sys.schemas s
ON t.[schema_id] = s.[schema_id]
WHERE  t.type = 'U'
AND 1=1

EXEC sp_executesql @sql
"
					});

					Console.WriteLine("=> Dropping User-Defined Table Types...");
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = settings.TargetConnectionString,
						CommandText = @"
-- Drop all user-defined data types

DECLARE @dynsql NVARCHAR(max)
SET @dynsql = ''
SELECT @dynsql += 'DROP TYPE ' + QUOTENAME(s.NAME) + '.' + QUOTENAME(t.NAME) + '; '
FROM sys.types t
JOIN sys.schemas s
ON t.[schema_id] = s.[schema_id]
WHERE s.[name]<>'sys' AND t.is_table_type=1

EXEC sp_executesql @dynsql
"
					});

					Console.WriteLine("=> Dropping User-Defined Data Types...");
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = settings.TargetConnectionString,
						CommandText = @"
-- Drop all user-defined data types

DECLARE @dynsql NVARCHAR(max)
SET @dynsql = ''
SELECT @dynsql += 'DROP TYPE ' + QUOTENAME(s.NAME) + '.' + QUOTENAME(t.NAME) + '; '
FROM sys.types t
JOIN sys.schemas s
ON t.[schema_id] = s.[schema_id]
WHERE s.[name]<>'sys' AND t.is_table_type=0

EXEC sp_executesql @dynsql
"
					});

					Console.WriteLine("=> Dropping User-Defined Functions...");
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = settings.TargetConnectionString,
						CommandText = @"
-- Drop all user-defined functions

DECLARE @dynsql NVARCHAR(max)
SET @dynsql = ''
SELECT @dynsql += 'DROP FUNCTION ' + QUOTENAME(s.NAME) + '.' + QUOTENAME(t.NAME) + '; '
FROM (SELECT * FROM sys.objects WHERE [type] IN ('FN', 'IF', 'TF', 'FS')) t
JOIN sys.schemas s
ON t.[schema_id] = s.[schema_id]

EXEC sp_executesql @dynsql
"
					});

					Console.WriteLine("=> Dropping XML Schema Collections...");
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = settings.TargetConnectionString,
						CommandText = @"
-- Drop XML schema collections

DECLARE @dynsql NVARCHAR(max)
SET @dynsql = ''
SELECT @dynsql += 'DROP XML SCHEMA COLLECTION ' + QUOTENAME(ss.NAME) + '.' + QUOTENAME(xsc.NAME) + '; '
FROM sys.xml_schema_collections xsc
INNER JOIN (
	SELECT
		*
	FROM
		sys.schemas
) ss ON xsc.schema_id = ss.schema_id
WHERE ss.name <> 'sys'

EXEC sp_executesql @dynsql
"
					});

					Console.WriteLine("=> Dropping Default Types...");
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = settings.TargetConnectionString,
						CommandText = @"
-- Drop default types

DECLARE @sql NVARCHAR(max)
SET @sql = ''
SELECT @sql += 'DROP DEFAULT ' + QUOTENAME(schema_name(so.schema_id)) + '.' + QUOTENAME(so.name) + ';'
FROM sys.objects so
WHERE so.type = 'D' AND so.parent_object_id = 0

EXEC sp_executesql @sql
"
					});

					Console.WriteLine("=> Dropping User Schemas...");
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = settings.TargetConnectionString,
						CommandText = @"
-- Drop user schemas

DECLARE @sql NVARCHAR(max)
SET @sql = ''
SELECT @sql += ' DROP SCHEMA ' + QUOTENAME(s.name) + '; '
FROM sys.schemas s
    INNER JOIN sys.sysusers u
        ON u.uid = s.principal_id
WHERE 
u.islogin = 1 AND
u.name NOT IN ('sys', 'guest', 'INFORMATION_SCHEMA') AND s.name <> 'dbo'

EXEC sp_executesql @sql
"
					});

					Console.WriteLine("=> Restoring multi-user mode...");
					await Sql.ExecuteAsync(new SqlExecuteSettings
					{
						ConnectionString = settings.TargetConnectionString,
						CommandText = @"
-- Switch to multi user mode

DECLARE @sqlfinish2 NVARCHAR(max)
SET @sqlfinish2 = 'ALTER DATABASE ' + quotename(db_name()) + ' SET MULTI_USER'
EXEC sp_executesql @sqlfinish2
"
					});

					Console.WriteLine("=> Database Purge Complete");
				}
			}
		}
	}
	
	/// <summary>
	/// Create a list of user table names from a connection string.
	/// </summary>
	/// <param name="connectionString"></param>
	/// <returns></returns>
	public static async Task<List<string>> LoadUserTableNames(string connectionString)
	{
		var tableNameList = new List<string>();
		
		using (var sqlReader = new SqlReader(new SqlReaderConfiguration
		       {
			       ConnectionString = connectionString,
			       CommandText = @"
select
	schema_name(schema_id) as [SCHEMA_NAME]
	,[Tables].name as [TABLE_NAME]
	,[Tables].is_memory_optimized as [TABLE_IS_MEMORY_OPTIMIZED]
	,[Tables].durability as [TABLE_DURABILITY]
	,[Tables].durability_desc as [TABLE_DURABILITY_DESC]
from
    sys.tables as [Tables]
where
    [Tables].is_ms_shipped = 0
group by
    schema_name(schema_id), [Tables].name, [Tables].is_memory_optimized, [Tables].durability, [Tables].durability_desc
order by
    [SCHEMA_NAME] asc, [TABLE_NAME] asc;
"
		       }))
		{
			await using (await sqlReader.ExecuteReaderAsync())
			{
				if (sqlReader.HasRows == false)
					return tableNameList;
				
				while (sqlReader.Read())
				{
					var schemaName = await sqlReader.SafeGetStringAsync("SCHEMA_NAME");
					var tableName = await sqlReader.SafeGetStringAsync("TABLE_NAME");

					tableNameList.Add($"[{schemaName}].[{tableName}]");
				}
			}
		}

		return tableNameList;
	}
	
	#endregion
	
	#region String Helpers
	
	/// <summary>
	/// Normalize a table name to be in the format [schema].[table].
	/// </summary>
	/// <param name="tableName"></param>
	/// <returns></returns>
	public static string NormalizeTableName(this string tableName)
	{
		if (string.IsNullOrEmpty(tableName))
			return string.Empty;
		
		var splits = tableName.Split('.', StringSplitOptions.RemoveEmptyEntries);
		var normalized = string.Empty;

		foreach (var segment in splits)
			normalized += $"[{segment.Trim('[').Trim(']')}].";

		normalized = normalized.TrimEnd('.');

		if (normalized.Contains('.') == false)
			return $"[dbo].{normalized}";
		
		return normalized;
	}
	
	#endregion
	
	#region Column Exists
	
	/// <summary>
	/// Determine if a column exists in an open SqlDataReader.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="columnName"></param>
	/// <returns></returns>
	public static bool ColumnExists(this SqlDataReader sqlDataReader, string columnName)
	{
		if (columnName.IsEmpty()) return false;
		if (sqlDataReader.IsClosed) return false;
		if (sqlDataReader.FieldCount == 0) return false;

		for (var i = 0; i < sqlDataReader.FieldCount; i++)
		{
			if (sqlDataReader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
				return true;
		}

		return false;
	}

	/// <summary>
	/// Determine if a column exists in an open SqlDataReader.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="index"></param>
	/// <returns></returns>
	public static bool ColumnExists(this SqlDataReader sqlDataReader, int index)
	{
		if (index < 0) return false;
		if (sqlDataReader.IsClosed) return false;
		if (sqlDataReader.FieldCount == 0) return false;
		if (index < sqlDataReader.FieldCount) return true;

		return false;
	}
	
	#endregion
	
	#region Column Name
	
	/// <summary>
	/// Get the column index for a named column in an open SqlDataReader.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="columnName"></param>
	/// <returns></returns>
	public static int ColumnIndex(this SqlDataReader sqlDataReader, string columnName)
	{
		if (columnName.IsEmpty()) return -1;
		if (sqlDataReader.IsClosed) return -1;
			
		for (var i = 0; i < sqlDataReader.FieldCount; i++)
		{
			if (sqlDataReader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
				return i;
		}

		return -1;
	}
	
	#endregion

	#region SafeGetString
	
	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<string> SafeGetStringAsync(this SqlDataReader sqlDataReader, string columnName, string defaultValue = "")
	{
		if (sqlDataReader.ColumnExists(columnName))
		{
			if (await sqlDataReader.IsDBNullAsync(columnName))
				return defaultValue;
			
			return sqlDataReader.GetValue(columnName).ToString() ?? defaultValue;
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static string SafeGetString(this SqlDataReader sqlDataReader, string columnName, string defaultValue = "")
	{
		if (sqlDataReader.ColumnExists(columnName))
		{
			if (sqlDataReader.IsDBNull(columnName))
				return defaultValue;
			
			return sqlDataReader.GetValue(columnName).ToString() ?? defaultValue;
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<string> SafeGetStringAsync(this SqlDataReader sqlDataReader, int index, string defaultValue = "")
	{
		if (sqlDataReader.ColumnExists(index))
		{
			if (await sqlDataReader.IsDBNullAsync(index))
				return defaultValue;
			
			return sqlDataReader.GetValue(index).ToString() ?? defaultValue;
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static string SafeGetString(this SqlDataReader sqlDataReader, int index, string defaultValue = "")
	{
		if (sqlDataReader.ColumnExists(index))
		{
			if (sqlDataReader.IsDBNull(index))
				return defaultValue;
			
			return sqlDataReader.GetValue(index).ToString() ?? defaultValue;
		}

		return defaultValue;
	}
	
	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<string> SafeGetStringAsync(this SqlReader sqlReader, string columnName, string defaultValue = "")
	{
		return sqlReader.SqlDataReader is not null ? await sqlReader.SqlDataReader.SafeGetStringAsync(columnName, defaultValue) : defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static string SafeGetString(this SqlReader sqlReader, string columnName, string defaultValue = "")
	{
		return sqlReader.SqlDataReader is not null ? sqlReader.SqlDataReader.SafeGetString(columnName, defaultValue) : defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<string> SafeGetStringAsync(this SqlReader sqlReader, int index, string defaultValue = "")
	{
		return sqlReader.SqlDataReader is not null ? await sqlReader.SqlDataReader.SafeGetStringAsync(index, defaultValue) : defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static string SafeGetString(this SqlReader sqlReader, int index, string defaultValue = "")
	{
		return sqlReader.SqlDataReader is not null ? sqlReader.SqlDataReader.SafeGetString(index, defaultValue) : defaultValue;
	}
	
	#endregion

	#region SafeGetGuid
	
	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<Guid> SafeGetGuidAsync(this SqlDataReader sqlDataReader, string columnName, Guid defaultValue = new ())
	{
		if (sqlDataReader.ColumnExists(columnName))
		{
			if (await sqlDataReader.IsDBNullAsync(columnName))
				return defaultValue;

			if (sqlDataReader.GetFieldType(columnName) == typeof(Guid))
				return sqlDataReader.GetFieldValue<Guid?>(columnName) ?? defaultValue;
			
			return Guid.Parse(sqlDataReader.GetValue(columnName).ToString() ?? defaultValue.ToString());
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static Guid SafeGetGuid(this SqlDataReader sqlDataReader, string columnName, Guid defaultValue = new ())
	{
		if (sqlDataReader.ColumnExists(columnName))
		{
			if (sqlDataReader.IsDBNull(columnName))
				return defaultValue;

			if (sqlDataReader.GetFieldType(columnName) == typeof(Guid))
				return sqlDataReader.GetFieldValue<Guid?>(columnName) ?? defaultValue;
			
			return Guid.Parse(sqlDataReader.GetValue(columnName).ToString() ?? defaultValue.ToString());
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<Guid> SafeGetGuidAsync(this SqlDataReader sqlDataReader, int index, Guid defaultValue = new ())
	{
		if (sqlDataReader.ColumnExists(index))
		{
			if (await sqlDataReader.IsDBNullAsync(index))
				return defaultValue;

			if (sqlDataReader.GetFieldType(index) == typeof(Guid))
				return sqlDataReader.GetFieldValue<Guid?>(index) ?? defaultValue;
			
			return Guid.Parse(sqlDataReader.GetValue(index).ToString() ?? defaultValue.ToString());
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static Guid SafeGetGuid(this SqlDataReader sqlDataReader, int index, Guid defaultValue = new ())
	{
		if (sqlDataReader.ColumnExists(index))
		{
			if (sqlDataReader.IsDBNull(index))
				return defaultValue;

			if (sqlDataReader.GetFieldType(index) == typeof(Guid))
				return sqlDataReader.GetFieldValue<Guid?>(index) ?? defaultValue;
			
			return Guid.Parse(sqlDataReader.GetValue(index).ToString() ?? defaultValue.ToString());
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<Guid> SafeGetGuidAsync(this SqlReader sqlReader, string columnName, Guid defaultValue = new ())
	{
		return sqlReader.SqlDataReader is not null ? await sqlReader.SqlDataReader.SafeGetGuidAsync(columnName, defaultValue) : defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static Guid SafeGetGuid(this SqlReader sqlReader, string columnName, Guid defaultValue = new ())
	{
		return sqlReader.SqlDataReader?.SafeGetGuid(columnName, defaultValue) ?? defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<Guid> SafeGetGuidAsync(this SqlReader sqlReader, int index, Guid defaultValue = new ())
	{
		return sqlReader.SqlDataReader is not null ? await sqlReader.SqlDataReader.SafeGetGuidAsync(index, defaultValue) : defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static Guid SafeGetGuid(this SqlReader sqlReader, int index, Guid defaultValue = new ())
	{
		return sqlReader.SqlDataReader?.SafeGetGuid(index, defaultValue) ?? defaultValue;
	}
	
	#endregion

	#region SafeGetInt
	
	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<int> SafeGetIntAsync(this SqlDataReader sqlDataReader, string columnName, int defaultValue = 0)
	{
		if (sqlDataReader.ColumnExists(columnName))
		{
			if (await sqlDataReader.IsDBNullAsync(columnName))
				return defaultValue;
			
			return Convert.ToInt32(sqlDataReader.GetValue(columnName));
		}

		return defaultValue;
	}
	
	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static int SafeGetInt(this SqlDataReader sqlDataReader, string columnName, int defaultValue = 0)
	{
		if (sqlDataReader.ColumnExists(columnName))
		{
			if (sqlDataReader.IsDBNull(columnName))
				return defaultValue;
			
			return Convert.ToInt32(sqlDataReader.GetValue(columnName));
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<int> SafeGetIntAsync(this SqlDataReader sqlDataReader, int index, int defaultValue = 0)
	{
		if (sqlDataReader.ColumnExists(index))
		{
			if (await sqlDataReader.IsDBNullAsync(index))
				return defaultValue;
			
			return Convert.ToInt32(sqlDataReader.GetValue(index));
		}

		return defaultValue;
	}
	
	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static int SafeGetInt(this SqlDataReader sqlDataReader, int index, int defaultValue = 0)
	{
		if (sqlDataReader.ColumnExists(index))
		{
			if (sqlDataReader.IsDBNull(index))
				return defaultValue;
			
			return Convert.ToInt32(sqlDataReader.GetValue(index));
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<int> SafeGetIntAsync(this SqlReader sqlReader, string columnName, int defaultValue = 0)
	{
		return sqlReader.SqlDataReader is not null ? await sqlReader.SqlDataReader.SafeGetIntAsync(columnName, defaultValue) : defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static int SafeGetInt(this SqlReader sqlReader, string columnName, int defaultValue = 0)
	{
		return sqlReader.SqlDataReader?.SafeGetInt(columnName, defaultValue) ?? defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<int> SafeGetIntAsync(this SqlReader sqlReader, int index, int defaultValue = 0)
	{
		return sqlReader.SqlDataReader is not null ? await sqlReader.SqlDataReader.SafeGetIntAsync(index, defaultValue) : defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static int SafeGetInt(this SqlReader sqlReader, int index, int defaultValue = 0)
	{
		return sqlReader.SqlDataReader?.SafeGetInt(index, defaultValue) ?? defaultValue;
	}
	
	#endregion
	
	#region SafeGetLong
	
	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<long> SafeGetLongAsync(this SqlDataReader sqlDataReader, string columnName, long defaultValue = 0)
	{
		if (sqlDataReader.ColumnExists(columnName))
		{
			if (await sqlDataReader.IsDBNullAsync(columnName))
				return defaultValue;
			
			return Convert.ToInt64(sqlDataReader.GetValue(columnName));
		}

		return defaultValue;
	}
	
	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static long SafeGetLong(this SqlDataReader sqlDataReader, string columnName, long defaultValue = 0)
	{
		if (sqlDataReader.ColumnExists(columnName))
		{
			if (sqlDataReader.IsDBNull(columnName))
				return defaultValue;
			
			return Convert.ToInt64(sqlDataReader.GetValue(columnName));
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<long> SafeGetLongAsync(this SqlDataReader sqlDataReader, int index, long defaultValue = 0)
	{
		if (sqlDataReader.ColumnExists(index))
		{
			if (await sqlDataReader.IsDBNullAsync(index))
				return defaultValue;
			
			return Convert.ToInt64(sqlDataReader.GetValue(index));
		}

		return defaultValue;
	}
	
	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static long SafeGetLong(this SqlDataReader sqlDataReader, int index, long defaultValue = 0)
	{
		if (sqlDataReader.ColumnExists(index))
		{
			if (sqlDataReader.IsDBNull(index))
				return defaultValue;
			
			return Convert.ToInt64(sqlDataReader.GetValue(index));
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<long> SafeGetLongAsync(this SqlReader sqlReader, string columnName, long defaultValue = 0)
	{
		return sqlReader.SqlDataReader is not null ? await sqlReader.SqlDataReader.SafeGetLongAsync(columnName, defaultValue) : defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static long SafeGetLong(this SqlReader sqlReader, string columnName, long defaultValue = 0)
	{
		return sqlReader.SqlDataReader?.SafeGetLong(columnName, defaultValue) ?? defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<long> SafeGetLongAsync(this SqlReader sqlReader, int index, long defaultValue = 0)
	{
		return sqlReader.SqlDataReader is not null ? await sqlReader.SqlDataReader.SafeGetLongAsync(index, defaultValue) : defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static long SafeGetLong(this SqlReader sqlReader, int index, long defaultValue = 0)
	{
		return sqlReader.SqlDataReader?.SafeGetLong(index, defaultValue) ?? defaultValue;
	}
	
	#endregion
	
	#region SafeGetDouble
	
	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<double> SafeGetDoubleAsync(this SqlDataReader sqlDataReader, string columnName, double defaultValue = 0)
	{
		if (sqlDataReader.ColumnExists(columnName))
		{
			if (await sqlDataReader.IsDBNullAsync(columnName))
				return defaultValue;
			
			return Convert.ToDouble(sqlDataReader.GetValue(columnName));
		}

		return defaultValue;
	}
	
	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static double SafeGetDouble(this SqlDataReader sqlDataReader, string columnName, double defaultValue = 0)
	{
		if (sqlDataReader.ColumnExists(columnName))
		{
			if (sqlDataReader.IsDBNull(columnName))
				return defaultValue;
			
			return Convert.ToDouble(sqlDataReader.GetValue(columnName));
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<double> SafeGetDoubleAsync(this SqlDataReader sqlDataReader, int index, double defaultValue = 0)
	{
		if (sqlDataReader.ColumnExists(index))
		{
			if (await sqlDataReader.IsDBNullAsync(index))
				return defaultValue;
			
			return Convert.ToDouble(sqlDataReader.GetValue(index));
		}

		return defaultValue;
	}
	
	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static double SafeGetDouble(this SqlDataReader sqlDataReader, int index, double defaultValue = 0)
	{
		if (sqlDataReader.ColumnExists(index))
		{
			if (sqlDataReader.IsDBNull(index))
				return defaultValue;
			
			return Convert.ToDouble(sqlDataReader.GetValue(index));
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<double> SafeGetDoubleAsync(this SqlReader sqlReader, string columnName, double defaultValue = 0)
	{
		return sqlReader.SqlDataReader is not null ? await sqlReader.SqlDataReader.SafeGetDoubleAsync(columnName, defaultValue) : defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static double SafeGetDouble(this SqlReader sqlReader, string columnName, double defaultValue = 0)
	{
		return sqlReader.SqlDataReader?.SafeGetDouble(columnName, defaultValue) ?? defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<double> SafeGetDoubleAsync(this SqlReader sqlReader, int index, double defaultValue = 0)
	{
		return sqlReader.SqlDataReader is not null ? await sqlReader.SqlDataReader.SafeGetDoubleAsync(index, defaultValue) : defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static double SafeGetDouble(this SqlReader sqlReader, int index, double defaultValue = 0)
	{
		return sqlReader.SqlDataReader?.SafeGetDouble(index, defaultValue) ?? defaultValue;
	}
	
	#endregion
	
	#region SafeGetDateTimeOffset
	
	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<DateTimeOffset> SafeGetDateTimeOffsetAsync(this SqlDataReader sqlDataReader, string columnName, DateTimeOffset defaultValue = new ())
	{
		if (sqlDataReader.ColumnExists(columnName))
		{
			if (await sqlDataReader.IsDBNullAsync(columnName))
				return defaultValue;

			if (sqlDataReader.GetFieldType(columnName) == typeof(DateTimeOffset))
				return sqlDataReader.GetFieldValue<DateTimeOffset?>(columnName) ?? defaultValue;

			if (sqlDataReader.GetFieldType(columnName) == typeof(DateTime))
				return sqlDataReader.GetFieldValue<DateTime?>(columnName) ?? defaultValue;
			
			return DateTimeOffset.Parse(sqlDataReader.GetValue(columnName).ToString() ?? defaultValue.ToString());
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static DateTimeOffset SafeGetDateTimeOffset(this SqlDataReader sqlDataReader, string columnName, DateTimeOffset defaultValue = new ())
	{
		if (sqlDataReader.ColumnExists(columnName))
		{
			if (sqlDataReader.IsDBNull(columnName))
				return defaultValue;

			if (sqlDataReader.GetFieldType(columnName) == typeof(DateTimeOffset))
				return sqlDataReader.GetFieldValue<DateTimeOffset?>(columnName) ?? defaultValue;

			if (sqlDataReader.GetFieldType(columnName) == typeof(DateTime))
				return sqlDataReader.GetFieldValue<DateTime?>(columnName) ?? defaultValue;
			
			return DateTimeOffset.Parse(sqlDataReader.GetValue(columnName).ToString() ?? defaultValue.ToString());
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<DateTimeOffset> SafeGetDateTimeOffsetAsync(this SqlDataReader sqlDataReader, int index, DateTimeOffset defaultValue = new ())
	{
		if (sqlDataReader.ColumnExists(index))
		{
			if (await sqlDataReader.IsDBNullAsync(index))
				return defaultValue;

			if (sqlDataReader.GetFieldType(index) == typeof(DateTimeOffset))
				return sqlDataReader.GetFieldValue<DateTimeOffset?>(index) ?? defaultValue;

			if (sqlDataReader.GetFieldType(index) == typeof(DateTime))
				return sqlDataReader.GetFieldValue<DateTime?>(index) ?? defaultValue;
			
			return DateTimeOffset.Parse(sqlDataReader.GetValue(index).ToString() ?? defaultValue.ToString());
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static DateTimeOffset SafeGetDateTimeOffset(this SqlDataReader sqlDataReader, int index, DateTimeOffset defaultValue = new ())
	{
		if (sqlDataReader.ColumnExists(index))
		{
			if (sqlDataReader.IsDBNull(index))
				return defaultValue;

			if (sqlDataReader.GetFieldType(index) == typeof(DateTimeOffset))
				return sqlDataReader.GetFieldValue<DateTimeOffset?>(index) ?? defaultValue;

			if (sqlDataReader.GetFieldType(index) == typeof(DateTime))
				return sqlDataReader.GetFieldValue<DateTime?>(index) ?? defaultValue;
			
			return DateTimeOffset.Parse(sqlDataReader.GetValue(index).ToString() ?? defaultValue.ToString());
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<DateTimeOffset> SafeGetDateTimeOffsetAsync(this SqlReader sqlReader, string columnName, DateTimeOffset defaultValue = new ())
	{
		return sqlReader.SqlDataReader is not null ? await sqlReader.SqlDataReader.SafeGetDateTimeOffsetAsync(columnName, defaultValue) : defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static DateTimeOffset SafeGetDateTimeOffset(this SqlReader sqlReader, string columnName, DateTimeOffset defaultValue = new ())
	{
		return sqlReader.SqlDataReader?.SafeGetDateTimeOffset(columnName, defaultValue) ?? defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<DateTimeOffset> SafeGetDateTimeOffsetAsync(this SqlReader sqlReader, int index, DateTimeOffset defaultValue = new ())
	{
		return sqlReader.SqlDataReader is not null ? await sqlReader.SqlDataReader.SafeGetDateTimeOffsetAsync(index, defaultValue) : defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static DateTimeOffset SafeGetDateTimeOffset(this SqlReader sqlReader, int index, DateTimeOffset defaultValue = new ())
	{
		return sqlReader.SqlDataReader?.SafeGetDateTimeOffset(index, defaultValue) ?? defaultValue;
	}
	
	#endregion
	
	#region SafeGetBoolean
	
	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<bool> SafeGetBooleanAsync(this SqlDataReader sqlDataReader, string columnName, bool defaultValue = false)
	{
		if (sqlDataReader.ColumnExists(columnName))
		{
			if (await sqlDataReader.IsDBNullAsync(columnName))
				return defaultValue;

			if (sqlDataReader.GetFieldType(columnName) == typeof(bool))
				return sqlDataReader.GetFieldValue<bool>(columnName);

			if (sqlDataReader.GetFieldType(columnName) == typeof(int))
			{
				var value = sqlDataReader.GetFieldValue<int>(columnName);

				if (value is -1 or 1)
					return true;

				return false;
			}

			if (sqlDataReader.GetFieldType(columnName) == typeof(string))
			{
				var value = sqlDataReader.GetFieldValue<string>(columnName);

				if (value.ToLower() is "y" or "yes" or "true" or "t" or "on")
					return true;

				return false;
			}
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static bool SafeGetBoolean(this SqlDataReader sqlDataReader, string columnName, bool defaultValue = false)
	{
		if (sqlDataReader.ColumnExists(columnName))
		{
			if (sqlDataReader.IsDBNull(columnName))
				return defaultValue;

			if (sqlDataReader.GetFieldType(columnName) == typeof(bool))
				return sqlDataReader.GetFieldValue<bool>(columnName);

			if (sqlDataReader.GetFieldType(columnName) == typeof(int))
			{
				var value = sqlDataReader.GetFieldValue<int>(columnName);

				if (value is -1 or 1)
					return true;

				return false;
			}

			if (sqlDataReader.GetFieldType(columnName) == typeof(string))
			{
				var value = sqlDataReader.GetFieldValue<string>(columnName);

				if (value.ToLower() is "y" or "yes" or "true" or "t" or "on")
					return true;

				return false;
			}
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<bool> SafeGetBooleanAsync(this SqlDataReader sqlDataReader, int index, bool defaultValue = false)
	{
		if (sqlDataReader.ColumnExists(index))
		{
			if (await sqlDataReader.IsDBNullAsync(index))
				return defaultValue;

			if (sqlDataReader.GetFieldType(index) == typeof(bool))
				return sqlDataReader.GetFieldValue<bool>(index);

			if (sqlDataReader.GetFieldType(index) == typeof(int))
			{
				var value = sqlDataReader.GetFieldValue<int>(index);

				if (value is -1 or 1)
					return true;

				return false;
			}

			if (sqlDataReader.GetFieldType(index) == typeof(string))
			{
				var value = sqlDataReader.GetFieldValue<string>(index);

				if (value.ToLower() is "y" or "yes" or "true" or "t" or "on")
					return true;

				return false;
			}
		}

		return defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlDataReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static bool SafeGetBoolean(this SqlDataReader sqlDataReader, int index, bool defaultValue = false)
	{
		if (sqlDataReader.ColumnExists(index))
		{
			if (sqlDataReader.IsDBNull(index))
				return defaultValue;

			if (sqlDataReader.GetFieldType(index) == typeof(bool))
				return sqlDataReader.GetFieldValue<bool>(index);

			if (sqlDataReader.GetFieldType(index) == typeof(int))
			{
				var value = sqlDataReader.GetFieldValue<int>(index);

				if (value is -1 or 1)
					return true;

				return false;
			}

			if (sqlDataReader.GetFieldType(index) == typeof(string))
			{
				var value = sqlDataReader.GetFieldValue<string>(index);

				if (value.ToLower() is "y" or "yes" or "true" or "t" or "on")
					return true;

				return false;
			}
		}

		return defaultValue;
	}
	
	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<bool> SafeGetBooleanAsync(this SqlReader sqlReader, string columnName, bool defaultValue = false)
	{
		return sqlReader.SqlDataReader is not null ? await sqlReader.SqlDataReader.SafeGetBooleanAsync(columnName, defaultValue) : defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="columnName"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static bool SafeGetBoolean(this SqlReader sqlReader, string columnName, bool defaultValue = false)
	{
		return sqlReader.SqlDataReader?.SafeGetBoolean(columnName, defaultValue) ?? defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static async ValueTask<bool> SafeGetBooleanAsync(this SqlReader sqlReader, int index, bool defaultValue = false)
	{
		return sqlReader.SqlDataReader is not null ? await sqlReader.SqlDataReader.SafeGetBooleanAsync(index, defaultValue) : defaultValue;
	}

	/// <summary>
	/// Get a SqlDataReader column value or a default value if the column does not exist or is null.
	/// </summary>
	/// <param name="sqlReader"></param>
	/// <param name="index"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static bool SafeGetBoolean(this SqlReader sqlReader, int index, bool defaultValue = false)
	{
		return sqlReader.SqlDataReader?.SafeGetBoolean(index, defaultValue) ?? defaultValue;
	}
	
	#endregion
}
