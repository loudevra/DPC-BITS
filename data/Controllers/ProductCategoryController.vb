Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Namespace DPC.Data.Controllers
    Public Class ProductCategoryController
        Public Shared Reload As Boolean
        ' Function to Fetch Warehouses Data
        Public Shared Function GetProductCategory() As ObservableCollection(Of ProductCategory)
            Dim warehouseList As New ObservableCollection(Of ProductCategory)()
            Try
                ' Use connection from SplashScreen
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT categoryID, categoryName FROM category;"
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                warehouseList.Add(New ProductCategory With {
                                    .PCID = reader.GetInt32("categoryID"),
                                    .Category = reader.GetString("categoryName")})
                            End While
                        End Using
                    End Using
                End Using ' ✅ Connection automatically returned to the pool
            Catch ex As Exception
                MessageBox.Show("Error fetching Warehouses: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
            Return warehouseList
        End Function
    End Class
End Namespace
