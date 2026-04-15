Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Helpers

Namespace DPC.Data.Controllers
    Public Class InventoryController

        ''' <summary>
        ''' Deducts stock for a product by name from the appropriate warehouse
        ''' </summary>
        Public Shared Function DeductProductStock(
            productName As String,
            warehouseID As Integer,
            quantity As Decimal
        ) As Boolean
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' Get product ID and check if it has variations
                    Dim productID As Integer = 0
                    Dim hasVariation As Boolean = False

                    ' FIXED: Cast columns to proper types in SQL
                    Dim checkQuery As String = "SELECT CAST(productID AS SIGNED) as productID, 
                                               CAST(productVariation AS SIGNED) as productVariation 
                                        FROM product 
                                        WHERE TRIM(productName) = TRIM(@productName) 
                                        LIMIT 1"

                    Using checkCmd As New MySqlCommand(checkQuery, conn)
                        checkCmd.Parameters.AddWithValue("@productName", productName.Trim())
                        Using reader = checkCmd.ExecuteReader()
                            If reader.Read() Then
                                ' Safe conversion with null checks
                                If Not IsDBNull(reader("productID")) Then
                                    productID = CInt(reader("productID"))
                                End If

                                If Not IsDBNull(reader("productVariation")) Then
                                    hasVariation = CBool(reader("productVariation"))
                                End If

                                If productID = 0 Then
                                    Debug.WriteLine($"❌ Invalid product ID for: '{productName}'")
                                    Return False
                                End If
                            Else
                                Debug.WriteLine($"❌ Product not found in database: '{productName}'")
                                Return False
                            End If
                        End Using
                    End Using

                    Debug.WriteLine($"📦 Found product: {productName} (ID: {productID}, HasVariation: {hasVariation})")

                    ' Now deduct stock
                    Dim updateQuery As String
                    If hasVariation Then
                        updateQuery = "UPDATE productvariationstock 
                              SET stockUnit = stockUnit - @quantity 
                              WHERE productID = @productID 
                              AND warehouseID = @warehouseID 
                              AND stockUnit >= @quantity
                              LIMIT 1"
                    Else
                        updateQuery = "UPDATE productnovariation 
                              SET stockUnit = stockUnit - @quantity 
                              WHERE productID = @productID 
                              AND warehouseID = @warehouseID 
                              AND stockUnit >= @quantity"
                    End If

                    Using updateCmd As New MySqlCommand(updateQuery, conn)
                        updateCmd.Parameters.Add("@productID", MySqlDbType.Int32).Value = productID
                        updateCmd.Parameters.Add("@warehouseID", MySqlDbType.Int32).Value = warehouseID
                        updateCmd.Parameters.Add("@quantity", MySqlDbType.Decimal).Value = quantity

                        Dim rowsAffected As Integer = updateCmd.ExecuteNonQuery()

                        If rowsAffected > 0 Then
                            Debug.WriteLine($"✅ Stock deducted successfully")
                            Debug.WriteLine($"   Product: {productName} (ID: {productID})")
                            Debug.WriteLine($"   Warehouse: {warehouseID}")
                            Debug.WriteLine($"   Quantity: {quantity}")
                            Return True
                        Else
                            Debug.WriteLine($"⚠️ Stock deduction UPDATE failed")
                            Debug.WriteLine($"   Product: {productName} (ID: {productID})")
                            Debug.WriteLine($"   Warehouse: {warehouseID}, Qty: {quantity}")

                            ' Check current stock level
                            Dim stockCheckQuery As String
                            If hasVariation Then
                                stockCheckQuery = "SELECT COALESCE(SUM(stockUnit), 0) as totalStock 
                                          FROM productvariationstock 
                                          WHERE productID = @productID AND warehouseID = @warehouseID"
                            Else
                                stockCheckQuery = "SELECT COALESCE(stockUnit, 0) as totalStock 
                                          FROM productnovariation 
                                          WHERE productID = @productID AND warehouseID = @warehouseID"
                            End If

                            Using stockCmd As New MySqlCommand(stockCheckQuery, conn)
                                stockCmd.Parameters.Add("@productID", MySqlDbType.Int32).Value = productID
                                stockCmd.Parameters.Add("@warehouseID", MySqlDbType.Int32).Value = warehouseID
                                Dim currentStock = stockCmd.ExecuteScalar()
                                Debug.WriteLine($"   Current stock in warehouse: {currentStock}")
                                Debug.WriteLine($"   Product exists in warehouse: {If(currentStock IsNot Nothing AndAlso CInt(currentStock) >= 0, "YES", "NO")}")
                            End Using

                            Return False
                        End If
                    End Using
                End Using
            Catch ex As Exception
                Debug.WriteLine($"❌ Exception in DeductProductStock: {ex.Message}")
                Debug.WriteLine($"   Product: {productName}, Warehouse: {warehouseID}, Qty: {quantity}")
                Debug.WriteLine($"   Type: {ex.GetType().Name}")
                If ex.InnerException IsNot Nothing Then
                    Debug.WriteLine($"   Inner: {ex.InnerException.Message}")
                End If
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Deducts stock for a product from the appropriate warehouse (by ID)
        ''' </summary>
        Public Shared Function DeductProductStock(
            productID As Integer,
            warehouseID As Integer,
            quantity As Integer,
            hasVariation As Boolean,
            Optional variationID As Integer = 0
        ) As Boolean
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim query As String

                    If hasVariation Then
                        ' For products with variations
                        query = "UPDATE productvariationstock 
                                SET stockUnit = stockUnit - @quantity 
                                WHERE productID = @productID 
                                AND warehouseID = @warehouseID 
                                AND variationID = @variationID 
                                AND stockUnit >= @quantity"
                    Else
                        ' For products without variations
                        query = "UPDATE productnovariation 
                                SET stockUnit = stockUnit - @quantity 
                                WHERE productID = @productID 
                                AND warehouseID = @warehouseID 
                                AND stockUnit >= @quantity"
                    End If

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@productID", productID)
                        cmd.Parameters.AddWithValue("@warehouseID", warehouseID)
                        cmd.Parameters.AddWithValue("@quantity", quantity)

                        If hasVariation Then
                            cmd.Parameters.AddWithValue("@variationID", variationID)
                        End If

                        Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                        Return rowsAffected > 0
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show($"Error deducting stock: {ex.Message}")
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Deducts stock for all products in a billing statement
        ''' </summary>
        Public Shared Function DeductBillingStatementStock(
            billingItems As List(Of Dictionary(Of String, String)),
            warehouseID As Integer
        ) As (Success As Boolean, FailedProducts As List(Of String))

            Dim failedProducts As New List(Of String)()
            Dim allSuccess As Boolean = True

            For Each item In billingItems
                ' Skip header rows
                If item.ContainsKey("IsCategoryHeader") AndAlso item("IsCategoryHeader") = "True" Then
                    Continue For
                End If

                If Not item.ContainsKey("ProductName") OrElse Not item.ContainsKey("Quantity") Then
                    Continue For
                End If

                Dim productName As String = item("ProductName")
                Dim quantity As Decimal = 0

                If Not Decimal.TryParse(item("Quantity").Replace(",", ""), quantity) Then
                    Continue For
                End If

                ' Attempt to deduct stock using the overload that accepts product name
                Dim success = DeductProductStock(productName, warehouseID, quantity)

                If Not success Then
                    allSuccess = False
                    failedProducts.Add(productName)
                End If
            Next

            Return (allSuccess, failedProducts)
        End Function
    End Class
    
    ''' <summary>
    ''' Represents an item in a billing statement
    ''' </summary>
    Public Class BillingItem
        Public Property ProductID As Integer
        Public Property WarehouseID As Integer
        Public Property Quantity As Integer
        Public Property HasVariation As Boolean
        Public Property VariationID As Integer
    End Class
End Namespace