Module MainModule

    Sub Main(Args() As String)
        If Args.Count <> 0 Then
            Exit Sub
        End If

        Try
            Console.InputEncoding = Text.Encoding.UTF8
            Dim readbuff As New List(Of String)
            Dim lineread As String = Console.In.ReadLine
            Do While lineread IsNot Nothing
                readbuff.Add(lineread)
                lineread = Console.In.ReadLine
            Loop

            Dim builder As New Text.StringBuilder()
            builder.AppendLine(readbuff(0))
            For j As Integer = 1 To readbuff.Count - 1
                builder.AppendLine(SortInLine(readbuff(j)))
            Next

            Console.Out.WriteLine(builder.ToString())

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Function SortInLine(source As String) As String
        Dim ans As String = source

        If Trim(source).StartsWith("</") Then
            Return ans
        End If

        Dim sp As Integer = 0
        Dim indentStr As String = ""
        While source(sp) = " "c
            indentStr &= " "c
            sp += 1
        End While
        Dim ep As Integer = source.Length - 1
        Dim lastStr As String
        If source.EndsWith(" />") Then
            ep -= 3
            lastStr = " />"
        Else
            ep -= 1
            lastStr = ">"
        End If

        Dim firstStr As String = ""
        While source(sp) <> " "c And sp <= ep
            firstStr &= source(sp)
            sp += 1
        End While

        If sp >= ep Then
            Return ans
        End If

        Dim phrase As String = ""
        Dim phraselist As New List(Of String)
        Dim workp As Integer = 0
        While PickupPhrase(source, sp, ep, phrase, workp) And sp <= ep
            phraselist.Add(phrase)
            sp = workp
        End While

        If phraselist.Count <= 1 Then
            Return ans
        End If

        phraselist.Sort()

        ans = indentStr & firstStr
        For Each s As String In phraselist
            ans &= " "c & s
        Next
        ans &= lastStr

        Return ans
    End Function

    Private Function PickupPhrase(source As String, startpt As Integer, endpt As Integer,
                ByRef phrase As String, ByRef nextpt As Integer) As Boolean
        Dim ans As Boolean = False
        Dim pt As Integer = startpt

        phrase = ""
        nextpt = startpt

        While source(pt) = " "c And pt <= endpt
            pt += 1
        End While

        startpt = pt

        If pt > endpt Then
            nextpt = pt
            Return ans
        End If

        While source(pt) <> " "c And pt <= endpt
            If source(pt) = """" Then
                pt += 1
                ''文字列処理
                While source(pt) <> """" And pt <= endpt
                    pt += 1
                End While
                pt += 1
            Else
                pt += 1
            End If
        End While

        If pt = startpt Then
            Return ans
        End If

        phrase = source.Substring(startpt, pt - startpt)
        nextpt = pt

        ans = True
        Return ans
    End Function

End Module
