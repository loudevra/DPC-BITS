Imports System.Collections.ObjectModel
Imports DocumentFormat.OpenXml.Bibliography
Imports DPC.DPC.Data.Model
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json

Namespace DPC.Data.Controllers.Stocks
    Public Class PullOutFormController
        Public Shared Function SearchProducts(searchText As String, warehouseID As Integer) As ObservableCollection(Of ProductDataModel)
            Dim products As New ObservableCollection(Of ProductDataModel)

           Dim query As String = "
SELECT p.productID,
       p.productName,
       p.productCode,
       pnv.sellingPrice,
       pnv.stockUnit
FROM product p
INNER JOIN productnovariation pnv ON p.productID = pnv.productID
WHERE pnv.warehouseID = @warehouseID
  AND pnv.stockUnit > 0
  AND (
       p.productName LIKE @searchText1 
       OR p.productID LIKE @searchText1 
       OR p.productCode LIKE @searchText1
  )

UNION

SELECT p.productID,
       p.productName,
       p.productCode,
       pvs.sellingPrice,
       pvs.stockUnit
FROM product p
INNER JOIN productvariationstock pvs ON p.productID = pvs.productID
WHERE pvs.warehouseID = @warehouseID
  AND pvs.stockUnit > 0
  AND (
       p.productName LIKE @searchText2 
       OR p.productID LIKE @searchText2 
       OR p.productCode LIKE @searchText2
  )

ORDER BY productName ASC
LIMIT 10;"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@warehouseID", warehouseID)
                        cmd.Parameters.AddWithValue("@searchText1", "%" & searchText & "%")
                        cmd.Parameters.AddWithValue("@searchText2", "%" & searchText & "%")
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim product As New ProductDataModel With {
                                    .ProductName = reader("productName").ToString(),
                                    .ProductID = reader("productID").ToString(),
                                    .StockUnits = Convert.ToDecimal(reader("stockUnit"))
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
            Dim query As String = "SELECT productVariation, productID
FROM product
WHERE productName = @productName
ORDER BY productName ASC
"
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@productName", productName)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                If Not String.IsNullOrWhiteSpace(reader("productID").ToString()) Then

                                    UpdateItems(reader("productID"), reader("productVariation"), quantity, stock)
                                End If
                            End While
                        End Using

                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error saving pull out: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
        End Sub

        Public Shared Sub UpdateItems(productID As String, productVariation As Integer, quantity As Integer, stock As Integer)
            Dim query As String = ""
            Dim updatedStock As Integer = stock - quantity

            If productVariation Then
                query = "UPDATE productvariationstock SET stockUnit = @updatedStock WHERE productID = @productID"
            Else
                query = "UPDATE productnovariation SET stockUnit = @updatedStock WHERE productID = @productID"
            End If

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@productID", productID)
                        cmd.Parameters.AddWithValue("@updatedStock", updatedStock)
                        cmd.ExecuteNonQuery()

                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error saving pull out: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
        End Sub

    End Class
End Namespace