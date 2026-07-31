Imports System.IO
Imports System.Reflection
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Text.Unicode

Namespace BASELIB.ConfigSettingsManager
    Public Interface IConfigLoaderSaver
        ReadOnly Property ConfigPath As String

        Function Load() As String

        Sub Save(settings As String)
    End Interface

    Public Interface IConfigSettingsManager
        Function Deserialize(Of T As {Class, New})(source As String) As T

        Function Serialize(Of T As Class)(settings As T) As String
    End Interface

    Public Class DefaultConfigLoaderSaver
        Implements IConfigLoaderSaver

        ''' <summary>
        ''' 設定ファイルのフルパス
        ''' </summary>
        Public Overridable ReadOnly Property ConfigPath As String Implements IConfigLoaderSaver.ConfigPath
            Get
                Dim exePath As String = [Assembly].GetEntryAssembly().Location
                Return Path.ChangeExtension(exePath, ".json")
            End Get
        End Property

        Public Overridable Function Load() As String Implements IConfigLoaderSaver.Load
            If Not File.Exists(ConfigPath) Then
                Return String.Empty
            End If
            Try
                Return File.ReadAllText(ConfigPath, Encoding.UTF8)
            Catch
                Return String.Empty
            End Try
        End Function

        Public Overridable Sub Save(settings As String) Implements IConfigLoaderSaver.Save
            If String.IsNullOrEmpty(settings) Then Return
            Try
                File.WriteAllText(ConfigPath, settings, Encoding.UTF8)
            Catch
                Throw
            End Try
        End Sub
    End Class

    Public Class DefaultConfigSettingsManager
        Implements IConfigSettingsManager

        Public Overridable Function Deserialize(Of T As {Class, New})(source As String) As T Implements IConfigSettingsManager.Deserialize
            Return JsonSerializer.Deserialize(Of T)(source)
        End Function

        Public Overridable Function Serialize(Of T As Class)(settings As T) As String Implements IConfigSettingsManager.Serialize
            Return JsonSerializer.Serialize(settings)
        End Function
    End Class

    Public Class JsonCryptSettingManager
        Inherits DefaultConfigSettingsManager
        Implements IConfigSettingsManager

        ''' <summary>
        ''' 暗号化対象となるプロパティ名の正規表現パターン（末尾が _Protected）
        ''' </summary>
        Public ReadOnly PatternOfEncryptedProperties As String = "(?i)_Protected$"

        ''' <summary>
        ''' 読み取り専用（内部変更を保存しない）プロパティ名の正規表現パターン（末尾が _ro）
        ''' </summary>
        Public ReadOnly PatternOfReadOnlyProperties As String = "(?i)_ro$"

        ' 読み込み時のファイルの状態を一時保存するバッファ
        Private _originalRootCache As JsonNode

        ''' <summary>
        ''' インスタンスを生成して返します
        ''' </summary>
        Public Overrides Function Deserialize(Of T As {Class, New})(source As String) As T Implements IConfigSettingsManager.Deserialize
            _originalRootCache = Nothing

            Try
                ' ファイルから読んだ生の状態をキャッシュ
                Dim cacheRoot As JsonNode = JsonNode.Parse(source)
                _originalRootCache = cacheRoot.DeepClone()

                Using doc As JsonDocument = JsonDocument.Parse(source)
                    Dim rootElement As JsonElement = doc.RootElement
                    Dim settings As T = JsonSerializer.Deserialize(Of T)(source)

                    'トップレベルのプロパティのみ検証
                    For Each prop In GetType(T).GetProperties(BindingFlags.Public Or BindingFlags.Instance)
                        ' 暗号化項目の復号
                        If System.Text.RegularExpressions.Regex.IsMatch(prop.Name, PatternOfEncryptedProperties) Then
                            If prop.PropertyType = GetType(String) Then
                                Dim jsonProp As JsonElement = Nothing
                                If rootElement.TryGetProperty(prop.Name, jsonProp) Then
                                    Dim encryptedValue As String = jsonProp.GetString()
                                    prop.SetValue(settings, PrvDecryptString(encryptedValue))
                                End If
                            End If
                        End If
                    Next

                    Return settings
                End Using
            Catch
                _originalRootCache = Nothing
                Return New T()
            End Try
        End Function

        ''' <summary>
        ''' 現在の設定インスタンスをJSONファイルに保存します
        ''' </summary>
        Public Overrides Function Serialize(Of T As Class)(settings As T) As String Implements IConfigSettingsManager.Serialize
            If settings Is Nothing Then Return String.Empty

            Try
                Dim source As String = JsonSerializer.Serialize(settings)
                Dim rootNode As JsonNode = JsonNode.Parse(source)
                Dim jsonObject As JsonObject = rootNode.AsObject()

                For Each prop In GetType(T).GetProperties(BindingFlags.Public Or BindingFlags.Instance)
                    ' 1. _ro 項目の処理
                    If _originalRootCache IsNot Nothing AndAlso System.Text.RegularExpressions.Regex.IsMatch(prop.Name, PatternOfReadOnlyProperties) Then
                        Dim cacheObj As JsonObject = TryCast(_originalRootCache, JsonObject)
                        If cacheObj IsNot Nothing AndAlso cacheObj.ContainsKey(prop.Name) AndAlso cacheObj(prop.Name) IsNot Nothing Then
                            jsonObject(prop.Name) = cacheObj(prop.Name).DeepClone()
                        End If
                    End If

                    ' 2. _Protected 項目の処理
                    If System.Text.RegularExpressions.Regex.IsMatch(prop.Name, PatternOfEncryptedProperties) Then
                        If prop.PropertyType = GetType(String) Then
                            If jsonObject.ContainsKey(prop.Name) Then
                                Dim plainText As String = jsonObject(prop.Name)?.ToString()
                                jsonObject(prop.Name) = JsonValue.Create(PrvEncryptString(plainText))
                            End If
                        End If
                    End If
                Next

                ' すべての日本語（Unicode範囲）をエスケープせずに出力する
                Dim options As New JsonSerializerOptions With {
                    .WriteIndented = True,
                    .Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                }

                Dim secureJson As String = jsonObject.ToJsonString(options)
                Return secureJson

            Catch
                Throw
            End Try
        End Function

#Region "暗号化・復号化ロジック（DPAPI）"
        Private Shared Function PrvDecryptString(encryptedText As String) As String
            If String.IsNullOrEmpty(encryptedText) Then Return encryptedText
            Try
                Dim isBase64 As Boolean = (encryptedText.Length Mod 4 = 0) AndAlso
                System.Text.RegularExpressions.Regex.IsMatch(encryptedText, "^[a-zA-Z0-9\+/]*={0,3}$")
                If Not isBase64 Then Return encryptedText

                Dim encryptedBytes As Byte() = Convert.FromBase64String(encryptedText)
                Dim plainBytes As Byte() = ProtectedData.Unprotect(encryptedBytes, Nothing, DataProtectionScope.CurrentUser)
                Return Encoding.UTF8.GetString(plainBytes)
            Catch
                Return encryptedText
            End Try
        End Function

        Private Shared Function PrvEncryptString(plainText As String) As String
            If String.IsNullOrEmpty(plainText) Then Return plainText
            Try
                Dim plainBytes As Byte() = Encoding.UTF8.GetBytes(plainText)
                Dim encryptedBytes As Byte() = ProtectedData.Protect(plainBytes, Nothing, DataProtectionScope.CurrentUser)
                Return Convert.ToBase64String(encryptedBytes)
            Catch
                Return plainText
            End Try
        End Function
#End Region
    End Class

End Namespace