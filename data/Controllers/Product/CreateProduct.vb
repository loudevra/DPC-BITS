Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Data.Helpers

Namespace DPC.Data.Controllers
    Public Class CreateProduct
        Public Shared Sub InsertNewProduct(Toggle As System.Windows.Controls.Primitives.ToggleButton, Checkbox As Controls.CheckBox,
            ProductName As TextBox, ProductCode As TextBox, Category As ComboBox, SubCategory As ComboBox, Warehouse As ComboBox,
            Brand As ComboBox, Supplier As ComboBox,
            RetailPrice As TextBox, PurchaseOrder As TextBox, DefaultTax As TextBox,
            DiscountRate As TextBox, StockUnits As TextBox, AlertQuantity As TextBox,
            MeasurementUnit As ComboBox, Description As TextBox, ValidDate As DatePicker,
            SerialNumbers As List(Of TextBox), ProductImage As String)

            ' Determine if the product is a variation
            Dim variation As Integer = If(Toggle.IsChecked = True, 1, 0)

            ' Add this check at the beginning of the InsertNewProduct function, after the variation check
            If Not String.IsNullOrEmpty(ProductCode.Text) AndAlso ProductController.IsProductCodeExists(ProductCode.Text.Trim()) Then
                MessageBox.Show("This product code already exists. Please use a different code.", "Duplicate Product Code", MessageBoxButton.OK, MessageBoxImage.Warning)
                ProductCode.Focus()
                Exit Sub
            End If


            ' ✅ Handle SubCategory when it's Nothing
            Dim subCategoryId As Integer = If(SubCategory.SelectedItem IsNot Nothing, CType(SubCategory.SelectedItem, ComboBoxItem).Tag, 0)

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                conn.Open()
                Using transaction = conn.BeginTransaction()
                    ' Call the appropriate insertion function based on variation flag
                    If variation = 0 Then
                        ' Validate required fields for no variation
                        If Not ProductController.ValidateProductFields(Checkbox, ProductName, ProductCode, Category, SubCategory, Warehouse, Brand, Supplier,
                              RetailPrice, PurchaseOrder, DefaultTax, DiscountRate, StockUnits,
                              AlertQuantity, MeasurementUnit, Description, ValidDate, SerialNumbers) Then
                            MessageBox.Show("Please fill in all required fields!", "Input Error", MessageBoxButton.OK)
                            Exit Sub
                        End If

                        ' Generate product ID
                        Dim productID As String = ProductController.GenerateProductCode()

                        ' Insert into product table first
                        Dim productQuery As String = "INSERT INTO product (productID, productName, productCode,categoryID, subcategoryID, supplierID, brandID, dateCreated, productVariation, productImage, measurementUnit, productDescription) 
                                          VALUES (@productID, @ProductName, @ProductCode ,@Category, @SubCategory, @SupplierID, @BrandID, @DateCreated, @variation, @ProductImage, @Description, @MeasurementUnit);"
                        Using productCmd As New MySqlCommand(productQuery, conn, transaction)
                            productCmd.Parameters.AddWithValue("@productID", productID)
                            productCmd.Parameters.AddWithValue("@ProductName", ProductName.Text)
                            productCmd.Parameters.AddWithValue("@ProductCode", ProductCode.Text)
                            productCmd.Parameters.AddWithValue("@Category", CType(Category.SelectedItem, ComboBoxItem).Tag)
                            productCmd.Parameters.AddWithValue("@SubCategory", subCategoryId) ' ✅ Now using 0 if Nothing
                            productCmd.Parameters.AddWithValue("@SupplierID", CType(Supplier.SelectedItem, ComboBoxItem).Tag)
                            productCmd.Parameters.AddWithValue("@BrandID", CType(Brand.SelectedItem, ComboBoxItem).Tag)
                            productCmd.Parameters.AddWithValue("@DateCreated", ValidDate.SelectedDate)
                            productCmd.Parameters.AddWithValue("@variation", variation)
                            productCmd.Parameters.AddWithValue("@ProductImage", ProductImage)
                            productCmd.Parameters.AddWithValue("@Description", Description.Text)
                            productCmd.Parameters.AddWithValue("@MeasurementUnit", CType(MeasurementUnit.SelectedItem, ComboBoxItem).Tag)
                            productCmd.ExecuteNonQuery()
                        End Using

                        ProductController.InsertNonVariationProduct(conn, transaction, productID, Warehouse, RetailPrice, PurchaseOrder, DefaultTax,
                        DiscountRate, StockUnits, AlertQuantity, ValidDate, SerialNumbers, Checkbox)

                        transaction.Commit()
                        MessageBox.Show($"Product {ProductName.Text} with Product Code {productID} has been inserted successfully.")
                    Else

                        ' Generate product ID
                        Dim productID As String = ProductController.GenerateProductCode()

                        ' Insert into product table first
                        Dim productQuery As String = "INSERT INTO product (productID, productName, categoryID, subcategoryID, supplierID, brandID, dateCreated, productVariation, productImage, measurementUnit, productDescription) 
                                          VALUES (@productID, @ProductName, @Category, @SubCategory, @SupplierID, @BrandID, @DateCreated, @variation, @ProductImage, @Description, @MeasurementUnit);"
                        Using productCmd As New MySqlCommand(productQuery, conn, transaction)
                            productCmd.Parameters.AddWithValue("@productID", productID)
                            productCmd.Parameters.AddWithValue("@ProductName", ProductName.Text)
                            productCmd.Parameters.AddWithValue("@Category", CType(Category.SelectedItem, ComboBoxItem).Tag)
                            productCmd.Parameters.AddWithValue("@SubCategory", subCategoryId) ' ✅ Now using 0 if Nothing
                            productCmd.Parameters.AddWithValue("@SupplierID", CType(Supplier.SelectedItem, ComboBoxItem).Tag)
                            productCmd.Parameters.AddWithValue("@BrandID", CType(Brand.SelectedItem, ComboBoxItem).Tag)
                            productCmd.Parameters.AddWithValue("@DateCreated", ValidDate.SelectedDate)
                            productCmd.Parameters.AddWithValue("@variation", variation)
                            productCmd.Parameters.AddWithValue("@ProductImage", ProductImage)
                            productCmd.Parameters.AddWithValue("@Description", Description.Text)
                            productCmd.Parameters.AddWithValue("@MeasurementUnit", CType(MeasurementUnit.SelectedItem, ComboBoxItem).Tag)
                            productCmd.ExecuteNonQuery()
                        End Using

                        'Insert variation data
                        ProductController.InsertVariationProduct(conn, transaction, productID)

                        transaction.Commit()
                        MessageBox.Show($"Product {ProductName.Text} with Product Code {productID} has been inserted successfully.")
                    End If


                End Using
            End Using
        End Sub

        Public Shared Sub InsertNonVariationProduct(conn As MySqlConnection, transaction As MySqlTransaction, productID As String,
                                     Warehouse As ComboBox, SellingPrice As TextBox, BuyingPrice As TextBox,
                                     DefaultTax As TextBox, DiscountRate As TextBox,
                                     StockUnits As TextBox, AlertQuantity As TextBox,
                                     ValidDate As DatePicker, SerialNumbers As List(Of TextBox),
                                     Checkbox As Controls.CheckBox)

            ' Insert into productnovariation table
            Dim query As String = "INSERT INTO productnovariation (productID, warehouseID, sellingPrice, buyingPrice, defaultTax, taxType, 
                                discountRate, discountType, stockUnit, alertQuantity, dateCreated, dateModified) 
                           VALUES (@productID, @WarehouseID, @SellingPrice, @BuyingPrice, @DefaultTax, NULL, 
                                @DiscountRate, NULL, @StockUnits, @AlertQuantity, @DateCreated, NULL);"
            Using cmd As New MySqlCommand(query, conn, transaction)
                cmd.Parameters.AddWithValue("@productID", productID)
                cmd.Parameters.AddWithValue("@WarehouseID", CType(Warehouse.SelectedItem, ComboBoxItem).Tag)
                cmd.Parameters.AddWithValue("@SellingPrice", SellingPrice.Text)
                cmd.Parameters.AddWithValue("@BuyingPrice", BuyingPrice.Text)
                cmd.Parameters.AddWithValue("@DefaultTax", DefaultTax.Text)
                cmd.Parameters.AddWithValue("@DiscountRate", DiscountRate.Text)
                cmd.Parameters.AddWithValue("@StockUnits", StockUnits.Text)
                cmd.Parameters.AddWithValue("@AlertQuantity", AlertQuantity.Text)
                cmd.Parameters.AddWithValue("@DateCreated", ValidDate.SelectedDate)
                cmd.ExecuteNonQuery()
            End Using

            ' Check if the product has serial numbers and insert them
            If Checkbox.IsChecked = True Then
                ProductController.InsertSerialNumbersForProduct(conn, transaction, SerialNumbers, productID)
            End If
        End Sub

        Public Shared Sub InsertVariationProduct(conn As MySqlConnection, transaction As MySqlTransaction, productID As String)
            ' Get saved variations from the AddVariation class
            Dim variations As List(Of ProductVariation) = DPC.Components.Forms.AddVariation.SavedVariations

            ' Loop through each variation and insert it
            For Each variation As ProductVariation In variations
                ' Insert into productvariation table with variation name
                Dim query As String = "INSERT INTO productvariation (productID, variationName, dateCreated) 
               VALUES (@productID, @variationName, @DateCreated);
               SELECT LAST_INSERT_ID();"

                Dim variationID As Long = 0

                ' Insert the variation and get the generated ID
                Using cmd As New MySqlCommand(query, conn, transaction)
                    cmd.Parameters.AddWithValue("@productID", productID)
                    cmd.Parameters.AddWithValue("@variationName", variation.VariationName)
                    cmd.Parameters.AddWithValue("@DateCreated", DateTime.Now)
                    variationID = Convert.ToInt64(cmd.ExecuteScalar())
                End Using

                ' Now insert all options for this variation
                For Each varOption As VariationOption In variation.Options
                    ' Insert option into the variationoption table
                    Dim optionQuery As String = "INSERT INTO variationoption (variationID, optionName, optionImage, dateCreated) 
                   VALUES (@variationID, @optionName, @optionImage, @dateCreated)"

                    Using optCmd As New MySqlCommand(optionQuery, conn, transaction)
                        optCmd.Parameters.AddWithValue("@variationID", variationID)
                        optCmd.Parameters.AddWithValue("@optionName", varOption.OptionName)
                        ' Only save image if it exists and images are enabled for this variation
                        If variation.EnableImage AndAlso Not String.IsNullOrEmpty(varOption.ImageBase64) Then
                            optCmd.Parameters.AddWithValue("@optionImage", varOption.ImageBase64)
                        Else
                            optCmd.Parameters.AddWithValue("@optionImage", DBNull.Value)
                        End If
                        optCmd.Parameters.AddWithValue("@dateCreated", DateTime.Now)
                        optCmd.ExecuteNonQuery()
                    End Using
                Next
            Next

            ' Get all variation combinations from the variation manager
            Dim allVariationData = ProductController.variationManager.GetAllVariationData()

            ' Loop through each variation combination and insert its stock data
            For Each kvp In allVariationData
                Dim combinationName As String = kvp.Key
                Dim variationData = kvp.Value

                ' Create insert query for productvariationstock table
                Dim query As String = "INSERT INTO productvariationstock " &
                                     "(productID, warehouseID, optionCombination, sellingPrice, buyingPrice, " &
                                     "defaultTax, taxType, discountRate, discountType, stockUnit, alertQuantity, " &
                                     "dateCreated, dateModified) " &
                                     "VALUES (@productID, @warehouseID, @optionCombination, @sellingPrice, " &
                                     "@buyingPrice, @defaultTax, @taxType, @discountRate, @discountType, " &
                                     "@stockUnit, @alertQuantity, @dateCreated, @dateModified)"

                Using cmd As New MySqlCommand(query, conn, transaction)
                    ' Set parameters from the variation data
                    cmd.Parameters.AddWithValue("@productID", productID)
                    cmd.Parameters.AddWithValue("@warehouseID", variationData.WarehouseId)
                    cmd.Parameters.AddWithValue("@optionCombination", combinationName)
                    cmd.Parameters.AddWithValue("@sellingPrice", variationData.RetailPrice)
                    cmd.Parameters.AddWithValue("@buyingPrice", variationData.PurchaseOrder)
                    cmd.Parameters.AddWithValue("@defaultTax", variationData.DefaultTax)
                    cmd.Parameters.AddWithValue("@taxType", DBNull.Value)  ' Set to NULL or appropriate value
                    cmd.Parameters.AddWithValue("@discountRate", variationData.DiscountRate)
                    cmd.Parameters.AddWithValue("@discountType", DBNull.Value)  ' Set to NULL or appropriate value
                    cmd.Parameters.AddWithValue("@stockUnit", variationData.StockUnits)
                    cmd.Parameters.AddWithValue("@alertQuantity", variationData.AlertQuantity)
                    cmd.Parameters.AddWithValue("@dateCreated", DateTime.Now)
                    cmd.Parameters.AddWithValue("@dateModified", DateTime.Now)

                    ' Execute the query
                    cmd.ExecuteNonQuery()

                    ' If serial numbers are enabled for this variation, insert them into serialnumberproduct table
                    If variationData.IncludeSerialNumbers AndAlso
                       variationData.SerialNumbers IsNot Nothing AndAlso
                       variationData.SerialNumbers.Count > 0 Then

                        ' Insert each serial number for this product
                        For Each serialNumber In variationData.SerialNumbers
                            If Not String.IsNullOrWhiteSpace(serialNumber) Then
                                Dim serialQuery As String = "INSERT INTO serialnumberproduct " &
                                                           "(SerialNumber, ProductID) " &
                                                           "VALUES (@serialNumber, @productID)"

                                Using serialCmd As New MySqlCommand(serialQuery, conn, transaction)
                                    serialCmd.Parameters.AddWithValue("@serialNumber", serialNumber)
                                    serialCmd.Parameters.AddWithValue("@productID", productID)
                                    serialCmd.ExecuteNonQuery()
                                End Using
                            End If
                        Next
                    End If
                End Using
            Next
        End Sub

    End Class
End Namespace
