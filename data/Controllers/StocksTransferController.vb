

Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models


Namespace DPC.Data.Controllers
    Public Class StocksTransferController
        ' Check if a product has variations
        Public Shared Function CheckProductVariation(productID As String) As Integer
            Dim query As String = "SELECT productVariation FROM product WHERE productID = @productID"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@productID", productID)
                        Return Convert.ToInt32(cmd.ExecuteScalar())
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return 0
                End Try
            End Using
        End Function

        Public Shared Sub GetProducts(comboBox As ComboBox)
            Dim query As String = "
        SELECT productID, productName
        FROM product
        ORDER BY productName ASC;
    "

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            comboBox.Items.Clear()

                            If reader.HasRows Then
                                While reader.Read()
                                    Dim productID As String = reader("productID").ToString()
                                    Dim productName As String = reader("productName").ToString().Trim()

                                    ' Format the display as "productID - productName"
                                    Dim item As New ComboBoxItem With {
                                        .Content = $"{productID} - {productName}",
                                        .Tag = productID
                                    }
                                    comboBox.Items.Add(item)
                                End While
                                comboBox.SelectedIndex = 0
                            Else
                                comboBox.Items.Clear()
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
        End Sub

        ' Get variations for a product
        Public Shared Sub GetProductVariations(productID As String, comboBox As ComboBox)
            Dim query As String = "
                SELECT variationID, variationName
                FROM productvariation
                WHERE productID = @productID
                ORDER BY variationName ASC;
            "

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@productID", productID)

                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            comboBox.Items.Clear()

                            If reader.HasRows Then
                                While reader.Read()
                                    Dim variationID As String = reader("variationID").ToString()
                                    Dim variationName As String = reader("variationName").ToString().Trim()

                                    Dim item As New ComboBoxItem With {
                                        .Content = variationName,
                                        .Tag = variationID
                                    }
                                    comboBox.Items.Add(item)
                                End While
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
        End Sub

        ' Get options for a variation
        Public Shared Sub GetVariationOptions(variationID As String, comboBox As ComboBox)
            Dim query As String = "
                SELECT optionID, optionName
                FROM variationoption
                WHERE variationID = @variationID
                ORDER BY optionName ASC;
            "

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@variationID", variationID)

                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            comboBox.Items.Clear()

                            If reader.HasRows Then
                                While reader.Read()
                                    Dim optionID As String = reader("optionID").ToString()
                                    Dim optionName As String = reader("optionName").ToString().Trim()

                                    Dim item As New ComboBoxItem With {
                                        .Content = optionName,
                                        .Tag = optionID
                                    }
                                    comboBox.Items.Add(item)
                                End While
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
        End Sub

        Public Shared Sub GetAvailableWarehouses(productID As String, comboBox As ComboBox, productVariation As Integer, optionCombination As String)
            Dim query As String = ""

            ' Adjust query based on product variation type
            If productVariation = 0 Then
                ' For products without variation
                query = "
                    SELECT DISTINCT w.warehouseID, w.warehouseName
                    FROM warehouse w
                    INNER JOIN productnovariation pnv ON w.warehouseID = pnv.warehouseID
                    WHERE pnv.productID = @productID AND pnv.stockUnit > 0
                    ORDER BY w.warehouseName ASC;
                "
            Else
                ' For products with variation
                query = "
                    SELECT DISTINCT w.warehouseID, w.warehouseName
                    FROM warehouse w
                    INNER JOIN productvariationstock pvs ON w.warehouseID = pvs.warehouseID
                    WHERE pvs.productID = @productID AND pvs.optionCombination = @optionCombination AND pvs.stockUnit > 0
                    ORDER BY w.warehouseName ASC;
                "
            End If

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@productID", productID)
                        If productVariation = 1 Then
                            cmd.Parameters.AddWithValue("@optionCombination", optionCombination)
                        End If

                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            comboBox.Items.Clear()

                            If reader.HasRows Then
                                While reader.Read()
                                    Dim warehouseID As String = reader("warehouseID").ToString()
                                    Dim warehouseName As String = reader("warehouseName").ToString().Trim()

                                    ' Add warehouse to ComboBox with warehouseID in the Tag
                                    Dim item As New ComboBoxItem With {
                                        .Content = warehouseName,
                                        .Tag = warehouseID
                                    }
                                    comboBox.Items.Add(item)
                                End While
                                If comboBox.Items.Count > 0 Then
                                    comboBox.SelectedIndex = 0 ' Set default selection to the first item
                                End If
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
        End Sub

        Public Shared Sub SetMaxProductQty(productID As String, warehouseID As String, txtProductQty As TextBox, maxUnits As TextBlock, productVariation As Integer, optionCombination As String)
            Dim maxStockQuery As String = ""

            ' Determine which table to query based on productVariation value
            If productVariation = 0 Then
                ' Check in productnovariation table
                maxStockQuery = "
                    SELECT IFNULL(stockUnit, 0) AS maxStock
                    FROM productnovariation
                    WHERE productID = @productID AND warehouseID = @warehouseID;
                "
            ElseIf productVariation = 1 Then
                ' Check in productvariationstock table
                maxStockQuery = "
                    SELECT IFNULL(stockUnit, 0) AS maxStock
                    FROM productvariationstock
                    WHERE productID = @productID AND warehouseID = @warehouseID AND optionCombination = @optionCombination;
                "
            End If

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmdMaxStock As New MySqlCommand(maxStockQuery, conn)
                        cmdMaxStock.Parameters.AddWithValue("@productID", productID)
                        cmdMaxStock.Parameters.AddWithValue("@warehouseID", warehouseID)
                        If productVariation = 1 Then
                            cmdMaxStock.Parameters.AddWithValue("@optionCombination", optionCombination)
                        End If

                        Dim result As Object = cmdMaxStock.ExecuteScalar()

                        If result IsNot Nothing AndAlso result IsNot DBNull.Value Then
                            ' Get the max stock from the queried table
                            Dim maxStock As Integer = Convert.ToInt32(result)

                            ' Set the max stock as the TextBox Tag value
                            txtProductQty.Tag = maxStock

                            ' Update the maxUnits TextBlock with the max stock value
                            maxUnits.Text = $"Max units available: {maxStock}"

                            ' Set the TextBox max value to the number of maxStock digits
                            txtProductQty.MaxLength = maxStock.ToString().Length
                            txtProductQty.IsEnabled = True
                            txtProductQty.Text = "0"
                        Else
                            ' If no stock is found, disable the TextBox or handle accordingly
                            txtProductQty.Text = "0"
                            txtProductQty.Tag = 0
                            txtProductQty.IsEnabled = False
                            maxUnits.Text = "No stock available."
                        End If
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
        End Sub

        Public Shared Sub GetAvailableTransferToWarehouses(excludedWarehouseID As String, comboBox As ComboBox)
            ' SQL query to get all warehouses except the selected transferFrom warehouse
            Dim query As String = "
                SELECT warehouseID, warehouseName
                FROM warehouse
                WHERE warehouseID <> @excludedWarehouseID
                ORDER BY warehouseName ASC;
            "

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@excludedWarehouseID", excludedWarehouseID)

                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            comboBox.Items.Clear()

                            If reader.HasRows Then
                                While reader.Read()
                                    Dim warehouseID As String = reader("warehouseID").ToString()
                                    Dim warehouseName As String = reader("warehouseName").ToString().Trim()

                                    ' Add warehouse to ComboBox with warehouseID in the Tag
                                    Dim item As New ComboBoxItem With {
                                        .Content = warehouseName,
                                        .Tag = warehouseID
                                    }
                                    comboBox.Items.Add(item)
                                End While
                                If comboBox.Items.Count > 0 Then
                                    comboBox.SelectedIndex = 0 ' Set default selection to the first item
                                End If
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
        End Sub

        Public Shared Sub TransferStock(productID As String, warehouseIDFrom As String, warehouseIDTo As String, transferQty As Integer, productVariation As Integer, optionCombination As String)
            ' Define queries based on product variation type
            Dim queryCheckFrom As String = ""
            Dim queryCheckTo As String = ""
            Dim queryInsertFrom As String = ""
            Dim queryInsertTo As String = ""
            Dim queryTransferFrom As String = ""
            Dim queryTransferTo As String = ""
            ' Adding a query for the logs and will update the 
            Dim actionTransferQuery = "UPDATE warehouse SET userActionBy = @userActionBy, transferWarehouseTo = @transferWarehouseNameTo, dateModified = NOW() WHERE warehouseID = @warehouseID"

            If productVariation = 0 Then
                ' For productnovariation (productVariation = 0)
                queryCheckFrom = "SELECT COUNT(*) FROM productnovariation WHERE productID = @productID AND warehouseID = @warehouseIDFrom"
                queryCheckTo = "SELECT COUNT(*) FROM productnovariation WHERE productID = @productID AND warehouseID = @warehouseIDTo"
                queryInsertFrom = "INSERT INTO productnovariation (productID, warehouseID, stockUnit, sellingPrice, buyingPrice, defaultTax, taxType, discountRate, alertQuantity, dateCreated, dateModified) VALUES (@productID, @warehouseIDFrom, 0, 0, 0, 0, NULL, 0, 0, NOW(), NOW())"
                queryInsertTo = "INSERT INTO productnovariation (productID, warehouseID, stockUnit, sellingPrice, buyingPrice, defaultTax, taxType, discountRate, alertQuantity, dateCreated, dateModified) VALUES (@productID, @warehouseIDTo, 0, 0, 0, 0, NULL, 0, 0, NOW(), NOW())"
                queryTransferFrom = "UPDATE productnovariation SET stockUnit = stockUnit - @transferQty, dateModified = NOW() WHERE productID = @productID AND warehouseID = @warehouseIDFrom"
                queryTransferTo = "UPDATE productnovariation SET stockUnit = stockUnit + @transferQty, dateModified = NOW() WHERE productID = @productID AND warehouseID = @warehouseIDTo"
            ElseIf productVariation = 1 Then
                ' For productvariationstock (productVariation = 1)
                queryCheckFrom = "SELECT COUNT(*) FROM productvariationstock WHERE productID = @productID AND warehouseID = @warehouseIDFrom AND optionCombination = @optionCombination"
                queryCheckTo = "SELECT COUNT(*) FROM productvariationstock WHERE productID = @productID AND warehouseID = @warehouseIDTo AND optionCombination = @optionCombination"
                queryInsertFrom = "INSERT INTO productvariationstock (productID, warehouseID, optionCombination, stockUnit, sellingPrice, buyingPrice, defaultTax, taxType, discountRate, alertQuantity, dateCreated, dateModified) VALUES (@productID, @warehouseIDFrom, @optionCombination, 0, 0, 0, 0, NULL, 0, 0, NOW(), NOW())"
                queryInsertTo = "INSERT INTO productvariationstock (productID, warehouseID, optionCombination, stockUnit, sellingPrice, buyingPrice, defaultTax, taxType, discountRate, alertQuantity, dateCreated, dateModified) VALUES (@productID, @warehouseIDTo, @optionCombination, 0, 0, 0, 0, NULL, 0, 0, NOW(), NOW())"
                queryTransferFrom = "UPDATE productvariationstock SET stockUnit = stockUnit - @transferQty, dateModified = NOW() WHERE productID = @productID AND warehouseID = @warehouseIDFrom AND optionCombination = @optionCombination"
                queryTransferTo = "UPDATE productvariationstock SET stockUnit = stockUnit + @transferQty, dateModified = NOW() WHERE productID = @productID AND warehouseID = @warehouseIDTo AND optionCombination = @optionCombination"
            End If

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    ' Start a transaction to ensure the entire operation is atomic
                    Using transaction As MySqlTransaction = conn.BeginTransaction()
                        Try
                            ' Step 1: Check existence in "from" warehouse, insert if not exists
                            Using cmdCheckFrom As New MySqlCommand(queryCheckFrom, conn, transaction)
                                cmdCheckFrom.Parameters.AddWithValue("@productID", productID)
                                cmdCheckFrom.Parameters.AddWithValue("@warehouseIDFrom", warehouseIDFrom)
                                If productVariation = 1 Then
                                    cmdCheckFrom.Parameters.AddWithValue("@optionCombination", optionCombination)
                                End If

                                Dim existsFrom As Integer = Convert.ToInt32(cmdCheckFrom.ExecuteScalar())
                                If existsFrom = 0 Then
                                    ' Insert product if it doesn't exist in the "from" warehouse
                                    Using cmdInsertFrom As New MySqlCommand(queryInsertFrom, conn, transaction)
                                        cmdInsertFrom.Parameters.AddWithValue("@productID", productID)
                                        cmdInsertFrom.Parameters.AddWithValue("@warehouseIDFrom", warehouseIDFrom)
                                        If productVariation = 1 Then
                                            cmdInsertFrom.Parameters.AddWithValue("@optionCombination", optionCombination)
                                        End If
                                        cmdInsertFrom.ExecuteNonQuery()
                                    End Using
                                End If
                            End Using

                            ' Step 2: Check existence in "to" warehouse, insert if not exists
                            Using cmdCheckTo As New MySqlCommand(queryCheckTo, conn, transaction)
                                cmdCheckTo.Parameters.AddWithValue("@productID", productID)
                                cmdCheckTo.Parameters.AddWithValue("@warehouseIDTo", warehouseIDTo)
                                If productVariation = 1 Then
                                    cmdCheckTo.Parameters.AddWithValue("@optionCombination", optionCombination)
                                End If

                                Dim existsTo As Integer = Convert.ToInt32(cmdCheckTo.ExecuteScalar())
                                If existsTo = 0 Then
                                    ' Insert product if it doesn't exist in the "to" warehouse
                                    Using cmdInsertTo As New MySqlCommand(queryInsertTo, conn, transaction)
                                        cmdInsertTo.Parameters.AddWithValue("@productID", productID)
                                        cmdInsertTo.Parameters.AddWithValue("@warehouseIDTo", warehouseIDTo)
                                        If productVariation = 1 Then
                                            cmdInsertTo.Parameters.AddWithValue("@optionCombination", optionCombination)
                                        End If
                                        cmdInsertTo.ExecuteNonQuery()
                                    End Using
                                End If
                            End Using

                            ' Step 3: Update stock in the "from" warehouse
                            Using cmdFrom As New MySqlCommand(queryTransferFrom, conn, transaction)
                                cmdFrom.Parameters.AddWithValue("@productID", productID)
                                cmdFrom.Parameters.AddWithValue("@warehouseIDFrom", warehouseIDFrom)
                                cmdFrom.Parameters.AddWithValue("@transferQty", transferQty)
                                If productVariation = 1 Then
                                    cmdFrom.Parameters.AddWithValue("@optionCombination", optionCombination)
                                End If
                                cmdFrom.ExecuteNonQuery()
                            End Using

                            ' Step 4: Update stock in the "to" warehouse
                            Using cmdTo As New MySqlCommand(queryTransferTo, conn, transaction)
                                cmdTo.Parameters.AddWithValue("@productID", productID)
                                cmdTo.Parameters.AddWithValue("@warehouseIDTo", warehouseIDTo)
                                cmdTo.Parameters.AddWithValue("@transferQty", transferQty)
                                If productVariation = 1 Then
                                    cmdTo.Parameters.AddWithValue("@optionCombination", optionCombination)
                                End If
                                cmdTo.ExecuteNonQuery()
                            End Using

                            '"UPDATE warehouse SET userActionBy = @userActionBy, transferWarehouseTo = @transferWarehouseNameTo, dateModified = NOW() WHERE warehouseID = @warehouseID"


                            'Step 5 Adding a Log whoever change the transfer stock
                            Using actionTransferCmd As New MySqlCommand(actionTransferQuery, conn, transaction)
                                actionTransferCmd.Parameters.AddWithValue("@userActionBy", CacheOnLoggedInName)
                                actionTransferCmd.Parameters.AddWithValue("@transferWarehouseNameTo", CacheOnWarehouseTransferName)
                                actionTransferCmd.Parameters.AddWithValue("@warehouseID", warehouseIDFrom)
                                actionTransferCmd.ExecuteNonQuery()
                            End Using

                            ' Commit the transaction if all operations succeed
                            transaction.Commit()
                        Catch ex As Exception
                            ' Roll back the transaction if any operation fails
                            transaction.Rollback()
                            Throw ' Re-throw the exception for the outer catch block
                        End Try
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
        End Sub
    End Class
End Namespace