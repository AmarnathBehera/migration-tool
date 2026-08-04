SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Anonymises personal data across Form_*, OM_*, and EmailLibrary_EmailMarketingRecipient.
-- Newsletter_* tables replaced with EmailLibrary_EmailMarketingRecipient (XbyK equivalent).
CREATE OR ALTER PROCEDURE [dbo].[BDO_GDPR_SanitizePersonalData] @email_address NVARCHAR(1000)
AS

DECLARE @Temp_TableName  TABLE (TABLE_NAME  NVARCHAR(MAX))
DECLARE @Temp_ColumnName TABLE (COLUMN_NAME NVARCHAR(MAX))

DECLARE @sqlParam       NVARCHAR(MAX) = N'@pEmail NVARCHAR(1000)'
DECLARE @sqlCommand     NVARCHAR(MAX)
DECLARE @table_name_var NVARCHAR(1000)

INSERT INTO @Temp_TableName (TABLE_NAME)
SELECT TABLE_NAME
FROM   INFORMATION_SCHEMA.TABLES
WHERE  TABLE_NAME = 'EmailLibrary_EmailMarketingRecipient'   -- replaces Newsletter_Subscriber / OpenedEmail / ClickedLink / Unsubscription
    OR TABLE_NAME LIKE 'Form[_]%'
    OR TABLE_NAME LIKE 'OM[_]%'

WHILE (SELECT COUNT(*) FROM @Temp_TableName) > 0
BEGIN
    SELECT TOP 1 @table_name_var = TABLE_NAME FROM @Temp_TableName

    INSERT INTO @Temp_ColumnName
    SELECT COLUMN_NAME
    FROM   INFORMATION_SCHEMA.COLUMNS
    WHERE  DATA_TYPE              = 'nvarchar'
      AND  TABLE_NAME             = @table_name_var
      AND  COLUMN_NAME NOT LIKE '%Class%'
      AND  COLUMN_NAME NOT LIKE '%ID'
      AND  CHARACTER_MAXIMUM_LENGTH > 15

    IF EXISTS (SELECT 1 FROM @Temp_ColumnName WHERE COLUMN_NAME LIKE '%email%')
    BEGIN
        SET @sqlCommand =
            'UPDATE ' + @table_name_var + ' SET ' +
            (SELECT STRING_AGG('[' + COLUMN_NAME + '] = ''GDPR_Anonymized''', ', ') FROM @Temp_ColumnName) +
            ' WHERE ' +
            (SELECT STRING_AGG('[' + COLUMN_NAME + '] = @pEmail', ' OR ') FROM @Temp_ColumnName WHERE COLUMN_NAME LIKE '%email%')

        EXECUTE sp_executesql @sqlCommand, @sqlParam, @pEmail = @email_address
    END

    DELETE @Temp_TableName  WHERE TABLE_NAME = @table_name_var
    DELETE FROM @Temp_ColumnName
END
GO
