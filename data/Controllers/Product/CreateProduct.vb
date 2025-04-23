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
            ProductName As TextBox, Category As ComboBox, SubCategory As ComboBox, Warehouse As ComboBox,
            Brand As ComboBox, Supplier As ComboBox,
            RetailPrice As TextBox, PurchaseOrder As TextBox, DefaultTax As TextBox,
            DiscountRate As TextBox, StockUnits As TextBox, AlertQuantity As TextBox,
            MeasurementUnit As ComboBox, Description As TextBox, ValidDate As DatePicker,
            SerialNumbers As List(Of TextBox), ProductImage As String)

            ' Determine if the product is a variation
            Dim variation As Integer = If(Toggle.IsChecked = True, 1, 0)

            ' Validate required fields
            If Not ProductController.ValidateProductFields(Checkbox, ProductName, Category, SubCategory, Warehouse, Brand, Supplier,
                  RetailPrice, PurchaseOrder, DefaultTax, DiscountRate, StockUnits,
                  AlertQuantity, MeasurementUnit, Description, ValidDate, SerialNumbers) Then
                MessageBox.Show("Please fill in all required fields!", "Input Error", MessageBoxButton.OK)
                Exit Sub
            End If

            ' Generate product ID
            Dim productID As String = ProductController.GenerateProductCode()

            ' ✅ Handle SubCategory when it's Nothing
            Dim subCategoryId As Integer = If(SubCategory.SelectedItem IsNot Nothing, CType(SubCategory.SelectedItem, ComboBoxItem).Tag, 0)

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                conn.Open()
                Using transaction = conn.BeginTransaction()
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

                    ' Call the appropriate insertion function based on variation flag
                    If variation = 0 Then
                        ProductController.InsertNonVariationProduct(conn, transaction, productID, Warehouse, RetailPrice, PurchaseOrder, DefaultTax,
                  DiscountRate, StockUnits, AlertQuantity, ValidDate, SerialNumbers, Checkbox)
                    Else
                        ProductController.InsertVariationProduct(conn, transaction, productID)
                    End If

                    transaction.Commit()
                    MessageBox.Show($"Product {ProductName.Text} with Product Code {productID} has been inserted successfully.")
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

    End Class
End Namespace
