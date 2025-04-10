Imports ClosedXML.Excel
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports Microsoft.Win32
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Data
Imports System.Reflection

Namespace DPC.Views.Stocks.ProductCategories
    ''' <summary>
    ''' Interaction logic for ProductCategories.xaml
    ''' </summary>
    Public Class ProductCategories
        Inherits Window

        Private view As ICollectionView
        Private windowLoaded As Boolean = False

        Public Sub New()
            InitializeComponent()

            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Child = sidebar

            ' Load Top Navigation Bar
            Dim topNav As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Child = topNav

            ' Load Data
            LoadData()

            ' Load DataGrid with items and create a CollectionViewSource for filtering
            view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource)
            If view IsNot Nothing Then
                view.Filter = AddressOf FilterDataGrid
            End If
        End Sub

        ' Function to load data into the DataGrid
        Private Sub LoadData()
            ' Fetch data from the database
            Dim categories As List(Of ProductCategory) = ProductCategoryController.GetAllCategoriesWithSubcategories()

            ' Convert the list to an ObservableCollection
            Dim sortedCategories As New ObservableCollection(Of ProductCategory)(categories.OrderBy(Function(c) c.categoryID))

            ' Set the data as the DataGrid's source
            dataGrid.ItemsSource = sortedCategories

            ' Apply sorting on the DataGrid's default view
            Dim view As ICollectionView = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource)
            If view IsNot Nothing Then
                view.SortDescriptions.Clear()
                view.SortDescriptions.Add(New SortDescription("categoryID", ListSortDirection.Ascending))
                view.Refresh()
            End If

        End Sub

        ' Function to filter DataGrid based on search text
        Private Function FilterDataGrid(item As Object) As Boolean
            If String.IsNullOrWhiteSpace(txtSearch.Text) Then
                Return True ' Show all items if search is empty
            End If

            Dim searchText As String = txtSearch.Text.ToLower()

            Return False
        End Function

        ' Event handler for TextBox TextChanged event
        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            view?.Refresh() ' Refresh the DataGrid filter whenever the text changes
        End Sub

        ' Event Handler for Export Button Click
        Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
            ' Check if DataGrid has data
            If dataGrid.Items.Count = 0 Then
                MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            ' Open SaveFileDialog
            Dim saveFileDialog As New SaveFileDialog() With {
                .Filter = "Excel Files (*.xlsx)|*.xlsx",
                .FileName = "DataGridExport.xlsx"
            }

            If saveFileDialog.ShowDialog() = True Then
                Try
                    ' Create Excel workbook
                    Using workbook As New XLWorkbook()
                        Dim dt As New DataTable()

                        ' Add DataGrid columns as table headers
                        For Each column As DataGridColumn In dataGrid.Columns
                            dt.Columns.Add(column.Header.ToString())
                        Next

                        ' Add rows from DataGrid items
                        For Each item In dataGrid.Items
                            Dim row As DataRow = dt.NewRow()
                            For i As Integer = 0 To dataGrid.Columns.Count - 1
                                Dim column As DataGridColumn = dataGrid.Columns(i)
                                Dim boundColumn = TryCast(column, DataGridBoundColumn)
                                If boundColumn IsNot Nothing AndAlso boundColumn.Binding IsNot Nothing Then
                                    Dim binding As Binding = TryCast(boundColumn.Binding, Binding)
                                    If binding IsNot Nothing AndAlso binding.Path IsNot Nothing Then
                                        Dim bindingPath As String = binding.Path.Path
                                        Dim prop As PropertyInfo = item.GetType().GetProperty(bindingPath)
                                        If prop IsNot Nothing Then
                                            row(i) = prop.GetValue(item, Nothing)?.ToString()
                                        End If
                                    End If
                                End If
                            Next
                            dt.Rows.Add(row)
                        Next

                        ' Add table to Excel sheet
                        Dim worksheet = workbook.Worksheets.Add(dt, "DataGridData")
                        worksheet.Columns().AdjustToContents()

                        ' Save Excel file
                        workbook.SaveAs(saveFileDialog.FileName)
                        MessageBox.Show("Export Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error exporting data: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        End Sub

        Private Sub AddCategory(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the AddCategory form
            Dim addCategoryWindow As New DPC.Components.Forms.AddCategory()

            ' Subscribe to the event to reload data after adding a category
            AddHandler addCategoryWindow.CategoryAdded, AddressOf OnCategoryAdded

            ' Open the popup
            PopupHelper.OpenPopupWithControl(sender, addCategoryWindow, "windowcenter", -50, 0, True, Me)
        End Sub

        ' Event handler to refresh the DataGrid
        Private Sub OnCategoryAdded(sender As Object, e As EventArgs)
            LoadData() ' Reloads the categories in the main view
        End Sub

        Private Sub AddSubcategory(sender As Object, e As RoutedEventArgs)
            Dim addSubcategoryWindow As New DPC.Components.Forms.AddSubcategory()

            ' Subscribe to the event to reload data after adding a category
            AddHandler addSubcategoryWindow.SubCategoryAdded, AddressOf OnSubCategoryAdded

            ' Open the popup
            PopupHelper.OpenPopupWithControl(sender, addSubcategoryWindow, "windowcenter", -50, 0, True, Me)
        End Sub

        Private Sub OnSubCategoryAdded(sender As Object, e As EventArgs)
            LoadData() ' Reloads the subcategories in the main view
        End Sub

    End Class
End Namespace
