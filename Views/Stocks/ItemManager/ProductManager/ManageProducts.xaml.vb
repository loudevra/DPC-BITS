Imports System.Windows.Controls.Primitives
Imports System.Windows.Controls
Imports ClosedXML.Excel
Imports Microsoft.Win32
Imports System.Data
Imports System.IO
Imports System.Reflection
Imports System.ComponentModel
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers

Namespace DPC.Views.Stocks.ItemManager.ProductManager
    Public Class ManageProducts
        Inherits UserControl

        Private view As ICollectionView

        Public Sub New()
            InitializeComponent()

            ' Load DataGrid with items and create a CollectionViewSource for filtering
            view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource)
            If view IsNot Nothing Then
                view.Filter = AddressOf FilterDataGrid
            End If

            ' Initialize the StockStatsController with the TextBlocks from our UI
            StockStatsController.Initialize(
                txtInStock,    ' TextBlock for in-stock count
                txtStockOut,   ' TextBlock for stock-out count
                txtTotal       ' TextBlock for total count
            )

            ' Setup search text changed event
            AddHandler txtSearch.TextChanged, AddressOf TxtSearch_TextChanged

            LoadData()
        End Sub

        ' Function to filter DataGrid based on search text
        Private Function FilterDataGrid(item As Object) As Boolean
            If String.IsNullOrWhiteSpace(txtSearch.Text) Then
                Return True ' Show all items if search is empty
            End If

            Dim searchText As String = txtSearch.Text.ToLower()

            ' Get property values from the item for filtering
            ' This depends on your data model structure
            Dim productItem = TryCast(item, Object) ' Change to your actual type if needed

            If productItem IsNot Nothing Then
                ' Check if any properties match the search text
                Try
                    For Each prop In productItem.GetType().GetProperties()
                        Dim value = prop.GetValue(productItem)
                        If value IsNot Nothing AndAlso value.ToString().ToLower().Contains(searchText) Then
                            Return True
                        End If
                    Next
                Catch ex As Exception
                    ' Log exception if needed
                End Try
            End If

            Return False
        End Function

        ' Event handler for search text changed
        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            view?.Refresh()
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
                .FileName = "ProductData.xlsx"
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
                        Dim worksheet = workbook.Worksheets.Add(dt, "ProductData")
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

        ' Load Data Using ProductController and update stock stats
        Public Sub LoadData()
            ProductController.LoadProductData(dataGrid)

            ' Update the stock statistics
            StockStatsController.UpdateStockStats()
        End Sub

        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs) Handles BtnAddNew.Click
            ViewLoader.DynamicView.NavigateToView("manageproducts", Me)
        End Sub

        ' Refresh data and stats when returning to this view
        Public Sub RefreshData()
            StockStatsController.RefreshProductData(dataGrid)
        End Sub
    End Class
End Namespace