Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models
Imports DocumentFormat.OpenXml.Bibliography


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
            Dim CheckProductQuery As String = ""
            Dim UpdateProductQuery As String = ""
            Dim BeforeStockUnit As Integer = 0

            ' Product Variations Statement
            If productVariation = 0 Then
                ' For productnovariation (productVariation = 0)
                CheckProductQuery = "SELECT * FROM productnovariation WHERE productID = @productID AND warehouseID = @warehouseIDFrom"
                UpdateProductQuery = "UPDATE productnovariation SET warehouseID = @warehouseIDTo, stockUnit = @transferQty, dateModified = NOW() WHERE productID = @productID"
            ElseIf productVariation = 1 Then
                ' For productvariationstock (productVariation = 1)
                CheckProductQuery = "SELECT * FROM productvariationstock WHERE productID = @productID AND warehouseID = @warehouseIDFrom AND optionCombination = @optionCombination"
                UpdateProductQuery = "UPDATE productvariationstock SET warehouseID = @warehouseIDTo, stockUnit = @transferQty, dateModified = NOW() WHERE productID = @productID AND optionCombination = @optionCombination"
            End If

            ' Query for Loggin the stock transfer
            Dim loggingStockTransferQuery As String = "INSERT INTO stocktransferlogs (warehouseFrom, warehouseTo, actionEmployeeID, actionEmployeeName, productID, productName, beforestockunit, newstockunit, dateLog) VALUES (@warehouseFrom, @warehouseTo, @actionEmployeeID, @actionEmployeeName, @productID, @productName, @beforeStockUnit, @newStockUnit, NOW())"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                conn.Open()

                Try
                    ' Begin the transaction 
                    Using transaction As MySqlTransaction = conn.BeginTransaction()
                        Try
                            ' Check for the existing productID and Check the current stockUnit and store it in BeforeStockUnit
                            Using CheckProductCmd As New MySqlCommand(CheckProductQuery, conn, transaction)
                                CheckProductCmd.Parameters.AddWithValue("@productID", productID)
                                CheckProductCmd.Parameters.AddWithValue("@warehouseIDFrom", warehouseIDFrom)
                                If productVariation = 1 Then
                                    CheckProductCmd.Parameters.AddWithValue("@optionCombination", optionCombination)
                                End If
                                Dim reader = CheckProductCmd.ExecuteReader()

                                If reader.HasRows Then
                                    While reader.Read() ' Move cursor to the first row
                                        BeforeStockUnit = reader("stockUnit")
                                    End While

                                End If

                                reader.Close()
                            End Using

                            ' Updating the whole warehouseID and the stockUnit
                            Using UpdateProductCmd As New MySqlCommand(UpdateProductQuery, conn, transaction)
                                UpdateProductCmd.Parameters.AddWithValue("@warehouseIDTo", warehouseIDTo)
                                UpdateProductCmd.Parameters.AddWithValue("@transferQty", transferQty)
                                UpdateProductCmd.Parameters.AddWithValue("@productID", productID)
                                If productVariation = 1 Then
                                    UpdateProductCmd.Parameters.AddWithValue("@optionCombination", optionCombination)
                                End If
                                UpdateProductCmd.ExecuteNonQuery()

                                MessageBox.Show("Successfully Updates the Stock in this Product")
                            End Using

                            ' Logging the Stock Transfer to the stocktransferlogs table
                            ' UPDATE 16 / 06 / 2025 at 12:34pm - Change the Codes where it will insert inside of the stocktransferlog table 
                            Using actionTransferCmd As New MySqlCommand(loggingStockTransferQuery, conn, transaction)
                                actionTransferCmd.Parameters.AddWithValue("@warehouseFrom", CacheOnWarehouseFromTransferName)
                                actionTransferCmd.Parameters.AddWithValue("@warehouseTo", CacheOnWarehouseToTransferName)
                                actionTransferCmd.Parameters.AddWithValue("@actionEmployeeID", CacheOnEmployeeID)
                                actionTransferCmd.Parameters.AddWithValue("@actionEmployeeName", CacheOnLoggedInName)
                                actionTransferCmd.Parameters.AddWithValue("@productID", CacheStockTransferProductID)
                                actionTransferCmd.Parameters.AddWithValue("@productName", CacheStockTransferProductName)
                                actionTransferCmd.Parameters.AddWithValue("@beforeStockUnit", BeforeStockUnit)
                                actionTransferCmd.Parameters.AddWithValue("@newStockUnit", transferQty)
                                actionTransferCmd.ExecuteNonQuery()

                                MessageBox.Show("Successfully Logs the Stock Transfer")
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