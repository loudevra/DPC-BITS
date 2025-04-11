Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models


Namespace DPC.Data.Controllers
    Public Class StocksTransferController
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

        Public Shared Sub GetAvailableWarehouses(productID As String, comboBox As ComboBox)
            ' SQL query to get warehouseID from both productnovariation and productvariationstock tables
            Dim query As String = "
                                    SELECT DISTINCT w.warehouseID, w.warehouseName
                                    FROM warehouse w
                                    WHERE w.warehouseID IN (
                                        SELECT warehouseID
                                        FROM productnovariation
                                        WHERE productID = @productID AND stockUnit > 0
                                        UNION
                                        SELECT warehouseID
                                        FROM productvariationstock
                                        WHERE productID = @productID AND stockUnit > 0
                                    )
                                    ORDER BY w.warehouseName ASC;
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
                                    Dim warehouseID As String = reader("warehouseID").ToString()
                                    Dim warehouseName As String = reader("warehouseName").ToString().Trim()

                                    ' Add warehouse to ComboBox with warehouseID in the Tag
                                    Dim item As New ComboBoxItem With {
                                .Content = warehouseName,
                                .Tag = warehouseID
                            }
                                    comboBox.Items.Add(item)
                                End While
                                comboBox.SelectedIndex = 0 ' Set default selection to the first item
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

        Public Shared Sub SetMaxProductQty(productID As String, warehouseID As String, txtProductQty As TextBox, maxUnits As TextBlock)
            ' First, get the productVariation for the selected product
            Dim query As String = "
        SELECT productVariation 
        FROM product 
        WHERE productID = @productID;
    "

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@productID", productID)

                        ' Use a single DataReader to read productVariation
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            If reader.HasRows AndAlso reader.Read() Then
                                Dim productVariation As Integer = Convert.ToInt32(reader("productVariation"))
                                reader.Close() ' Ensure to close the first reader before the next query

                                ' Determine which table to query based on productVariation value
                                Dim maxStockQuery As String = ""

                                If productVariation = 0 Then
                                    ' Check in productnovariation table
                                    maxStockQuery = "
                                SELECT IFNULL(MAX(stockUnit), 0) AS maxStock
                                FROM productnovariation
                                WHERE productID = @productID AND warehouseID = @warehouseID;
                            "
                                ElseIf productVariation = 1 Then
                                    ' Check in productvariationstock table
                                    maxStockQuery = "
                                SELECT IFNULL(MAX(stockUnit), 0) AS maxStock
                                FROM productvariationstock
                                WHERE productID = @productID AND warehouseID = @warehouseID;
                            "
                                End If

                                ' Now query the stock table to get the max stock
                                Using cmdMaxStock As New MySqlCommand(maxStockQuery, conn)
                                    cmdMaxStock.Parameters.AddWithValue("@productID", productID)
                                    cmdMaxStock.Parameters.AddWithValue("@warehouseID", warehouseID)

                                    ' Execute the second query to get max stock from the appropriate table
                                    Using stockReader As MySqlDataReader = cmdMaxStock.ExecuteReader()
                                        If stockReader.HasRows AndAlso stockReader.Read() Then
                                            ' Get the max stock from the queried table
                                            Dim maxStock As Integer = Convert.ToInt32(stockReader("maxStock"))

                                            ' Set the max stock as the TextBox Tag value
                                            txtProductQty.Tag = maxStock
                                            txtProductQty.MaxLength = maxStock.ToString().Length ' Optional: limit the number of characters

                                            ' Update the maxUnits TextBlock with the max stock value
                                            maxUnits.Text = $"Max units available: {maxStock}"

                                            ' Set the TextBox max value to the number of maxStock digits
                                            txtProductQty.MaxLength = maxStock.ToString().Length
                                            txtProductQty.IsEnabled = True
                                        Else
                                            ' If no stock is found, disable the TextBox or handle accordingly
                                            txtProductQty.Text = "0"
                                            txtProductQty.IsEnabled = False
                                            maxUnits.Text = "No stock available."
                                        End If
                                        stockReader.Close() ' Close stockReader after usage
                                    End Using
                                End Using
                            Else
                                ' If productVariation is not found, handle accordingly
                                txtProductQty.Text = "0"
                                txtProductQty.IsEnabled = False
                                maxUnits.Text = "Product variation not found."
                            End If
                        End Using
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
                                comboBox.SelectedIndex = 0 ' Set default selection to the first item
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

        Public Shared Sub TransferStock(productID As String, warehouseIDFrom As String, warehouseIDTo As String, transferQty As Integer)
            ' Step 1: Retrieve productVariation for the selected productID
            Dim queryGetProductVariation As String = "
                                                    SELECT productVariation
                                                    FROM product 
                                                    WHERE productID = @productID;
                                                    "

            ' Step 2: Determine which table to check based on productVariation (0 = productnovariation, 1 = productvariationstock)
            Dim queryCheckFrom As String = ""
            Dim queryCheckTo As String = ""
            Dim queryInsertFrom As String = ""
            Dim queryInsertTo As String = ""
            Dim queryTransferFrom As String = ""
            Dim queryTransferTo As String = ""

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()

                    ' Step 3: Retrieve the productVariation value
                    Using cmdGetProductVariation As New MySqlCommand(queryGetProductVariation, conn)
                        cmdGetProductVariation.Parameters.AddWithValue("@productID", productID)

                        Dim productVariation As Integer = Convert.ToInt32(cmdGetProductVariation.ExecuteScalar())

                        ' Based on productVariation value, choose the correct table and queries
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
                            queryCheckFrom = "SELECT COUNT(*) FROM productvariationstock WHERE productID = @productID AND warehouseID = @warehouseIDFrom"
                            queryCheckTo = "SELECT COUNT(*) FROM productvariationstock WHERE productID = @productID AND warehouseID = @warehouseIDTo"
                            queryInsertFrom = "INSERT INTO productvariationstock (productID, warehouseID, optionCombination, stockUnit, sellingPrice, buyingPrice, defaultTax, taxType, discountRate, alertQuantity, dateCreated, dateModified) VALUES (@productID, @warehouseIDFrom, '', 0, 0, 0, 0, NULL, 0, 0, NOW(), NOW())"
                            queryInsertTo = "INSERT INTO productvariationstock (productID, warehouseID, optionCombination, stockUnit, sellingPrice, buyingPrice, defaultTax, taxType, discountRate, alertQuantity, dateCreated, dateModified) VALUES (@productID, @warehouseIDTo, '', 0, 0, 0, 0, NULL, 0, 0, NOW(), NOW())"
                            queryTransferFrom = "UPDATE productvariationstock SET stockUnit = stockUnit - @transferQty, dateModified = NOW() WHERE productID = @productID AND warehouseID = @warehouseIDFrom"
                            queryTransferTo = "UPDATE productvariationstock SET stockUnit = stockUnit + @transferQty, dateModified = NOW() WHERE productID = @productID AND warehouseID = @warehouseIDTo"
                        End If

                        ' Step 4: Check existence in "from" warehouse, insert if not exists
                        Using cmdCheckFrom As New MySqlCommand(queryCheckFrom, conn)
                            cmdCheckFrom.Parameters.AddWithValue("@productID", productID)
                            cmdCheckFrom.Parameters.AddWithValue("@warehouseIDFrom", warehouseIDFrom)

                            Dim existsFrom As Integer = Convert.ToInt32(cmdCheckFrom.ExecuteScalar())
                            If existsFrom = 0 Then
                                ' Insert product if it doesn't exist in the "from" warehouse
                                Using cmdInsertFrom As New MySqlCommand(queryInsertFrom, conn)
                                    cmdInsertFrom.Parameters.AddWithValue("@productID", productID)
                                    cmdInsertFrom.Parameters.AddWithValue("@warehouseIDFrom", warehouseIDFrom)
                                    cmdInsertFrom.ExecuteNonQuery()
                                End Using
                            End If
                        End Using

                        ' Step 5: Check existence in "to" warehouse, insert if not exists
                        Using cmdCheckTo As New MySqlCommand(queryCheckTo, conn)
                            cmdCheckTo.Parameters.AddWithValue("@productID", productID)
                            cmdCheckTo.Parameters.AddWithValue("@warehouseIDTo", warehouseIDTo)

                            Dim existsTo As Integer = Convert.ToInt32(cmdCheckTo.ExecuteScalar())
                            If existsTo = 0 Then
                                ' Insert product if it doesn't exist in the "to" warehouse
                                Using cmdInsertTo As New MySqlCommand(queryInsertTo, conn)
                                    cmdInsertTo.Parameters.AddWithValue("@productID", productID)
                                    cmdInsertTo.Parameters.AddWithValue("@warehouseIDTo", warehouseIDTo)
                                    cmdInsertTo.ExecuteNonQuery()
                                End Using
                            End If
                        End Using

                        ' Step 6: Update stock in the "from" warehouse
                        Using cmdFrom As New MySqlCommand(queryTransferFrom, conn)
                            cmdFrom.Parameters.AddWithValue("@productID", productID)
                            cmdFrom.Parameters.AddWithValue("@warehouseIDFrom", warehouseIDFrom)
                            cmdFrom.Parameters.AddWithValue("@transferQty", transferQty)
                            cmdFrom.ExecuteNonQuery()
                        End Using

                        ' Step 7: Update stock in the "to" warehouse
                        Using cmdTo As New MySqlCommand(queryTransferTo, conn)
                            cmdTo.Parameters.AddWithValue("@productID", productID)
                            cmdTo.Parameters.AddWithValue("@warehouseIDTo", warehouseIDTo)
                            cmdTo.Parameters.AddWithValue("@transferQty", transferQty)
                            cmdTo.ExecuteNonQuery()
                        End Using

                        ' Commit the transaction (if using MySQL transaction, not needed here)
                        MessageBox.Show("Stock transferred successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
        End Sub


    End Class
End Namespace
