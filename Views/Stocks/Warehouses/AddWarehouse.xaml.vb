Imports System.Collections.ObjectModel
Imports System.Windows
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Model
' Import the DynamicDialogs namespace
Imports DPC.DPC.Components.Dynamic

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
                ' Replace MessageBox with DynamicDialogs.ShowError
                DynamicDialogs.ShowError(Me, "Error loading business locations: " & ex.Message)
            End Try
        End Sub

        ' Add Warehouse Function
        Private Sub AddWarehouse(sender As Object, e As RoutedEventArgs)
            Dim name As String = txtName.Text.Trim()
            Dim description As String = txtDescription.Text.Trim()

            ' Validate that a location is selected
            If cmbBusinessLocation.SelectedItem Is Nothing Then
                DynamicDialogs.ShowWarning(Me, "Please select a business location.")
                Return
            End If

            Dim selectedLocation As KeyValuePair(Of Integer, String) = CType(cmbBusinessLocation.SelectedItem, KeyValuePair(Of Integer, String))
            Dim businessLocationID As Integer = selectedLocation.Key

            If String.IsNullOrEmpty(name) Then
                ' Replace MessageBox with DynamicDialogs.ShowWarning
                DynamicDialogs.ShowWarning(Me, "Warehouse name cannot be empty.", "Validation Error")
                Return
            End If

            Dim success As Boolean = WarehouseController.AddWarehouse(name, description, businessLocationID)

            If success Then
                WarehouseController.Reload = True
                ' Replace MessageBox with DynamicDialogs.ShowSuccess
                Dim successDialog = DynamicDialogs.ShowSuccess(Me, "Warehouse added successfully.", "Success")
                ' Add handler to close the window when the dialog is closed
                AddHandler successDialog.DialogClosed, Sub(s, args)
                                                           Me.Close()
                                                       End Sub
            Else
                ' Replace MessageBox with DynamicDialogs.ShowError
                DynamicDialogs.ShowError(Me, "Failed to add warehouse.", "Error")
            End If
        End Sub

        ' Close Popup
        Private Sub ClosePopup(sender As Object, e As RoutedEventArgs)
            ' You could optionally add a confirmation dialog here
            Me.Close()
        End Sub
    End Class
End Namespace