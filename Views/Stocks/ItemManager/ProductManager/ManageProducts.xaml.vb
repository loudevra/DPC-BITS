Imports System.ComponentModel
Imports System.Data
Imports System.IO
Imports System.Reflection
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports ClosedXML.Excel
Imports DocumentFormat.OpenXml.Spreadsheet
Imports DocumentFormat.OpenXml.Wordprocessing
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports Microsoft.Win32
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Model
Imports System.Collections.ObjectModel
Imports DocumentFormat.OpenXml.Office.MetaAttributes
Imports DPC.DPC.Components.ConfirmationModals

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

            ' Excel exporter if datagrid does not have a model
            ExcelExporter.ExportExcel(dataGrid, 11, "ProductExport")
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
    GROUP_CONCAT(DISTINCT WarehouseFiltered.warehouseName SEPARATOR ', ') AS Warehouse,  
    SUM(COALESCE(pnv.stockUnit, 0) + COALESCE(pvs.stockUnit, 0)) AS StockQuantity,  
    MAX(COALESCE(pnv.alertQuantity, pvs.alertQuantity, 0)) AS AlertQuantity,  
    MAX(COALESCE(pnv.buyingPrice, pvs.buyingPrice, 0)) AS BuyingPrice,
    MAX(COALESCE(pnv.sellingPrice, pvs.sellingPrice, 0)) AS SellingPrice,
    p.productImage AS ProductImage,  
    p.productVariation AS HasVariations  

FROM product p  

LEFT JOIN category c ON p.categoryID = c.categoryID  
LEFT JOIN subcategory sc ON p.subcategoryID = sc.subcategoryID  
LEFT JOIN brand b ON p.brandID = b.brandID  
LEFT JOIN supplier s ON p.supplierID = s.supplierID  

-- Products without variations  
LEFT JOIN productnovariation pnv ON p.productID = pnv.productID AND p.productVariation = 0  
LEFT JOIN warehouse w ON pnv.warehouseID = w.warehouseID AND pnv.stockUnit > 0  

-- Products with variations  
LEFT JOIN productvariationstock pvs ON p.productID = pvs.productID AND p.productVariation = 1  
LEFT JOIN warehouse wv ON pvs.warehouseID = wv.warehouseID AND pvs.stockUnit > 0  

-- Unified warehouse list filtered by stock  
LEFT JOIN (
    SELECT warehouseID, warehouseName FROM warehouse
) AS WarehouseFiltered ON 
    (p.productVariation = 0 AND w.warehouseID = WarehouseFiltered.warehouseID) OR
    (p.productVariation = 1 AND wv.warehouseID = WarehouseFiltered.warehouseID)  

