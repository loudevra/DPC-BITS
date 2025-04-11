Imports System.Windows.Controls
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports System.Data

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
                    MessageBox.Show("DataGrid control not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Get all suppliers with error handling
                Dim allSuppliers As ObservableCollection(Of Object)
                Try
                    Dim supplierList = SupplierController.GetSuppliers()
                    If supplierList Is Nothing Then
                        MessageBox.Show("Supplier data returned null.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                        allSuppliers = New ObservableCollection(Of Object)()
                    Else
                        allSuppliers = New ObservableCollection(Of Object)(supplierList)
                    End If
                Catch ex As Exception
                    MessageBox.Show("Error retrieving supplier data: " & ex.Message, "Data Error", MessageBoxButton.OK, MessageBoxImage.Error)
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
            ExcelExporter.ExportDataGridToExcel(dataGrid, columnsToExclude, "Suppliers", "Suppliers List")
        End Sub

        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs)
            DPC.Data.Helpers.DynamicView.NavigateToView("newsuppliers", Me)
        End Sub
    End Class
End Namespace