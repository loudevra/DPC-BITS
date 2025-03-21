Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model

Namespace DPC.Data.Controllers
    Public Class ManageController
        ' Fetch Warehouses Data Using Connection Pooling
        Public Shared Function GetManageProducts() As ObservableCollection(Of ManageProducts)
            Dim manageproductslist As New ObservableCollection(Of ManageProducts)()
            Dim query As String = "SELECT productid,productName,description 
                                    FROM products;"

            ' Always get a new connection from the pool
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                manageproductslist.Add(New ManageProducts With {
                                    .ID = reader.GetInt32("productid"),
                                    .Name = reader.GetString("productName")})
                                '.Description = reader.GetString("description")})


                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error fetching Warehouses: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using ' ✅ Connection is automatically returned to the pool

            Return manageproductslist
        End Function
    End Class
End Namespace
