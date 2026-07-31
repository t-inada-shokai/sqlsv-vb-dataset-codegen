IF OBJECT_ID('adm_S_VBストアドプロシジャ定義SQL作成') IS NOT NULL DROP PROCEDURE dbo.adm_S_VBストアドプロシジャ定義SQL作成
GO

-- =============================================
-- Author:      稲田商会
-- Create date: 2025/07/18
-- Description: 関数 vb版ストアドプロシジャ定義SQLの作成
-- =============================================
CREATE PROCEDURE [dbo].[adm_S_VBストアドプロシジャ定義SQL作成] 
AS
BEGIN

    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

DECLARE @CRLF nvarchar(2);
SET @CRLF=CHAR(13) + CHAR(10);

declare @all_storedname as table(
    no bigint not null,
    name sysname not null,
    objectid int not null
);

insert into @all_storedname
select 
    ROW_NUMBER() over (ORDER BY OBJECT_NAME(A.object_id)) as no,
    OBJECT_NAME(A.object_id) as name,
    A.object_id as objectid
from sys.sql_modules A
where OBJECT_NAME(A.object_id) not like 'adm_%'
and OBJECT_NAME(A.object_id) not like 'private_%'
and A.object_id not in (select SA.object_id from sys.triggers SA)
and A.object_id not in (select SB.object_id from sys.views SB)
;

declare @captypename as table(
    keyname sysname not null
    ,capname sysname not null
);
insert into @captypename
(keyname,capname)
values
('bigint', 'BigInt')
,('binary', 'Binary')
,('bit', 'Bit')
,('char', 'Char')
,('date', 'Date')
,('datetime', 'DateTime')
,('datetime2', 'DateTime2')
,('datetimeoffset', 'DateTimeOffset')
,('decimal', 'Decimal')
,('float', 'Float')
,('geography', 'Geography')
,('geometry', 'Geometry')
,('hierarchyid', 'Hierarchyid')
,('image', 'Image')
,('int', 'Int')
,('json', 'Json')
,('money', 'Money')
,('nchar', 'NChar')
,('ntext', 'NText')
,('numeric', 'Numeric')
,('nvarchar', 'NVarChar')
,('real', 'Real')
,('smalldatetime', 'SmallDateTime')
,('smallint', 'SmallInt')
,('smallmoney', 'SmallMoney')
,('sql_variant', 'Sql_Variant')
,('sysname', 'Sysname')
,('text', 'Text')
,('time', 'Time')
,('timestamp', 'Timestamp')
,('tinyint', 'TinyInt')
,('uniqueidentifier', 'UniqueIdentifier')
,('varbinary', 'VarBinary')
,('varchar', 'VarChar')
,('vector', 'Vector')
,('xml', 'XML');

declare @buff nvarchar(max);
declare @s04 nvarchar(4)='    ';
declare @s08 nvarchar(8)=@s04+@s04;
declare @s12 nvarchar(12)=@s08+@s04;
declare @s16 nvarchar(16)=@s12+@s04;
declare @s20 nvarchar(20)=@s16+@s04;
declare @s24 nvarchar(24)=@s20+@s04;
declare @s28 nvarchar(28)=@s24+@s04;
declare @s32 nvarchar(32)=@s28+@s04;

