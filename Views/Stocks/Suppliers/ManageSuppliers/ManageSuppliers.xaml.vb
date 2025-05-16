Imports System.Windows.Controls
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports System.Data
Imports DPC.DPC.Components.Dynamic ' Added import for DynamicDialogs

Namespace DPC.Views.Stocks.Suppliers.ManageSuppliers
    Public Class ManageSuppliers
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
                ' Changed from MessageBox to DynamicDialogs
                DynamicDialogs.ShowError(Me, "DataGrid not found in the XAML.", "Initialization Error")
                Return
            End If

            If paginationPanel Is Nothing Then
                ' Changed from MessageBox to DynamicDialogs
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

            ' Initialize and load suppliers data
            LoadData()
        End Sub

        ' Event handler for TextChanged to update the filter
        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            If _searchFilterHelper IsNot Nothing Then
                _searchFilterHelper.SearchText = txtSearch.Text
            End If
        End Sub

        ' Load Data Using SupplierController
        Public Sub LoadData()
            Try
                ' Check if DataGrid exists
                If dataGrid Is Nothing Then
                    ' Changed from MessageBox to DynamicDialogs
                    DynamicDialogs.ShowError(Me, "DataGrid control not found.", "Error")
                    Return
                End If

                ' Get all suppliers with error handling
                Dim allSuppliers As ObservableCollection(Of Object)
                Try
                    Dim supplierList = SupplierController.GetSuppliers()
                    If supplierList Is Nothing Then
                        ' Changed from MessageBox to DynamicDialogs
                        DynamicDialogs.ShowWarning(Me, "Supplier data returned null.", "Data Error")
                        allSuppliers = New ObservableCollection(Of Object)()
                    Else
                        allSuppliers = New ObservableCollection(Of Object)(supplierList)
                    End If
                Catch ex As Exception
                    ' Changed from MessageBox to DynamicDialogs
                    DynamicDialogs.ShowError(Me, "Error retrieving supplier data: " & ex.Message, "Data Error")
                    allSuppliers = New ObservableCollection(Of Object)()
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
                _paginationHelper.AllItems = allSuppliers

                ' Initialize search filter helper with our pagination helper
                _searchFilterHelper = New SearchFilterHelper(_paginationHelper,
                    "SupplierID", "SupplierName", "SupplierCompany", "OfficeAddress",
                    "SupplierEmail", "SupplierPhone", "BrandNames")

            Catch ex As Exception
                ' Changed from MessageBox to DynamicDialogs with more details option
                Dim errorDialog = DynamicDialogs.ShowError(Me,
                    "Error in LoadData: " & ex.Message,
                    "Error",
                    "View Details")

                ' Add exception data for details
                errorDialog.DialogData = ex

                ' Handle "View Details" button click
                AddHandler errorDialog.PrimaryAction, Sub(s, args)
                                                          Dim exception = DirectCast(args.Data, Exception)
                                                          ' Show stack trace in another dialog
                                                          DynamicDialogs.ShowInformation(Me,
                                                              "Stack Trace: " & exception.StackTrace,
                                                              "Error Details")
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
                ExcelExporter.ExportDataGridToExcel(dataGrid, columnsToExclude, "Suppliers", "Suppliers List")

                ' Show success message after export
                DynamicDialogs.ShowSuccess(Me, "Data exported successfully to Excel.", "Export Complete")
            Catch ex As Exception
                ' Show error if export fails
                DynamicDialogs.ShowError(Me, "Failed to export data: " & ex.Message, "Export Error")
            End Try
        End Sub

        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("newsuppliers", Me)
        End Sub
    End Class
End Namespace