Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models
Imports System.IO

Namespace DPC.Data.Controllers
    Public Class GetProduct

        'Call this on comboboxes to get brands
        Public Shared Sub GetBrands(comboBox As ComboBox)
            Dim query As String = "
                SELECT brandID, brandName
                FROM brand
                ORDER BY brandName ASC;
                "

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            comboBox.Items.Clear()
                            While reader.Read()
                                Dim brandName As String = reader("brandName").ToString()
                                Dim brandId As Integer = Convert.ToInt32(reader("brandID"))
                                Dim item As New ComboBoxItem With {
                        .Content = brandName,
                        .Tag = brandId
                    }
                                comboBox.Items.Add(item)
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}")
                End Try
            End Using
        End Sub
        'Call this on comboboxes to get Suppliers based on the selected brand
        Public Shared Sub GetSuppliersByBrand(brandID As Integer, comboBox As ComboBox)
            Dim query As String = "
        SELECT s.supplierID, s.supplierName
        FROM supplier s
        INNER JOIN supplierbrand sb ON s.supplierID = sb.supplierID
        WHERE sb.brandID = @brandID
        ORDER BY s.supplierName ASC;
    "

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@brandID", brandID)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            comboBox.Items.Clear()

                            If reader.HasRows Then
                                While reader.Read()
                                    Dim supplierName As String = reader("supplierName").ToString().Trim()
                                    Dim supplierID As String = reader("supplierID").ToString()

                                    Dim item As New ComboBoxItem With {
                            .Content = supplierName,
                            .Tag = supplierID
                        }
                                    comboBox.Items.Add(item)
                                End While
                                comboBox.SelectedIndex = 0
                            Else
                                'MessageBox.Show("No suppliers found for the selected brand.", "Information", MessageBoxButton.OK, MessageBoxImage.Information)
                                comboBox.Items.Clear()
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
        End Sub

        'Call this on comboboxes to get categories
        Public Shared Sub GetProductCategory(comboBox As ComboBox)
            Dim query As String = "
                SELECT * FROM category ORDER BY categoryName ASC;
                "

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            comboBox.Items.Clear()
                            While reader.Read()
                                Dim categoryName As String = reader("categoryName").ToString()
                                Dim categoryId As Integer = Convert.ToInt32(reader("categoryID"))
                                Dim item As New ComboBoxItem With {
                        .Content = categoryName,
                        .Tag = categoryId
                    }
                                comboBox.Items.Add(item)
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}")
                End Try
            End Using
        End Sub

        'Call this on comboboxes to get subcategories based on the selectet category
        Public Shared Sub GetProductSubcategory(categoryName As String, comboBox As ComboBox, label As TextBlock, stackPanel As StackPanel)
            ' SQL query to get subcategoryID and subcategoryName for the given category
            Dim query As String = "SELECT subcategoryID, subcategoryName FROM subcategory WHERE categoryID = (SELECT categoryID FROM category WHERE LOWER(categoryName) = LOWER(@categoryName))"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@categoryName", categoryName)

                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            comboBox.Items.Clear()

                            If reader.HasRows Then
                                While reader.Read()
                                    Dim subcategoryName As String = reader("subcategoryName").ToString()
                                    Dim subcategoryId As Integer = Convert.ToInt32(reader("subcategoryID"))

                                    ' Create ComboBoxItem for each subcategory
                                    Dim item As New ComboBoxItem With {
                            .Content = subcategoryName,
                            .Tag = subcategoryId
                        }

                                    comboBox.Items.Add(item)
                                End While

                                ' Show UI elements if subcategories exist
                                label.Visibility = Visibility.Visible
                                comboBox.Visibility = Visibility.Visible
                                stackPanel.Visibility = Visibility.Visible

                                ' Optionally, set default selected item (first subcategory)
                                If comboBox.Items.Count > 0 Then
                                    comboBox.SelectedIndex = 0
                                End If
                            Else
                                ' Hide UI elements if no subcategories found
                                comboBox.Visibility = Visibility.Collapsed
                                label.Visibility = Visibility.Collapsed
                                stackPanel.Visibility = Visibility.Collapsed
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
        End Sub

        'Call this on comboboxes to get warehouses
        Public Shared Sub GetWarehouse(comboBox As ComboBox)
            Dim query As String = "SELECT warehouseID, warehouseName FROM warehouse ORDER BY warehouseName ASC"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            comboBox.Items.Clear()

                            If reader.HasRows Then
                                While reader.Read()
                                    Dim warehouseName As String = reader("warehouseName").ToString().Trim()
                                    Dim warehouseId As Integer = Convert.ToInt32(reader("warehouseID"))

                                    Dim item As New ComboBoxItem With {
                            .Content = warehouseName,
                            .Tag = warehouseId
                        }
                                    comboBox.Items.Add(item)
                                End While
                                comboBox.SelectedIndex = 0 ' Select the first item by default
                            Else
                                MessageBox.Show($"Error: No Warehouses Available!")
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}")
                End Try
            End Using
        End Sub

        ' In the ProductController class
        Public Shared Function SearchProductsBySupplier(supplierID As String, searchText As String) As ObservableCollection(Of ProductDataModel)
            Dim products As New ObservableCollection(Of ProductDataModel)

            ' In a real implementation, this would query the database
            ' Example implementation:
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim query As String = "SELECT p.productID, p.productName, pnv.buyingPrice, pnv.defaultTax, pnv.stockUnit " &
                                  "FROM product p " &
                                  "LEFT JOIN productnovariation pnv ON p.productID = pnv.productID " &
                                  "WHERE p.supplierID = @supplierID AND p.productName LIKE @searchPattern " &
                                  "ORDER BY p.productName"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@supplierID", supplierID)
                        cmd.Parameters.AddWithValue("@searchPattern", "%" & searchText & "%")

                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim product As New ProductDataModel() With {
                            .ProductID = reader("productID").ToString(),
                            .ProductName = reader("productName").ToString(),
                            .BuyingPrice = CDec(reader("buyingPrice")),
                            .DefaultTax = CDec(reader("defaultTax")),
                            .StockUnits = CInt(reader("stockUnit")),
                            .SupplierID = supplierID
                        }
                                products.Add(product)
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                ' Log exception
                Debug.WriteLine("Error searching products: " & ex.Message)
            End Try

            Return products
        End Function

        Public Shared Sub LoadProductData(dataGrid As DataGrid)
            ' Query to load data with products grouped by productID, excluding warehouses with zero stock
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

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Dim adapter As New MySqlDataAdapter(query, conn)
                    Dim table As New DataTable()
                    adapter.Fill(table)

                    ' Add column for Image display
                    If Not table.Columns.Contains("ImageSource") Then
                        table.Columns.Add("ImageSource", GetType(BitmapImage))
                    End If

                    ' Calculate total counts for status cards
                    Dim totalProducts As Integer = table.Rows.Count
                    Dim inStockProducts As Integer = 0
                    Dim stockOutProducts As Integer = 0

                    ' Process each row to create the proper image source and count products by stock status
                    For Each row As DataRow In table.Rows
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

                    ' Set the data source for the DataGrid
                    dataGrid.ItemsSource = table.DefaultView

                    ' Update status cards with counts
                    UpdateStatusCards(inStockProducts, stockOutProducts, totalProducts)

                Catch ex As Exception
                    MessageBox.Show($"Error loading data: {ex.Message}")
                End Try
            End Using
        End Sub

        ' Helper method to update the status cards with product counts
        Private Shared Sub UpdateStatusCards(inStock As Integer, stockOut As Integer, total As Integer)
            ' Since we need to update UI elements from a different class, we should define
            ' and raise an event for the UI to listen to, or implement a callback mechanism

            ' Example of event-based approach:
            'RaiseEvent ProductStatsUpdated(inStock, stockOut, total)

            ' Note: You'll need to define this event at the class level:
            ' Public Shared Event ProductStatsUpdated(inStock As Integer, stockOut As Integer, total As Integer)

            ' And then handle it in the ManageProducts class to update the TextBlocks
        End Sub


    End Class
End Namespace