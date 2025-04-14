Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models
Imports System.IO
Imports System.Text

Namespace DPC.Data.Controllers
    Public Class ProductController
        Public Shared SerialNumbers As New List(Of TextBox)
        Public Shared Property MainContainer As StackPanel
        Public Shared Property TxtStockUnits As TextBox
        Public Shared popup As Popup
        Public Shared RecentlyClosed As Boolean = False

        Public _variations As New List(Of ProductVariation)

        ' Static property to store variations data globally
        Public Shared _savedVariations As List(Of ProductVariation) = New List(Of ProductVariation)

        'Get functions
        '(For comboboxes only)
        Public Shared Sub GetBrands(comboBox As ComboBox)
            GetProductData.GetBrands(comboBox)
        End Sub

        Public Shared Sub GetSuppliersByBrand(brandID As Integer, comboBox As ComboBox)
            GetProductData.GetSuppliersByBrand(brandID, comboBox)
        End Sub

        Public Shared Sub GetProductCategory(comboBox As ComboBox)
            GetProductData.GetProductCategory(comboBox)
        End Sub

        Public Shared Sub GetProductSubcategory(categoryName As String, comboBox As ComboBox, label As TextBlock, stackPanel As StackPanel)
            GetProductData.GetProductSubcategory(categoryName, comboBox, label, stackPanel)
        End Sub

        Public Shared Sub GetWarehouse(comboBox As ComboBox)
            GetProductData.GetWarehouse(comboBox)
        End Sub

        Public Shared Function GenerateProductCode() As String
            Return CreateProductData.GenerateProductCode()
        End Function

        Public Shared Function GetNextProductCounter(datePart As String) As Integer
            Return CreateProductData.GetNextProductCounter(datePart)
        End Function



        'Serial Row Functions
        Public Shared Sub BtnAddRow_Click(sender As Object, e As RoutedEventArgs, Optional skipStockUpdate As Boolean = False)
            CreateProductData.AddSerialRow(sender, e, skipStockUpdate)
        End Sub

        ' Remove Row Function
        Public Shared Sub BtnRemoveRow_Click(sender As Object, e As RoutedEventArgs)
            DeleteProductData.RemoveSerialRow(sender, e)
        End Sub

        ' Row Controller Handler
        Public Shared Sub BtnRowController_Click(sender As Object, e As RoutedEventArgs)
            CreateProductData.OpenRowController(sender, e)
        End Sub

        ' Remove Latest Row Function
        Public Shared Sub RemoveLatestRow()
            DeleteProductData.RemoveLatestRow()
        End Sub

        'Import serialnumbers
        Public Shared Sub ImportSerialNumbers_Click()
            CreateProductData.ImportSerialNumbers()
        End Sub



        ' Load Product Data
        Public Shared Sub LoadProductData(dataGrid As DataGrid)
            GetProductData.LoadProductData(dataGrid)
        End Sub



        ' Add these methods to your ProductController class