set @buff = 
'Imports System.Data.SqlClient'+@crlf
+@crlf
+'Namespace AppCommonsClass'+@crlf
+@s04+'Friend Class ClsDBAccess_Stored'+@crlf
+@s08+'Protected Friend MustInherit Class StoredProcedure_base'+@crlf
+@s12+'Implements IDisposable'+@crlf
+@crlf
+@s12+'Private disposedValue As Boolean = False'+@crlf
+@s12+'Private ReadOnly privateDS As New DataSet'+@crlf
+@crlf
+@s12+'Public Property Command As SqlCommand'+@crlf
+@crlf
+@s12+'Public MustOverride Sub Initialize(Command As SqlCommand)'+@crlf
+@s12+'Protected Friend MustOverride Function OriginalStoredProcedureCommandLine() As String'+@crlf
+@crlf
+@s12+'Friend Function ParamRead(Of X)(name As String) As X'+@crlf
+@s16+'Dim ans As X'+@crlf
+@s16+'If IsDBNull(Me.Command.Parameters.Item(name).Value) Then'+@crlf
+@s20+'ans = Nothing'+@crlf
+@s16+'Else'+@crlf
+@s20+'ans = CType(Me.Command.Parameters.Item(name).Value, X)'+@crlf
+@s16+'End If'+@crlf
+@s16+'Return ans'+@crlf
+@s12+'End Function'+@crlf
+@crlf
+@s12+'Friend Sub ParamWrite(Of X)(name As String, value As X)'+@crlf
+@s16+'Dim ans As Object'+@crlf
+@s16+'If IsNothing(value) Then'+@crlf
+@s20+'ans = DBNull.Value'+@crlf
+@s16+'Else'+@crlf
+@s20+'ans = value'+@crlf
+@s16+'End If'+@crlf
+@s16+'Me.Command.Parameters.Item(name).Value = ans'+@crlf
+@s12+'End Sub'+@crlf
+@crlf
+@s12+'Public Overloads Function AdapterFill() As Integer'+@crlf
+@s16+'Using adp As New SqlDataAdapter(Me.Command)'+@crlf
+@s20+'Return adp.Fill(privateDS)'+@crlf
+@s16+'End Using'+@crlf
+@s12+'End Function'+@crlf
+@crlf
+@s12+'Public Overloads Function AdapterFill(ds As DataSet) As Integer'+@crlf
+@s16+'Using adp As New SqlDataAdapter(Me.Command)'+@crlf
+@s20+'Return adp.Fill(ds)'+@crlf
+@s16+'End Using'+@crlf
+@s12+'End Function'+@crlf
+@crlf
+@s12+'Public Overloads Function AdapterFill(ParamArray dts() As DataTable) As Integer'+@crlf
+@s16+'Dim ans As Integer = 0'+@crlf
+@s16+'Using reader As SqlDataReader = Me.Command.ExecuteReader()'+@crlf
+@s20+'For i As Integer = LBound(dts) To UBound(dts)'+@crlf
+@s24+'dts(i).Load(reader)'+@crlf
+@s24+'ans += dts(i).Rows.Count'+@crlf
+@s24+'If reader.NextResult() = False Then'+@crlf
+@s28+'Exit For'+@crlf
+@s24+'End If'+@crlf
+@s20+'Next'+@crlf
+@s16+'End Using'+@crlf
+@s16+'Return ans'+@crlf
+@s12+'End Function'+@crlf
+@crlf
+@s12+'Public Function ExecuteNonQuery() As Integer'+@crlf
+@s16+'Return Me.Command.ExecuteNonQuery()'+@crlf
+@s12+'End Function'+@crlf
+@crlf
+@s12+'Public Function GetTableAs(Of X As {New, DataTable})(index As Integer) As X'+@crlf
+@s16+'Dim adjust_index As Integer'+@crlf
+@s16+'If privateDS.Tables.Count > 0 Then'+@crlf
+@s20+'adjust_index = index Mod privateDS.Tables.Count'+@crlf
+@s16+'Else'+@crlf
+@s20+'Return Nothing'+@crlf
+@s16+'End If'+@crlf
+@s16+'Dim dt As New X'+@crlf
+@s16+'dt.Load(privateDS.Tables(adjust_index).CreateDataReader)'+@crlf
+@s16+'Return dt'+@crlf
+@s12+'End Function'+@crlf
+@crlf
+@s12+'Public Function GetTable(index As Integer) As DataTable'+@crlf
+@s16+'Dim adjust_index As Integer'+@crlf
+@s16+'If privateDS.Tables.Count > 0 Then'+@crlf
+@s20+'adjust_index = index Mod privateDS.Tables.Count'+@crlf
+@s16+'Else'+@crlf
+@s20+'Return Nothing'+@crlf
+@s16+'End If'+@crlf
+@s16+'Dim dt As New DataTable'+@crlf
+@s16+'dt.Load(privateDS.Tables(adjust_index).CreateDataReader)'+@crlf
+@s16+'Return dt'+@crlf
+@s12+'End Function'+@crlf
+@crlf
+@s12+'Public Function GetDataSet() As DataSet'+@crlf
+@s16+'Return privateDS'+@crlf
+@s12+'End Function'+@crlf
;

