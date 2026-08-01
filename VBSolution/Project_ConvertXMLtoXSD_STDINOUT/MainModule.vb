Module MainModule

	Sub Main(Args() As String)
		If Args.Count <> 0 Then
			Exit Sub
		End If

		Try
			Console.InputEncoding = Text.Encoding.GetEncoding("Shift_JIS") 'Text.Encoding.UTF8
			Console.OutputEncoding = Text.Encoding.UTF8

			Dim inbuf As Dictionary(Of Integer, String)
			Dim outbuf As Dictionary(Of Integer, String)

			inbuf = fncReadXML(Console.In)
			outbuf = fncConvXML(inbuf)
			FncSaveXML(outbuf, Console.Out)

		Catch ex As Exception
			Throw ex 'MsgBox(ex.Message)
		End Try
	End Sub

	Const DSNAME = "DSSQLSV"
	Dim indentcolumn As Integer = 0
	ReadOnly viewVal As Boolean = True

	Function FncReadXML(file As IO.TextReader) As Dictionary(Of Integer, String)

		Dim buf As New Dictionary(Of Integer, String)

		Dim i = 0
		Dim line As String = file.ReadLine()
		While line IsNot Nothing
			buf.Add(i, Trim(Replace(Replace(line, Chr(34), "'"), Chr(9), "    ")))
			i += 1
			line = file.ReadLine()
		End While

		Return buf
	End Function

	Function FncSaveXML(buf As Dictionary(Of Integer, String), file As IO.TextWriter) As Boolean

		For i = 0 To buf.Count - 1
			file.Write(Replace(buf(i), "'&'", Chr(34)) & Chr(13) & Chr(10))
		Next

		Return True
	End Function

	Function FncAdd(dic As Dictionary(Of Integer, String), ind As Integer, buf As String) As Integer
		indentcolumn += ind
		Dim indentsp As String
		indentsp = ""
		If indentcolumn > 0 Then
			indentsp = New String(" "c, indentcolumn * 2)
		End If
		dic.Add(dic.Count, indentsp & buf)
		Return dic.Count
	End Function

	Function FncFormatDate(datestr As String) As String
		Dim ans As String '= ""

		If IsDate(datestr) Then
			Dim dateval As DateTime
			dateval = DateValue(datestr)
			Dim sb1 As New Text.StringBuilder
			sb1.AppendFormat("{0,19:yyyy-MM-ddTHH:mm:ss}", dateval)
			ans = sb1.ToString & "+09:00"
		Else
			ans = datestr
		End If
		Return ans
	End Function

	Function FncConvXML(inbuf As Dictionary(Of Integer, String)) As Dictionary(Of Integer, String)
		Dim outbuf As New Dictionary(Of Integer, String)

		Dim strPattern As String, matches As Text.RegularExpressions.MatchCollection, submatches As Text.RegularExpressions.GroupCollection

		Dim i As Integer
		Dim tmpName As String, tmpStr As String
		Dim typeName As String, maxLenVal As String, defaultVal As String, allowNull As String
		Dim autoinc As String, ai_seed As String, ai_incval As String
		Dim onName As String
		'Dim fieldName As String
		Dim columnNo As Integer, columnInd As Integer
		Dim tableNo As Integer, tableInd As Integer
		'Dim xc As String, xs As String
		columnNo = 0
		tableNo = 0
		For i = 0 To inbuf.Count - 1
			Dim thisSentence As String, topWord As String
			thisSentence = inbuf(i)
			'topWord = ""
			If InStr(thisSentence, " ") > 0 Then
				topWord = Left(thisSentence, InStr(thisSentence, " ") - 1)
			Else
				topWord = thisSentence
			End If

			Select Case topWord
				Case "<?xml"
					'ヘッダ
					Call FncAdd(outbuf, 0, "<?xml version='&'1.0'&' encoding='&'utf-8'&'?>")
					Call FncAdd(outbuf, 0, "<xs:schema id='&'" & DSNAME & "'&' targetNamespace='&'http://tempuri.org/" & DSNAME & ".xsd'&' xmlns:mstns='&'http://tempuri.org/" & DSNAME & ".xsd'&' xmlns='&'http://tempuri.org/" & DSNAME & ".xsd'&' xmlns:xs='&'http://www.w3.org/2001/XMLSchema'&' xmlns:msdata='&'urn:schemas-microsoft-com:xml-msdata'&' xmlns:msprop='&'urn:schemas-microsoft-com:xml-msprop'&' attributeFormDefault='&'qualified'&' elementFormDefault='&'qualified'&'>")

					Call FncAdd(outbuf, 1, "<xs:annotation>")
					Call FncAdd(outbuf, 1, "<xs:appinfo source='&'urn:schemas-microsoft-com:xml-msdatasource'&'>")
					Call FncAdd(outbuf, 1, "<DataSource DefaultConnectionIndex='&'0'&' FunctionsComponentName='&'QueriesTableAdapter'&' Modifier='&'AutoLayout, AnsiClass, Class, Public'&' SchemaSerializationMode='&'IncludeSchema'&' xmlns='&'urn:schemas-microsoft-com:xml-msdatasource'&'>")
					Call FncAdd(outbuf, 1, "<Connections />")
					Call FncAdd(outbuf, 0, "<Tables />")
					Call FncAdd(outbuf, 0, "<Sources />")
					Call FncAdd(outbuf, -1, "</DataSource>")
					Call FncAdd(outbuf, -1, "</xs:appinfo>")
					Call FncAdd(outbuf, -1, "</xs:annotation>")

					Call FncAdd(outbuf, 0, "<xs:element name='&'" & DSNAME & "'&' msdata:IsDataSet='&'true'&' msdata:UseCurrentLocale='&'true'&' msprop:EnableTableAdapterManager='&'true'&' msprop:Generator_DataSetName='&'" & DSNAME & "'&' msprop:Generator_UserDSName='&'" & DSNAME & "'&'>")

				Case "<Tables>"
					Call FncAdd(outbuf, 1, "<xs:complexType>")
					Call FncAdd(outbuf, 1, "<xs:choice minOccurs='&'0'&' maxOccurs='&'unbounded'&'>")

				Case "</Tables>"
					Call FncAdd(outbuf, -1, "</xs:choice>")
					Call FncAdd(outbuf, -1, "</xs:complexType>")

				Case "<Table"
					If tableNo = 0 Then
						tableInd = 1
					Else
						tableInd = 0
					End If
					strPattern = "<Table name='(.+?)'>"
					matches = Text.RegularExpressions.Regex.Matches(thisSentence, strPattern, Text.RegularExpressions.RegexOptions.IgnoreCase)
					If matches.Count > 0 Then
						submatches = matches(0).Groups
						tmpName = submatches(1).Value
						tmpStr = "<xs:element name='&'" & tmpName & "'&' "
						tmpStr &= "msprop:Generator_TableClassName='&'" & tmpName & "DataTable'&' "
						tmpStr &= "msprop:Generator_TableVarName='&'table" & tmpName & "'&' "
						tmpStr &= "msprop:Generator_TablePropName='&'" & tmpName & "'&' "
						tmpStr &= "msprop:Generator_RowDeletingName='&'" & tmpName & "RowDeleting'&' "
						tmpStr &= "msprop:Generator_RowChangingName='&'" & tmpName & "RowChanging'&' "
						tmpStr &= "msprop:Generator_RowEvHandlerName='&'" & tmpName & "RowChangeEventHandler'&' "
						tmpStr &= "msprop:Generator_RowDeletedName='&'" & tmpName & "RowDeleted'&' "
						tmpStr &= "msprop:Generator_UserTableName='&'" & tmpName & "'&' "
						tmpStr &= "msprop:Generator_RowChangedName='&'" & tmpName & "RowChanged'&' "
						tmpStr &= "msprop:Generator_RowEvArgName='&'" & tmpName & "RowChangeEvent'&' "
						tmpStr &= "msprop:Generator_RowClassName='&'" & tmpName & "Row'&'"
						tmpStr &= ">"
						Call FncAdd(outbuf, tableInd, tmpStr)
						Call FncAdd(outbuf, 1, "<xs:complexType>")
						Call FncAdd(outbuf, 1, "<xs:sequence>")
					End If
					columnNo = 0

				Case "</Table>"
					Call FncAdd(outbuf, -1, "</xs:sequence>")
					Call FncAdd(outbuf, -1, "</xs:complexType>")
					Call FncAdd(outbuf, -1, "</xs:element>")
					columnNo = 0
					tableNo += 1

				Case "<Column"
					If columnNo = 0 Then
						columnInd = 1
					Else
						columnInd = 0
					End If
					strPattern = "<Column name='(\S+)'"
					'strPattern=strPattern & "( type='(int|string|decimal|datetime|boolean|geography|base64binary)')?"
					strPattern &= "( type='(.+?)')?"
					strPattern &= "( maxlength='([\dmax]+)')?"
					strPattern &= "( default='\((.+?)\)')?"
					strPattern &= "( allownull='(true|false)')?"
					strPattern &= "( autoincrement='(true|false)')?"
					strPattern &= "( autoincrement_seedvalue='([\d ]+)')?"
					strPattern &= "( autoincrement_incrementvalue='([\d ]+)')?"
					strPattern &= " />"
					matches = Text.RegularExpressions.Regex.Matches(thisSentence, strPattern, Text.RegularExpressions.RegexOptions.IgnoreCase)
					If matches.Count > 0 Then
						submatches = matches(0).Groups
						tmpName = submatches(1).Value
						typeName = submatches(3).Value
						maxLenVal = submatches(5).Value
						defaultVal = submatches(7).Value
						allowNull = submatches(9).Value
						autoinc = submatches(11).Value
						ai_seed = Trim(submatches(13).Value)
						ai_incval = Trim(submatches(15).Value)

						tmpStr = "<xs:element name='&'" & tmpName & "'&' "
						If autoinc = "true" Then
							tmpStr &= "msdata:AutoIncrement='&'true'&' "
							If ai_seed <> "" Then
								tmpStr &= "msdata:AutoIncrementSeed='&'" & ai_seed & "'&' "
							End If
							If ai_incval <> "" Then
								tmpStr &= "msdata:AutoIncrementStep='&'" & ai_incval & "'&' "
							End If
						End If
						tmpStr &= "msprop:Generator_ColumnVarNameInTable='&'column" & tmpName & "'&' "
						tmpStr &= "msprop:Generator_ColumnPropNameInRow='&'" & tmpName & "'&' "
						tmpStr &= "msprop:Generator_ColumnPropNameInTable='&'" & tmpName & "Column'&' "
						tmpStr &= "msprop:Generator_UserColumnName='&'" & tmpName & "'&'"
						Select Case LCase(typeName)
							Case "boolean"
								tmpStr &= " type='&'xs:boolean'&'"
								Select Case defaultVal
									Case "(0)"
										tmpStr &= " default='&'false'&'"
									Case "(1)"
										tmpStr &= " default='&'true'&'"
								End Select
								If allowNull = "true" Then
									tmpStr &= " minOccurs='&'0'&'"
								End If
								tmpStr &= " />"
								Call FncAdd(outbuf, columnInd, tmpStr)

							Case "string"
								If Left(defaultVal, 1) = "'" And Right(defaultVal, 1) = "'" Then
									tmpStr &= " default='&'" & Mid(defaultVal, 2, Len(defaultVal) - 2) & "'&'"
								End If
								If allowNull = "true" Then
									tmpStr &= " minOccurs='&'0'&'"
								End If
								tmpStr &= ">"
								Call FncAdd(outbuf, columnInd, tmpStr)
								Call FncAdd(outbuf, 1, "<xs:simpleType>")
								Call FncAdd(outbuf, 1, "<xs:restriction base='&'xs:string'&'>")
								If UCase(maxLenVal) = "MAX" Then
									maxLenVal = "2147483647"
								End If
								Call FncAdd(outbuf, 1, "<xs:maxLength value='&'" & maxLenVal & "'&' />")
								Call FncAdd(outbuf, -1, "</xs:restriction>")
								Call FncAdd(outbuf, -1, "</xs:simpleType>")
								Call FncAdd(outbuf, -1, "</xs:element>")

							Case "base64binary"
								If Left(defaultVal, 1) = "'" And Right(defaultVal, 1) = "'" Then
									tmpStr &= " default='&'" & Mid(defaultVal, 2, Len(defaultVal) - 2) & "'&'"
								End If
								If allowNull = "true" Then
									tmpStr &= " minOccurs='&'0'&'"
								End If
								If UCase(maxLenVal) = "MAX" Then
									'maxLenVal = "2147483647"
								End If
								tmpStr &= " type='&'xs:base64Binary'&'"
								tmpStr &= " />"
								Call FncAdd(outbuf, columnInd, tmpStr)

							Case "int"
								tmpStr &= " type='&'xs:int'&'"
								If Left(defaultVal, 1) = "(" And Right(defaultVal, 1) = ")" Then
									tmpStr &= " default='&'" & Mid(defaultVal, 2, Len(defaultVal) - 2) & "'&'"
								End If
								If allowNull = "true" Then
									tmpStr &= " minOccurs='&'0'&'"
								End If
								tmpStr &= " />"
								Call FncAdd(outbuf, columnInd, tmpStr)

							Case "int64"
								tmpStr &= " type='&'xs:long'&'"
								If Left(defaultVal, 1) = "(" And Right(defaultVal, 1) = ")" Then
									tmpStr &= " default='&'" & Mid(defaultVal, 2, Len(defaultVal) - 2) & "'&'"
								End If
								If allowNull = "true" Then
									tmpStr &= " minOccurs='&'0'&'"
								End If
								tmpStr &= " />"
								Call FncAdd(outbuf, columnInd, tmpStr)

							Case "int16"
								tmpStr &= " type='&'xs:short'&'"
								If Left(defaultVal, 1) = "(" And Right(defaultVal, 1) = ")" Then
									tmpStr &= " default='&'" & Mid(defaultVal, 2, Len(defaultVal) - 2) & "'&'"
								End If
								If allowNull = "true" Then
									tmpStr &= " minOccurs='&'0'&'"
								End If
								tmpStr &= " />"
								Call FncAdd(outbuf, columnInd, tmpStr)

							Case "byte"
								tmpStr &= " type='&'xs:unsignedByte'&'"
								If Left(defaultVal, 1) = "(" And Right(defaultVal, 1) = ")" Then
									tmpStr &= " default='&'" & Mid(defaultVal, 2, Len(defaultVal) - 2) & "'&'"
								End If
								If allowNull = "true" Then
									tmpStr &= " minOccurs='&'0'&'"
								End If
								tmpStr &= " />"
								Call FncAdd(outbuf, columnInd, tmpStr)

							Case "guid"
								tmpStr &= " msdata:DataType='&'System.Guid, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'&' type='&'xs:string'&'"
								If Left(defaultVal, 1) = "(" And Right(defaultVal, 1) = ")" Then
									tmpStr &= " default='&'" & Mid(defaultVal, 2, Len(defaultVal) - 2) & "'&'"
								End If
								If allowNull = "true" Then
									tmpStr &= " minOccurs='&'0'&'"
								End If
								tmpStr &= " />"
								Call FncAdd(outbuf, columnInd, tmpStr)

							Case "single"
								tmpStr &= " type='&'xs:single'&'"
								If Left(defaultVal, 1) = "(" And Right(defaultVal, 1) = ")" Then
									tmpStr &= " default='&'" & Mid(defaultVal, 2, Len(defaultVal) - 2) & "'&'"
								End If
								If allowNull = "true" Then
									tmpStr &= " minOccurs='&'0'&'"
								End If
								tmpStr &= " />"
								Call FncAdd(outbuf, columnInd, tmpStr)

							Case "double"
								tmpStr &= " type='&'xs:double'&'"
								If Left(defaultVal, 1) = "(" And Right(defaultVal, 1) = ")" Then
									tmpStr &= " default='&'" & Mid(defaultVal, 2, Len(defaultVal) - 2) & "'&'"
								End If
								If allowNull = "true" Then
									tmpStr &= " minOccurs='&'0'&'"
								End If
								tmpStr &= " />"
								Call FncAdd(outbuf, columnInd, tmpStr)

							Case "decimal"
								tmpStr &= " type='&'xs:decimal'&'"
								If Left(defaultVal, 1) = "(" And Right(defaultVal, 1) = ")" Then
									tmpStr &= " default='&'" & Mid(defaultVal, 2, Len(defaultVal) - 2) & "'&'"
								End If
								If allowNull = "true" Then
									tmpStr &= " minOccurs='&'0'&'"
								End If
								tmpStr &= " />"
								Call FncAdd(outbuf, columnInd, tmpStr)

							Case "datetime"
								tmpStr &= " type='&'xs:dateTime'&'"
								If Left(defaultVal, 1) = "'" And Right(defaultVal, 1) = "'" Then
									tmpStr &= " default='&'" & FncFormatDate(Mid(defaultVal, 2, Len(defaultVal) - 2)) & "'&'"
								End If
								If allowNull = "true" Then
									tmpStr &= " minOccurs='&'0'&'"
								End If
								tmpStr &= " />"
								Call FncAdd(outbuf, columnInd, tmpStr)

							Case "datetimeoffset"
								tmpStr &= " type='&'xs:anyType'&'"
								tmpStr &= " msdata:DataType='&'System.DateTimeOffset'&'"
								If Left(defaultVal, 1) = "'" And Right(defaultVal, 1) = "'" Then
									tmpStr &= " default='&'" & FncFormatDate(Mid(defaultVal, 2, Len(defaultVal) - 2)) & "'&'"
								End If
								If allowNull = "true" Then
									tmpStr &= " minOccurs='&'0'&'"
								End If
								tmpStr &= " />"
								Call FncAdd(outbuf, columnInd, tmpStr)

							Case "timespan"
								tmpStr &= " type='&'xs:timespan'&'"
								If Left(defaultVal, 1) = "'" And Right(defaultVal, 1) = "'" Then
									tmpStr &= " default='&'" & FncFormatDate(Mid(defaultVal, 2, Len(defaultVal) - 2)) & "'&'"
								End If
								If allowNull = "true" Then
									tmpStr &= " minOccurs='&'0'&'"
								End If
								tmpStr &= " />"
								Call FncAdd(outbuf, columnInd, tmpStr)

							Case Else
								Throw New Exception("unknown typename:" & typeName)
								'MsgBox("unknown typename:" & typeName)

						End Select
					End If
					columnNo += 1

				Case "<Indexes>"

				Case "</Indexes>"

				Case "<Index"
					strPattern = "<Index name='(\S+)'"
					strPattern &= "( type='(\S+)')?"
					strPattern &= "( on='(\S+)')?"
					strPattern &= ">"
					matches = Text.RegularExpressions.Regex.Matches(thisSentence, strPattern, Text.RegularExpressions.RegexOptions.IgnoreCase)
					If matches.Count > 0 Then
						submatches = matches(0).Groups
						tmpName = submatches(1).Value
						typeName = submatches(3).Value
						onName = submatches(5).Value

						tmpStr = "<xs:unique name='&'" & tmpName & "'&'"
						tmpStr &= " msdata:ConstraintName='&'" & tmpName & "'&'"
						Select Case typeName
							Case "PrimaryKey"
								tmpStr &= " msdata:PrimaryKey='&'true'&'"
							Case Else
								'donothing
						End Select
						tmpStr &= ">"
						Call FncAdd(outbuf, 0, tmpStr)
						tmpStr = "<xs:selector xpath='&'.//mstns:" & onName & "'&' />"
						Call FncAdd(outbuf, 1, tmpStr)
					End If

				Case "</Index>"
					Call FncAdd(outbuf, -1, "</xs:unique>")

				Case "<KeyColumn"
					strPattern = "<KeyColumn name='(\S+)' />"
					matches = Text.RegularExpressions.Regex.Matches(thisSentence, strPattern, Text.RegularExpressions.RegexOptions.IgnoreCase)
					If matches.Count > 0 Then
						submatches = matches(0).Groups
						tmpName = submatches(1).Value

						tmpStr = "<xs:field xpath='&'mstns:" & tmpName & "'&' />"
						Call FncAdd(outbuf, 0, tmpStr)
					End If

				Case Else
					'donothing

			End Select
		Next

		Call FncAdd(outbuf, -1, "</xs:element>")
		Call FncAdd(outbuf, -1, "</xs:schema>")

		Return outbuf
	End Function

End Module
