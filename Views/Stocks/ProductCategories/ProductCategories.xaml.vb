Imports ClosedXML.Excel
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Data.Helpers
Imports Microsoft.Win32
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Data
Imports System.Reflection
Imports System.Windows.Controls.Primitives

Namespace DPC.Views.Stocks.ProductCategories
    ''' <summary>
    ''' Interaction logic for ProductCategories.xaml
    ''' </summary>
    Public Class ProductCategories
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

            ' Initialize and load product categories data
            LoadData()
        End Sub

        ' Event handler for TextChanged to update the filter
        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            If _searchFilterHelper IsNot Nothing Then
                _searchFilterHelper.SearchText = txtSearch.Text
            End If
        End Sub

        ' Load Data Using ProductCategoryController
        Public Sub LoadData()
            Try
                ' Check if DataGrid exists
                If dataGrid Is Nothing Then
                    MessageBox.Show("DataGrid control not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Get all categories with error handling
                Dim allCategories As ObservableCollection(Of Object)
                Try
                    Dim categoryList = ProductCategoryController.GetAllCategoriesWithSubcategories()
                    If categoryList Is Nothing Then
                        MessageBox.Show("Category data returned null.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                        allCategories = New ObservableCollection(Of Object)()
                    Else
                        allCategories = New ObservableCollection(Of Object)(categoryList.OrderBy(Function(c) c.categoryID))
                    End If
                Catch ex As Exception
                    MessageBox.Show("Error retrieving category data: " & ex.Message, "Data Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    allCategories = New ObservableCollection(Of Object)()
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
                _paginationHelper.AllItems = allCategories

                ' Initialize search filter helper with our pagination helper
                _searchFilterHelper = New SearchFilterHelper(_paginationHelper,
                    "categoryID", "categoryName", "categoryDescription")

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

        ' Event Handler for Export Button Click
        Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
            ' Check if DataGrid has data
            If dataGrid.Items.Count = 0 Then
                MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            ' New model Instance
            Dim subCategory As New Subcategory

            ' Create a list of column headers to exclude
            Dim columnsToExclude As New List(Of String) From {"Settings"}
            ' Use the ExcelExporter helper with column exclusions
            ExcelExporter.ExportDataGridToExcel(dataGrid, columnsToExclude, "CategoriesExport", "Product Categories List", subCategory, "subcategoryName")

        End Sub

        Private Sub AddCategory(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the AddCategory form
            Dim addCategoryWindow As New DPC.Components.Forms.AddCategory()

            ' Subscribe to the event to reload data after adding a category
            AddHandler addCategoryWindow.CategoryAdded, AddressOf OnCategoryAdded

            ' Get the parent window to center the popup
            Dim parentWindow = Window.GetWindow(Me)

            ' Open the popup
            PopupHelper.OpenPopupWithControl(sender, addCategoryWindow, "windowcenter", True, -50, 0, parentWindow)
        End Sub

        ' Event handler to refresh the DataGrid
        Private Sub OnCategoryAdded(sender As Object, e As EventArgs)
            LoadData() ' Reloads the categories in the main view
        End Sub

        Private Sub AddSubcategory(sender As Object, e As RoutedEventArgs)
            Dim addSubcategoryWindow As New DPC.Components.Forms.AddSubcategory()

            ' Subscribe to the event to reload data after adding a category
            AddHandler addSubcategoryWindow.SubCategoryAdded, AddressOf OnSubCategoryAdded

            ' Get the parent window to center the popup
            Dim parentWindow = Window.GetWindow(Me)

            ' Open the popup
            PopupHelper.OpenPopupWithControl(sender, addSubcategoryWindow, "windowcenter", True, -50, 0, parentWindow)
        End Sub

        Private Sub OnSubCategoryAdded(sender As Object, e As EventArgs)
            LoadData() ' Reloads the subcategories in the main view
        End Sub
    End Class
End Namespace