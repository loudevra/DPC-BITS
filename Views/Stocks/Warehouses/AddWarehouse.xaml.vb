Imports System.Collections.ObjectModel
Imports System.Windows
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Model

Namespace DPC.Views.Warehouse
    Partial Public Class AddWarehouse
        Public Sub New()
            InitializeComponent()
            LoadBusinessLocations()
        End Sub

        ' Load Business Locations into ComboBox
        Private Sub LoadBusinessLocations()
            Try
                Dim locations As ObservableCollection(Of KeyValuePair(Of Integer, String)) = WarehouseController.GetBusinessLocations()
                cmbBusinessLocation.ItemsSource = locations
                If locations.Count > 0 Then
                    cmbBusinessLocation.SelectedIndex = 0 ' Default to first item
                End If
            Catch ex As Exception
                MessageBox.Show("Error loading business locations: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' Add Warehouse Function
        Private Sub AddWarehouse(sender As Object, e As RoutedEventArgs)
            Dim name As String = txtName.Text.Trim()
            Dim description As String = txtDescription.Text.Trim()
            Dim selectedLocation As KeyValuePair(Of Integer, String) = CType(cmbBusinessLocation.SelectedItem, KeyValuePair(Of Integer, String))
            Dim businessLocationID As Integer = selectedLocation.Key

            If String.IsNullOrEmpty(name) Then
                MessageBox.Show("Warehouse name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim success As Boolean = WarehouseController.AddWarehouse(name, description, businessLocationID)
            If success Then
                WarehouseController.Reload = True
                MessageBox.Show("Warehouse added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                Me.Close()
            Else
                MessageBox.Show("Failed to add warehouse.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End If

        End Sub

        ' Close Popup
        Private Sub ClosePopup(sender As Object, e As RoutedEventArgs)
            Me.Close()
        End Sub
    End Class
End Namespace
