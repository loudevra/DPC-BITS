Imports DPC.DPC.Data.Controllers

Namespace DPC.Views.Stocks.StocksTransfer

    Public Class StocksTransfer
        Inherits UserControl

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
            ' Get the selected product's productID
            Dim selectedProduct As ComboBoxItem = CType(ComboBoxProduct.SelectedItem, ComboBoxItem)
            If selectedProduct IsNot Nothing Then
                Dim productID As String = CType(selectedProduct.Tag, String)

                ' Call GetAvailableWarehouses to populate the warehouses ComboBox
                StocksTransferController.GetAvailableWarehouses(productID, ComboBoxTransferFrom)

                ' After selecting the product, trigger the warehouse selection change logic
                If ComboBoxTransferFrom.Items.Count > 0 Then
                    ComboBoxTransferFrom.SelectedIndex = 0
                    ComboBoxTransferFrom_SelectionChanged(Nothing, Nothing)
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
                    StocksTransferController.SetMaxProductQty(productID, warehouseID, TxtProductQty, maxUnits)

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
                End If
            End If
        End Sub

        'Transfer btn
        Private Sub BtnTransfer(sender As Object, e As RoutedEventArgs)
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
                ' Call the controller to handle the transfer
                StocksTransferController.TransferStock(productID, warehouseIDFrom, warehouseIDTo, transferQty)

                ' Reload the ComboBoxes after the successful transfer
                ReloadComboBoxes()

                ' Optionally, clear TxtProductQty to reset the input field
                TxtProductQty.Clear()

                ' Show success message
                MessageBox.Show("Stock transferred successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
            Else
                MessageBox.Show("Please enter a valid quantity to transfer.", "Invalid Quantity", MessageBoxButton.OK, MessageBoxImage.Warning)
            End If
        End Sub

        Private Sub ReloadComboBoxes()
            ' Clear and reload the product ComboBox
            ComboBoxProduct.SelectedIndex = -1
            StocksTransferController.GetProducts(ComboBoxProduct)

            ' Clear and reload the transferFrom ComboBox
            ComboBoxTransferFrom.SelectedIndex = -1
            Dim selectedProduct As ComboBoxItem = CType(ComboBoxProduct.SelectedItem, ComboBoxItem)
            If selectedProduct IsNot Nothing Then
                Dim productID As String = CType(selectedProduct.Tag, String)
                StocksTransferController.GetAvailableWarehouses(productID, ComboBoxTransferFrom)
            End If

            ' Clear and reload the transferTo ComboBox
            ComboBoxTransferTo.SelectedIndex = -1
            If ComboBoxTransferFrom.SelectedItem IsNot Nothing Then
                Dim selectedTransferFromWarehouse As ComboBoxItem = CType(ComboBoxTransferFrom.SelectedItem, ComboBoxItem)
                Dim warehouseIDFrom As String = CType(selectedTransferFromWarehouse.Tag, String)
                StocksTransferController.GetAvailableTransferToWarehouses(warehouseIDFrom, ComboBoxTransferTo)
            End If
        End Sub
    End Class
End Namespace