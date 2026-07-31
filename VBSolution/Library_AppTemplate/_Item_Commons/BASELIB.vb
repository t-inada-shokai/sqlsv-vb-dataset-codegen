Imports System.IO
Imports System.Threading

Namespace BASELIB
    Public Module MAINMODULE
        ''' <summary>
        ''' コマンドラインアプリのメイン処理のデリゲート
        ''' </summary>
        ''' <returns>アプリの終了コード</returns>
        Public Delegate Function MainTask_Func() As Integer

        ''' <summary>
        ''' コマンドラインアプリの開始点
        ''' </summary>
        ''' <param name="args">アプリのコマンドライン引数</param>
        ''' <param name="maintask">メイン処理のデリゲート</param>
        ''' <param name="serializerManager">コマンドライン引数や設定ファイルの処理クラス</param>
        ''' <returns>アプリの終了コード</returns>
        Public Function EntryPoint(maintask As MainTask_Func, serializerManager As ISerializerManager) As Integer
            Try
                prj.AppStatus.SilentMode = True
                If maintask Is Nothing OrElse serializerManager Is Nothing Then
                    prj.AppStatus.ExitCode = 99
                    Return prj.AppStatus.ExitCode
                End If

                'アプリ開始処理
                prj.Initialize(AddressOf serializerManager.Deserialize)

                ' アプリの重複起動を検証するためのアプリ固有の名前の定義
                Dim MutexName As String = CreateMutexName()

                Dim createNew As Boolean
                Using appMutex As New Mutex(True, MutexName, createNew)
                    If Not createNew Then
                        WriteErrorLog("二重起動", New Exception("アプリケーションは既に起動しています。"))
                        If prj.AppStatus.SilentMode = False Then
                            MessageBox.Show("アプリケーションは既に起動しています。", "二重起動", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        End If
                        prj.AppStatus.ExitCode = 1
                        Return prj.AppStatus.ExitCode
                    End If

                    Try
                        'ビジュアルスタイルとテキストレンダリングの有効化
                        Application.EnableVisualStyles()
                        Application.SetCompatibleTextRenderingDefault(False)

                        '未処理例外のキャッチ設定
                        Dim handlerThreadException As New ThreadExceptionEventHandler(AddressOf OnThreadException)
                        Dim handlerUnhandledException As New UnhandledExceptionEventHandler(AddressOf OnUnhandledException)
                        AddHandler Application.ThreadException, handlerThreadException
                        AddHandler AppDomain.CurrentDomain.UnhandledException, handlerUnhandledException

                        'メイン処理
                        prj.AppStatus.ExitCode = maintask()

                    Catch ex As Exception
                        WriteErrorLog("起動処理エラー", ex)
                        If prj.AppStatus.ExitCode = 0 Then
                            prj.AppStatus.ExitCode = 99
                        End If
                    Finally
                        If createNew Then
                            appMutex.ReleaseMutex()
                        End If
                    End Try
                End Using
            Finally
                'アプリ終了処理
                prj.Terminate(AddressOf serializerManager.Serialize)
            End Try
            Return prj.AppStatus.ExitCode
        End Function

        ''' <summary>
        ''' フォームアプリの開始点
        ''' </summary>
        ''' <param name="args">アプリのコマンドライン引数</param>
        ''' <param name="mainform">メインフォーム</param>
        ''' <param name="serializerManager">コマンドライン引数や設定ファイルの処理クラス</param>
        ''' <returns>アプリの終了コード</returns>
        Public Function EntryPoint(mainform As System.Windows.Forms.Form, serializerManager As ISerializerManager) As Integer
            Try
                prj.AppStatus.SilentMode = False
                If mainform Is Nothing OrElse serializerManager Is Nothing Then
                    prj.AppStatus.ExitCode = 99
                    Return prj.AppStatus.ExitCode
                End If

                'アプリ開始処理
                prj.Initialize(AddressOf serializerManager.Deserialize)

                ' アプリの重複起動を検証するためのアプリ固有の名前の定義
                Dim MutexName As String = CreateMutexName()

                Dim createNew As Boolean
                Using appMutex As New Mutex(True, MutexName, createNew)
                    If Not createNew Then
                        WriteErrorLog("二重起動", New Exception("アプリケーションは既に起動しています。"))
                        If prj.AppStatus.SilentMode = False Then
                            MessageBox.Show("アプリケーションは既に起動しています。", "二重起動", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        End If
                        prj.AppStatus.ExitCode = 1
                        Return prj.AppStatus.ExitCode
                    End If

                    Try
                        'ビジュアルスタイルとテキストレンダリングの有効化
                        Application.EnableVisualStyles()
                        Application.SetCompatibleTextRenderingDefault(False)

                        '未処理例外のキャッチ設定
                        Dim handlerThreadException As New ThreadExceptionEventHandler(AddressOf OnThreadException)
                        Dim handlerUnhandledException As New UnhandledExceptionEventHandler(AddressOf OnUnhandledException)
                        AddHandler Application.ThreadException, handlerThreadException
                        AddHandler AppDomain.CurrentDomain.UnhandledException, handlerUnhandledException

                        'メイン処理
                        Application.Run(mainform)
                        prj.AppStatus.ExitCode = 0

                    Catch ex As Exception
                        WriteErrorLog("起動処理エラー", ex)
                        If prj.AppStatus.ExitCode = 0 Then
                            prj.AppStatus.ExitCode = 99
                        End If
                    Finally
                        If createNew Then
                            appMutex.ReleaseMutex()
                        End If
                    End Try
                End Using
            Finally
                'アプリ終了処理
                prj.Terminate(AddressOf serializerManager.Serialize)
            End Try
            Return prj.AppStatus.ExitCode
        End Function

        ''' <summary>
        ''' フォームアプリ(サイレントモード時はコマンドラインアプリ)の開始点
        ''' </summary>
        ''' <param name="args">アプリのコマンドライン引数</param>
        ''' <param name="mainform">メインフォーム</param>
        ''' <param name="silentmode_Task">サイレントモード時のメイン処理のデリゲート</param>
        ''' <param name="serializerManager">コマンドライン引数や設定ファイルの処理クラス</param>
        ''' <returns>アプリの終了コード</returns>
        Public Function EntryPoint(mainform As System.Windows.Forms.Form, silentmode_Task As MainTask_Func, serializerManager As ISerializerManager) As Integer
            Try
                prj.AppStatus.SilentMode = False
                If (mainform Is Nothing AndAlso silentmode_Task Is Nothing) OrElse serializerManager Is Nothing Then
                    prj.AppStatus.ExitCode = 99
                    Return prj.AppStatus.ExitCode
                End If

                'アプリ開始処理
                prj.Initialize(AddressOf serializerManager.Deserialize)

                ' アプリの重複起動を検証するためのアプリ固有の名前の定義
                Dim MutexName As String = CreateMutexName()

                Dim createNew As Boolean
                Using appMutex As New Mutex(True, MutexName, createNew)
                    If Not createNew Then
                        WriteErrorLog("二重起動", New Exception("アプリケーションは既に起動しています。"))
                        If prj.AppStatus.SilentMode = False Then
                            MessageBox.Show("アプリケーションは既に起動しています。", "二重起動", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        End If
                        prj.AppStatus.ExitCode = 1
                        Return prj.AppStatus.ExitCode
                    End If

                    Try
                        'ビジュアルスタイルとテキストレンダリングの有効化
                        Application.EnableVisualStyles()
                        Application.SetCompatibleTextRenderingDefault(False)

                        '未処理例外のキャッチ設定
                        Dim handlerThreadException As New ThreadExceptionEventHandler(AddressOf OnThreadException)
                        Dim handlerUnhandledException As New UnhandledExceptionEventHandler(AddressOf OnUnhandledException)
                        AddHandler Application.ThreadException, handlerThreadException
                        AddHandler AppDomain.CurrentDomain.UnhandledException, handlerUnhandledException

                        'メイン処理
                        If prj.AppStatus.SilentMode = False AndAlso mainform IsNot Nothing Then
                            Application.Run(mainform)
                            prj.AppStatus.ExitCode = 0
                        Else
                            prj.AppStatus.ExitCode = silentmode_Task()
                        End If

                    Catch ex As Exception
                        WriteErrorLog("起動処理エラー", ex)
                        If prj.AppStatus.ExitCode = 0 Then
                            prj.AppStatus.ExitCode = 99
                        End If
                    Finally
                        If createNew Then
                            appMutex.ReleaseMutex()
                        End If
                    End Try
                End Using
            Finally
                'アプリ終了処理
                prj.Terminate(AddressOf serializerManager.Serialize)
            End Try
            Return prj.AppStatus.ExitCode
        End Function

        Private Sub OnThreadException(sender As Object, e As System.Threading.ThreadExceptionEventArgs)
            Call WriteErrorLog("画面スレッド例外", e.Exception)
            If prj.AppStatus.SilentMode = False Then
                MessageBox.Show(
                    $"画面エラーが発生しました: {e.Exception.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End If
        End Sub

        Private Sub OnUnhandledException(sender As Object, e As System.UnhandledExceptionEventArgs)
            Dim ex As Exception = TryCast(e.ExceptionObject, Exception)
            Call WriteErrorLog("システム致命的例外", ex)

            If prj.AppStatus.SilentMode = False Then
                MessageBox.Show(
                    $"システムエラーが発生しました: {If(ex IsNot Nothing, ex.Message, "不明なエラー")}",
                    "致命的エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End If
        End Sub

        ''' <summary>
        ''' バイナリと同じ位置にエラーログを書き出す
        ''' </summary>
        Public Sub WriteErrorLog(ByVal errorType As String, ByVal ex As Exception)
            Try
                '実行ファイル（バイナリ）と同じフォルダーのパスを取得
                Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory
                Dim logPath As String = Path.Combine(baseDir, "error.log")

                'ログメッセージの組み立て
                Dim sb As New System.Text.StringBuilder()
                sb.AppendLine($"==================================================")
                sb.AppendLine($"発生日時: {DateTime.Now:yyyy/MM/dd HH:mm:ss}")
                sb.AppendLine($"エラー種別: {errorType}")
                If ex IsNot Nothing Then
                    sb.AppendLine($"メッセージ: {ex.Message}")
                    sb.AppendLine($"スタックトレース:")
                    sb.AppendLine(ex.StackTrace)
                Else
                    sb.AppendLine($"メッセージ: 例外情報が取得できませんでした。")
                End If
                sb.AppendLine()

                '追記モードでファイルに書き込み
                ' （Program Files等でアクセス拒否された場合に備え、ここは例外を想定しておく）
                File.AppendAllText(logPath, sb.ToString(), System.Text.Encoding.UTF8)

            Catch logEx As Exception
                ' ログ書き込み自体が権限不足等で失敗した場合、アプリが無限ループで落ちるのを防ぐため、
                ' ここではあえて何もせずエラーを握りつぶします（最悪、デバッグ出力にのみ流す）
                System.Diagnostics.Debug.WriteLine($"ログ書き込み失敗: {logEx.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' アプリの重複起動を検証するためのアプリ固有の名前の定義
        ''' </summary>
        Private Function CreateMutexName() As String
            Const Mutex_Prefix As String = "MyApp_"
            Dim asm As System.Reflection.Assembly = System.Reflection.Assembly.GetExecutingAssembly()
            Dim attr As System.Runtime.InteropServices.GuidAttribute =
            CType(asm.GetCustomAttributes(GetType(System.Runtime.InteropServices.GuidAttribute), True)(0),
                System.Runtime.InteropServices.GuidAttribute)
            Dim ans As String = Mutex_Prefix & attr.Value
            If ans.Length > 260 Then
                ans = ans.Substring(0, 250)
            End If
            Return ans
        End Function
    End Module

    Public Module ProjectCommonVariable
        Public prj As New ClsProjectCommonVariable

        Interface IClsProjectCommonVariable
            'ReadOnly Property DBAccess As BASELIB.DBAccess.Cls基底
            ReadOnly Property CommandLineArgs As ProjectCommonVariable.IClsCommandLineArgs
            ReadOnly Property ConfigSettings As ProjectCommonVariable.IClsConfigSettings
            ReadOnly Property AppStatus As ProjectCommonVariable.IClsAppStatus
            ReadOnly Property UserDefined As ProjectCommonVariable.IClsUserDefined

            Sub Initialize(deserializer As DeserializerDelegate)
            Sub Terminate(serializer As SerializerDelegate)

            Delegate Sub DeserializerDelegate(target_prj As IClsProjectCommonVariable, configFullText As String, commandLineArgs As String())
            Delegate Sub SerializerDelegate(target_prj As IClsProjectCommonVariable, ByRef configFullText As String)
        End Interface

        Interface ISerializerManager
            Sub Deserialize(target_prj As IClsProjectCommonVariable, configFullText As String, commandLineArgs As String())
            Sub Serialize(target_prj As IClsProjectCommonVariable, ByRef configFullText As String)
        End Interface

        Interface IClsCommandLineArgs
            Sub SetRawArgs(args As String())
            ReadOnly Property RawArgs As String()
        End Interface

        Interface IClsConfigSettings
            'Function Settings(Of T As {New, Class})() As T
            Sub Load()
            'Sub Load(Of T As {New, Class})()
            Sub Save()
            'Sub Save(Of T As {Class})(settings As T)
            Property ConfigFullText As String
        End Interface

        Interface IClsAppStatus
            Property SilentMode As Boolean
            Property ExitCode As Integer
        End Interface

        Interface IClsUserDefined
            Function Values(Of T As {New, Class})() As T
            Function SetValues(Of T As {New, Class})(obj As T) As T
        End Interface

        Public Class ClsProjectCommonVariable
            Implements IClsProjectCommonVariable

            'Public ReadOnly Property DBAccess As New BASELIB.DBAccess.Cls基底 Implements IClsProjectCommonVariable.DBAccess
            Public ReadOnly Property CommandLineArgs As IClsCommandLineArgs = New ClsCommandLineArgs Implements IClsProjectCommonVariable.CommandLineArgs
            Public ReadOnly Property ConfigSettings As IClsConfigSettings = New ClsConfigSettings Implements IClsProjectCommonVariable.ConfigSettings
            Public ReadOnly Property AppStatus As IClsAppStatus = New ClsAppStatus Implements IClsProjectCommonVariable.AppStatus
            Public ReadOnly Property UserDefined As IClsUserDefined = New ClsUserDefined Implements IClsProjectCommonVariable.UserDefined

            Public Class ClsConfigSettings
                Implements IClsConfigSettings
                Private innerText As String
                'Private innerSettings As Object
                Private ReadOnly myConfigManager As BASELIB.ConfigSettingsManager.IConfigSettingsManager
                Private ReadOnly myConfigLoader As BASELIB.ConfigSettingsManager.DefaultConfigLoaderSaver

                Public Sub New()
                    myConfigManager = New BASELIB.ConfigSettingsManager.JsonCryptSettingManager
                    myConfigLoader = New ConfigSettingsManager.DefaultConfigLoaderSaver
                End Sub

                Public Sub New(configManager As BASELIB.ConfigSettingsManager.IConfigSettingsManager)
                    myConfigManager = configManager
                    myConfigLoader = New ConfigSettingsManager.DefaultConfigLoaderSaver
                End Sub

                'Public Function Settings(Of T As {New, Class})() As T Implements IClsConfigSettings.Settings
                '    If innerSettings Is Nothing Then
                '        innerSettings = myConfigManager.Deserialize(Of T)(innerText)
                '    End If
                '    Return DirectCast(innerSettings, T)
                'End Function

                Public Sub Load() Implements IClsConfigSettings.Load
                    innerText = myConfigLoader.Load()
                    'innerSettings = Nothing
                End Sub

                'Public Sub Load(Of T As {New, Class})() Implements IClsConfigSettings.Load
                '    innerText = myConfigLoader.Load()
                '    innerSettings = myConfigManager.Deserialize(Of T)(innerText)
                'End Sub

                Public Sub Save() Implements IClsConfigSettings.Save
                    'innerText = myConfigManager.Serialize(innerSettings)
                    myConfigLoader.Save(innerText)
                End Sub

                'Public Sub Save(Of T As {Class})(settings As T) Implements IClsConfigSettings.Save
                '    innerText = myConfigManager.Serialize(settings)
                '    myConfigLoader.Save(innerText)
                'End Sub

                Public Property ConfigFullText As String Implements IClsConfigSettings.ConfigFullText
                    Get
                        Return innerText
                    End Get
                    Set(value As String)
                        innerText = value
                    End Set
                End Property
            End Class

            Public Class ClsCommandLineArgs
                Implements IClsCommandLineArgs

                Private _RawArgs As String() = {}

                Public Sub SetRawArgs(args As String()) Implements IClsCommandLineArgs.SetRawArgs
                    _RawArgs = args
                End Sub

                Public ReadOnly Property RawArgs As String() Implements IClsCommandLineArgs.RawArgs
                    Get
                        Return _RawArgs
                    End Get
                End Property
            End Class

            Public Class ClsAppStatus
                Implements IClsAppStatus
                Public Property SilentMode As Boolean Implements IClsAppStatus.SilentMode
                Public Property ExitCode As Integer Implements IClsAppStatus.ExitCode
            End Class

            Public Class ClsUserDefined
                Implements IClsUserDefined
                Private innerValues As Object
                Public Function Values(Of T As {New, Class})() As T Implements IClsUserDefined.Values
                    If innerValues Is Nothing Then
                        innerValues = New T()
                    End If
                    Return DirectCast(innerValues, T)
                End Function
                Public Function SetValues(Of T As {New, Class})(obj As T) As T Implements IClsUserDefined.SetValues
                    innerValues = obj
                    Return DirectCast(innerValues, T)
                End Function
            End Class

            Public Sub Initialize(deserializer As IClsProjectCommonVariable.DeserializerDelegate) Implements IClsProjectCommonVariable.Initialize

                Me.CommandLineArgs.SetRawArgs(Environment.GetCommandLineArgs())
                Me.ConfigSettings.Load()

                deserializer(Me, Me.ConfigSettings.ConfigFullText, Me.CommandLineArgs.RawArgs)

                'Me.DBAccess.Initialize()
            End Sub

            Public Sub Terminate(serializer As IClsProjectCommonVariable.SerializerDelegate) Implements IClsProjectCommonVariable.Terminate
                serializer(Me, Me.ConfigSettings.ConfigFullText)

                Me.ConfigSettings.Save()
            End Sub
        End Class
    End Module
End Namespace
