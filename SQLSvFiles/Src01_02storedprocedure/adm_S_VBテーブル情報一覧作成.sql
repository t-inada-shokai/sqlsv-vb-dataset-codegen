IF OBJECT_ID('adm_S_VBテーブル情報一覧作成') IS NOT NULL DROP PROCEDURE dbo.adm_S_VBテーブル情報一覧作成
GO
-- =============================================
-- Author:      稲田商会
-- Create date: 2019/05/28
-- Description: テーブル情報一覧の作成
-- =============================================

CREATE PROCEDURE [dbo].[adm_S_VBテーブル情報一覧作成] 
AS
BEGIN

    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

DECLARE @table_names as TABLE(
    table_name SYSNAME not null
    , table_id INT not null
    , table_type SYSNAME not null
)
;

INSERT INTO @table_names
SELECT 
    o.name AS table_name
    , o.[object_id] AS table_id
    , 'table' AS table_type
FROM 
    sys.objects o WITH (NOWAIT)
WHERE 
    o.[type]='U' 
    AND (
        o.name like 'C_%' 
        OR o.name like 'D_%' 
        OR o.name like 'M_%'
        OR o.name like 'R_%'
        OR o.name like 'P_%'
        OR o.name like 'GEO_%'
        OR o.name like 'type_%'
    ) 
    AND NOT (
        o.name like 'private_%'
        OR charindex('_private_', o.name) > 0
    )
ORDER BY 
    o.name ASC
;
INSERT INTO @table_names
SELECT 
    t.name AS table_name
    , t.type_table_object_id AS table_id
    , 'type' AS table_type
FROM 
    sys.table_types t WITH (NOWAIT)
WHERE 
    t.name like 'type_%'
    AND NOT (
        t.name like 'private_%'
        OR charindex('_private_', t.name) > 0
    )
ORDER BY 
    t.name ASC
;

DECLARE @table_keys as TABLE(
    table_name SYSNAME not null
    ,table_id INT not null
    ,key_name SYSNAME not null
    ,[object_id] INT not null
    ,unique_index_id INT not null
);

INSERT INTO @table_keys
SELECT 
    TABLE_ME.table_name
    ,TABLE_ME.table_id AS table_id
    ,k.name AS key_name
    ,k.[object_id]
    ,k.unique_index_id
FROM 
    @table_names TABLE_ME
INNER JOIN
    sys.key_constraints k WITH (NOWAIT)
ON 
    TABLE_ME.table_id=k.parent_object_id
WHERE 
    k.type='PK'
;

DECLARE @table_index as TABLE(
    table_name SYSNAME not null
    ,table_id INT not null
    ,index_name SYSNAME not null
    ,index_id INT not null
);
INSERT INTO @table_index
SELECT 
    TABLE_ME.table_name
    ,TABLE_ME.table_id
    , ind.name AS index_name
    , ind.index_id
FROM 
    @table_names TABLE_ME
LEFT OUTER JOIN 
    sys.indexes ind WITH (NOWAIT)
ON 
    TABLE_ME.table_id=ind.[object_id]
WHERE 
    ind.is_unique=1
    AND ind.is_primary_key = 0
    AND ind.[type] = 2
;

DECLARE 
    @TAB nvarchar(1)
    ,@CRLF nvarchar(2)
;
SET @TAB=CHAR(9);
SET @CRLF=CHAR(13)+CHAR(10);
DECLARE @XML nvarchar(max);