set @buff = @buff 
+@crlf
+@s12+'Protected Overridable Sub Dispose(disposing As Boolean)'+@crlf
+@s16+'If Not disposedValue Then'+@crlf
+@s20+'If disposing Then'+@crlf
+@s24+''' donothing'+@crlf
+@s20+'End If'+@crlf
+@crlf
+@s20+'disposedValue = True'+@crlf
+@s16+'End If'+@crlf
+@s12+'End Sub'+@crlf
+@crlf
+@s12+'Public Sub Dispose() Implements IDisposable.Dispose'+@crlf
+@s16+'Dispose(disposing:=True)'+@crlf
+@s16+'GC.SuppressFinalize(Me)'+@crlf
+@s12+'End Sub'+@crlf
+@s08+'End Class'+@crlf
;

declare @i int=1;
declare @imax int=isnull((select max(A.no) from @all_storedname A),0);

while(@i<=@imax)
begin
    declare @stored_name sysname;
    declare @stored_objectid int;
    select 
        @stored_name = A.name,
        @stored_objectid = A.objectid
    from @all_storedname A 
    where A.no=@i;

    declare @all_arguments as table(
        no bigint not null,
        name sysname not null,
        istable bit not null,
        istable_output bit not null,
        typename sysname not null,
        typemaxlen int not null,
        typeprecision int,
        typescale int,
        isoutput bit not null,
        isreturnvalue bit not null,
        isnullable bit not null
    );

    delete from @all_arguments;

    insert into @all_arguments
    select 
        ROW_NUMBER() OVER (ORDER BY A.object_id) as no,
        case when A.name='' then '__ReturnValue' else A.name end as name,
        B.is_table_type as istable,
        case when B.is_table_type=1 and (LOWER(A.name) like '@outputtable_%' or LOWER(A.name) like '@output_%') then 1 else 0 end as istable_output,
        isnull(BSYS.name,B.name) as typename,
        A.max_length as typemaxlen,
        A.precision as typeprecision,
        A.scale as typescale,
        A.is_output as isoutput,
        case when A.name='' then 1 else 0 end as isreturnvalue,
        A.is_nullable as isnullable
    from sys.parameters A
    left outer join sys.types B
    on A.user_type_id=B.user_type_id
    left outer join sys.types BSYS
    on B.system_type_id=BSYS.user_type_id AND B.is_user_defined=1
    where A.object_id = @stored_objectid
    ;

    declare @j int;
    declare @jmax int;
    declare @tmpbuff nvarchar(max);
    declare @argname sysname;
    declare @argistable bit;
    declare @argistable_output bit;
    declare @argtypename sysname;
    declare @argtypemaxlen int;
    declare @argtypeprecision int;
    declare @argtypescale int;
    declare @argisoutput bit;
    declare @argisreturnvalue bit;
    declare @argisnullable bit;
    declare @count_table_output int;

    set @jmax=isnull((select max(A.no) from @all_arguments A),0);
    set @count_table_output=isnull((select count(A.no) from @all_arguments A where A.istable_output=1),0);

    set @buff = @buff + @crlf
        +@s08+'Public Class Proc_'+@stored_name+@crlf
        +@s12+'Inherits StoredProcedure_base'+@crlf
        ;

    -- テーブル変数の順番を定数化
    declare @k int =0;
    set @j=1;
    set @tmpbuff='';
    while @j<=@jmax
    begin
        select 
            @argname = A.name,
            @argistable = A.istable,
            @argistable_output = A.istable_output,
            @argtypename = isnull(B.capname, A.typename),
            @argtypemaxlen = A.typemaxlen,
            @argtypeprecision = A.typeprecision,
            @argtypescale = A.typescale,
            @argisoutput = A.isoutput,
            @argisreturnvalue = A.isreturnvalue,
            @argisnullable = A.isnullable
        from @all_arguments A
        left outer join @captypename B
        on A.typename=B.keyname
        where A.no=@j;

        if @argistable_output=1 
        begin
            set @tmpbuff = @tmpbuff + @s12 + 'Private Const const_'+substring(@argname,2,len(@argname)-1)+' As Integer = ' + cast(-@count_table_output+@k as nvarchar(10)) + @crlf;
            set @k = @k + 1;
        end;

        set @j = @j+1;
    end;
    set @buff = @buff + @tmpbuff;

    -- コマンドラインを返す関数
    set @buff = @buff + @crlf
        +@s12+'Protected Friend Overrides Function OriginalStoredProcedureCommandLine() As String'+@crlf
        +@s16+'Dim sql As String = ""'+@crlf

    -- 一時テーブルの作成
    set @j=1;
    set @tmpbuff='';
    while @j<=@jmax
    begin
        select 
            @argname = A.name,
            @argistable = A.istable,
            @argistable_output = A.istable_output,
            @argtypename = isnull(B.capname, A.typename),
            @argtypemaxlen = A.typemaxlen,
            @argtypeprecision = A.typeprecision,
            @argtypescale = A.typescale,
            @argisoutput = A.isoutput,
            @argisreturnvalue = A.isreturnvalue,
            @argisnullable = A.isnullable
        from @all_arguments A
        left outer join @captypename B
        on A.typename=B.keyname
        where A.no=@j;

        if @argistable_output=1 
            set @tmpbuff = @tmpbuff + @s16 + 'sql &= "' + replace('select top (0) * into #%%tablename%% from @%%tablename%%;"', '%%tablename%%', substring(@argname,2,len(@argname)-1)) + @crlf;

        set @j = @j+1;
    end;
    set @buff = @buff + @tmpbuff;

    -- ストアドの実行
    set @buff = @buff
        +@s16+'sql &= "exec dbo.'+@stored_name+' ';
        
    -- ストアドの引数の構成
    set @j=1;
    set @tmpbuff='';
    while @j<=@jmax
    begin
        select 
            @argname = A.name,
            @argistable = A.istable,
            @argistable_output = A.istable_output,
            @argtypename = isnull(B.capname, A.typename),
            @argtypemaxlen = A.typemaxlen,
            @argtypeprecision = A.typeprecision,
            @argtypescale = A.typescale,
            @argisoutput = A.isoutput,
            @argisreturnvalue = A.isreturnvalue,
            @argisnullable = A.isnullable
        from @all_arguments A
        left outer join @captypename B
        on A.typename=B.keyname
        where A.no=@j;

        if @argisreturnvalue=0
            set @tmpbuff = @tmpbuff + @argname+'='+@argname+case when @argisoutput=1 then ' output' else '' end+',';

        set @j = @j+1;
    end;
    set @tmpbuff=case when len(@tmpbuff)>0 then SUBSTRING(@tmpbuff,1,len(@tmpbuff)-1) else '' end;

    set @buff = @buff
        +@tmpbuff+';"'+@crlf;

    -- output_%テーブル変数をSelectで出力
    set @j=1;
    set @tmpbuff='';
    while @j<=@jmax
    begin
        select 
            @argname = A.name,
            @argistable = A.istable,
            @argistable_output = A.istable_output,
            @argtypename = isnull(B.capname, A.typename),
            @argtypemaxlen = A.typemaxlen,
            @argtypeprecision = A.typeprecision,
            @argtypescale = A.typescale,
            @argisoutput = A.isoutput,
            @argisreturnvalue = A.isreturnvalue,
            @argisnullable = A.isnullable
        from @all_arguments A
        left outer join @captypename B
        on A.typename=B.keyname
        where A.no=@j;

        if @argistable_output=1
            set @tmpbuff = @tmpbuff + @s16 + replace('sql &= "select * from #%%tablename%%;"', '%%tablename%%', substring(@argname,2,len(@argname)-1)) +@crlf;

        set @j = @j+1;
    end;

    set @buff = @buff
        +@tmpbuff+@crlf;

    set @buff = @buff
        +@s16+'Return sql'+@crlf
        +@s12+'End Function'+@crlf
        +@crlf
        +@s12+'Public Overrides Sub Initialize(command As SqlCommand)'+@crlf
        +@s16+'Me.Command = command'+@crlf
        +@crlf
        +@s16+'Me.Command.CommandText = Me.OriginalStoredProcedureCommandLine()'+@crlf
    ;

    set @j=1;
    set @jmax=isnull((select max(A.no) from @all_arguments A),0);
    set @tmpbuff='';
    while @j<=@jmax
    begin
        select 
            @argname = A.name,
            @argistable = A.istable,
            @argistable_output = A.istable_output,
            @argtypename = isnull(B.capname, A.typename),
            @argtypemaxlen = A.typemaxlen,
            @argtypeprecision = A.typeprecision,
            @argtypescale = A.typescale,
            @argisoutput = A.isoutput,
            @argisreturnvalue = A.isreturnvalue,
            @argisnullable = A.isnullable
        from @all_arguments A
        left outer join @captypename B
        on A.typename=B.keyname
        where A.no=@j;

        set @tmpbuff = @tmpbuff + @crlf 
        + case when @argistable=1 then
        -- tableはreadonlyなのでoutputはない
            @s16+'Me.Command.Parameters.Add(New SqlParameter("'+@argname+'", SqlDbType.Structured) With {.TypeName = "dbo.'+@argtypename+'"})'
        when LOWER(@argtypename) in ('char','varchar','varbinary','binary') then
            @s16+'Me.Command.Parameters.Add(New SqlParameter("'+@argname+'", SqlDbType.'+@argtypename+case when @argtypemaxlen>0 then ', '+cast(@argtypemaxlen as nvarchar(10)) else '' end+')'
            +case when @argisoutput=1 then ' With {.Direction = ParameterDirection.'+case when @argisreturnvalue=1 then 'ReturnValue' else 'Output' end+'}' else '' end+')'
        when LOWER(@argtypename) in ('nchar','nvarchar') then
            @s16+'Me.Command.Parameters.Add(New SqlParameter("'+@argname+'", SqlDbType.'+@argtypename+case when @argtypemaxlen>0 then ', '+cast(@argtypemaxlen/2 as nvarchar(10)) else '' end+')'
            +case when @argisoutput=1 then ' With {.Direction = ParameterDirection.'+case when @argisreturnvalue=1 then 'ReturnValue' else 'Output' end+'}' else '' end+')'
        when LOWER(@argtypename) in ('sysname') then
            @s16+'Me.Command.Parameters.Add(New SqlParameter("'+@argname+'", SqlDbType.nvarchar, 128)'
            +case when @argisoutput=1 then ' With {.Direction = ParameterDirection.'+case when @argisreturnvalue=1 then 'ReturnValue' else 'Output' end+'}' else '' end+')'
        else
            @s16+'Me.Command.Parameters.Add(New SqlParameter("'+@argname+'", SqlDbType.'+@argtypename+')'
            +case when @argisoutput=1 then ' With {.Direction = ParameterDirection.'+case when @argisreturnvalue=1 then 'ReturnValue' else 'Output' end+'}' else '' end+')'
        end
        +@CRLF;

        set @j = @j+1;
    end;
    set @buff=@buff
        +@tmpbuff+@crlf
        +@s12+'End Sub'+@crlf
    ;

    set @j=1;
    set @jmax=isnull((select max(A.no) from @all_arguments A),0);
    set @tmpbuff='';
    while @j<=@jmax
    begin
        select 
            @argname = A.name,
            @argistable = A.istable,
            @argistable_output = A.istable_output,
            @argtypename = isnull(B.capname, A.typename),
            @argtypemaxlen = A.typemaxlen,
            @argtypeprecision = A.typeprecision,
            @argtypescale = A.typescale,
            @argisoutput = A.isoutput,
            @argisreturnvalue = A.isreturnvalue,
            @argisnullable = A.isnullable
        from @all_arguments A
        left outer join @captypename B
        on A.typename=B.keyname
        where A.no=@j;

        -- if len(@argname)=0
        -- begin
        --     print(@argname);
        --     print(@j);
        -- end;

        set @tmpbuff = @tmpbuff
        + case 
        when @argistable=1 then
            @crlf
            +@s12+'Public '+case when @argistable_output=1 then '' else 'WriteOnly ' end+'Property Param_'+ substring(@argname,2,len(@argname)-1) +' As DSSQLSV.'+@argtypename+'DataTable'+@crlf
            +case when @argistable_output=1 then
                @s16+'Get'+@crlf
                +@s20+'Return Me.GetTableAs(Of DSSQLSV.'+@argtypename+'DataTable)(const_'+substring(@argname,2,len(@argname)-1)+')'+@crlf
                +@s16+'End Get'+@crlf
            else '' end
            +@s16+'Set(value As DSSQLSV.'+@argtypename+'DataTable)'+@crlf
            +@s20+'If IsNothing(value) Then'+@crlf
            +@s24+'value = New DSSQLSV.'+@argtypename+'DataTable'+@crlf
            +@s20+'End If'+@crlf
            +@s20+'Me.ParamWrite(Of DSSQLSV.'+@argtypename+'DataTable)("'+@argname+'", value)'+@crlf
            +@s16+'End Set'+@crlf
            +@s12+'End Property'+@crlf

        when LOWER(@argtypename) in ('char','varchar','nchar','nvarchar','sysname') then
            @crlf
            +@s12+'Public '+case when @argisoutput=1 then 'ReadOnly' else 'WriteOnly' end
            +' Property Param_'+ substring(@argname,2,len(@argname)-1) +' As String'+@crlf
            +case when @argisoutput=1 then
                @s16+'Get'+@crlf
                +@s20+'Return Me.ParamRead(Of String)("'+@argname+'")'+@crlf
                +@s16+'End Get'+@crlf
            else 
                @s16+'Set(value As String)'+@crlf
                +@s20+'Me.ParamWrite(Of String)("'+@argname+'", value)'+@crlf
                +@s16+'End Set'+@crlf
            end
            +@s12+'End Property'+@crlf

        when LOWER(@argtypename) in ('varbinary','binary') then
            @crlf
            +@s12+'Public '+case when @argisoutput=1 then 'ReadOnly' else 'WriteOnly' end
            +' Property Param_'+ substring(@argname,2,len(@argname)-1) +' As Byte()'+@crlf
            +case when @argisoutput=1 then
                @s16+'Get'+@crlf
                +@s20+'Return Me.ParamRead(Of String)("'+@argname+'")'+@crlf
                +@s16+'End Get'+@crlf
            else
                @s16+'Set(value As Byte())'+@crlf
                +@s20+'Me.ParamWrite(Of Byte())("'+@argname+'", value)'+@crlf
                +@s16+'End Set'+@crlf
            end
            +@s12+'End Property'+@crlf

        when LOWER(@argtypename) in ('date','datetime','datetime2','time','time2') then
            @crlf
            +@s12+'Public '+case when @argisoutput=1 then 'ReadOnly' else 'WriteOnly' end
            +' Property Param_'+ substring(@argname,2,len(@argname)-1) +' As DateTime'+case when @argisnullable=1 then '?' else '' end+@crlf
            +case when @argisoutput=1 then
                @s16+'Get'+@crlf
                +@s20+'Return Me.ParamRead(Of DateTime'+case when @argisnullable=1 then '?' else '' end+')("'+@argname+'")'+@crlf
                +@s16+'End Get'+@crlf
            else
                @s16+'Set(value As DateTime'+case when @argisnullable=1 then '?' else '' end+')'+@crlf
                +@s20+'Me.ParamWrite(Of DateTime'+case when @argisnullable=1 then '?' else '' end+')("'+@argname+'", value)'+@crlf
                +@s16+'End Set'+@crlf
            end
            +@s12+'End Property'+@crlf

        when LOWER(@argtypename) in ('datetimeoffset') then
            @crlf
            +@s12+'Public '+case when @argisoutput=1 then 'ReadOnly' else 'WriteOnly' end
            +' Property Param_'+ substring(@argname,2,len(@argname)-1) +' As DateTimeOffset'+case when @argisnullable=1 then '?' else '' end+@crlf
            +case when @argisoutput=1 then
                @s16+'Get'+@crlf
                +@s20+'Return Me.ParamRead(Of DateTimeOffset'+case when @argisnullable=1 then '?' else '' end+')("'+@argname+'")'+@crlf
                +@s16+'End Get'+@crlf
            else
                @s16+'Set(value As DateTimeOffset'+case when @argisnullable=1 then '?' else '' end+')'+@crlf
                +@s20+'Me.ParamWrite(Of DateTimeOffset'+case when @argisnullable=1 then '?' else '' end+')("'+@argname+'", value)'+@crlf
                +@s16+'End Set'+@crlf
            end
            +@s12+'End Property'+@crlf

        when LOWER(@argtypename) in ('tinyint','shortint','int') then
            @crlf
            +@s12+'Public '+case when @argisoutput=1 then 'ReadOnly' else 'WriteOnly' end
            +' Property Param_'+ substring(@argname,2,len(@argname)-1) +' As Integer'+case when @argisnullable=1 then '?' else '' end+@crlf
            +case when @argisoutput=1 then
                @s16+'Get'+@crlf
                +@s20+'Return Me.ParamRead(Of Integer'+case when @argisnullable=1 then '?' else '' end+')("'+@argname+'")'+@crlf
                +@s16+'End Get'+@crlf
            else
                @s16+'Set(value As Integer'+case when @argisnullable=1 then '?' else '' end+')'+@crlf
                +@s20+'Me.ParamWrite(Of Integer'+case when @argisnullable=1 then '?' else '' end+')("'+@argname+'", value)'+@crlf
                +@s16+'End Set'+@crlf
            end
            +@s12+'End Property'+@crlf

        when LOWER(@argtypename) in ('decimal') then
            @crlf
            +@s12+'Public '+case when @argisoutput=1 then 'ReadOnly' else 'WriteOnly' end
            +' Property Param_'+ substring(@argname,2,len(@argname)-1) +' As Decimal'+case when @argisnullable=1 then '?' else '' end+@crlf
            +case when @argisoutput=1 then
                @s16+'Get'+@crlf
                +@s20+'Return Me.ParamRead(Of Decimal'+case when @argisnullable=1 then '?' else '' end+')("'+@argname+'")'+@crlf
                +@s16+'End Get'+@crlf
            else
                @s16+'Set(value As Decimal'+case when @argisnullable=1 then '?' else '' end+')'+@crlf
                +@s20+'Me.ParamWrite(Of Decimal'+case when @argisnullable=1 then '?' else '' end+')("'+@argname+'", value)'+@crlf
                +@s16+'End Set'+@crlf
            end
            +@s12+'End Property'+@crlf

        when LOWER(@argtypename) in ('bit') then
            @crlf
            +@s12+'Public '+case when @argisoutput=1 then 'ReadOnly' else 'WriteOnly' end
            +' Property Param_'+ substring(@argname,2,len(@argname)-1) +' As Boolean'+case when @argisnullable=1 then '?' else '' end+@crlf
            +case when @argisoutput=1 then
                @s16+'Get'+@crlf
                +@s20+'Return Me.ParamRead(Of Boolean'+case when @argisnullable=1 then '?' else '' end+')("'+@argname+'")'+@crlf
                +@s16+'End Get'+@crlf
            else
                @s16+'Set(value As Boolean'+case when @argisnullable=1 then '?' else '' end+')'+@crlf
                +@s20+'Me.ParamWrite(Of Boolean'+case when @argisnullable=1 then '?' else '' end+')("'+@argname+'", value)'+@crlf
                +@s16+'End Set'+@crlf
            end
            +@s12+'End Property'+@crlf

        else
            @crlf
            +@s12+'Public Property Param_'+ substring(@argname,2,len(@argname)-1) +' As Object'+@crlf
            +@s16+'Get'+@crlf
            +@s20+'Throw New Exception("'+ @argtypename +' is not supported")'+@crlf
            +@s16+'End Get'+@crlf
            +@s16+'Set(value As Object)'+@crlf
            +@s20+'Throw New Exception("'+ @argtypename +' is not supported")'+@crlf
            +@s16+'End Set'+@crlf
            +@s12+'End Property'+@crlf
        end;

        set @j = @j+1;
    end;
    set @buff=@buff + @tmpbuff 
        +@s08+'End Class'+@crlf;

    set @i = @i+1;
end;

set @buff = @buff 
    +@s04+'End Class'+@crlf
    +'End Namespace'+@crlf
    ;

select @buff;

END
