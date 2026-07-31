Imports System.Data.SqlClient

Module AppCommons
    Public DBConnectionSetting As New AppCommonsClass.ClsDBConnectionSettings
    Public DBAccess As New AppCommonsClass.ClsDBAccess

    Public Const constDefaultTripDistanceMax As Integer = 500

    Public TripDistanceMax As Integer = constDefaultTripDistanceMax

    Public PersistentValues As New AppCommonsClass.ClsPersistentData

#If DEBUG = False Then
    Public Const constIsDEBUG As Boolean = False
#End If
#If DEBUG = True Then
    Public Const constIsDEBUG As Boolean = True
#End If
End Module

Namespace AppCommonsClass
    Public Class ClsVBLanguages
        Public Shared Function IIf(Of T)(decision As Boolean, v1 As T, v2 As T) As T
            If decision Then
                Return v1
            Else
                Return v2
            End If
        End Function

        Public Shared Sub SkipEvent(Of T)(item As T, job As Action(Of T))
            Dim tempItem As T = item
#Disable Warning IDE0059 ' 値の不必要な代入
            item = Nothing
#Enable Warning IDE0059 ' 値の不必要な代入
            job(tempItem)
#Disable Warning IDE0059 ' 値の不必要な代入
            item = tempItem
            tempItem = Nothing
#Enable Warning IDE0059 ' 値の不必要な代入
        End Sub
    End Class

    Public Class ClsPersistentData
        Private PrvDicOfValues As New Dictionary(Of String, ClsItemValues)

        Public Class ClsItemValues
            Public KeyName As String
            Public ValueObject As Object = Nothing
            Public ValueTypes As String
        End Class

#Region "辞書の保存復元"
        ''' <summary>
        ''' Base64文字列からオブジェクトを復元
        ''' </summary>
        ''' <param name="vBackupStr"></param>
        Public Sub RestoreValues(vBackupStr As String)
            Try
                Dim serializer As New System.Xml.Serialization.XmlSerializer(GetType(List(Of ClsItemValues)))
                Dim tr As New IO.StringReader(vBackupStr)
                Dim xr As Xml.XmlReader = Xml.XmlReader.Create(tr)
                Dim tmpObj As Object = serializer.Deserialize(xr)
                Dim tmpDic As List(Of ClsItemValues) = CType(tmpObj, List(Of ClsItemValues))
                PrvDicOfValues.Clear()
                For Each i In tmpDic
                    PrvDicOfValues.Add(i.KeyName, i)
                Next
            Catch
            End Try
            If PrvDicOfValues IsNot Nothing Then
                Return
            End If
            PrvDicOfValues = New Dictionary(Of String, ClsItemValues)
        End Sub

        ''' <summary>
        ''' オブジェクトをBase64文字列化
        ''' </summary>
        ''' <param name="vBackupStr"></param>
        Public Sub BackupValues(ByRef vBackupStr As String)
            Dim tmpObj As New List(Of ClsItemValues)
            For Each i In PrvDicOfValues
                Dim tmpItem As New ClsItemValues With {
                    .KeyName = i.Key,
                    .ValueObject = i.Value.ValueObject,
                    .ValueTypes = i.Value.ValueTypes
                }
                tmpObj.Add(tmpItem)
            Next
            Dim tw As New IO.StringWriter
            Dim settings As New Xml.XmlWriterSettings With {
                .Encoding = New Text.UTF8Encoding(False)
            }
            Dim xw As Xml.XmlWriter = Xml.XmlWriter.Create(tw, settings)
            Dim serializer As New Xml.Serialization.XmlSerializer(GetType(List(Of ClsItemValues)))
            serializer.Serialize(xw, tmpObj)
            vBackupStr = tw.ToString 'System.Convert.ToBase64String(tw.ToArray)
        End Sub
#End Region

