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
Imports MySql.Data.MySqlClient

Namespace DPC.Views.Stocks.ItemManager.ProductManager
    Public Class ManageProducts
        Inherits UserControl

        Private _dataTable As DataTable
        Private _isInitialized As Boolean = False

        Public Sub New()
            InitializeComponent()

            ' Set up the TextChanged event immediately
            AddHandler txtSearch.TextChanged, AddressOf TxtSearch_TextChanged

            ' Initialize the StockStatsController with the TextBlocks from our UI
            StockStatsController.Initialize(
                txtInStock,    ' TextBlock for in-stock count
                txtStockOut,   ' TextBlock for stock-out count
                txtTotal       ' TextBlock for total count
            )

            ' Load data after initialization is complete
            AddHandler Me.Loaded, AddressOf UserControl_Loaded
        End Sub

        Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
            If Not _isInitialized Then
                LoadData()
                _isInitialized = True
            End If
        End Sub

        ' Event handler for search text changed
        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            ApplyFilter()
        End Sub

        ' Apply the filter to the DataTable view
        Private Sub ApplyFilter()
            If _dataTable Is Nothing Then Return

            Dim searchText As String = txtSearch.Text.Trim().ToLower()

            If String.IsNullOrWhiteSpace(searchText) Then
                ' If search is empty, clear filter
                _dataTable.DefaultView.RowFilter = ""
            Else
                ' Build filter string for each searchable column
                Dim filterExpressions As New List(Of String)

                ' Loop through columns and add filter for string columns
                For Each column As DataColumn In _dataTable.Columns
                    ' Skip image/binary columns
                    If column.DataType Is GetType(Byte()) OrElse
                       column.ColumnName = "ImageSource" OrElse
                       column.ColumnName = "ProductImage" Then
                        Continue For
                    End If

                    ' Add filter expression for this column
                    filterExpressions.Add(String.Format("CONVERT({0}, 'System.String') LIKE '%{1}%'",
                                                       column.ColumnName,
                                                       searchText.Replace("'", "''")))
                Next

                ' Combine all expressions with OR
                If filterExpressions.Count > 0 Then
                    _dataTable.DefaultView.RowFilter = String.Join(" OR ", filterExpressions)
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
            Try
                ' Modified version to save the DataTable
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    ' Query from paste-2.txt
                    Dim query As String = "
                SELECT 
                    p.productID AS ID,
                    p.productName AS Name,
                    c.categoryName AS Category,
                    sc.subcategoryName AS SubCategory,
                    b.brandName AS Brand,
                    s.supplierName AS Supplier,
                    GROUP_CONCAT(
                        DISTINCT CASE 
                            WHEN pnv.stockUnit > 0 THEN w.warehouseName
                            WHEN pvs.stockUnit > 0 THEN wv.warehouseName
                            ELSE NULL
                        END
                        SEPARATOR ', '
                    ) AS Warehouse,
                    SUM(COALESCE(pnv.stockUnit, 0) + COALESCE(pvs.stockUnit, 0)) AS StockQuantity,
                    MAX(COALESCE(pnv.alertQuantity, pvs.alertQuantity, 0)) AS AlertQuantity,
                    p.productImage AS ProductImage,
                    p.productVariation AS HasVariations
                FROM product p
                LEFT JOIN category c ON p.categoryID = c.categoryID
                LEFT JOIN subcategory sc ON p.subcategoryID = sc.subcategoryID
                LEFT JOIN brand b ON p.brandID = b.brandID
                LEFT JOIN supplier s ON p.supplierID = s.supplierID
                
                -- For products without variations
                LEFT JOIN productnovariation pnv ON p.productID = pnv.productID AND p.productVariation = 0
                LEFT JOIN warehouse w ON pnv.warehouseID = w.warehouseID
                
                -- For products with variations
                LEFT JOIN productvariationstock pvs ON p.productID = pvs.productID AND p.productVariation = 1
                LEFT JOIN warehouse wv ON pvs.warehouseID = wv.warehouseID
                
                GROUP BY p.productID, p.productName, c.categoryName, sc.subcategoryName, b.brandName, s.supplierName, p.productImage, p.productVariation
                ORDER BY p.productName;
            "
                    conn.Open()
                    Dim adapter As New MySqlDataAdapter(query, conn)
                    _dataTable = New DataTable()
                    adapter.Fill(_dataTable)

                    ' Add column for Image display
                    If Not _dataTable.Columns.Contains("ImageSource") Then
                        _dataTable.Columns.Add("ImageSource", GetType(BitmapImage))
                    End If

                    ' Process images and count stocks
                    Dim inStockProducts As Integer = 0
                    Dim stockOutProducts As Integer = 0

                    For Each row As DataRow In _dataTable.Rows
                        ' Process the image
                        If row("ProductImage") IsNot DBNull.Value Then
                            Dim base64String As String = row("ProductImage").ToString()
                            Try
                                ' Convert Base64 to BitmapImage
                                Dim imageBytes As Byte() = Convert.FromBase64String(base64String)
                                Dim imageSource As New BitmapImage()
                                Using stream As New MemoryStream(imageBytes)
                                    imageSource.BeginInit()
                                    imageSource.StreamSource = stream
                                    imageSource.CacheOption = BitmapCacheOption.OnLoad
                                    imageSource.EndInit()
                                    imageSource.Freeze() ' Important for cross-thread usage
                                End Using
                                row("ImageSource") = imageSource
                            Catch ex As Exception
                                ' Set a default image or handle error
                                Console.WriteLine($"Error processing image: {ex.Message}")
                            End Try
                        End If

                        ' Count products by stock status
                        Dim stockQuantity As Integer = Convert.ToInt32(row("StockQuantity"))
                        If stockQuantity > 0 Then
                            inStockProducts += 1
                        Else
                            stockOutProducts += 1
                            ' For products with no stock, ensure warehouse is empty
                            row("Warehouse") = DBNull.Value
                        End If
                    Next

                    ' Set the data source
                    dataGrid.ItemsSource = _dataTable.DefaultView

                    ' Update status cards
                    txtInStock.Text = inStockProducts.ToString()
                    txtStockOut.Text = stockOutProducts.ToString()
                    txtTotal.Text = _dataTable.Rows.Count.ToString()
                End Using

            Catch ex As Exception
                MessageBox.Show($"Error loading data: {ex.Message}")
            End Try
        End Sub

        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs) Handles BtnAddNew.Click
            ViewLoader.DynamicView.NavigateToView("manageproducts", Me)
        End Sub

        ' Refresh data and stats when returning to this view
        Public Sub RefreshData()
            LoadData() ' This will reload the data and update the stats
        End Sub
    End Class
End Namespace