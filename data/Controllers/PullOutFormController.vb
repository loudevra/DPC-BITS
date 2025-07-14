Imports System.Collections.ObjectModel
Imports DocumentFormat.OpenXml.Bibliography
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Views.Stocks.ItemManager.Consumables
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json

Namespace DPC.Data.Controllers.Stocks
    Public Class PullOutFormController
        Public Shared Function SearchProducts(searchText As String, warehouseID As Integer) As ObservableCollection(Of ProductDataModel)
            Dim products As New ObservableCollection(Of ProductDataModel)

            Dim query As String = "
SELECT *
FROM consumables
WHERE WarehouseID = @warehouseID
  AND Stock > 0
  AND (
       ProductName LIKE @searchText1 
       OR ProductID LIKE @searchText1 
  )

LIMIT 10;"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@warehouseID", warehouseID)
                        cmd.Parameters.AddWithValue("@searchText1", "%" & searchText & "%")
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim product As New ProductDataModel With {
                                    .ProductName = reader("ProductName").ToString(),
                                    .ProductID = reader("ProductID").ToString(),
                                    .StockUnits = Convert.ToDecimal(reader("Stock"))
                                }
                                products.Add(product)
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error searching products: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using

            Return products
        End Function

        Public Shared Function GeneratePOR() As String
            Dim prefix As String = "POR-"
            Dim datePart As String = DateTime.Now.ToString("MMddyyyy") ' MMDDYYYY format
            Dim counter As Integer = GetNextINvoiceCounter(datePart)

            ' Format counter to be 4 digits (e.g., 0001, 0025, 0150)
            Dim counterPart As String = counter.ToString("D4")

            ' Concatenate to get full ProductCode
            Return prefix & datePart & "-" & counterPart
        End Function

        Public Shared Function GetNextINvoiceCounter(datePart As String) As Integer

            'will be creating a new table soon
            Dim query As String = "SELECT MAX(CAST(SUBSTRING(PORNumber, 14, 4) AS UNSIGNED)) FROM pullout " &
                  "WHERE PORNumber LIKE 'POR-" & datePart & "%'"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Dim result As Object = cmd.ExecuteScalar()

                        ' If no previous records exist for today, start with 0001
                        If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                            Return Convert.ToInt32(result) + 1
                        Else
                            Return 1
                        End If
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error generating Product Code: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return 1
                End Try
            End Using
        End Function

        Public Shared Function SavePullOut(porNumber As String, pulloutTo As String, itemsList As List(Of Dictionary(Of String, String))) As Boolean
            Dim query As String = "INSERT INTO pullout (PORNumber, PulloutTo, PulloutItems, PORDate, PreparedBy, CreatedAt) VALUES (@PORNumber, @PulloutTo, @ItemsList, @PORDate, @PreparedBy, @CreatedAt)"
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@PORNumber", porNumber)
                        cmd.Parameters.AddWithValue("@PulloutTo", pulloutTo)
                        cmd.Parameters.AddWithValue("@ItemsList", JsonConvert.SerializeObject(itemsList, Formatting.Indented))
                        cmd.Parameters.AddWithValue("@PORDate", Date.Now.ToString("yyyy-MM-dd"))
                        cmd.Parameters.AddWithValue("@PreparedBy", CacheOnLoggedInName)
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now)
                        Dim rowsAffected As Integer = cmd.ExecuteNonQuery()


                        For Each item In itemsList
                            UpdateStock(item("ItemName"), item("Quantity"), item("Stocks"))
                        Next

                        Return rowsAffected > 0
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error saving pull out: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return False
                End Try
            End Using
        End Function

        Public Shared Sub UpdateStock(productName As String, quantity As Integer, stock As Integer)
            Dim updatedStock As Integer = stock - quantity
            Dim _productID As String = ""

            Dim queryID As String = "SELECT ProductID FROM consumables WHERE ProductName = @productName"
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(queryID, conn)
                        cmd.Parameters.AddWithValue("@productName", productName)
                        Using reader As MySqlDataReader = cmd.ExecuteReader
                            While reader.Read()
                                _productID = reader("ProductID").ToString()
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error saving pull out: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using

            Dim query As String = "UPDATE consumables SET Stock = @updatedStock WHERE ProductID = @productID"
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@productID", _productID)
                        cmd.Parameters.AddWithValue("@updatedStock", updatedStock)
                        cmd.ExecuteNonQuery()
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error saving pull out: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
        End Sub

        Public Shared Function GetConsumables() As ObservableCollection(Of ConsumableModels)
            Dim _consumables As New ObservableCollection(Of ConsumableModels)


            Dim query As String = "SELECT * FROM consumables"
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader
                            While reader.Read()

                                _consumables.Add(New ConsumableModels With {
                                    .ProductID = reader("ProductID"),
                                    .ProductName = reader("ProductName"),
                                    .WarehouseName = reader("WarehouseName"),
                                    .Stock = reader("Stock")
                                })

                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error saving pull out: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using

            Return _consumables
        End Function

        Public Shared Function GenerateProductID() As String
            Dim prefix As String = "60"
            Dim datePart As String = DateTime.Now.ToString("MMddyyyy") ' MMDDYYYY format
            Dim counter As Integer = GetNextIDCounter(datePart)

            ' Format counter to be 4 digits (e.g., 0001, 0025, 0150)
            Dim counterPart As String = counter.ToString("D4")

            ' Concatenate to get full ProductCode
            Return prefix & datePart & counterPart
        End Function

        Public Shared Function GetNextIDCounter(datePart As String) As Integer

            'will be creating a new table soon
            Dim query As String = "SELECT MAX(CAST(SUBSTRING(ProductID, 11, 4) AS UNSIGNED)) FROM consumables " &
                  "WHERE ProductID LIKE '60" & datePart & "%'"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Dim result As Object = cmd.ExecuteScalar()

                        ' If no previous records exist for today, start with 0001
                        If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                            Return Convert.ToInt32(result) + 1
                        Else
                            Return 1
                        End If
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error generating Product Code: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return 1
                End Try
            End Using
        End Function

        Public Shared Function InsertConsumable(_productName As String, _warehouseID As Integer, _warehouseName As String, _stock As Integer)

            Dim _id As String = GenerateProductID()
            Dim query As String = "INSERT INTO consumables (ProductID, ProductName, WarehouseID, WarehouseName, Stock, AddedBy, CreatedAt)" &
                                "VALUES (@ProductID, @ProductName, @WarehouseID, @WarehouseName, @Stock, @AddedBy, @CreatedAt)"
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@ProductID", _id)
                        cmd.Parameters.AddWithValue("@ProductName", _productName)
                        cmd.Parameters.AddWithValue("@WarehouseID", _warehouseID)
                        cmd.Parameters.AddWithValue("@WarehouseName", _warehouseName)
                        cmd.Parameters.AddWithValue("@Stock", _stock)
                        cmd.Parameters.AddWithValue("@AddedBy", CacheOnLoggedInName)
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now)
                        cmd.ExecuteNonQuery()
                    End Using

                    Return True
                Catch ex As Exception
                    MessageBox.Show($"Error saving pull out: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using

            Return False
        End Function
    End Class
End Namespace