#Region "UI State Management"
        Public Sub VariationChecker(toggle As ToggleButton,
                          stackPanelVariation As StackPanel,
                          stackPanelWarehouse As StackPanel,
                          stackPanelRetailPrice As StackPanel,
                          stackPanelOrderPrice As StackPanel,
                          stackPanelTaxRate As StackPanel,
                          stackPanelDiscountRate As StackPanel,
                          borderStocks As Border,
                          stackPanelAlertQuantity As StackPanel,
                          stackPanelStockUnits As StackPanel,
                          outerStackPanel As StackPanel)
            Try
                If toggle Is Nothing Then
                    Throw New Exception("ToggleButton is not initialized.")
                End If

                ' Update UI based on IsChecked state
                If toggle.IsChecked = True Then
                    stackPanelVariation.Visibility = Visibility.Visible
                    stackPanelWarehouse.Visibility = Visibility.Collapsed
                    stackPanelRetailPrice.Visibility = Visibility.Collapsed
                    stackPanelOrderPrice.Visibility = Visibility.Collapsed
                    stackPanelTaxRate.Visibility = Visibility.Collapsed
                    stackPanelDiscountRate.Visibility = Visibility.Collapsed
                    borderStocks.Visibility = Visibility.Collapsed
                    stackPanelAlertQuantity.Visibility = Visibility.Collapsed
                    stackPanelStockUnits.Visibility = Visibility.Collapsed
                    outerStackPanel.Visibility = Visibility.Collapsed
                Else
                    stackPanelVariation.Visibility = Visibility.Collapsed
                    stackPanelWarehouse.Visibility = Visibility.Visible
                    stackPanelRetailPrice.Visibility = Visibility.Visible
                    stackPanelOrderPrice.Visibility = Visibility.Visible
                    stackPanelTaxRate.Visibility = Visibility.Visible
                    stackPanelDiscountRate.Visibility = Visibility.Visible
                    borderStocks.Visibility = Visibility.Visible
                    stackPanelAlertQuantity.Visibility = Visibility.Visible
                    stackPanelStockUnits.Visibility = Visibility.Visible
                    outerStackPanel.Visibility = Visibility.Visible
                End If
            Catch ex As Exception
                MessageBox.Show($"Error: {ex.Message}")
            End Try
        End Sub

        Public Sub SerialNumberChecker(checkbox As CheckBox,
                             stackPanelSerialRow As StackPanel,
                             txtStockUnits As TextBox,
                             borderStockUnits As Border)
            If checkbox.IsChecked = True Then
                stackPanelSerialRow.Visibility = Visibility.Visible
                txtStockUnits.IsReadOnly = True
            Else
                stackPanelSerialRow.Visibility = Visibility.Collapsed
                txtStockUnits.IsReadOnly = False
            End If

            If txtStockUnits.IsReadOnly = True Then
                borderStockUnits.BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE"))
            Else
                borderStockUnits.BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#474747"))
            End If
        End Sub

        Public Sub ClearInputFields(txtProductName As TextBox,
                          txtRetailPrice As TextBox,
                          txtPurchaseOrder As TextBox,
                          txtDefaultTax As TextBox,
                          txtDiscountRate As TextBox,
                          txtStockUnits As TextBox,
                          txtAlertQuantity As TextBox,
                          txtDescription As TextBox,
                          comboBoxCategory As ComboBox,
                          comboBoxSubCategory As ComboBox,
                          comboBoxWarehouse As ComboBox,
                          comboBoxMeasurementUnit As ComboBox,
                          comboBoxBrand As ComboBox,
                          comboBoxSupplier As ComboBox,
                          singleDatePicker As DatePicker,
                          mainContainer As Panel)
            ' Clear TextBoxes
            txtProductName.Clear()
            txtRetailPrice.Clear()
            txtPurchaseOrder.Clear()
            txtDefaultTax.Text = "12"
            txtDiscountRate.Clear()
            txtStockUnits.Text = "1"
            txtAlertQuantity.Clear()
            txtDescription.Clear()

            ' Reset ComboBoxes to first item (index 0)
            If comboBoxCategory.Items.Count > 0 Then comboBoxCategory.SelectedIndex = 0
            If comboBoxSubCategory.Items.Count > 0 Then comboBoxSubCategory.SelectedIndex = 0
            If comboBoxWarehouse.Items.Count > 0 Then comboBoxWarehouse.SelectedIndex = 0
            If comboBoxMeasurementUnit.Items.Count > 0 Then comboBoxMeasurementUnit.SelectedIndex = 0
            If comboBoxBrand.Items.Count > 0 Then comboBoxBrand.SelectedIndex = 0
            If comboBoxSupplier.Items.Count > 0 Then comboBoxSupplier.SelectedIndex = 0

            ' Set DatePicker to current date
            singleDatePicker.SelectedDate = DateTime.Now

            ' Clear Serial Numbers and reset to one row
            If SerialNumbers IsNot Nothing Then
                SerialNumbers.Clear()
            End If

            If mainContainer IsNot Nothing Then
                mainContainer.Children.Clear()
            End If

            ' Add back one row for Serial Number input
            BtnAddRow_Click(Nothing, Nothing)
        End Sub
#End Region