SELECT 
    @XML='<?xml version="1.0" encoding="ShiftJis"?>'+@CRLF
    +'<Tables>'+@CRLF
    +(
    SELECT 
        '<Table name="'+TABLE_ME.table_name+'">'+@CRLF
        +(
        SELECT 
            '<Column name="'+c.name+'"'
            +CASE isnull(systp.name,tp.name) 
                WHEN 'bigint' THEN ' type="int64"'
                WHEN 'binary' THEN ' type="base64binary" maxlength="'+CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR(5)) END+'"'
                WHEN 'bit' THEN ' type="boolean"'
                WHEN 'char' THEN ' type="string" maxlength="'+CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR(5)) END+'"'
                WHEN 'date' THEN ' type="datetime"'
                WHEN 'datetime' THEN ' type="datetime"'
                WHEN 'datetime2' THEN ' type="datetime"'
                WHEN 'datetimeoffset' THEN ' type="datetimeoffset"'
                WHEN 'decimal' THEN ' type="decimal"'
                WHEN 'float' THEN ' type="double"'
                WHEN 'geography' THEN ' type="string" maxlength="MAX"'
                WHEN 'geometry' THEN ' type="string" maxlength="MAX"'
                WHEN 'hierarchyid' THEN ' type="string" maxlength="MAX"'
                WHEN 'image' THEN ' type="base64binary" maxlength="'+CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR(5)) END+'"'
                WHEN 'int' THEN ' type="int"'
                WHEN 'json' THEN ' type="string" maxlength="MAX"'
                WHEN 'money' THEN ' type="decimal"'
                WHEN 'nchar' THEN ' type="string" maxlength="'+CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length/2 AS VARCHAR(5)) END+'"'
                WHEN 'ntext' THEN ' type="string" maxlength="'+CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length/2 AS VARCHAR(5)) END+'"'
                WHEN 'numeric' THEN ' type="decimal"'
                WHEN 'nvarchar' THEN ' type="string" maxlength="'+CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length/2 AS VARCHAR(5)) END+'"'
                WHEN 'real' THEN ' type="single"'
                WHEN 'rowversion' THEN ' type="base64binary" maxlength="'+CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR(5)) END+'"'
                WHEN 'smalldatetime' THEN ' type="datetime"'
                WHEN 'smallint' THEN ' type="int16"'
                WHEN 'smallmoney' THEN ' type="decimal"'
                WHEN 'sql_variant' THEN ' type="base64binary" maxlength="'+CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR(5)) END+'"'
                WHEN 'sysname' THEN ' type="string" maxlength="'+CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR(5)) END+'"'
                WHEN 'text' THEN ' type="string" maxlength="'+CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR(5)) END+'"'
                WHEN 'time' THEN ' type="timespan"'
                WHEN 'timestamp' THEN ' type="base64binary" maxlength="'+CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR(5)) END+'"'
                WHEN 'tinyint' THEN ' type="byte"'
                WHEN 'uniqueidentifier' THEN ' type="guid"'
                WHEN 'varbinary' THEN ' type="base64binary" maxlength="'+CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR(5)) END+'"'
                WHEN 'varchar' THEN ' type="string" maxlength="'+CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR(5)) END+'"'
                WHEN 'vector' THEN ' type="base64binary" maxlength="'+CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR(5)) END+'"'
                WHEN 'xml' THEN ' type="string" maxlength="MAX"'
            ELSE '' END
            +CASE WHEN dc.[definition] IS NOT NULL 
                THEN ' default="'+dc.[definition]+'"' 
            ELSE '' END 
            +CASE WHEN c.is_nullable = 1 
                THEN ' allowNull="true"' 
            ELSE '' END
            +CASE WHEN idc.is_identity = 1 
                THEN ' autoincrement="true"'
                    +' autoincrement_seedvalue="'+CAST(ISNULL(idc.seed_value, '0') AS CHAR)+'"'
                    +' autoincrement_incrementvalue="'+CAST(ISNULL(idc.increment_value, '1') AS CHAR)+'"'
            ELSE '' END 
            +
            ' />'+@CRLF
        FROM 
            sys.columns c WITH (NOWAIT)
        LEFT OUTER JOIN 
            sys.types tp WITH (NOWAIT)
        ON 
            c.user_type_id=tp.user_type_id
        LEFT OUTER JOIN 
            sys.computed_columns cc WITH (NOWAIT) 
        ON 
            c.[object_id] = cc.[object_id] AND c.column_id = cc.column_id
        LEFT OUTER JOIN 
            sys.default_constraints dc WITH (NOWAIT) 
        ON 
            c.default_object_id != 0 
            AND c.[object_id] = dc.parent_object_id 
            AND c.column_id = dc.parent_column_id
        LEFT OUTER JOIN 
            sys.identity_columns idc WITH (NOWAIT) 
        ON 
            c.is_identity = 1 
            AND c.[object_id] = idc.[object_id] 
            AND c.column_id = idc.column_id
        LEFT OUTER JOIN
            sys.types systp WITH (NOWAIT)
        ON
            tp.system_type_id=systp.user_type_id and tp.is_user_defined=1
        WHERE 
            c.[object_id]=TABLE_ME.table_id
            AND ISNULL(cc.is_computed,0)!=1 
        ORDER BY 
            c.column_id ASC
        FOR XML PATH(N''), TYPE
        ).value('.', 'NVARCHAR(MAX)')
        +'</Table>'+@CRLF
    FROM 
        @table_names TABLE_ME
    ORDER BY 
        TABLE_ME.table_name ASC
    FOR XML PATH(N''), TYPE
    ).value('.', 'NVARCHAR(MAX)')
    +'</Tables>'+@CRLF
    +'<Indexes>'+@CRLF
    +ISNULL((
    SELECT 
        '<Index name="'+TABLE_KEY.key_name+'" type="PrimaryKey" on="'+TABLE_KEY.table_name+'">'+@CRLF
        +(
        SELECT 
            '<KeyColumn name="'+c.name+'" />'+@CRLF
        FROM 
            sys.index_columns ic WITH (NOWAIT)
        LEFT OUTER JOIN
            sys.columns c WITH (NOWAIT)
        ON 
            ic.[object_id]=c.[object_id]
            AND ic.column_id=c.column_id
        WHERE 
            ic.[object_id]=TABLE_KEY.table_id
            AND ic.index_id=TABLE_KEY.unique_index_id
        ORDER BY
            ic.index_column_id ASC
        FOR XML PATH(N''), TYPE
        ).value('.', 'NVARCHAR(MAX)')
        +'</Index>'+@CRLF
    FROM
        @table_keys TABLE_KEY
    ORDER BY
        TABLE_KEY.table_name ASC
        ,TABLE_KEY.key_name ASC
    For XML PATH(N''), TYPE
    ).value('.', 'NVARCHAR(MAX)')
    ,'')
    + ISNULL((
    SELECT
        '<Index name="' + TABLE_INDEX.index_name + '" type="UniqueKey" on="' + TABLE_INDEX.table_name + '">' + @CRLF
        +(
        SELECT 
            '<KeyColumn name="' + c.name + '" />'+@CRLF
        FROM 
            sys.index_columns ic WITH (NOWAIT)
        LEFT OUTER JOIN 
            sys.columns c WITH (NOWAIT) 
        ON 
            ic.[object_id] = c.[object_id] 
            AND ic.column_id = c.column_id
        WHERE 
            ic.[object_id]=TABLE_INDEX.table_id
            AND ic.index_id=TABLE_INDEX.index_id
        ORDER BY 
            ic.index_id
        FOR XML PATH(''), TYPE
        ).value('.', 'NVARCHAR(MAX)')
        +'</Index>'+@CRLF  
    FROM 
        @table_index TABLE_INDEX
    ORDER BY 
        TABLE_INDEX.table_name ASC, TABLE_INDEX.index_name ASC
    FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)')
    , '')
    +'</Indexes>'
;

SELECT @XML;

END
