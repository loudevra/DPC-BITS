
Imports System.Transactions
Imports MySql.Data.MySqlClient
Imports OfficeOpenXml
Imports DPC.DPC.Data.Helpers



Namespace DPC.Data.Controllers
    Public Class UpdateProduct

        Public Shared Sub UpdateSelectedProduct(Toggle As System.Windows.Controls.Primitives.ToggleButton,
                                                Checkbox As Controls.CheckBox,
                                                ProductName As String,
                                                ProductCode As String,
                                                Category As Int64,
                                                SubCategory As Int64,
                                                Warehouse As Integer,
                                                Brand As Int64,
                                                Supplier As Int64,
                                                RetailPrice As TextBox,
                                                PurchaseOrder As TextBox,
                                                DefaultTax As Double,
                                                DiscountRate As Double,
                                                StockUnits As Integer,
                                                AlertQuantity As Integer,
                                                MeasurementUnit As String,
                                                Description As String,
                                                SerialNumbers As List(Of String),
                                                ProductImage As String)

            ' Determine if the product is a variation
            Dim variation As Integer = If(Toggle.IsChecked = True, 1, 0)

            ' ✅ Handle SubCategory when it's Nothing
            Dim connStr As String = SplashScreen.GetDatabaseConnection().ConnectionString
            Using conn As New MySqlConnection(connStr)
                conn.Open()
                Using transaction = conn.BeginTransaction()
                    Try
                        'If no variation product
                        If variation = 0 Then
                            'validate required fields for no variation
                            If Not ProductController.EditProductValidateFields(Checkbox, ProductName, ProductCode, Category, SubCategory, Warehouse, Brand, Supplier, RetailPrice, PurchaseOrder, DefaultTax, DiscountRate, StockUnits, AlertQuantity, MeasurementUnit, Description, SerialNumbers) Then
                                MessageBox.Show("Please fill in all required fields.", "Input Error", MessageBoxButton.OK)
                                Exit Sub
                            End If

                            UpdateProductsTable(conn, transaction, cacheProductID, ProductCode, ProductName, Category, SubCategory, Supplier, Brand, ProductImage, MeasurementUnit, Description, 0)

                            UpdateProductNoVariationTable(conn, transaction, RetailPrice.Text, PurchaseOrder.Text, DefaultTax, DiscountRate, StockUnits, AlertQuantity, cacheProductID)

                            If Checkbox.IsChecked Then
                                MessageBox.Show("Serial Numbers On")
                                UpdateSerialNumbersForProduct(conn, transaction, SerialNumbers, cacheProductID)
                            Else
                                MessageBox.Show("Serial Numbers Off")
                                DeleteSerialNumbersIfCheckboxEqualsOff(conn, transaction, cacheProductID)
                            End If

                            transaction.Commit()
                            MessageBox.Show("The product was updated succesfully", "PRODUCT UPDATE NOTICE")
                            cacheProductUpdateCompletion = True
                        End If
                    Catch ex As Exception
                        MessageBox.Show("An error occurred while updating the product: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                        transaction.Rollback()
                    End Try
                End Using
            End Using
        End Sub


        Public Shared Sub UpdateProductsTable(conn As MySqlConnection,
                                              transaction As MySqlTransaction,
                                              prodID As String, prodCode As String,
                                              prodName As String,
                                              Category As Int64,
                                              subCategory As Int64,
                                              Supplier As Int64,
                                              Brand As Int64,
                                              prodImage As String,
                                              prodDescription As String,
                                              measureUnit As String,
                                              prodVariation As Integer)
            Dim query As String = "UPDATE product 
                                   SET productCode = @prodCode, 
                                       productName = @prodName, 
                                       categoryID = @categoryID, 
                                       subcategoryID = @subcategoryID, 
                                       supplierID = @supplierID, 
                                       brandID = @brandID, 
                                       productImage = @prodImage, 
                                       measurementUnit = @measureUnit, 
                                       productDescription = @prodDescription, 
                                       productVariation = @prodVariation 
                                   WHERE productID = @prodID"
            Using cmd As New MySqlCommand(query, conn, transaction)
                cmd.Parameters.AddWithValue("@prodID", prodID)
                cmd.Parameters.AddWithValue("@prodCode", prodCode)
                cmd.Parameters.AddWithValue("@prodName", prodName)
                cmd.Parameters.AddWithValue("@categoryID", Category)
                cmd.Parameters.AddWithValue("@subcategoryID", subCategory)
                cmd.Parameters.AddWithValue("@supplierID", Supplier)
                cmd.Parameters.AddWithValue("@brandID", Brand)
                cmd.Parameters.AddWithValue("@prodImage", prodImage)
                cmd.Parameters.AddWithValue("@measureUnit", measureUnit)
                cmd.Parameters.AddWithValue("@prodDescription", prodDescription)
                cmd.Parameters.AddWithValue("@prodVariation", prodVariation)
                cmd.ExecuteNonQuery()
            End Using
        End Sub

        Public Shared Sub UpdateProductNoVariationTable(conn As MySqlConnection,
                                                        transaction As MySqlTransaction,
                                                        sellingPrice As String,
                                                        buyingPrice As Double,
                                                        defaultTax As Double,
                                                        discountRate As Double,
                                                        stockUnit As Integer,
                                                        alertQuantity As Integer,
                                                        productID As String)
            Dim UpdateQuery As String = "UPDATE productnovariation
                                         SET sellingPrice = @sellingPrice,
                                             buyingPrice = @buyingPrice,
                                             defaultTax = @defaultTax,
                                             discountRate = @discountRate,
                                             stockUnit = @stockUnit,
                                             alertQuantity = @alertQuantity
                                         WHERE productID = @prodID"
            Using cmd As New MySqlCommand(UpdateQuery, conn, transaction)
                cmd.Parameters.AddWithValue("@sellingPrice", sellingPrice)
                cmd.Parameters.AddWithValue("@buyingPrice", buyingPrice)
                cmd.Parameters.AddWithValue("@defaultTax", defaultTax)
                cmd.Parameters.AddWithValue("@discountRate", discountRate)
                cmd.Parameters.AddWithValue("@stockUnit", stockUnit)
                cmd.Parameters.AddWithValue("@alertQuantity", alertQuantity)
                cmd.Parameters.AddWithValue("@prodID", productID)
                cmd.ExecuteNonQuery()
            End Using
        End Sub


        Public Shared Sub UpdateSerialNumbersForProduct(conn As MySqlConnection, transaction As MySqlTransaction, SerialNumbers As List(Of String), productID As String)
            Dim DeleteQuery As String = "DELETE FROM serialnumberproduct WHERE ProductID = @ProductID"
            Dim InsertQuery As String = "INSERT INTO serialnumberproduct (SerialNumber, ProductID) VALUES (@SerialNumber, @ProductID)"

            'Delete existing serial numbers for the product
            Using DeleteCmd As New MySqlCommand(DeleteQuery, conn, transaction)
                DeleteCmd.Parameters.AddWithValue("@ProductID", productID)
                DeleteCmd.ExecuteNonQuery()
            End Using

            Using cmd As New MySqlCommand(InsertQuery, conn, transaction)
                For Each serialNumber As String In SerialNumbers
                    If Not String.IsNullOrWhiteSpace(serialNumber) Then
                        cmd.Parameters.Clear()
                        cmd.Parameters.AddWithValue("@SerialNumber", serialNumber)
                        cmd.Parameters.AddWithValue("@ProductID", productID)
                        cmd.ExecuteNonQuery()
                    End If
                Next
            End Using

            cacheSerialNumbers.Clear()
        End Sub

        Public Shared Sub DeleteSerialNumbersIfCheckboxEqualsOff(conn As MySqlConnection, transaction As MySqlTransaction, productID As String)
            Dim DeleteQuery As String = "DELETE FROM serialnumberproduct WHERE ProductID = @ProductID"

            Using DeleteCmd As New MySqlCommand(DeleteQuery, conn, transaction)
                DeleteCmd.Parameters.AddWithValue("@ProductID", productID)
                DeleteCmd.ExecuteNonQuery()
            End Using
        End Sub


        Public Shared Sub DeleteProductData(dataTable As String, productID As String)
            Dim query As String = "DELETE FROM " & dataTable & " WHERE ProductID = @ProductID"

            Dim connStr As String = SplashScreen.GetDatabaseConnection().ConnectionString
            Using conn As New MySqlConnection(connStr)
                conn.Open()
                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@ProductID", productID)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

        End Sub
    End Class
End Namespace
