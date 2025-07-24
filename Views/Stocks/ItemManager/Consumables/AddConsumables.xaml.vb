Imports System.Collections.ObjectModel
Imports System.Windows
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Controllers.Stocks
Imports DPC.DPC.Data.Model

Namespace DPC.Views.ItemManager.Consumables
    Partial Public Class AddConsumables
        Public Sub New()
            InitializeComponent()
            LoadBusinessLocations()
        End Sub

        ' Load Business Locations into ComboBox
        Private Sub LoadBusinessLocations()
            Try
                Dim warehouses = StocksLabelController.GetWarehouse()
                cmbWarehouses.ItemsSource = warehouses
                cmbWarehouses.DisplayMemberPath = "Value"
                cmbWarehouses.SelectedValuePath = "Key"
                If warehouses.Count > 0 Then
                    cmbWarehouses.SelectedIndex = 0 ' Default to first item
                End If
            Catch ex As Exception
                MessageBox.Show("Error loading business locations: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub AddConsumable(sender As Object, e As RoutedEventArgs)
            If cmbWarehouses.SelectedItem IsNot Nothing Then
                Dim selectedItem = CType(cmbWarehouses.SelectedItem, KeyValuePair(Of Integer, String))
                Dim warehouseID As Integer = selectedItem.Key
                Dim warehouseName As String = selectedItem.Value

                If PullOutFormController.InsertConsumable(txtName.Text, warehouseID, warehouseName, txtStock.Text) Then
                    Me.Close()
                End If
            End If


        End Sub

        ' Close Popup
        Private Sub ClosePopup(sender As Object, e As RoutedEventArgs)
            Me.Close()
        End Sub
    End Class
End Namespace