#Region "Validation"
        Public Function ValidateImageFile(filePath As String) As Boolean
            Dim fileInfo As New FileInfo(filePath)

            ' Check file size (2MB max)
            If fileInfo.Length > 2 * 1024 * 1024 Then
                MessageBox.Show("File is too large! Please upload an image under 2MB.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End If

            ' Check file extension
            Dim validExtensions As String() = {".jpg", ".jpeg", ".png"}
            If Not validExtensions.Contains(fileInfo.Extension.ToLower()) Then
                MessageBox.Show("Invalid file format! Only JPG and PNG are allowed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End If

            Return True
        End Function

        Public Shared Sub IntegerOnlyTextInputHandler(sender As Object, e As TextCompositionEventArgs)
            If Not IsNumeric(e.Text) OrElse Not Integer.TryParse(e.Text, New Integer()) Then
                e.Handled = True ' Block non-integer input
            End If
        End Sub

        Public Shared Sub IntegerOnlyPasteHandler(sender As Object, e As DataObjectPastingEventArgs)
            If e.DataObject.GetDataPresent(GetType(String)) Then
                Dim pastedText As String = CStr(e.DataObject.GetData(GetType(String)))
                If Not Integer.TryParse(pastedText, New Integer()) Then
                    e.CancelCommand() ' Cancel if pasted data is not an integer
                End If
            Else
                e.CancelCommand()
            End If
        End Sub


        Public Sub ProcessStockUnitsEntry(txtStockUnits As TextBox, mainContainer As Panel)
            Dim stockUnits As Integer

            ' Validate if input is a valid number and greater than zero
            If Integer.TryParse(txtStockUnits.Text, stockUnits) Then
                If stockUnits > 0 Then
                    ' Clear previous rows
                    mainContainer.Children.Clear()

                    ' Clear SerialNumbers to remove old references
                    SerialNumbers.Clear()

                    ' Call BtnAddRow_Click the specified number of times
                    For i As Integer = 1 To stockUnits
                        BtnAddRow_Click(Nothing, Nothing)
                    Next

                    ' Ensure the textbox retains the correct value
                    txtStockUnits.Text = stockUnits.ToString()
                Else
                    MessageBox.Show("Please enter a number greater than zero.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning)
                End If
            Else
                MessageBox.Show("Please enter a valid number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning)
            End If
        End Sub
#End Region

#Region "Variation Management"
        Public Sub UpdateProductVariationText(variations As List(Of ProductVariation), txtProductVariation As TextBlock)
            Try
                If variations IsNot Nothing AndAlso variations.Count > 0 Then
                    ' Build a string to display the variation information
                    Dim variationText As New StringBuilder()

                    For i As Integer = 0 To variations.Count - 1
                        Dim variation As ProductVariation = variations(i)
                        variationText.Append(variation.VariationName)

                        ' Add options summary
                        If variation.Options IsNot Nothing AndAlso variation.Options.Count > 0 Then
                            variationText.Append(" (")

                            ' Limit to showing first 3 options if there are many
                            Dim maxOptions As Integer = Math.Min(3, variation.Options.Count)
                            For j As Integer = 0 To maxOptions - 1
                                variationText.Append(variation.Options(j).OptionName)

                                If j < maxOptions - 1 Then
                                    variationText.Append(", ")
                                End If
                            Next

                            ' If there are more options than we're showing
                            If variation.Options.Count > 3 Then
                                variationText.Append($", +{variation.Options.Count - 3} more")
                            End If

                            variationText.Append(")")
                        End If

                        ' Add separator between variations
                        If i < variations.Count - 1 Then
                            variationText.Append(" | ")
                        End If
                    Next

                    ' Update the TextBlock with the variation information
                    txtProductVariation.Text = variationText.ToString()
                    txtProductVariation.Visibility = Visibility.Visible
                Else
                    ' No variations, clear the text
                    txtProductVariation.Text = "No variations defined"
                    txtProductVariation.Visibility = Visibility.Collapsed
                End If
            Catch ex As Exception
                MessageBox.Show($"Error updating product variation text: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
#End Region


        Private Shared Function ValidateProductFields(Checkbox As Controls.CheckBox, ProductName As TextBox, Category As ComboBox,
                                              SubCategory As ComboBox, Warehouse As ComboBox, Brand As ComboBox,
                                              Supplier As ComboBox, RetailPrice As TextBox, PurchaseOrder As TextBox,
                                              DefaultTax As TextBox, DiscountRate As TextBox, StockUnits As TextBox,
                                              AlertQuantity As TextBox, MeasurementUnit As ComboBox, Description As TextBox,
                                              ValidDate As DatePicker, SerialNumbers As List(Of TextBox)) As Boolean

            ' Check if any of the required fields are empty (except SubCategory, which can be Nothing)
            If String.IsNullOrWhiteSpace(ProductName.Text) OrElse
       Category.SelectedItem Is Nothing OrElse
       Warehouse.SelectedItem Is Nothing OrElse
       Brand.SelectedItem Is Nothing OrElse
       Supplier.SelectedItem Is Nothing OrElse
       String.IsNullOrWhiteSpace(RetailPrice.Text) OrElse
       String.IsNullOrWhiteSpace(PurchaseOrder.Text) OrElse
       String.IsNullOrWhiteSpace(DefaultTax.Text) OrElse
       String.IsNullOrWhiteSpace(DiscountRate.Text) OrElse
       String.IsNullOrWhiteSpace(StockUnits.Text) OrElse
       String.IsNullOrWhiteSpace(AlertQuantity.Text) OrElse
       MeasurementUnit.SelectedItem Is Nothing OrElse
       String.IsNullOrWhiteSpace(Description.Text) OrElse
       ValidDate.SelectedDate Is Nothing OrElse
       (Checkbox.IsChecked = True AndAlso SerialNumbers.Any(Function(txt) String.IsNullOrWhiteSpace(txt.Text))) Then
                Return False
            End If

            ' ✅ If SubCategory is Nothing, set it to 0 when saving later
            Return True
        End Function


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
            If Not ValidateProductFields(Checkbox, ProductName, Category, SubCategory, Warehouse, Brand, Supplier,
                  RetailPrice, PurchaseOrder, DefaultTax, DiscountRate, StockUnits,
                  AlertQuantity, MeasurementUnit, Description, ValidDate, SerialNumbers) Then
                MessageBox.Show("Please fill in all required fields!", "Input Error", MessageBoxButton.OK)
                Exit Sub
            End If

            ' Generate product ID
            Dim productID As String = GenerateProductCode()

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
                        InsertNonVariationProduct(conn, transaction, productID, Warehouse, RetailPrice, PurchaseOrder, DefaultTax,
                  DiscountRate, StockUnits, AlertQuantity, ValidDate, SerialNumbers, Checkbox)
                    Else
                        InsertVariationProduct(conn, transaction, productID)
                    End If

                    transaction.Commit()
                    MessageBox.Show($"Product {ProductName.Text} with Product Code {productID} has been inserted successfully.")
                End Using
            End Using
        End Sub

        'no variation insert
        Private Shared Sub InsertNonVariationProduct(conn As MySqlConnection, transaction As MySqlTransaction, productID As String,
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
                InsertSerialNumbersForProduct(conn, transaction, SerialNumbers, productID)
            End If
        End Sub

        'variation insert
        Private Shared Sub InsertVariationProduct(conn As MySqlConnection, transaction As MySqlTransaction, productID As String)

            ' Insert into productvariation table
            Dim query As String = "INSERT INTO productvariation (productID, dateCreated) 
                           VALUES (@productID, @DateCreated);"
            Using cmd As New MySqlCommand(query, conn, transaction)
                cmd.Parameters.AddWithValue("@productID", productID)
                cmd.Parameters.AddWithValue("@DateCreated", DateTime.Now)
                cmd.ExecuteNonQuery()
            End Using
        End Sub

        Private Shared Sub InsertSerialNumbersForProduct(conn As MySqlConnection, transaction As MySqlTransaction,
                                          SerialNumbers As List(Of TextBox), productID As String)
            Dim query As String = "INSERT INTO serialnumberproduct (SerialNumber, ProductID) VALUES (@SerialNumber, @ProductID)"
            Using cmd As New MySqlCommand(query, conn, transaction)
                cmd.Parameters.AddWithValue("@ProductID", productID)

                For Each serialNumberTextBox As TextBox In SerialNumbers
                    If Not String.IsNullOrWhiteSpace(serialNumberTextBox.Text) Then
                        cmd.Parameters.Clear()
                        cmd.Parameters.AddWithValue("@SerialNumber", serialNumberTextBox.Text)
                        cmd.Parameters.AddWithValue("@ProductID", productID)
                        cmd.ExecuteNonQuery()
                    End If
                Next
            End Using
        End Sub

        Public Function GetProductVariations() As List(Of ProductVariation)
            Return DPC.Components.Forms.AddVariation.SavedVariations
        End Function

        ' New methods migrated from ProductVariationDetails

#Region "Variation UI Management"
        ''' <summary>
        ''' Loads variation combinations into a UI panel
        ''' </summary>
        ''' <param name="variationsPanel">The StackPanel that will contain the variation buttons</param>
        ''' <param name="buttonClickHandler">Event handler for when variation buttons are clicked</param>
        Public Shared Sub LoadVariationCombinations(variationsPanel As StackPanel, buttonClickHandler As RoutedEventHandler)
            ' Clear existing buttons in the variations panel
            If variationsPanel IsNot Nothing Then
                variationsPanel.Children.Clear()
            End If

            ' Get variations from the saved variations collection
            Dim variations As List(Of ProductVariation) = DPC.Components.Forms.AddVariation.SavedVariations

            ' Check if we have any variations
            If variations Is Nothing OrElse variations.Count = 0 Then
                ' No variations, add a default placeholder option
                AddVariationButton(variationsPanel, "Default Variation", True, buttonClickHandler)
                Return
            End If

            ' Check if we have one or two variations
            If variations.Count = 1 Then
                ' Single variation case - just show options
                Dim variation As ProductVariation = variations(0)
                If variation.Options IsNot Nothing AndAlso variation.Options.Count > 0 Then
                    Dim isFirst As Boolean = True
                    For Each opt As VariationOption In variation.Options
                        AddVariationButton(variationsPanel, opt.OptionName, isFirst, buttonClickHandler)
                        isFirst = False
                    Next
                End If
            ElseIf variations.Count = 2 Then
                ' Two variations case - create combinations
                Dim variation1 As ProductVariation = variations(0)
                Dim variation2 As ProductVariation = variations(1)

                If variation1.Options IsNot Nothing AndAlso variation1.Options.Count > 0 AndAlso
                   variation2.Options IsNot Nothing AndAlso variation2.Options.Count > 0 Then

                    Dim isFirst As Boolean = True
                    For Each option1 As VariationOption In variation1.Options
                        For Each option2 As VariationOption In variation2.Options
                            ' Create combination label: "Color, Size"
                            Dim combinationName As String = $"{option1.OptionName}, {option2.OptionName}"
                            AddVariationButton(variationsPanel, combinationName, isFirst, buttonClickHandler)
                            isFirst = False
                        Next
                    Next
                End If
            End If
        End Sub

        ''' <summary>
        ''' Creates a variation button with consistent styling
        ''' </summary>
        ''' <param name="container">The container to add the button to</param>
        ''' <param name="labelText">The text to display on the button</param>
        ''' <param name="isSelected">Whether this button is selected by default</param>
        ''' <param name="clickHandler">The event handler for button clicks</param>
        Public Shared Sub AddVariationButton(container As StackPanel, labelText As String, isSelected As Boolean, clickHandler As RoutedEventHandler)
            Dim btn As New System.Windows.Controls.Button With {
                .Width = Double.NaN,  ' Auto width
                .Height = Double.NaN, ' Auto height
                .Background = Brushes.Transparent,
                .HorizontalAlignment = HorizontalAlignment.Left,
                .BorderThickness = New Thickness(0),
                .VerticalAlignment = VerticalAlignment.Center,
                .Margin = New Thickness(0, 0, 15, 0),
                .Tag = labelText ' Store the combination name in the Tag property
            }

            ' Try to apply the style if it exists, otherwise use default styling
            Try
                btn.Style = Application.Current.FindResource("RoundedButtonStyle")
            Catch ex As Exception
                ' Style not found, use default styling
            End Try

            ' Create the Grid layout for the button content
            Dim grid As New Grid()
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

            ' Add vertical line if selected
            Dim border As New Border With {
                .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#555555")),
                .BorderThickness = New Thickness(1, 0, 0, 0),
                .Width = 1,
                .Height = Double.NaN,  ' Auto height
                .Margin = New Thickness(0),
                .Visibility = If(isSelected, Visibility.Visible, Visibility.Collapsed)
            }
            Grid.SetColumn(border, 0)
            grid.Children.Add(border)

            ' Add the text
            Dim textBlock As New TextBlock With {
                .Text = labelText,
                .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString(If(isSelected, "#555555", "#AEAEAE"))),
                .FontSize = 14,
                .FontWeight = FontWeights.SemiBold,
                .Margin = New Thickness(5, 0, 0, 0),
                .VerticalAlignment = VerticalAlignment.Center
            }
            Grid.SetColumn(textBlock, 1)
            grid.Children.Add(textBlock)

            ' Set the grid as button content
            btn.Content = grid

            ' Add click handler
            If clickHandler IsNot Nothing Then
                AddHandler btn.Click, clickHandler
            End If

            ' Add to container
            container.Children.Add(btn)
        End Sub

        ''' <summary>
        ''' Updates the UI to show the selected variation
        ''' </summary>
        ''' <param name="container">The container with buttons</param>
        ''' <param name="selectedButton">The button that was clicked</param>
        Public Shared Sub UpdateVariationSelection(container As StackPanel, selectedButton As System.Windows.Controls.Button)
            ' Update UI to show this variation is selected
            For Each child As UIElement In container.Children
                If TypeOf child Is System.Windows.Controls.Button Then
                    Dim childBtn As System.Windows.Controls.Button = DirectCast(child, System.Windows.Controls.Button)
                    Dim childGrid As Grid = TryCast(childBtn.Content, Grid)
                    If childGrid IsNot Nothing Then
                        ' Update the border visibility and text color for all buttons
                        For Each gridChild As UIElement In childGrid.Children
                            If TypeOf gridChild Is Border Then
                                DirectCast(gridChild, Border).Visibility = If(child Is selectedButton, Visibility.Visible, Visibility.Collapsed)
                            ElseIf TypeOf gridChild Is TextBlock Then
                                DirectCast(gridChild, TextBlock).Foreground = New SolidColorBrush(
                                ColorConverter.ConvertFromString(If(child Is selectedButton, "#555555", "#AEAEAE")))
                            End If
                        Next
                    End If
                End If
            Next
        End Sub
#End Region

#Region "Serial Number Management"
        ''' <summary>
        ''' Updates the UI when the serial number checkbox changes
        ''' </summary>
        ''' <param name="checkbox">The serial number checkbox</param>
        ''' <param name="serialRowPanel">The panel containing serial number rows</param>
        ''' <param name="txtStockUnits">The stock units text box</param>
        ''' <param name="borderStockUnits">The border around the stock units text box</param>
        ''' <param name="mainContainer">The container for serial number rows</param>
        Public Shared Sub SerialNumberChecker(checkbox As CheckBox,
                                     serialRowPanel As Panel,
                                     txtStockUnits As TextBox,
                                     borderStockUnits As Border,
                                     mainContainer As Panel)
            If checkbox.IsChecked = True Then
                serialRowPanel.Visibility = Visibility.Visible
                txtStockUnits.IsReadOnly = True

                ' Ensure we have the correct number of serial rows based on stock units
                If mainContainer.Children.Count = 0 Then
                    Dim stockUnits As Integer = 1 ' Default

                    If Not String.IsNullOrEmpty(txtStockUnits.Text) AndAlso Integer.TryParse(txtStockUnits.Text, stockUnits) Then
                        ' We have a valid number
                        If mainContainer.Children.Count <> stockUnits Then
                            mainContainer.Children.Clear()
                            If SerialNumbers IsNot Nothing Then
                                SerialNumbers.Clear()
                            End If

                            ' Create the correct number of rows
                            For i As Integer = 1 To stockUnits
                                BtnAddRow_Click(Nothing, Nothing)
                            Next
                        End If
                    Else
                        ' Default to 1 row if no valid stock units
                        txtStockUnits.Text = "1"
                        If mainContainer.Children.Count <> 1 Then
                            mainContainer.Children.Clear()
                            If SerialNumbers IsNot Nothing Then
                                SerialNumbers.Clear()
                            End If
                            BtnAddRow_Click(Nothing, Nothing)
                        End If
                    End If
                End If
            Else
                serialRowPanel.Visibility = Visibility.Collapsed
                txtStockUnits.IsReadOnly = False
            End If

            ' Update border color based on readonly status
            If txtStockUnits.IsReadOnly = True Then
                borderStockUnits.BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE"))
            Else
                borderStockUnits.BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#474747"))
            End If
        End Sub

        ''' <summary>
        ''' Handles stock units text input and updates serial number rows
        ''' </summary>
        ''' <param name="txtStockUnits">The stock units textbox</param>
        ''' <param name="mainContainer">The container for serial number rows</param>
        ''' <param name="e">The key event args</param>
        ''' <returns>True if the event was handled, False otherwise</returns>
        Public Shared Function HandleStockUnitsKeyDown(txtStockUnits As TextBox, mainContainer As Panel, e As KeyEventArgs) As Boolean
            ' Check if Enter key is pressed
            If e.Key = Key.Enter Then
                Dim stockUnits As Integer

                ' Validate if input is a valid number and greater than zero
                If Integer.TryParse(txtStockUnits.Text, stockUnits) Then
                    If stockUnits > 0 Then
                        ' Clear previous rows
                        If mainContainer IsNot Nothing Then
                            mainContainer.Children.Clear()
                        End If

                        ' Clear SerialNumbers to remove old references
                        If SerialNumbers IsNot Nothing Then
                            SerialNumbers.Clear()
                        End If

                        ' Call BtnAddRow_Click the specified number of times
                        For i As Integer = 1 To stockUnits
                            BtnAddRow_Click(Nothing, Nothing)
                        Next

                        ' Ensure the textbox retains the correct value
                        txtStockUnits.Text = stockUnits.ToString()
                        Return True
                    Else
                        MessageBox.Show("Please enter a number greater than zero.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning)
                        Return True
                    End If
                Else
                    MessageBox.Show("Please enter a valid number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return True
                End If
            End If

            Return False
        End Function
#End Region

#Region "Helper Methods"
        ''' <summary>
        ''' Finds a visual child element by name within a parent control
        ''' </summary>
        ''' <typeparam name="T">Type of the child element</typeparam>
        ''' <param name="parent">Parent element to search in</param>
        ''' <param name="name">Name of the child element to find</param>
        ''' <returns>The found element or Nothing</returns>
        Public Shared Function FindVisualChild(Of T As DependencyObject)(parent As DependencyObject, name As String) As T
            For i As Integer = 0 To VisualTreeHelper.GetChildrenCount(parent) - 1
                Dim child As DependencyObject = VisualTreeHelper.GetChild(parent, i)

                ' Check if this is the element we're looking for
                If TypeOf child Is T AndAlso (TryCast(child, FrameworkElement)).Name = name Then
                    Return DirectCast(child, T)
                End If

                ' Search in this child's children
                Dim result As T = FindVisualChild(Of T)(child, name)
                If result IsNot Nothing Then
                    Return result
                End If
            Next

            Return Nothing
        End Function
#End Region

        ' In ProductController class

        ' Class-level properties to store forms data
        Public Shared VariationManager As New ProductVariationManager()

        ' Migrated function for saving form data
        Public Shared Sub SaveVariationData(combinationName As String,
                                           txtRetailPrice As TextBox,
                                           txtPurchaseOrder As TextBox,
                                           txtDefaultTax As TextBox,
                                           txtDiscountRate As TextBox,
                                           txtStockUnits As TextBox,
                                           txtAlertQuantity As TextBox,
                                           checkBoxSerialNumber As CheckBox,
                                           comboBoxWarehouse As ComboBox,
                                           mainContainer As StackPanel)

            If String.IsNullOrEmpty(combinationName) Then
                Return
            End If

            ' Get the current variation data
            Dim variationData As ProductVariationData = VariationManager.GetVariationData(combinationName)
            If variationData Is Nothing Then
                Return
            End If

            ' Save form values to the model (with improved error handling)
            Try
                If txtRetailPrice IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtRetailPrice.Text) Then
                    variationData.RetailPrice = Decimal.Parse(txtRetailPrice.Text)
                End If

                If txtPurchaseOrder IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtPurchaseOrder.Text) Then
                    variationData.PurchaseOrder = Decimal.Parse(txtPurchaseOrder.Text)
                End If

                If txtDefaultTax IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtDefaultTax.Text) Then
                    variationData.DefaultTax = Decimal.Parse(txtDefaultTax.Text)
                End If

                If txtDiscountRate IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtDiscountRate.Text) Then
                    variationData.DiscountRate = Decimal.Parse(txtDiscountRate.Text)
                End If

                If txtStockUnits IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtStockUnits.Text) Then
                    variationData.StockUnits = Integer.Parse(txtStockUnits.Text)
                End If

                If txtAlertQuantity IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtAlertQuantity.Text) Then
                    variationData.AlertQuantity = Integer.Parse(txtAlertQuantity.Text)
                End If

                If comboBoxWarehouse IsNot Nothing AndAlso comboBoxWarehouse.SelectedItem IsNot Nothing Then
                    Dim selectedItem As ComboBoxItem = DirectCast(comboBoxWarehouse.SelectedItem, ComboBoxItem)
                    If selectedItem IsNot Nothing AndAlso selectedItem.Tag IsNot Nothing Then
                        variationData.WarehouseId = Convert.ToInt32(selectedItem.Tag)
                    End If
                End If

                ' Save serial number settings
                If checkBoxSerialNumber IsNot Nothing Then
                    ' Save the checkbox state
                    variationData.IncludeSerialNumbers = checkBoxSerialNumber.IsChecked.Value

                    ' Clear previous serial numbers before adding current ones
                    variationData.SerialNumbers.Clear()

                    ' If serial numbers are enabled, collect all serial numbers
                    If variationData.IncludeSerialNumbers AndAlso mainContainer IsNot Nothing Then
                        ' Iterate through all rows in MainContainer
                        For Each child As UIElement In mainContainer.Children
                            If TypeOf child Is Grid Then
                                Dim grid As Grid = DirectCast(child, Grid)
                                ' Look for TextBox in each row's grid
                                For Each gridChild As UIElement In grid.Children
                                    If TypeOf gridChild Is TextBox Then
                                        Dim serialTextBox As TextBox = DirectCast(gridChild, TextBox)
                                        ' Add the serial number to our collection (even if empty - we'll filter on load)
                                        variationData.SerialNumbers.Add(serialTextBox.Text)
                                        Exit For ' We only want one TextBox per row
                                    End If
                                Next
                            End If
                        Next
                    End If
                End If
            Catch ex As Exception
                ' Handle parsing errors gracefully
                MessageBox.Show("An error occurred while saving form data: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' Migrated function for loading form data
        Public Shared Sub LoadVariationData(combinationName As String,
                                          txtRetailPrice As TextBox,
                                          txtPurchaseOrder As TextBox,
                                          txtDefaultTax As TextBox,
                                          txtDiscountRate As TextBox,
                                          txtStockUnits As TextBox,
                                          txtAlertQuantity As TextBox,
                                          checkBoxSerialNumber As CheckBox,
                                          comboBoxWarehouse As ComboBox,
                                          mainContainer As StackPanel)

            ' Get the variation data
            Dim variationData As ProductVariationData = VariationManager.GetVariationData(combinationName)
            If variationData Is Nothing Then
                Return
            End If

            ' Load data into form controls
            If txtRetailPrice IsNot Nothing Then
                txtRetailPrice.Text = If(variationData.RetailPrice = 0, "", variationData.RetailPrice.ToString())
            End If

            If txtPurchaseOrder IsNot Nothing Then
                txtPurchaseOrder.Text = If(variationData.PurchaseOrder = 0, "", variationData.PurchaseOrder.ToString())
            End If

            If txtDefaultTax IsNot Nothing Then
                txtDefaultTax.Text = If(variationData.DefaultTax = 0, "12", variationData.DefaultTax.ToString())
            End If

            If txtDiscountRate IsNot Nothing Then
                txtDiscountRate.Text = If(variationData.DiscountRate = 0, "", variationData.DiscountRate.ToString())
            End If

            If txtStockUnits IsNot Nothing Then
                txtStockUnits.Text = If(variationData.StockUnits = 0, "1", variationData.StockUnits.ToString())
            End If

            If txtAlertQuantity IsNot Nothing Then
                txtAlertQuantity.Text = If(variationData.AlertQuantity = 0, "", variationData.AlertQuantity.ToString())
            End If

            ' Set warehouse selection
            If comboBoxWarehouse IsNot Nothing AndAlso variationData.WarehouseId > 0 Then
                For i As Integer = 0 To comboBoxWarehouse.Items.Count - 1
                    Dim item As ComboBoxItem = DirectCast(comboBoxWarehouse.Items(i), ComboBoxItem)
                    If item IsNot Nothing AndAlso item.Tag IsNot Nothing AndAlso CInt(item.Tag) = variationData.WarehouseId Then
                        comboBoxWarehouse.SelectedIndex = i
                        Exit For
                    End If
                Next
            End If

            ' Set serial number checkbox state first
            If checkBoxSerialNumber IsNot Nothing Then
                checkBoxSerialNumber.IsChecked = variationData.IncludeSerialNumbers
            End If

            ' Clear existing serial numbers
            If SerialNumbers IsNot Nothing Then
                SerialNumbers.Clear()
            End If

            ' Find MainContainer to manage serial number rows
            If mainContainer IsNot Nothing Then
                ' Clear existing rows
                mainContainer.Children.Clear()

                ' Handle serial numbers based on saved state
                If variationData.IncludeSerialNumbers Then
                    ' Create a row for each saved serial number
                    If variationData.SerialNumbers.Count > 0 Then
                        For Each serialNumber As String In variationData.SerialNumbers
                            AddSerialRow(mainContainer, serialNumber)
                        Next
                    Else
                        ' Add a default row if no serial numbers saved but feature is enabled
                        BtnAddRow_Click(Nothing, Nothing)
                    End If
                Else
                    ' Just add one default row if serial numbers are not being used
                    BtnAddRow_Click(Nothing, Nothing)
                End If

                ' Update stock units to match the number of serial rows if serial numbers are enabled
                If variationData.IncludeSerialNumbers AndAlso txtStockUnits IsNot Nothing Then
                    txtStockUnits.Text = mainContainer.Children.Count.ToString()
                End If
            End If
        End Sub

        ' Add a helper function for adding single serial row with value
        Public Shared Sub AddSerialRow(mainContainer As StackPanel, Optional initialValue As String = "")
            ' Create a new row
            Dim grid As New Grid With {.Margin = New Thickness(0)}

            ' Create TextBox for the serial number
            Dim txtSerial As New TextBox With {
                .Text = initialValue,
                .Style = TryCast(Application.Current.FindResource("RoundedTextboxStyle"), Style),
                .Margin = New Thickness(10, 5, 10, 5),
                .BorderThickness = New Thickness(1),
                .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE"))
            }
            Grid.SetColumn(txtSerial, 0)

            ' Create remove button
            Dim btnRemove As New Button With {
                .Content = "Remove",
                .Style = TryCast(Application.Current.FindResource("RoundedButtonStyle"), Style),
                .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#d23636")),
                .Foreground = Brushes.White,
                .Margin = New Thickness(5),
                .Padding = New Thickness(10, 5, 10, 5),
                .BorderThickness = New Thickness(0),
                .HorizontalAlignment = HorizontalAlignment.Right
            }
            Grid.SetColumn(btnRemove, 1)
            AddHandler btnRemove.Click, AddressOf BtnRemoveRow_Click

            grid.Children.Add(txtSerial)
            grid.Children.Add(btnRemove)

            ' Add to container and track the textbox
            mainContainer.Children.Add(grid)
            SerialNumbers.Add(txtSerial)
        End Sub

        ' Form validation logic
        Public Shared Function ValidateForm(txtRetailPrice As TextBox,
                                             txtPurchaseOrder As TextBox,
                                             comboBoxWarehouse As ComboBox,
                                             checkBoxSerialNumber As CheckBox) As Boolean

            ' Check for required fields
            If txtRetailPrice Is Nothing OrElse String.IsNullOrWhiteSpace(txtRetailPrice.Text) Then
                MessageBox.Show("Please enter a selling price.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                If txtRetailPrice IsNot Nothing Then
                    txtRetailPrice.Focus()
                End If
                Return False
            End If

            If txtPurchaseOrder Is Nothing OrElse String.IsNullOrWhiteSpace(txtPurchaseOrder.Text) Then
                MessageBox.Show("Please enter a buying price.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                If txtPurchaseOrder IsNot Nothing Then
                    txtPurchaseOrder.Focus()
                End If
                Return False
            End If

            If comboBoxWarehouse Is Nothing OrElse comboBoxWarehouse.SelectedIndex < 0 Then
                MessageBox.Show("Please select a warehouse.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                If comboBoxWarehouse IsNot Nothing Then
                    comboBoxWarehouse.Focus()
                End If
                Return False
            End If

            ' Add validation for serial numbers if included
            If checkBoxSerialNumber IsNot Nothing AndAlso checkBoxSerialNumber.IsChecked.Value Then
                ' Check if any serial numbers are added
                If SerialNumbers Is Nothing OrElse SerialNumbers.Count = 0 Then
                    MessageBox.Show("Please add at least one serial number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return False
                End If

                ' Check if all serial numbers have values
                For Each serialNumber As TextBox In SerialNumbers
                    If String.IsNullOrWhiteSpace(serialNumber.Text) Then
                        MessageBox.Show("All serial numbers must have a value.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                        serialNumber.Focus()
                        Return False
                    End If
                Next
            End If

            Return True
        End Function

    End Class
End Namespace
