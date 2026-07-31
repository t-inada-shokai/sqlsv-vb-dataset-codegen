Imports BASEDLL
Imports BASEDLL.BASELIB.ProjectCommonVariable

Module MainModule

    <STAThread>
    Public Function Main() As Integer
        Dim mySerializerManager As New MySerializerManager

        ' エントリーポイントの呼び出し
        Return BASEDLL.BASELIB.MAINMODULE.EntryPoint(AddressOf MainTask, mySerializerManager)
    End Function

    'コマンドラインオプション、設定ファイルの読込み、設定ファイルへの書込みを行うクラス
    Private Class MySerializerManager
        Implements BASEDLL.BASELIB.ISerializerManager

        Private objCSM As BASEDLL.BASELIB.ConfigSettingsManager.IConfigSettingsManager

        ''' <summary>
        ''' コマンドラインオプション、設定ファイルの読込を行うメソッド
        ''' </summary>
        ''' <param name="target_prj">アプリからアクセスされる共用変数</param>
        ''' <param name="ConfigFullText">設定ファイルの内容（全文）</param>
        ''' <param name="CommandLineArgs">コマンドラインオプションの文字列配列</param>
        Public Sub Deserialize(target_prj As IClsProjectCommonVariable, ConfigFullText As String, CommandLineArgs() As String) Implements ISerializerManager.Deserialize
            Dim userdefval As New UserDefinedValues
            'JSON形式の設定ファイルをConfigObject型のオブジェクトとして読み込む
            'その際に不要な情報は捨てられる
            objCSM = New BASELIB.ConfigSettingsManager.JsonCryptSettingManager
            userdefval.ConfigObject = objCSM.Deserialize(Of ConfigObject)(ConfigFullText)

            Dim i As Integer = LBound(CommandLineArgs)
            Dim lastindex As Integer = UBound(CommandLineArgs)
            While i <= lastindex
                userdefval.FilePathList.Add(CommandLineArgs(i))
                i += 1
            End While

            target_prj.AppStatus.SilentMode = True
            target_prj.UserDefined.SetValues(Of UserDefinedValues)(userdefval)
        End Sub

        ''' <summary>
        ''' 設定ファイルに書込みを行うメソッド
        ''' </summary>
        ''' <param name="target_prj">アプリからアクセスされる共用変数</param>
        ''' <param name="ConfigFullText">設定ファイルの内容（全文）</param>
        Public Sub Serialize(target_prj As IClsProjectCommonVariable, ByRef ConfigFullText As String) Implements ISerializerManager.Serialize
            Dim userdefval As UserDefinedValues = target_prj.UserDefined.Values(Of UserDefinedValues)()
            ConfigFullText = objCSM.Serialize(userdefval.ConfigObject)
        End Sub
    End Class

    ''' <summary>
    ''' アプリの設定のうちDB接続の情報を保持するクラス
    ''' </summary>
    Public Class ConfigObject
        Public Property DBDataSource As String
        Public Property DBDatabaseName As String
        Public Property DBUserID_protected As String
        Public Property DBPassword_protected As String
    End Class

    ''' <summary>
    ''' アプリの設定を保持するクラス
    ''' </summary>
    Public Class UserDefinedValues
        Public FilePathList As New List(Of String)
        Public ConfigObject As New ConfigObject
    End Class

    Public Function MainTask() As Integer
        Dim fl As List(Of String)

        With prj.UserDefined.Values(Of UserDefinedValues)()
            AppCommons.DBConnectionSetting.DataSource = .ConfigObject.DBDataSource
            AppCommons.DBConnectionSetting.DatabaseName = .ConfigObject.DBDatabaseName
            AppCommons.DBConnectionSetting.UserID = .ConfigObject.DBUserID_protected
            AppCommons.DBConnectionSetting.Password = .ConfigObject.DBPassword_protected
            AppCommons.DBConnectionSetting.OpenConnection()

            fl = .FilePathList
        End With

        For i As Integer = 1 To fl.Count - 1 '0はこのプログラムのパスなので無視する
            Dim filepath As String = fl(i)

            Dim fi As New IO.FileInfo(filepath)

            If Not fi.Exists Then
                Console.WriteLine("File not found: " & filepath)
                Continue For
            End If
            Dim text As String = fi.Length.ToString()
            Dim result As String = ProcessText(text)
            Console.WriteLine("Processed: [" & filepath & "] => " & result)
        Next
        Return 0
    End Function

    Private Sub Usage()
        Console.WriteLine("Usage: SampleApp.exe <File1> <File2> ...")
    End Sub

    Public Function ProcessText(text As String) As String
        Dim result As String = ""

        Try
            Dim dt As New DSSQLSV.TYPE_D_SAMPLE_DATADataTable

            Call AppCommons.DBAccess.KICK_TESTSTOREDPROCEDURE(text, dt)

            For Each row As DSSQLSV.TYPE_D_SAMPLE_DATARow In dt.Rows
                result &= "{ uid: " & row.uid.ToString()
                result &= ", original_str: '" & row.original_str & "'"
                result &= ", castAsBigInt: " & row.castAsBigInt.ToString()
                result &= ", mul_2: " & row.mul_2
                result &= ", mul_2_AsStr: '" & row.mul_2_AsStr & "'"
                result &= ", updatedatetime: '" & row.updatedatetime.ToString("yyyy-MM-ddTHH:mm:ss.sK") & "'"
                result &= "}" & vbCrLf
            Next
        Catch ex As Exception
            If constIsDEBUG Then
                Throw ex
            Else
                Console.WriteLine(ex.Message)
            End If
        End Try
        Return result
    End Function

End Module