#Region "辞書アクセス"
        Public Sub SaveValue(key As String, value As Object)
            Dim tmpItemValue As New ClsItemValues With {
                .ValueTypes = value.GetType().ToString(),
                .ValueObject = value
            }
            If PrvDicOfValues.ContainsKey(key) Then
                PrvDicOfValues(key) = tmpItemValue
            Else
                PrvDicOfValues.Add(key, tmpItemValue)
            End If
        End Sub

        Public Function TryLoadValue(key As String, ByRef value As Object) As Boolean
            If PrvDicOfValues.ContainsKey(key) Then
                Dim tmpValue As ClsItemValues = PrvDicOfValues(key)
                value = tmpValue.ValueObject
                Return True
            Else
                Return False
            End If
        End Function

        Public Function TryLoadValue(Of T)(key As String, ByRef value As T, Optional ByRef typeMissMatch As Boolean = True) As Boolean
            If PrvDicOfValues.ContainsKey(key) Then
                Dim tmpValue As ClsItemValues = PrvDicOfValues(key)
                If tmpValue.ValueTypes = GetType(T).ToString() Then
                    value = CType(tmpValue.ValueObject, T)
                    typeMissMatch = False
                    Return True
                Else
                    typeMissMatch = True
                    Return False
                End If
            Else
                Return False
            End If
        End Function

        Public Function LoadValue(key As String) As Object
            Dim result As Object = Nothing
            Dim typemissmatch As Boolean = False
            If TryLoadValue(key, result, typemissmatch) Then
                Return result
            Else
                If typemissmatch = True Then
                    Throw New TypeAccessException("値の型が指定した型と一致しません")
                Else
                    Throw New KeyNotFoundException("該当するキーが見つかりません")
                End If
            End If
        End Function

        Public Function LoadValue(Of T)(key As String) As T
            Dim result As T = Nothing
            Dim typemissmatch As Boolean = False
            If TryLoadValue(Of T)(key, result, typemissmatch) Then
                Return result
            Else
                If typemissmatch = True Then
                    Throw New TypeAccessException("値の型が指定した型と一致しません")
                Else
                    Throw New KeyNotFoundException("該当するキーが見つかりません")
                End If
            End If
        End Function

        Public Function IsExistsKey(key As String) As Boolean
            Return PrvDicOfValues.ContainsKey(key)
        End Function

        Public Function GetValueType(key As String) As String
            If PrvDicOfValues.ContainsKey(key) Then
                Return PrvDicOfValues(key).ValueTypes
            Else
                Throw New KeyNotFoundException("該当するキーが見つかりません")
            End If
        End Function

        Public Function TryGetValueType(key As String, ByRef valueType As String) As Boolean
            If PrvDicOfValues.ContainsKey(key) Then
                valueType = PrvDicOfValues(key).ValueTypes
                Return True
            Else
                Return False
            End If
        End Function
