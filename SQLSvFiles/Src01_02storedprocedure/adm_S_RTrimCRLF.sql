IF OBJECT_ID('adm_S_RTrimCRLF') IS NOT NULL DROP FUNCTION dbo.adm_S_RTrimCRLF
GO
-- =============================================
-- Author:      àÓìcè§âÔ
-- Create date: 2019/02/27
-- Description: ï∂ññÇÃCRLFÇ®ÇÊÇ—ãÛîíÇÃçÌèú
-- =============================================
CREATE FUNCTION [dbo].[adm_S_RTrimCRLF] 
(
    -- Add the parameters for the function here
    @Str nvarchar(max)
)
RETURNS nvarchar(max)
AS
BEGIN
    -- Declare the return variable here
    DECLARE @ResultVar nvarchar(max);
    
    -- Add the T-SQL statements to compute the return value here
    DECLARE @buf nvarchar(1);
    DECLARE @pt int;
    DECLARE @ln int;

    SET @ln=LEN(@Str);
    SET @pt=0;
    SET @buf=SUBSTRING(@Str,@ln-@pt,1);

    WHILE @pt<=@ln
    BEGIN
        IF @buf NOT IN (' ',CHAR(13),CHAR(10))
            BREAK;
        
        SET @pt=@pt+1;
        SET @buf=SUBSTRING(@Str,@ln-@pt,1);
    END
    ;
    
    SET @ResultVar=SUBSTRING(@Str,1,@ln-@pt);

    -- Return the result of the function
    RETURN @ResultVar

END
