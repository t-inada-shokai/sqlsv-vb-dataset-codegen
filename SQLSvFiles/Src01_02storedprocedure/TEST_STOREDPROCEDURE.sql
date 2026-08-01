IF OBJECT_ID('TEST_STOREDPROCEDURE') IS NOT NULL DROP PROCEDURE dbo.TEST_STOREDPROCEDURE
GO
-- =============================================
-- Author:      T-INADA-SHOKAI
-- Create date: 20260730
-- Description: テスト用のストアド
-- =============================================
CREATE PROCEDURE [dbo].[TEST_STOREDPROCEDURE] 
    -- Add the parameters for the stored procedure here
    @p1 nvarchar(20) = '' 
    ,@outputtable_result dbo.TYPE_D_SAMPLE_DATA readonly
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

    declare @row dbo.TYPE_D_SAMPLE_DATA;

    -- Insert statements for procedure here
    if isnumeric(@p1) = 1 
    begin
        insert into @row
        (
            original_str
            ,castAsBigInt
            ,mul_2
            ,mul_2_AsStr
        )
        select 
            @p1 as original_str
            ,cast(@p1 as bigint) as castAsBigInt
            ,cast(@p1 as bigint) * 2 as mul_2
            ,cast( cast(@p1 as bigint) * 2 as nvarchar(20)) as mul_2_AsStr
        ;
    end
    else
    begin
        insert into @row
        (
            original_str
            ,castAsBigInt
            ,mul_2
            ,mul_2_AsStr
        )
        select 
            @p1 as original_str
            ,null as castAsBigInt
            ,null as mul_2
            ,'[ "'+@p1+'", "'+@p1+'"]' as mul_2_AsStr
        ;
    end
    ;

    insert into dbo.D_SAMPLE_DATA
    (
        original_str
        ,castAsBigInt
        ,mul_2
        ,mul_2_AsStr
    )
    select 
        A.original_str
        ,A.castAsBigInt
        ,A.mul_2
        ,A.mul_2_AsStr
    from @row A;

    insert into #outputtable_result
    (
        uid
        ,original_str
        ,castAsBigInt
        ,mul_2
        ,mul_2_AsStr
        ,updatedatetime
    )
    select 
        A.uid
        ,A.original_str
        ,A.castAsBigInt
        ,A.mul_2
        ,A.mul_2_AsStr
        ,A.updatedatetime
    from dbo.D_SAMPLE_DATA A;

END
