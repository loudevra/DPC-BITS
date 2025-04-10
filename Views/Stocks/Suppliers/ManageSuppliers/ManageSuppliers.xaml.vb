Imports System.Windows
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Components
Imports System.Windows.Controls
Imports System.ComponentModel
Imports DPC.DPC.Data.Helpers

Namespace DPC.Views.Stocks.Suppliers.ManageSuppliers
    Public Class ManageSuppliers
        Inherits Window

        Private view As ICollectionView

        Public Sub New()
            InitializeComponent()

            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Child = sidebar

            ' Load Top Navigation Bar
            Dim topNav As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Child = topNav

            LoadData()

            ' Load DataGrid with items and create a CollectionViewSource for filtering
            view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource)
            If view IsNot Nothing Then
                view.Filter = AddressOf FilterDataGrid
            End If

            ' Set up search text box event handler
            AddHandler txtSearch.TextChanged, AddressOf TxtSearch_TextChanged
        End Sub

        ' Function to filter DataGrid based on search text
        Private Function FilterDataGrid(item As Object) As Boolean
            If String.IsNullOrWhiteSpace(txtSearch.Text) Then
                Return True ' Show all items if search is empty
            End If

            Dim searchText As String = txtSearch.Text.ToLower()

            ' Get the supplier from the item
            Dim supplier = TryCast(item, Data.Model.Supplier)
            If supplier IsNot Nothing Then
                ' Check each relevant property for a match
                Return supplier.SupplierName?.ToLower().Contains(searchText) OrElse
                       supplier.SupplierCompany?.ToLower().Contains(searchText) OrElse
                       supplier.OfficeAddress?.ToLower().Contains(searchText) OrElse
                       supplier.SupplierEmail?.ToLower().Contains(searchText) OrElse
                       supplier.SupplierPhone?.ToLower().Contains(searchText) OrElse
                       supplier.BrandNames?.ToLower().Contains(searchText)
            End If

            Return False
        End Function

        ' Event handler for search text changes
        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            view?.Refresh() ' Refresh the view to apply the filter
        End Sub

        ' Event Handler for Export Button Click - Using the new ExcelExporter helper
        Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
            ' Create a list of column headers to exclude
            Dim columnsToExclude As New List(Of String) From {"Settings", "Actions"}

            ' Use the ExcelExporter helper with column exclusions
            ExcelExporter.ExportDataGridToExcel(dataGrid, columnsToExclude, "Suppliers", "Suppliers List")
        End Sub

        ' Load Data Using SupplierController
        Public Sub LoadData()
            dataGrid.ItemsSource = SupplierController.GetSuppliers()
        End Sub

        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs) Handles btnAddNew.Click
            DPC.Data.Helpers.DynamicView.NavigateToView("newsuppliers", Me)
        End Sub
    End Class
End Namespace