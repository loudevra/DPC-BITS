Imports System.Windows
Imports System.Windows.Controls
Imports ClosedXML.Excel
Imports Microsoft.Win32
Imports System.Data
Imports System.IO
Imports System.Reflection
Imports System.ComponentModel
Imports DPC.DPC.Data.Controllers
Imports System.Windows.Controls.Primitives
Imports DocumentFormat.OpenXml.Office.CustomUI
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Model
Imports MaterialDesignThemes.Wpf
Imports MaterialDesignThemes.Wpf.Theme

Namespace DPC.Views.Stocks.ProductCategories
    ''' <summary>
    ''' Interaction logic for ProductCategories.xaml
    ''' </summary>
    Public Class ProductCategories
        Inherits Window

        Private view As ICollectionView

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

        ' Function to filter DataGrid based on search text
        Private Function FilterDataGrid(item As Object) As Boolean
            If String.IsNullOrWhiteSpace(txtSearch.Text) Then
                Return True ' Show all items if search is empty
            End If

            Dim searchText As String = txtSearch.Text.ToLower()

            Return False
        End Function

        ' Event handler for TextBox TextChanged event
        Private Sub txtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            If view IsNot Nothing Then
                view.Refresh() ' Refresh the DataGrid filter whenever the text changes
            End If
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
        Public Sub LoadData()
            dataGrid.ItemsSource = ProductCategoryController.GetProductCategory()
        End Sub


        Private Sub addCategory(sender As Object, e As RoutedEventArgs)
            ' Pass the button and the user control to the helper
            Dim addCategoryWindow As New DPC.Components.Forms.AddCategory()
            PopupHelper.OpenPopupWithControl(sender, addCategoryWindow)
        End Sub





























    End Class
End Namespace