#End Region
    End Class

    Public Class ClsDBConnectionSettings
        Private prvDataSource As String = ""
        Private prvDatabaseName As String = ""
        Private prvUserID As String = ""
        Private prvPassword As String = ""
        Private prvIsConnectOK As Boolean = False

        Public Property DataSource As String
            Get
                Return prvDataSource
            End Get
            Set(value As String)
                prvDataSource = value
                connstr = ""
            End Set
        End Property
        Public Property DatabaseName As String
            Get
                Return prvDatabaseName
            End Get
            Set(value As String)
                prvDatabaseName = value
                connstr = ""
            End Set
        End Property
        Public Property UserID As String
            Get
                Return prvUserID
            End Get
            Set(value As String)
                prvUserID = value
                connstr = ""
            End Set
        End Property
        Public Property Password As String
            Get
                Return prvPassword
            End Get
            Set(value As String)
                prvPassword = value
                connstr = ""
            End Set
        End Property
        Public Property IsConnectOK As Boolean
            Get
                Return prvIsConnectOK
            End Get
            Private Set(value As Boolean)
                prvIsConnectOK = value
                If value = False Then
                    connstr = ""
                End If
            End Set
        End Property

        Private connstr As String = ""

        Public ReadOnly Property ConnectionString As String
            Get
                If connstr = "" Then
                    connstr = ClsDBConnectionSettings.GetConnectionString(Me.DataSource, Me.DatabaseName, Me.UserID, Me.Password)
                End If
                Return connstr
            End Get
        End Property

        Public Shared Function GetConnectionString(DataSource As String, DatabaseName As String, UserID As String, Password As String) As String
            Dim csb As New SqlConnectionStringBuilder()
            Dim ans As String '= ""
            Try
                csb.DataSource = DataSource
                csb.InitialCatalog = DatabaseName
                If UserID = "" And Password = "" Then
                    'Windows認証
                    csb.UserID = ""
                    csb.Password = ""
                    csb.IntegratedSecurity = True
                Else
                    'SQLServer認証
                    csb.UserID = UserID
                    csb.Password = Password
                    csb.IntegratedSecurity = False
                End If
                csb.ConnectTimeout = 5
                csb.ConnectRetryCount = 0
                csb.ConnectRetryInterval = 10
                ans = csb.ToString()
            Catch ex As Exception
                If constIsDEBUG Then
                    Throw ex
                Else
                    'donothing
                    ans = ""
                End If
            End Try
            Return ans
        End Function

        Public Function OpenConnection(Optional raiseException As Boolean = False) As Boolean
            Dim ans As Boolean '= False

            Try
                ans = ClsDBConnectionSettings.OpenConnection(Me.DataSource, Me.DatabaseName, Me.UserID, Me.Password, raiseException)
                Me.IsConnectOK = ans
            Catch
                Throw
            End Try

            Return ans
        End Function

        Public Shared Function OpenConnection(DataSource As String, DatabaseName As String, UserID As String, Password As String, Optional raiseException As Boolean = False) As Boolean
            Dim ans As Boolean = False
            Dim connstr As String = AppCommonsClass.ClsDBConnectionSettings.GetConnectionString(
                DataSource, DatabaseName, UserID, Password
            )

            Try
                Dim dt As New DataTable
                Using conn As New SqlConnection(connstr)
                    Using com As New SqlCommand("select name from sys.tables;")
                        conn.Open()
                        com.Connection = conn
                        Dim rd As New SqlDataAdapter(com)
                        rd.Fill(dt)
                        conn.Close()
                    End Using
                End Using
                If dt.Rows.Count > 0 Then
                    ans = True
                End If
            Catch
                If raiseException Then
                    Throw
                End If
            End Try
            Return ans
        End Function

    End Class

    Public Class ClsAddModifyDeleteTableArray(Of T As DataTable)
        Private WithEvents PrvBasetable As T
        Private PrvDeleteTable As T

        Public Sub New(ByVal basetable As T)
            Me.ResetBase(basetable)
        End Sub

        Public Sub ResetBase(ByVal basetable As T)
            PrvBasetable = basetable
            PrvDeleteTable = CType(Activator.CreateInstance(GetType(T)), T)
        End Sub

        Public Sub AcceptChanges()
            PrvBasetable.AcceptChanges()
            PrvDeleteTable.Clear()
        End Sub

        Public ReadOnly Property Base As T
            Get
                Return PrvBasetable
            End Get
        End Property

        Public ReadOnly Property Deleted As T
            Get
                Return PrvDeleteTable
            End Get
        End Property

        Public ReadOnly Property Inserted As T
            Get
                Dim PrvInsertedTable As T = CType(Activator.CreateInstance(GetType(T)), T)
                For Each row In PrvBasetable.Select("", "", DataViewRowState.Added)
                    Dim newrow = PrvInsertedTable.NewRow
                    newrow.ItemArray = row.ItemArray
                    PrvInsertedTable.Rows.Add(newrow)
                Next
                Return PrvInsertedTable
            End Get
        End Property

        Public ReadOnly Property Updated As T
            Get
                Dim PrvUpdatedTable As T = CType(Activator.CreateInstance(GetType(T)), T)
                For Each row In PrvBasetable.Select("", "", DataViewRowState.ModifiedCurrent)
                    Dim newrow = PrvUpdatedTable.NewRow
                    newrow.ItemArray = row.ItemArray
                    PrvUpdatedTable.Rows.Add(newrow)
                Next
                Return PrvUpdatedTable
            End Get
        End Property

        Private Sub PrvBasetable_RowDeleting(sender As Object, e As DataRowChangeEventArgs) Handles PrvBasetable.RowDeleting
            Dim newrow = PrvDeleteTable.NewRow
            newrow.ItemArray = e.Row.ItemArray
            PrvDeleteTable.Rows.Add(newrow)
        End Sub

        Private Sub PrvBasetable_TableCleared(sender As Object, e As DataTableClearEventArgs) Handles PrvBasetable.TableCleared
            PrvDeleteTable.Clear()
        End Sub
    End Class

    Public Class ClsDBAccess
        Public Const constCommandTimeout As Integer = 300

        Public Structure STCCircularFence
            Public Lontitude As Decimal
            Public Latitude As Decimal
            Public Radius As Decimal
        End Structure

        Public Structure STCRectangleFence
            Public LeftLongitude As Decimal
            Public TopLatitude As Decimal
            Public RightLongitude As Decimal
            Public BottomLatitude As Decimal
        End Structure

        Public Function KICK_TESTSTOREDPROCEDURE(text As String, ByRef dt As DSSQLSV.TYPE_D_SAMPLE_DATADataTable) As Boolean

            If AppCommons.DBConnectionSetting.IsConnectOK = False Then
                Return False
            End If

            Using conn As New SqlConnection()
                conn.ConnectionString = AppCommons.DBConnectionSetting.ConnectionString
                conn.Open()

                Using com As New SqlCommand() With {.Connection = conn, .CommandTimeout = constCommandTimeout}
                    Using proc As New AppCommonsClass.ClsDBAccess_Stored.Proc_TEST_STOREDPROCEDURE

                        proc.Initialize(com)

                        If text.Length > 20 Then
                            text = text.Substring(0, 20)
                        End If
                        proc.Param_p1 = text

                        proc.AdapterFill()

                        dt = proc.Param_outputtable_result
                    End Using
                End Using

                conn.Close()
            End Using

            Return True
        End Function

    End Class
End Namespace