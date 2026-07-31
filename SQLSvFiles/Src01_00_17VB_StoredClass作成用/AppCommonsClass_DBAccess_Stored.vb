Imports System.Data.SqlClient

Namespace AppCommonsClass
    Friend Class ClsDBAccess_Stored
        Protected Friend MustInherit Class StoredProcedure_base
            Implements IDisposable

            Private disposedValue As Boolean = False
            Private ReadOnly privateDS As New DataSet

            Public Property Command As SqlCommand

            Public MustOverride Sub Initialize(Command As SqlCommand)
            Protected Friend MustOverride Function OriginalStoredProcedureCommandLine() As String

            Friend Function ParamRead(Of X)(name As String) As X
                Dim ans As X
                If IsDBNull(Me.Command.Parameters.Item(name).Value) Then
                    ans = Nothing
                Else
                    ans = CType(Me.Command.Parameters.Item(name).Value, X)
                End If
                Return ans
            End Function

            Friend Sub ParamWrite(Of X)(name As String, value As X)
                Dim ans As Object
                If IsNothing(value) Then
                    ans = DBNull.Value
                Else
                    ans = value
                End If
                Me.Command.Parameters.Item(name).Value = ans
            End Sub

            Public Overloads Function AdapterFill() As Integer
                Using adp As New SqlDataAdapter(Me.Command)
                    Return adp.Fill(privateDS)
                End Using
            End Function

            Public Overloads Function AdapterFill(ds As DataSet) As Integer
                Using adp As New SqlDataAdapter(Me.Command)
                    Return adp.Fill(ds)
                End Using
            End Function

            Public Overloads Function AdapterFill(ParamArray dts() As DataTable) As Integer
                Dim ans As Integer = 0
                Using reader As SqlDataReader = Me.Command.ExecuteReader()
                    For i As Integer = LBound(dts) To UBound(dts)
                        dts(i).Load(reader)
                        ans += dts(i).Rows.Count
                        If reader.NextResult() = False Then
                            Exit For
                        End If
                    Next
                End Using
                Return ans
            End Function

            Public Function ExecuteNonQuery() As Integer
                Return Me.Command.ExecuteNonQuery()
            End Function

            Public Function GetTableAs(Of X As {New, DataTable})(index As Integer) As X
                Dim adjust_index As Integer
                If privateDS.Tables.Count > 0 Then
                    adjust_index = index Mod privateDS.Tables.Count
                Else
                    Return Nothing
                End If
                Dim dt As New X
                dt.Load(privateDS.Tables(adjust_index).CreateDataReader)
                Return dt
            End Function

            Public Function GetTable(index As Integer) As DataTable
                Dim adjust_index As Integer
                If privateDS.Tables.Count > 0 Then
                    adjust_index = index Mod privateDS.Tables.Count
                Else
                    Return Nothing
                End If
                Dim dt As New DataTable
                dt.Load(privateDS.Tables(adjust_index).CreateDataReader)
                Return dt
            End Function

            Public Function GetDataSet() As DataSet
                Return privateDS
            End Function

            Protected Overridable Sub Dispose(disposing As Boolean)
                If Not disposedValue Then
                    If disposing Then
                        ' donothing
                    End If

                    disposedValue = True
                End If
            End Sub

            Public Sub Dispose() Implements IDisposable.Dispose
                Dispose(disposing:=True)
                GC.SuppressFinalize(Me)
            End Sub
        End Class

        Public Class Proc_TEST_STOREDPROCEDURE
            Inherits StoredProcedure_base
            Private Const const_outputtable_result As Integer = -1

            Protected Friend Overrides Function OriginalStoredProcedureCommandLine() As String
                Dim sql As String = ""
                sql &= "select top (0) * into #outputtable_result from @outputtable_result;"
                sql &= "exec dbo.TEST_STOREDPROCEDURE @p1=@p1,@outputtable_result=@outputtable_result;"
                sql &= "select * from #outputtable_result;"

                Return sql
            End Function

            Public Overrides Sub Initialize(command As SqlCommand)
                Me.Command = command

                Me.Command.CommandText = Me.OriginalStoredProcedureCommandLine()

                Me.Command.Parameters.Add(New SqlParameter("@p1", SqlDbType.NVarChar, 20))

                Me.Command.Parameters.Add(New SqlParameter("@outputtable_result", SqlDbType.Structured) With {.TypeName = "dbo.TYPE_D_SAMPLE_DATA"})

            End Sub

            Public WriteOnly Property Param_p1 As String
                Set(value As String)
                    Me.ParamWrite(Of String)("@p1", value)
                End Set
            End Property

            Public Property Param_outputtable_result As DSSQLSV.TYPE_D_SAMPLE_DATADataTable
                Get
                    Return Me.GetTableAs(Of DSSQLSV.TYPE_D_SAMPLE_DATADataTable)(const_outputtable_result)
                End Get
                Set(value As DSSQLSV.TYPE_D_SAMPLE_DATADataTable)
                    If IsNothing(value) Then
                        value = New DSSQLSV.TYPE_D_SAMPLE_DATADataTable
                    End If
                    Me.ParamWrite(Of DSSQLSV.TYPE_D_SAMPLE_DATADataTable)("@outputtable_result", value)
                End Set
            End Property
        End Class
    End Class
End Namespace