GROUP BY  
    p.productID, p.productName, c.categoryName, sc.subcategoryName,  
    b.brandName, s.supplierName, p.productImage, p.productVariation

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
            ViewLoader.DynamicView.NavigateToView("newproducts", Me)
        End Sub

        ' Refresh data and stats when returning to this view
        Public Sub RefreshData()
            LoadData() ' This will reload the data and update the stats
        End Sub

        'Checks other product details regardless if has variation or not
        Public Sub GetProductDetailsBasedOnVariation(table As String, param As Int64)
            Dim GetProduct As String = "SELECT warehouseID, sellingPrice, buyingPrice, stockUnit, alertQuantity FROM " & table & " WHERE productID = @productID"
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(GetProduct, conn)
                        cmd.Parameters.AddWithValue("@productID", param)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            If reader.Read() Then

                                'store data in a cache
                                cacheWarehouseID = Convert.ToString(reader("warehouseID"))
                                cacheSellingPrice = Convert.ToDouble(reader("sellingPrice"))
                                cacheBuyingPrice = Convert.ToDouble(reader("buyingPrice"))
                                cacheStockUnit = Convert.ToInt16(reader("stockUnit"))
                                cacheAlertQuantity = Convert.ToInt64(reader("alertQuantity"))
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error retrieving product details: " & ex.Message)
                End Try
            End Using
        End Sub
        Private Sub OpenEditProduct(sender As Object, e As RoutedEventArgs)
            Dim editProductWindow As New DPC.Views.Stocks.ItemManager.ProductManager.EditProduct()
            Dim product As DataRowView = TryCast(dataGrid.SelectedItem, DataRowView)


            'get product details
            Dim connStr As String = SplashScreen.GetDatabaseConnection().ConnectionString
            Using conn As New MySqlConnection(connStr)
                Dim GetProductQuery As String = "SELECT * FROM product WHERE productID = @productID"
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(GetProductQuery, conn)
                        cmd.Parameters.AddWithValue("@productID", product("ID"))
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()

                                'store data in a cache
                                cacheProductID = product("ID")
                                cacheProductCode = reader("productCode").ToString()
                                cacheProductName = reader("productName").ToString()
                                cacheCategoryID = Convert.ToInt64(reader("categoryID"))
                                cacheSubCategoryID = Convert.ToInt64(reader("subcategoryID"))
                                cacheSupplierID = Convert.ToInt64(reader("subcategoryID"))
                                cacheBrandID = Convert.ToInt64(reader("brandID"))
                                cacheProductImage = reader("productImage").ToString()
                                cacheMeasurementUnit = reader("measurementUnit").ToString()
                                cacheProductDescription = reader("productDescription").ToString()
                                cacheProductVariation = Convert.ToBoolean(reader("productVariation"))
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error retrieving product code: " & ex.Message)
                End Try

                If cacheProductVariation Then
                    'if has variation
                    GetProductDetailsBasedOnVariation("productvariationstock", product("ID"))
                    MessageBox.Show("Products with variation are still on progress...")
                Else
                    'if has no variation
                    GetProductDetailsBasedOnVariation("productnovariation", product("ID"))

                    'store selected product's serial numbers
                    Dim serialData As List(Of Tuple(Of Integer, String)) = GetProduct.GetSerialDataForProduct(product("ID"))

                    cacheSerialNumbers = serialData.Select(Function(x) x.Item2).ToList()
                    cacheSerialID = serialData.Select(Function(x) x.Item1).ToList()



                    'Dim message As String = String.Join(Environment.NewLine, serialData.Select(Function(x) $"SerialID: {x.Item1}, SerialNumber: {x.Item2}"))
                    'MessageBox.Show(message, "Serial Data")

                    ViewLoader.DynamicView.NavigateToView("editproduct", Me)
                End If
            End Using
        End Sub

        Private Sub DeleteProduct(sender As Object, e As RoutedEventArgs)
            Dim deleteProduct As New DPC.Components.ConfirmationModals.DeleteProductConfirmation()
            Dim product As DataRowView = TryCast(dataGrid.SelectedItem, DataRowView)

            cacheDeleteProductID = product("ID")
            cacheDeleteProductName = product("Name")

            AddHandler deleteProduct.Confirm, AddressOf DeleteProductConfirmation_Closed
            Dim parentWindow As Window = Window.GetWindow(Me)
            PopupHelper.OpenPopupWithControl(sender, deleteProduct, "windowcenter", -100, 0, False, parentWindow)
        End Sub

        Private Sub DeleteProductConfirmation_Closed()
            Dim VariationQuery As String = "SELECT productVariation FROM product WHERE productID = @productID"
            Dim SerialQuery As String = "SELECT COUNT(*) FROM serialnumberproduct WHERE productID = @productID"

            Dim hasVariation As Boolean
            Dim hasSerials As Boolean


            'Handles the deletion of product
            Dim connStr As String = SplashScreen.GetDatabaseConnection().ConnectionString
            Try
                Using conn As New MySqlConnection(connStr)
                    conn.Open()
                    Using VariationCmd As New MySqlCommand(VariationQuery, conn)
                        VariationCmd.Parameters.AddWithValue("@productID", cacheDeleteProductID)
                        Dim result = VariationCmd.ExecuteScalar()
                        If result IsNot Nothing Then
                            hasVariation = Convert.ToBoolean(result)
                        End If
                    End Using

                    Using SerialCmd As New MySqlCommand(SerialQuery, conn)
                        SerialCmd.Parameters.AddWithValue("@productID", cacheDeleteProductID)
                        Dim serialCount As Integer = Convert.ToInt32(SerialCmd.ExecuteScalar())
                        hasSerials = (serialCount > 0)
                    End Using
                End Using

                'Checks if the product has variation
                If hasVariation Then
                    MessageBox.Show("Cannot delete product with variations yet.")
                Else

                    ' If no variations, delete from productnovariation and product tables
                    UpdateProduct.DeleteProductData("productnovariation", cacheDeleteProductID)

                    'Checks if has serials
                    If hasSerials Then
                        UpdateProduct.DeleteProductData("serialnumberproduct", cacheDeleteProductID)
                    End If
                    UpdateProduct.DeleteProductData("product", cacheDeleteProductID)
                End If

                MessageBox.Show("Product Deleted Successfully!")
                LoadData()
            Catch ex As Exception
                MessageBox.Show("Error deleting product: " & ex.Message)
            End Try
        End Sub


    End Class
End Namespace