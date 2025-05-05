Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Data.Helpers
Imports System.IO
Imports System.Text

Namespace DPC.Data.Controllers
    Public Class LogicProduct

        'Check if product has variation or no variation
        Public Shared Sub VariationChecker(toggle As ToggleButton,
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
                    ProductController.IsVariation = True

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
                    ProductController.IsVariation = False

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

        'Check if product has serial number or no serial number
        Public Shared Sub SerialNumberChecker(checkbox As CheckBox,
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

        Public Shared Function ValidateImageFile(filePath As String) As Boolean
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

        Public Shared Sub ProcessStockUnitsEntry(txtStockUnits As TextBox, mainContainer As Panel)
            Dim stockUnits As Integer

            ' Validate if input is a valid number and greater than zero
            If Integer.TryParse(txtStockUnits.Text, stockUnits) Then
                If stockUnits > 0 Then
                    ' Clear previous rows
                    mainContainer.Children.Clear()

                    ' Clear SerialNumbers to remove old references
                    ProductController.SerialNumbers.Clear()

                    ' Call BtnAddRow_Click the specified number of times
                    For i As Integer = 1 To stockUnits
                        ProductController.BtnAddRow_Click(Nothing, Nothing)
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

        Public Shared Sub UpdateProductVariationText(variations As List(Of ProductVariation), txtProductVariation As TextBlock)
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

        Public Shared Function ValidateProductFields(Checkbox As Controls.CheckBox, ProductName As TextBox, ProductCode As TextBox, Category As ComboBox,
                                       SubCategory As ComboBox, Warehouse As ComboBox, Brand As ComboBox,
                                       Supplier As ComboBox, RetailPrice As TextBox, PurchaseOrder As TextBox,
                                       DefaultTax As TextBox, DiscountRate As TextBox, StockUnits As TextBox,
                                       AlertQuantity As TextBox, MeasurementUnit As ComboBox, Description As TextBox,
                                       ValidDate As DatePicker, SerialNumbers As List(Of TextBox)) As Boolean

            ' Check if any of the required fields are empty (except SubCategory, which can be Nothing)
            If String.IsNullOrWhiteSpace(ProductName.Text) OrElse
               String.IsNullOrWhiteSpace(ProductCode.Text) OrElse
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

        Public Shared Function IsProductCodeExists(productCode As String) As Boolean
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                conn.Open()
                Dim query As String = "SELECT COUNT(*) FROM product WHERE productCode = @productCode"
                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@productCode", productCode.Trim())
                    Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                    Return count > 0
                End Using
            End Using
        End Function
    End Class
End Namespace
