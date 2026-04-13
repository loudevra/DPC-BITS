Imports MySql.Data.MySqlClient
Imports DPC

Public Module PermissionCache

    ' The role name of whoever is currently logged in
    Private _roleName As String = ""

    ' Dictionary of "ColumnName" -> True/False
    ' e.g. "Sales" -> True, "Employees" -> False
    Private _permissions As Dictionary(Of String, Boolean)

    Public Sub LoadForRole(roleName As String)
        _roleName = If(roleName, "").Trim()
        _permissions = New Dictionary(Of String, Boolean)(
            StringComparer.OrdinalIgnoreCase)

        ' Administrator is always privileged — no need to hit DB
        ' but we still load so the Permissions grid stays accurate
        Try
            Using conn As MySqlConnection = DPC.SplashScreen.GetDatabaseConnection()

                conn.Open()

                Dim cmd As New MySqlCommand(
                    "SELECT * FROM permissions WHERE Role = @role LIMIT 1", conn)
                cmd.Parameters.AddWithValue("@role", _roleName)

                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        ' Load every column (skip column 0 which is "Role")
                        For i As Integer = 1 To reader.FieldCount - 1
                            Dim columnName As String = reader.GetName(i)
                            Dim hasAccess As Boolean =
                                (Not reader.IsDBNull(i)) AndAlso
                                (reader.GetInt32(i) = 1)
                            _permissions(columnName) = hasAccess
                        Next
                    End If
                End Using
            End Using
        Catch ex As Exception
            ' If DB fails, default to no access (fail safe)
            Console.WriteLine($"PermissionCache load failed: {ex.Message}")
        End Try
    End Sub

    Public Function Can(permissionColumnName As String) As Boolean
        ' Safety check — if cache never loaded, deny access
        If _permissions Is Nothing Then Return False

        ' Administrator bypass — full access to everything
        If _roleName = "Administrator" Then Return True

        ' Look up this specific permission
        Dim result As Boolean = False
        _permissions.TryGetValue(permissionColumnName, result)
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

