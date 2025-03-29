Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Models

Namespace DPC.Data.Controllers
    Public Class ClientGroupController
        Public Function CreateClientGroup(group As ClientGroup) As Boolean
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "INSERT INTO ClientGroup (GroupName, Description) VALUES (@GroupName, @Description)"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@GroupName", group.GroupName)
                        cmd.Parameters.AddWithValue("@Description", group.Description)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
                Return True
            Catch ex As Exception
                Console.WriteLine("Error creating ClientGroup: " & ex.Message)
                Return False
            End Try
        End Function

        Public Function GetAllClientGroups() As List(Of ClientGroup)
            Dim clientGroups As New List(Of ClientGroup)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT * FROM ClientGroup"
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                clientGroups.Add(New ClientGroup With {
                                    .ClientGroupID = reader.GetInt32("ClientGroupID"),
                                    .GroupName = reader.GetString("GroupName"),
                                    .Description = reader.GetString("Description"),
                                    .ClientCount = reader.GetInt32("ClientCount")
                                })
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                Console.WriteLine("Error fetching ClientGroups: " & ex.Message)
            End Try
            Return clientGroups
        End Function
    End Class
End Namespace
