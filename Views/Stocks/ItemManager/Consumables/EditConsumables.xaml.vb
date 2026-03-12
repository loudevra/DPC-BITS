Imports System.Windows
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Controllers.Stocks
Imports DPC.DPC.Views.Stocks.ItemManager.Consumables
Imports DPC.Views.Stocks.ItemManager.Consumables

Namespace DPC.Views.ItemManager.Consumables
    Partial Public Class EditConsumables
        Private _selectedConsumable As ConsumableModels

        ' Constructor that accepts the data row model
        Public Sub New(item As ConsumableModels)
            InitializeComponent()
            _selectedConsumable = item
            LoadBusinessLocations()
            PopulateFields()
        End Sub

        ' Load available warehouses for selection
        Private Sub LoadBusinessLocations()
            Try
                Dim warehouses = StocksLabelController.GetWarehouse()
                cmbWarehouses.ItemsSource = warehouses
                cmbWarehouses.DisplayMemberPath = "Value"
                cmbWarehouses.SelectedValuePath = "Key"
            Catch ex As Exception
                MessageBox.Show("Error loading warehouses: " & ex.Message)
            End Try
        End Sub

        ' Pre-fill the window with existing record data
        Private Sub PopulateFields()
            If _selectedConsumable IsNot Nothing Then
                txtName.Text = _selectedConsumable.ProductName
                txtStock.Text = _selectedConsumable.Stock.ToString()

                ' Select the current warehouse in the ComboBox
                For Each item As KeyValuePair(Of Integer, String) In cmbWarehouses.ItemsSource
                    If item.Value = _selectedConsumable.WarehouseName Then
                        cmbWarehouses.SelectedItem = item
                        Exit For
                    End If
                Next
            End If
        End Sub

        ' Process the update through the controller
        Private Sub BtnUpdate_Click(sender As Object, e As RoutedEventArgs)
            If cmbWarehouses.SelectedItem IsNot Nothing Then
                Dim selectedWarehouse = CType(cmbWarehouses.SelectedItem, KeyValuePair(Of Integer, String))

                ' Pass updated data to the controller
                If PullOutFormController.UpdateConsumable(_selectedConsumable.ProductID, txtName.Text, selectedWarehouse.Key, selectedWarehouse.Value, txtStock.Text) Then
                    Me.DialogResult = True
                    Me.Close()
                End If
            Else
                MessageBox.Show("Please select a warehouse.")
            End If
        End Sub

        ' Text formatting logic (Uppercase)
        Private Sub txtName_TextChanged(sender As Object, e As TextChangedEventArgs)
            HandleUpperCase(TryCast(sender, TextBox), AddressOf txtName_TextChanged)
        End Sub

        Private Sub txtStock_TextChanged(sender As Object, e As TextChangedEventArgs)
            HandleUpperCase(TryCast(sender, TextBox), AddressOf txtStock_TextChanged)
        End Sub

        Private Sub HandleUpperCase(tb As TextBox, handler As TextChangedEventHandler)
            If tb Is Nothing Then Return
            Dim original = tb.Text
            Dim upper = original.ToUpperInvariant()
            If original = upper Then Return

            RemoveHandler tb.TextChanged, handler
            tb.Text = upper
            tb.SelectionStart = tb.Text.Length
            AddHandler tb.TextChanged, handler
        End Sub

        ' Close the modal without saving
        Private Sub ClosePopup(sender As Object, e As RoutedEventArgs)
            Me.DialogResult = False
            Me.Close()
        End Sub
    End Class
End Namespace