Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Components.Dynamic ' Added for DynamicDialogs

Namespace DPC.Views.Stocks.StocksTransfer

    Public Class StocksTransfer
        Inherits UserControl

        Private _currentProductVariation As Integer = 0
        Private _currentOptionCombination As String = ""

        Public Sub New()
            InitializeComponent()

            ' Select the product to transfer
            StocksTransferController.GetProducts(ComboBoxProduct)

            ' Add an event handler to the product ComboBox selection change to update available warehouses
            AddHandler ComboBoxProduct.SelectionChanged, AddressOf ComboBoxProduct_SelectionChanged

            ' Add an event handler to the warehouse ComboBox selection change to update the max product quantity
            AddHandler ComboBoxTransferFrom.SelectionChanged, AddressOf ComboBoxTransferFrom_SelectionChanged

            ' Call to populate warehouse list on initial load if a product is already selected
            If ComboBoxProduct.Items.Count > 0 Then
                ComboBoxProduct.SelectedIndex = 0
                ComboBoxProduct_SelectionChanged(Nothing, Nothing)
            End If
        End Sub

        ' Event handler for when the selected product changes
        Private Sub ComboBoxProduct_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            ' Reset variation controls
            StackPanelVariations.Visibility = Visibility.Collapsed
            ComboBoxVariation.Items.Clear()
            ComboBoxOption.Items.Clear()
            _currentOptionCombination = ""

            ' Get the selected product's productID
            Dim selectedProduct As ComboBoxItem = CType(ComboBoxProduct.SelectedItem, ComboBoxItem)
            If selectedProduct IsNot Nothing Then
                Dim productID As String = CType(selectedProduct.Tag, String)

                ' Check if this product has variations
                _currentProductVariation = StocksTransferController.CheckProductVariation(productID)

                ' Show variation controls if the product has variations
                If _currentProductVariation = 1 Then
                    StackPanelVariations.Visibility = Visibility.Visible
                    StocksTransferController.GetProductVariations(productID, ComboBoxVariation)

                    ' If variations were loaded, trigger the variation selection change
                    If ComboBoxVariation.Items.Count > 0 Then
                        ComboBoxVariation.SelectedIndex = 0
                        ComboBoxVariation_SelectionChanged(Nothing, Nothing)
                    End If
                End If

                ' Call GetAvailableWarehouses to populate the warehouses ComboBox
                StocksTransferController.GetAvailableWarehouses(productID, ComboBoxTransferFrom, _currentProductVariation, _currentOptionCombination)

                ' After selecting the product, trigger the warehouse selection change logic
                If ComboBoxTransferFrom.Items.Count > 0 Then
                    ComboBoxTransferFrom.SelectedIndex = 0
                    ComboBoxTransferFrom_SelectionChanged(Nothing, Nothing)
                End If
            End If
        End Sub

        ' Event handler for when the selected variation changes
        Private Sub ComboBoxVariation_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            ComboBoxOption.Items.Clear()
            _currentOptionCombination = ""

            Dim selectedVariation As ComboBoxItem = CType(ComboBoxVariation.SelectedItem, ComboBoxItem)
            If selectedVariation IsNot Nothing Then
                Dim variationID As String = CType(selectedVariation.Tag, String)

                ' Get options for the selected variation
                StocksTransferController.GetVariationOptions(variationID, ComboBoxOption)

                ' If options were loaded, trigger the option selection change
                If ComboBoxOption.Items.Count > 0 Then
                    ComboBoxOption.SelectedIndex = 0
                    ComboBoxOption_SelectionChanged(Nothing, Nothing)
                End If
            End If
        End Sub

        ' Event handler for when the selected option changes
        Private Sub ComboBoxOption_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim selectedOption As ComboBoxItem = CType(ComboBoxOption.SelectedItem, ComboBoxItem)
            If selectedOption IsNot Nothing Then
                ' Store the option name as the current option combination
                _currentOptionCombination = selectedOption.Content.ToString()

                ' Update available warehouses for this product and option
                Dim selectedProduct As ComboBoxItem = CType(ComboBoxProduct.SelectedItem, ComboBoxItem)
                If selectedProduct IsNot Nothing Then
                    Dim productID As String = CType(selectedProduct.Tag, String)
                    StocksTransferController.GetAvailableWarehouses(productID, ComboBoxTransferFrom, _currentProductVariation, _currentOptionCombination)

                    ' After selecting the option, update the warehouse list and trigger selection
                    If ComboBoxTransferFrom.Items.Count > 0 Then
                        ComboBoxTransferFrom.SelectedIndex = 0
                        ComboBoxTransferFrom_SelectionChanged(Nothing, Nothing)
                    End If
                End If
            End If
        End Sub

        ' Event handler for when the selected warehouse changes
        Private Sub ComboBoxTransferFrom_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If ComboBoxTransferFrom.SelectedItem IsNot Nothing Then
                ' Get the selected warehouse's warehouseID
                Dim selectedWarehouse As ComboBoxItem = CType(ComboBoxTransferFrom.SelectedItem, ComboBoxItem)
                Dim warehouseID As String = CType(selectedWarehouse.Tag, String)

                ' Get the selected product's productID
                Dim selectedProduct As ComboBoxItem = CType(ComboBoxProduct.SelectedItem, ComboBoxItem)
                If selectedProduct IsNot Nothing Then
                    Dim productID As String = CType(selectedProduct.Tag, String)

                    ' Reset TxtProductQty to 0 when warehouse changes
                    TxtProductQty.Text = "0"

                    ' Set the maximum allowed quantity for the selected product and warehouse
                    StocksTransferController.SetMaxProductQty(productID, warehouseID, TxtProductQty, maxUnits, _currentProductVariation, _currentOptionCombination)

                    ' Add the PreviewTextInput handler to prevent invalid inputs
                    AddHandler TxtProductQty.PreviewTextInput, AddressOf TxtProductQty_PreviewTextInput

                    ' Call function to populate ComboBoxTransferTo with warehouses except the selected one
                    StocksTransferController.GetAvailableTransferToWarehouses(warehouseID, ComboBoxTransferTo)
                End If
            End If
        End Sub

        Private Sub TxtProductQty_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            ' Ensure the entered text is a valid number
            Dim newText As String = CType(sender, TextBox).Text & e.Text

            ' Get the max stock value (this should be set previously)
            Dim maxStock As Integer = If(TxtProductQty.Tag IsNot Nothing, CType(TxtProductQty.Tag, Integer), 0)

            ' Check if the new number is greater than maxStock
            If Integer.TryParse(newText, New Integer()) Then
                If Convert.ToInt32(newText) > maxStock Then
                    e.Handled = True ' Prevent entering more than maxStock

                    ' Add visual feedback using DynamicDialogs when trying to exceed max quantity
                    If Not IsDialogActive() Then
                        DynamicDialogs.ShowWarning(Me, "Cannot exceed maximum available quantity of " & maxStock & ".", "Quantity Limit")
                    End If
                End If
            End If
        End Sub

        ' Helper method to check if a dialog is currently showing to prevent multiple dialogs
        Private Function IsDialogActive() As Boolean
            Return DynamicDialogs.ActiveInstance IsNot Nothing
        End Function

        'Transfer btn
        Private Sub BtnTransfer(sender As Object, e As RoutedEventArgs)
            Try
                ' Retrieve selected product and warehouse data
                Dim selectedProduct As ComboBoxItem = CType(ComboBoxProduct.SelectedItem, ComboBoxItem)
                Dim productID As String = CType(selectedProduct.Tag, String)

                Dim selectedTransferFromWarehouse As ComboBoxItem = CType(ComboBoxTransferFrom.SelectedItem, ComboBoxItem)
                Dim warehouseIDFrom As String = CType(selectedTransferFromWarehouse.Tag, String)

                Dim selectedTransferToWarehouse As ComboBoxItem = CType(ComboBoxTransferTo.SelectedItem, ComboBoxItem)
                Dim warehouseIDTo As String = CType(selectedTransferToWarehouse.Tag, String)

                ' Get the transfer quantity from the TxtProductQty TextBox
                Dim transferQty As Integer
                If Integer.TryParse(TxtProductQty.Text, transferQty) AndAlso transferQty > 0 Then
                    ' Create transfer data to pass to the dialog
                    Dim transferData = New With {
                        productID,
                        .SourceWarehouse = selectedTransferFromWarehouse.Content.ToString(),
                        .DestinationWarehouse = selectedTransferToWarehouse.Content.ToString(),
                        .Quantity = transferQty
                    }

                    ' Show confirmation dialog before transferring
                    Dim confirmDialog = DynamicDialogs.ShowConfirmation(
                        Me,
                        String.Format("Transfer {0} units from {1} to {2}?",
                            transferQty,
                            selectedTransferFromWarehouse.Content.ToString(),
                            selectedTransferToWarehouse.Content.ToString()),
                        "Confirm Transfer",
                        "Transfer",
                        "Cancel",
                        data:=transferData)

                    ' Add handler for confirmation
                    AddHandler confirmDialog.PrimaryAction, AddressOf OnTransferConfirmed
                Else
                    ' Replaced MessageBox with DynamicDialogs for invalid quantity
                    DynamicDialogs.ShowWarning(Me, "Please enter a valid quantity to transfer.", "Invalid Quantity")
                End If
            Catch ex As Exception
                ' Show error dialog for any exception during transfer setup
                DynamicDialogs.ShowError(Me, "Error preparing transfer: " & ex.Message, "Transfer Error")
            End Try
        End Sub

        ' Event handler for transfer confirmation
        Private Sub OnTransferConfirmed(sender As Object, e As DynamicDialogs.DialogEventArgs)
            Try
                ' Get the data passed from the confirmation dialog
                Dim data = DirectCast(e.Data, Object)

                ' Extract transfer details from the dialog data
                Dim productID = data.ProductID
                Dim transferQty = data.Quantity

                ' Get the warehouse IDs from the currently selected items
                Dim selectedTransferFromWarehouse As ComboBoxItem = CType(ComboBoxTransferFrom.SelectedItem, ComboBoxItem)
                Dim warehouseIDFrom As String = CType(selectedTransferFromWarehouse.Tag, String)

                Dim selectedTransferToWarehouse As ComboBoxItem = CType(ComboBoxTransferTo.SelectedItem, ComboBoxItem)
                Dim warehouseIDTo As String = CType(selectedTransferToWarehouse.Tag, String)

                ' Call the controller to handle the transfer
                ' Instead of assigning the result, simply call the method
                StocksTransferController.TransferStock(
            productID, warehouseIDFrom, warehouseIDTo,
            transferQty, _currentProductVariation, _currentOptionCombination)

                ' Reload the ComboBoxes after the successful transfer
                ReloadComboBoxes()

                ' Optionally, clear TxtProductQty to reset the input field
                TxtProductQty.Clear()

                ' Show success message using DynamicDialogs
                DynamicDialogs.ShowSuccess(Me, "Stock transferred successfully.", "Transfer Complete")
            Catch ex As Exception
                ' Show error dialog if exception occurs during transfer
                DynamicDialogs.ShowError(Me, "Error during transfer: " & ex.Message, "Transfer Error")
            End Try
        End Sub

        Private Sub ReloadComboBoxes()
            ' Reset variation controls
            StackPanelVariations.Visibility = Visibility.Collapsed
            ComboBoxVariation.Items.Clear()
            ComboBoxOption.Items.Clear()
            _currentOptionCombination = ""

            ' Clear and reload the product ComboBox
            ComboBoxProduct.SelectedIndex = -1
            StocksTransferController.GetProducts(ComboBoxProduct)

            ' Clear and reload the transferFrom ComboBox
            ComboBoxTransferFrom.SelectedIndex = -1
            ComboBoxTransferTo.SelectedIndex = -1

            ' Reset the quantity and maximum units
            TxtProductQty.Text = "0"
            maxUnits.Text = ""

            ' Reselect the first product to trigger all cascading updates
            If ComboBoxProduct.Items.Count > 0 Then
                ComboBoxProduct.SelectedIndex = 0
                ComboBoxProduct_SelectionChanged(Nothing, Nothing)
            End If
        End Sub
    End Class
End Namespace