SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Searches Form_*, OM_*, and EmailLibrary_* tables for personal data by email.
-- OM_Contact is special-cased to join OM_Activity; Personas_Persona removed (not in XbyK).
CREATE OR ALTER PROCEDURE [dbo].[BDO_GetPersonalData] @email_address NVARCHAR(1000)
AS

DECLARE @Temp_TableName TABLE (TABLE_NAME NVARCHAR(MAX))
DECLARE @Temp_XML       TABLE (XmlOutput XML)
DECLARE @Temp_Column    TABLE (COLUMN_NAME NVARCHAR(MAX))

DECLARE @sqlParam         NVARCHAR(MAX) = N'@ResultXml xml output, @pEmail nvarchar(1000)'
DECLARE @ResultXml        XML
DECLARE @table_name_var   NVARCHAR(1000)
DECLARE @sqlCommand       NVARCHAR(MAX)
DECLARE @sqlWhereClause   NVARCHAR(MAX)
DECLARE @table_column_var NVARCHAR(1000)

INSERT INTO @Temp_TableName (TABLE_NAME)
SELECT TABLE_NAME
FROM   INFORMATION_SCHEMA.TABLES
WHERE  TABLE_NAME LIKE 'Form[_]%'
    OR TABLE_NAME LIKE 'OM[_]%'
    OR TABLE_NAME LIKE 'EmailLibrary[_]%'

WHILE (SELECT COUNT(*) FROM @Temp_TableName) > 0
BEGIN
    SELECT TOP 1 @table_name_var = TABLE_NAME FROM @Temp_TableName

    IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE  COLUMN_NAME LIKE '%email%'
          AND  DATA_TYPE   = 'nvarchar'
          AND  TABLE_NAME  = @table_name_var
    )
    BEGIN
        INSERT INTO @Temp_Column (COLUMN_NAME)
        SELECT COLUMN_NAME
        FROM   INFORMATION_SCHEMA.COLUMNS
        WHERE  COLUMN_NAME LIKE '%email%'
          AND  DATA_TYPE   = 'nvarchar'
          AND  TABLE_NAME  = @table_name_var

        SET @sqlWhereClause = ''

        WHILE (SELECT COUNT(*) FROM @Temp_Column) > 0
        BEGIN
            SELECT TOP 1 @table_column_var = COLUMN_NAME FROM @Temp_Column
            DELETE @Temp_Column WHERE COLUMN_NAME = @table_column_var

            IF EXISTS (SELECT TOP 1 COLUMN_NAME FROM @Temp_Column)
                SET @sqlWhereClause = @sqlWhereClause + @table_column_var + ' = @pEmail OR '
            ELSE
                SET @sqlWhereClause = @sqlWhereClause + @table_column_var + ' = @pEmail'
        END

        IF @table_name_var = 'OM_Contact'
        BEGIN
            -- Join activities; CMS_Country join retained if ContactCountryID exists, Personas removed.
            SET @sqlCommand = N'set @ResultXml = (
                SELECT OM_Contact.*,
                       OM_Activity.ActivityCreated,
                       OM_Activity.ActivityType,
                       OM_Activity.ActivityURL,
                       OM_Activity.ActivityTitle,
                       OM_Activity.ActivityURLReferrer
                FROM (
                    SELECT OM_Contact.*
                    FROM   OM_Contact
                    WHERE  OM_Contact.ContactEmail = @pEmail
                ) OM_Contact
                LEFT JOIN OM_Activity ON OM_Contact.ContactID = OM_Activity.ActivityContactID
                FOR XML AUTO, ELEMENTS
            )'
            EXECUTE sp_executesql @sqlCommand, @sqlParam, @ResultXml OUTPUT, @pEmail = @email_address
        END
        ELSE
        BEGIN
            SET @sqlCommand = N'set @ResultXml = (SELECT * FROM ' + @table_name_var + ' WHERE ' + @sqlWhereClause + ' FOR XML AUTO, ELEMENTS)'
            EXECUTE sp_executesql @sqlCommand, @sqlParam, @ResultXml OUTPUT, @pEmail = @email_address
        END

        IF (@ResultXml IS NOT NULL)
            INSERT INTO @Temp_XML SELECT @ResultXml

        SET @ResultXml = NULL
    END

    DELETE @Temp_TableName WHERE TABLE_NAME = @table_name_var
END

SELECT XmlOutput.query('/') FROM @Temp_XML FOR XML PATH(''), ROOT('XML'), ELEMENTS
GO
