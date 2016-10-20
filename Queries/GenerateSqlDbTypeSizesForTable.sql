USE master;

/* Make the sp if it doesn't already exist. Also acts as a work around for SQL Server requiring CREATE PROCEDURE to be the first statement in the batch */
IF OBJECT_ID('GenerateSqlDbTypeSizesForTable') IS NULL
	EXEC ('CREATE PROCEDURE [dbo].[GenerateSqlDbTypeSizesForTable] @tableName nvarchar(250) AS BEGIN PRINT(''Not implemented''); END');
GO

-- =============================================
-- Author:		Josh Keegan
-- Create date: 20/10/2016
-- Description:	Generates the SqlDbTypeSize constants for a given database table. 
--				For use with SqlServerHelpers .Net library.
--				Requires ToUnderscoreCaseUpper svf to be installed on the server, which is available at https://github.com/JoshKeegan/MsSqlServerQueries/blob/master/Installable/Scalar-Valued%20Functions/ToUnderscoreCaseUpper.sql
--				Usage: 
--					DECLARE @currDb nvarchar(250) = DB_NAME();
--
--					EXEC master.dbo.GenerateSqlDbTypeSizesForTable
--						@targetDb = @currDb,
--						@tableName = 'users'
-- =============================================
ALTER PROCEDURE GenerateSqlDbTypeSizesForTable
	@targetDb nvarchar(250),
	@tableName nvarchar(250)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	/* Clean up any existing temp table */
	IF OBJECT_ID('tempdb..##InformationSchemaColumns') IS NOT NULL
	BEGIN
		DROP TABLE ##InformationSchemaColumns;
	END

	/* Get Information Schema for the target database into a temp table */
	DECLARE @sql nvarchar(max) = '
	SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
	INTO ##InformationSchemaColumns
	FROM [' + @targetDb + '].INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = @tableName';

	EXECUTE sp_executesql @sql, N'@tableName nvarchar(250)', @tableName;

	DECLARE colsCursor CURSOR FOR
	SELECT 
		master.dbo.ToUnderscoreCaseUpper(COLUMN_NAME) + '_FIELD' AS constName,
		/* C# is case-sensitive. Get data type with same case as SqlDbType */
		CASE DATA_TYPE
			WHEN 'BigInt' THEN 'BigInt'
			WHEN 'Binary' THEN 'Binary'
			WHEN 'Bit' THEN 'Bit'
			WHEN 'Char' THEN 'Char'
			WHEN 'DateTime' THEN 'DateTime'
			WHEN 'Decimal' THEN 'Decimal'
			WHEN 'Float' THEN 'Float'
			WHEN 'Image' THEN 'Image' 
			WHEN 'Int' THEN 'Int'
			WHEN 'Money' THEN 'Money'
			WHEN 'NChar' THEN 'NChar'
			WHEN 'NText' THEN 'NText'
			WHEN 'NVarChar' THEN 'NVarChar'
			WHEN 'Real' THEN 'Real'
			WHEN 'UniqueIdentifier' THEN 'UniqueIdentifier'
			WHEN 'SmallDateTime' THEN 'SmallDateTime'
			WHEN 'SmallInt' THEN 'SmallInt'
			WHEN 'SmallMoney' THEN 'SmallMoney'
			WHEN 'Text' THEN 'Text'
			WHEN 'Timestamp' THEN 'Timestamp'
			WHEN 'TinyInt' THEN 'TinyInt'
			WHEN 'VarBinary' THEN 'VarBinary'
			WHEN 'VarChar' THEN 'VarChar'
			WHEN 'Variant' THEN 'Variant'
			WHEN 'Xml' THEN 'Xml' 
			WHEN 'Udt' THEN 'Udt' 
			WHEN 'Structured' THEN 'Structured'
			WHEN 'Date' THEN 'Date'
			WHEN 'Time' THEN 'Time'
			WHEN 'DateTime2' THEN 'DateTime2'
			WHEN 'DateTimeOffset' THEN 'DateTimeOffset'
		END AS sqlDbType,
		CHARACTER_MAXIMUM_LENGTH AS maxLength
	FROM ##InformationSchemaColumns;

	DECLARE @constName nvarchar(max);
	DECLARE @sqlDbType varchar(max);
	DECLARE @maxLength int;

	OPEN colsCursor;
	FETCH NEXT FROM colsCursor INTO @constName, @sqlDbType, @maxLength;

	WHILE @@FETCH_STATUS = 0
	BEGIN
		/* Construct a line of C# to represent this DB field */
		DECLARE @line nvarchar(max) = 'internal static readonly SqlDbTypeSize ' + @constName + ' = new SqlDbTypeSize(SqlDbType.' + @sqlDbType;

		/* If there's an explicit max length, include it */
		IF @maxLength IS NOT NULL
		BEGIN
			SET @line += ', ' + CAST(@maxLength AS nvarchar);
		END

		SET @line += ');';

		/* Print the line of C# out */
		PRINT @line;
	
		FETCH NEXT FROM colsCursor INTO @constName, @sqlDbType, @maxLength;
	END

	/* Clean Up Cursor */
	CLOSE colsCursor;
	DEALLOCATE colsCursor;

	/* Clean up temp table */
	DROP TABLE ##InformationSchemaColumns;
END
GO
