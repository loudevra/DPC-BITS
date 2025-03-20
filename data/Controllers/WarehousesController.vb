Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model

Namespace DPC.Data.Controllers
    Public Class WarehousesController
        ' Fetch Warehouses Data Using Connection Pooling
        Public Shared Function GetWarehouses() As ObservableCollection(Of Warehouses)
            Dim supplierList As New ObservableCollection(Of Warehouses)()
            Dim query As String = "SELECT warehouseid,name,description 
                                    FROM warehouse;"

            ' Always get a new connection from the pool
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                supplierList.Add(New Warehouses With {
                                    .ID = reader.GetInt32("warehouseid"),
                                    .Name = reader.GetString("name"),
                                    .Description = reader.GetString("description")})


                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error fetching Warehouses: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using ' ✅ Connection is automatically returned to the pool

            Return supplierList
        End Function
    End Class
End Namespace
