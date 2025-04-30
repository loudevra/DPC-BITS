Imports MySql.Data.MySqlClient
Imports System.Windows.Controls
Imports System.ComponentModel


Namespace DPC.Data.Controllers
    Public Class StockStatsController
#Region "Properties"

        Private Shared _txtInStock As TextBlock

        Private Shared _txtStockOut As TextBlock

        Private Shared _txtTotal As TextBlock
#End Region

#Region "Constructor"

        Public Shared Sub Initialize(txtInStock As TextBlock, txtStockOut As TextBlock, txtTotal As TextBlock)
            _txtInStock = txtInStock
            _txtStockOut = txtStockOut
            _txtTotal = txtTotal
        End Sub
#End Region

#Region "Data Retrieval Methods"

        Public Shared Function GetInStockCount() As Integer
            Dim count As Integer = 0

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT COUNT(*) FROM product WHERE StockUnits > 0"

                    Using cmd As New MySqlCommand(query, conn)
                        count = Convert.ToInt32(cmd.ExecuteScalar())
                    End Using
                End Using
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Error getting in-stock count: " & ex.Message)
            End Try

            Return count
        End Function


        Public Shared Function GetStockOutCount() As Integer
            Dim count As Integer = 0

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT COUNT(*) FROM product WHERE StockUnits = 0"

                    Using cmd As New MySqlCommand(query, conn)
                        count = Convert.ToInt32(cmd.ExecuteScalar())
                    End Using
                End Using
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Error getting stock-out count: " & ex.Message)
            End Try

            Return count
        End Function


        Public Shared Function GetTotalProductCount() As Integer
            Dim count As Integer = 0

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT COUNT(*) FROM product"

                    Using cmd As New MySqlCommand(query, conn)
                        count = Convert.ToInt32(cmd.ExecuteScalar())
                    End Using
                End Using
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Error getting total product count: " & ex.Message)
            End Try

            Return count
        End Function

        Public Shared Function GetLowStockCount() As Integer
            Dim count As Integer = 0

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT COUNT(*) FROM product WHERE StockUnits > 0 AND StockUnits <= AlertQuantity"

                    Using cmd As New MySqlCommand(query, conn)
                        count = Convert.ToInt32(cmd.ExecuteScalar())
                    End Using
                End Using
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Error getting low stock count: " & ex.Message)
            End Try

            Return count
        End Function
#End Region

#Region "UI Update Methods"

        Public Shared Sub UpdateStockStats()
            Try
                Dim inStock As Integer = GetInStockCount()
                Dim stockOut As Integer = GetStockOutCount()
                Dim total As Integer = GetTotalProductCount()

                ' Update UI elements on the UI thread
                _txtInStock?.Dispatcher.Invoke(Sub() _txtInStock.Text = inStock.ToString())

                _txtStockOut?.Dispatcher.Invoke(Sub() _txtStockOut.Text = stockOut.ToString())

                _txtTotal?.Dispatcher.Invoke(Sub() _txtTotal.Text = total.ToString())
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Error updating stock stats: " & ex.Message)
            End Try
        End Sub


        Public Shared Sub RefreshProductData(dataGrid As DataGrid)
            Try
                ' First update the statistics
                UpdateStockStats()

                ' Then refresh the DataGrid if needed
                If dataGrid IsNot Nothing Then
                    ' Refresh the data source
                    ProductController.LoadProductData(dataGrid)

                    ' Refresh the view
                    Dim view As ICollectionView = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource)
                    view?.Refresh()
                End If
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Error refreshing product data: " & ex.Message)
            End Try
        End Sub
#End Region
    End Class
End Namespace