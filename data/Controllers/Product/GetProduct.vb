Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models

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

        Public Shared Sub LoadProductData(dataGrid As DataGrid)
            ' Query to load data from the appropriate product table based on productVariation flag
            Dim query As String = "
                            -- For products without variations
                            SELECT 
                                p.productID AS ID,
                                p.productName AS Name,
                                c.categoryName AS Category,
                                sc.subcategoryName AS SubCategory,
                                b.brandName AS Brand,
                                s.supplierName AS Supplier,
                                pnv.warehouseID AS Warehouse,
                                pnv.stockUnit AS StockQuantity,
                                p.productImage AS ProductImage -- Fetching the productImage
                            FROM product p
                            LEFT JOIN category c ON p.categoryID = c.categoryID
                            LEFT JOIN subcategory sc ON p.subcategoryID = sc.subcategoryID
                            LEFT JOIN brand b ON p.brandID = b.brandID
                            LEFT JOIN supplier s ON p.supplierID = s.supplierID
                            LEFT JOIN productnovariation pnv ON p.productID = pnv.productID
                            WHERE p.productVariation = 0

                            UNION

                            -- For products with variations
                            SELECT 
                                p.productID AS ID,
                                p.productName AS Name,
                                c.categoryName AS Category,
                                sc.subcategoryName AS SubCategory,
                                b.brandName AS Brand,
                                s.supplierName AS Supplier,
                                pvs.optionCombination AS Warehouse,  -- This can represent the specific variation's option combination
                                pvs.stockUnit AS StockQuantity,
                                p.productImage AS ProductImage -- Fetching the productImage
                            FROM product p
                            LEFT JOIN category c ON p.categoryID = c.categoryID
                            LEFT JOIN subcategory sc ON p.subcategoryID = sc.subcategoryID
                            LEFT JOIN brand b ON p.brandID = b.brandID
                            LEFT JOIN supplier s ON p.supplierID = s.supplierID
                            LEFT JOIN productvariation pv ON p.productID = pv.productID
                            LEFT JOIN productvariationstock pvs ON pv.productID = pvs.productID
                            WHERE p.productVariation = 1;
                        "

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Dim adapter As New MySqlDataAdapter(query, conn)
                    Dim table As New DataTable()
                    adapter.Fill(table)

                    ' Add a column for Total Products manually if not exists
                    If Not table.Columns.Contains("TotalProducts") Then
                        table.Columns.Add("TotalProducts", GetType(Integer))
                    End If

                    ' Add a column to hold the image (if necessary)
                    If Not table.Columns.Contains("ImageSource") Then
                        table.Columns.Add("ImageSource", GetType(Byte()))
                    End If

                    ' Assuming we are marking the products with variation flag as 1
                    For Each row As DataRow In table.Rows
                        row("TotalProducts") = 1 ' Flag for variation

                        ' Fetch and process the image from the productImage column
                        If row("ProductImage") IsNot DBNull.Value Then
                            ' Convert the base64 string to byte array and store it in the ImageSource column
                            Dim base64String As String = row("ProductImage").ToString()
                            Try
                                ' Decode the base64 string to bytes and store them
                                Dim imageBytes As Byte() = Convert.FromBase64String(base64String)
                                row("ImageSource") = imageBytes ' You can store the image as a byte array or convert to base64 string
                            Catch ex As Exception
                                ' Handle any base64 decoding errors here
                                MessageBox.Show($"Error decoding image: {ex.Message}")
                            End Try
                        End If
                    Next

                    dataGrid.ItemsSource = table.DefaultView
                Catch ex As Exception
                    MessageBox.Show($"Error loading data: {ex.Message}")
                End Try
            End Using
        End Sub

    End Class
End Namespace