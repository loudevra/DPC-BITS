Imports System.Windows.Controls
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports System.Data
Imports DPC.DPC.Views.Warehouse
Imports DPC.DPC.Data.Model

Namespace DPC.Views.Stocks.Warehouses
    Public Class Warehouses
        Inherits UserControl

        ' Properties for pagination
        Private _paginationHelper As PaginationHelper
        Private _searchFilterHelper As SearchFilterHelper

        ' UI elements for direct access
        Private popup As Popup
        Private recentlyClosed As Boolean = False

        Public Sub New()
            InitializeComponent()
            InitializeControls()
        End Sub

        Public Sub InitializeControls()
            ' Find UI elements using their name
            dataGrid = TryCast(FindName("dataGrid"), DataGrid)
            txtSearch = TryCast(FindName("txtSearch"), TextBox)
            cboPageSize = TryCast(FindName("cboPageSize"), ComboBox)
            paginationPanel = TryCast(FindName("paginationPanel"), StackPanel)

            ' Verify that required controls are found
            If dataGrid Is Nothing Then
                MessageBox.Show("DataGrid not found in the XAML.", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return
            End If

            If paginationPanel Is Nothing Then
                MessageBox.Show("Pagination panel not found in the XAML.", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return
            End If

            ' Wire up event handlers
            If txtSearch IsNot Nothing Then
                AddHandler txtSearch.TextChanged, AddressOf TxtSearch_TextChanged
            End If

            If cboPageSize IsNot Nothing Then
                AddHandler cboPageSize.SelectionChanged, AddressOf CboPageSize_SelectionChanged
            End If

            ' Set up button click event
            If btnAddNew IsNot Nothing Then
                AddHandler btnAddNew.Click, AddressOf BtnAddNew_Click
            End If

            ' Initialize and load warehouses data
            LoadData()
        End Sub

        ' Event handler for TextChanged to update the filter
        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            If _searchFilterHelper IsNot Nothing Then
                _searchFilterHelper.SearchText = txtSearch.Text
            End If
        End Sub

        ' Load Data Using WarehouseController
        Public Sub LoadData()
            Try
                ' Check if DataGrid exists
                If dataGrid Is Nothing Then
                    MessageBox.Show("DataGrid control not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Get all warehouses with error handling
                Dim allWarehouses As ObservableCollection(Of Object)
                Try
                    Dim warehouseList = WarehouseController.GetWarehouses()
                    If warehouseList Is Nothing Then
                        MessageBox.Show("Warehouse data returned null.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                        allWarehouses = New ObservableCollection(Of Object)()
                    Else
                        allWarehouses = New ObservableCollection(Of Object)(warehouseList)
                    End If
                Catch ex As Exception
                    MessageBox.Show("Error retrieving warehouse data: " & ex.Message, "Data Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    allWarehouses = New ObservableCollection(Of Object)()
                End Try

                ' Clear the pagination panel to avoid duplicate controls
                paginationPanel.Children.Clear()

                ' Initialize pagination helper with our DataGrid and pagination panel
                _paginationHelper = New PaginationHelper(dataGrid, paginationPanel)

                ' Set the items per page from the combo box if available
                If cboPageSize IsNot Nothing Then
                    Dim selectedItem = TryCast(cboPageSize.SelectedItem, ComboBoxItem)
                    If selectedItem IsNot Nothing Then
                        Dim itemsPerPageText As String = TryCast(selectedItem.Content, String)
                        Dim itemsPerPage As Integer
                        If Integer.TryParse(itemsPerPageText, itemsPerPage) Then
                            _paginationHelper.ItemsPerPage = itemsPerPage
                        End If
                    End If
                End If

                ' Set the all items to the helper
                _paginationHelper.AllItems = allWarehouses

                ' Initialize search filter helper with our pagination helper
                _searchFilterHelper = New SearchFilterHelper(_paginationHelper,
                    "ID", "Name", "TotalProducts", "StockQuantity", "Worth")

            Catch ex As Exception
                MessageBox.Show("Error in LoadData: " & ex.Message & vbCrLf & "Stack Trace: " & ex.StackTrace,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub CboPageSize_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If _paginationHelper Is Nothing Then Return

            ' Get the selected value from the ComboBox
            Dim selectedComboBoxItem As ComboBoxItem = TryCast(cboPageSize.SelectedItem, ComboBoxItem)
            If selectedComboBoxItem IsNot Nothing Then
                Dim itemsPerPageText As String = TryCast(selectedComboBoxItem.Content, String)
                Dim newItemsPerPage As Integer

                If Integer.TryParse(itemsPerPageText, newItemsPerPage) Then
                    ' Update the pagination helper's items per page
                    _paginationHelper.ItemsPerPage = newItemsPerPage
                End If
            End If
        End Sub

        ' Event Handler for Export Button Click - Using the ExcelExporter helper
        Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
            If dataGrid Is Nothing Then Return

            ' Create a list of column headers to exclude
            Dim columnsToExclude As New List(Of String) From {"Settings"}
            ' Use the ExcelExporter helper with column exclusions
            ExcelExporter.ExportDataGridToExcel(dataGrid, columnsToExclude, "Warehouses", "Warehouses List")
        End Sub

        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs)
            ' Find the parent window to use as owner
            Dim parentWindow As Window = Window.GetWindow(Me)

            Dim addWarehousePopup As New AddWarehouse()

            ' Set owner if we found a parent window
            If parentWindow IsNot Nothing Then
                addWarehousePopup.Owner = parentWindow
            End If

            addWarehousePopup.ShowDialog() ' Show as modal popup

            ' Check if reload flag is set and refresh the data
            If WarehouseController.Reload Then
                LoadData()
                WarehouseController.Reload = False ' Reset the flag after reloading
            End If
        End Sub

        Private Sub OpenEditWarehouse(sender As Object, e As RoutedEventArgs)
            Dim clickedButton As Button = TryCast(sender, Button)
            If clickedButton Is Nothing Then Return

            If recentlyClosed Then
                recentlyClosed = False
                Return
            End If

            If popup IsNot Nothing AndAlso popup.IsOpen Then
                popup.IsOpen = False
                recentlyClosed = True
                Return
            End If

            popup = New Popup With {
                .PlacementTarget = clickedButton,
                .Placement = PlacementMode.Bottom,
                .StaysOpen = False,
                .AllowsTransparency = True
            }

            Dim editWarehouseWindow As New DPC.Components.Forms.EditWarehouse()

            ' Converts selected row items into brand model
            Dim warehouse As DPC.Data.Model.Warehouses = TryCast(dataGrid.SelectedItem, DPC.Data.Model.Warehouses)

            ' Passes data to pop up
            editWarehouseWindow.TxtWarehouseName.Text = warehouse.Name
            editWarehouseWindow.WarehouseID = Convert.ToInt32(warehouse.ID)
            editWarehouseWindow.WarehouseNameOld = warehouse.Name
            editWarehouseWindow.Warehouses = Me

            ' Handle the BrandAdded event
            AddHandler editWarehouseWindow.WarehouseUpdated, AddressOf OnWarehouseUpdated

            popup.Child = editWarehouseWindow

            AddHandler popup.Closed, Sub()
                                         recentlyClosed = True
                                         Task.Delay(100).ContinueWith(Sub() recentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
                                     End Sub

            popup.IsOpen = True
        End Sub

        Private Sub OnWarehouseUpdated()
            LoadData()
        End Sub

        Private Sub DeleteWarehouse(sender As Object, e As RoutedEventArgs)
            Dim clickedButton As Button = TryCast(sender, Button)
            If clickedButton Is Nothing Then Return

            If recentlyClosed Then
                recentlyClosed = False
                Return
            End If

            If popup IsNot Nothing AndAlso popup.IsOpen Then
                popup.IsOpen = False
                recentlyClosed = True
                Return
            End If

            popup = New Popup With {
                .PlacementTarget = clickedButton,
                .Placement = PlacementMode.Bottom,
                .StaysOpen = False,
                .AllowsTransparency = True
            }

            Dim deleteWarehouseWindow As New DPC.Components.ConfirmationModals.ConfirmWarehouseDeletion()

            ' Converts selected row items into brand model
            Dim warehouse As DPC.Data.Model.Warehouses = TryCast(dataGrid.SelectedItem, DPC.Data.Model.Warehouses)

            deleteWarehouseWindow.warehouseID = warehouse.ID
            deleteWarehouseWindow.Warehouse = Me

            popup.Child = deleteWarehouseWindow

            AddHandler popup.Closed, Sub()
                                         recentlyClosed = True
                                         Task.Delay(100).ContinueWith(Sub() recentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
                                     End Sub

            popup.IsOpen = True
        End Sub

        Private Sub dataGrid_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)

        End Sub
    End Class
End Namespace