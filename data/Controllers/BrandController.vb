Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports MySql.Data.MySqlClient

Namespace DPC.Data.Controllers
    Public Class BrandController
        Public Shared Function GetBrands() As ObservableCollection(Of Brand)
            Dim brandList As New ObservableCollection(Of Brand)()
            Dim query As String = "SELECT brandid, brandname FROM Brand;"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                brandList.Add(New Brand With {
                                    .ID = reader.GetInt32("brandid"), ' Changed to GetInt32 for auto-increment
                                    .Name = reader.GetString("brandname")
                                })
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error fetching brands: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using

            Return brandList
        End Function
    End Class
End Namespace
