Imports DPC.DPC.Data.Controllers

Namespace DPC.Views.Stocks.StocksTransfer

    Public Class StocksTransfer
        Inherits Window

        Public Sub New()
            InitializeComponent()

            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Content = sidebar

            ' Load Top Navigation Bar
            Dim topNav As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Content = topNav

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

                    ' Set the maximum allowed quantity for the selected product and warehouse
                    StocksTransferController.SetMaxProductQty(productID, warehouseID, TxtProductQty, maxUnits)

                    ' Call function to populate ComboBoxTransferTo with warehouses except the selected one
                    StocksTransferController.GetAvailableTransferToWarehouses(warehouseID, ComboBoxTransferTo)
                End If
            End If
        End Sub



        ' Restrict TxtProductQty to only allow digits and max stock value
        Private Sub TxtProductQty_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            ' Allow only digits (0-9) to be entered
            e.Handled = Not IsNumeric(e.Text)
        End Sub

    End Class
End Namespace
