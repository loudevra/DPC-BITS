Imports System.Windows.Controls
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports System.Data
Imports DPC.DPC.Views.Warehouse
Imports DPC.DPC.Components.Dynamic ' Added for DynamicDialogs

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

        Private Sub InitializeControls()
            ' Find UI elements using their name
            dataGrid = TryCast(FindName("dataGrid"), DataGrid)
            txtSearch = TryCast(FindName("txtSearch"), TextBox)
            cboPageSize = TryCast(FindName("cboPageSize"), ComboBox)
            paginationPanel = TryCast(FindName("paginationPanel"), StackPanel)

            ' Verify that required controls are found
            If dataGrid Is Nothing Then
                ' Replaced MessageBox with DynamicDialogs
                DynamicDialogs.ShowError(Me, "DataGrid not found in the XAML.", "Initialization Error")
                Return
            End If

            If paginationPanel Is Nothing Then
                ' Replaced MessageBox with DynamicDialogs
                DynamicDialogs.ShowError(Me, "Pagination panel not found in the XAML.", "Initialization Error")
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
                    ' Replaced MessageBox with DynamicDialogs
                    DynamicDialogs.ShowError(Me, "DataGrid control not found.", "Error")
                    Return
                End If

                ' Get all warehouses with error handling
                Dim allWarehouses As ObservableCollection(Of Object)
                Try
                    Dim warehouseList = WarehouseController.GetWarehouses()
                    If warehouseList Is Nothing Then
                        ' Replaced MessageBox with DynamicDialogs
                        DynamicDialogs.ShowWarning(Me, "Warehouse data returned null.", "Data Error")
                        allWarehouses = New ObservableCollection(Of Object)()
                    Else
                        allWarehouses = New ObservableCollection(Of Object)(warehouseList)
                    End If
                Catch ex As Exception
                    ' Replaced MessageBox with DynamicDialogs
                    DynamicDialogs.ShowError(Me, "Error retrieving warehouse data: " & ex.Message, "Data Error")
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
                ' Replaced MessageBox with DynamicDialogs and added error details
                Dim errorDialog = DynamicDialogs.ShowError(Me,
                    "Error in LoadData: " & ex.Message,
                    "Error",
                    "View Details")

                ' Add custom data
                errorDialog.DialogData = ex

                ' Handle error details button click
                AddHandler errorDialog.PrimaryAction, Sub(s, args)
                                                          Dim exception = DirectCast(args.Data, Exception)
                                                          Dim detailsDialog = DynamicDialogs.ShowError(Me,
                                                              "Stack Trace: " & exception.StackTrace,
                                                              "Error Details",
                                                              "Close")
                                                      End Sub
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

            Try
                ' Create a list of column headers to exclude
                Dim columnsToExclude As New List(Of String) From {"Settings"}
                ' Use the ExcelExporter helper with column exclusions
                ExcelExporter.ExportDataGridToExcel(dataGrid, columnsToExclude, "Warehouses", "Warehouses List")

                ' Added success message with DynamicDialogs
                DynamicDialogs.ShowSuccess(Me, "Data successfully exported to Excel.", "Export Complete")
            Catch ex As Exception
                ' Added error handling with DynamicDialogs
                DynamicDialogs.ShowError(Me, "Failed to export data: " & ex.Message, "Export Error")
            End Try
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

                ' Added success message with DynamicDialogs
                DynamicDialogs.ShowSuccess(Me, "Warehouse data has been successfully updated.", "Update Complete")
            End If
        End Sub
    End Class
End Namespace