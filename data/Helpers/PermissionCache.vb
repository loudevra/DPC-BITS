Imports MySql.Data.MySqlClient
Imports DPC

Public Module PermissionCache

    Private _roleName As String = ""
    Private _permissions As Dictionary(Of String, Boolean)

    Public Sub LoadForRole(roleName As String)
        _roleName = If(roleName, "").Trim()
        _permissions = New Dictionary(Of String, Boolean)(
            StringComparer.OrdinalIgnoreCase)

        Try
            Using conn As MySqlConnection = DPC.SplashScreen.GetDatabaseConnection()
                conn.Open()

                Dim cmd As New MySqlCommand(
                    "SELECT * FROM permissions WHERE Role = @role LIMIT 1", conn)
                cmd.Parameters.AddWithValue("@role", _roleName)

                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        For i As Integer = 1 To reader.FieldCount - 1
                            Dim columnName As String = reader.GetName(i)
                            Dim hasAccess As Boolean =
                                (Not reader.IsDBNull(i)) AndAlso
                                (Convert.ToInt32(reader(i)) = 1)

                            _permissions(columnName) = hasAccess
                        Next
                    End If
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine($"PermissionCache load failed: {ex.Message}")
        End Try
    End Sub

    Private Function NormalizePermissionName(permissionColumnName As String) As String
        Dim key As String = If(permissionColumnName, "").Trim()

        Select Case key.ToLower()
            Case "stocks"
                Return "Stock"
            Case "crm"
                Return "Crm"
            Case "dashboard"
                Return "Sales"
            Case "data & reports"
                Return "Reports"
            Case "hrm"
                Return "Employees"
            Case "software updates"
                Return "POS"
            Case Else
                Return key
        End Select
    End Function

    Public Function Can(permissionColumnName As String) As Boolean
        If _permissions Is Nothing Then Return False

        If _roleName = "Administrator" Then Return True

        Dim normalizedKey As String = NormalizePermissionName(permissionColumnName)

        Dim result As Boolean = False
        _permissions.TryGetValue(normalizedKey, result)
        Return result
    End Function

    Public Function CanAny(ParamArray columnNames() As String) As Boolean
        Return columnNames.Any(Function(c) Can(c))
    End Function

    Public Function CanAll(ParamArray columnNames() As String) As Boolean
        Return columnNames.All(Function(c) Can(c))
    End Function

    Public ReadOnly Property IsPrivileged As Boolean
        Get
            Return _roleName = "Administrator" OrElse
                   _roleName = "Business Owner"
        End Get
    End Property

    Public ReadOnly Property CurrentRole As String
        Get
            Return _roleName
        End Get
    End Property

    Public Sub Clear()
        _roleName = ""
        _permissions = Nothing
    End Sub

End Module