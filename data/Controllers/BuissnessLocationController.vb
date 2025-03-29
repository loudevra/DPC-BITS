Imports MySql.Data.MySqlClient


Namespace DPC.Data.Controllers
    Public Class BusinessLocationController
        ' Function to get all business locations from the database
        Public Shared Function GetAllLocations() As List(Of String)
            Dim locations As New List(Of String)

            Dim query As String = "SELECT LocationName FROM businesslocations"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                locations.Add(reader("LocationName").ToString())
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error fetching business locations: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using

            Return locations
        End Function
    End Class
End Namespace