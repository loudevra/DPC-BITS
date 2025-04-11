Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models


Namespace DPC.Data.Controllers
    Public Class ProductController
        Public Shared SerialNumbers As New List(Of TextBox)
        Public Shared Property MainContainer As StackPanel
        Public Shared Property TxtStockUnits As TextBox
        Private Shared popup As Popup
        Private Shared recentlyClosed As Boolean = False

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

        ' Function to generate ProductCode in format 20MMDDYYYYXXXX
        Private Shared Function GenerateProductCode() As String
            Dim prefix As String = "20"
            Dim datePart As String = DateTime.Now.ToString("MMddyyyy") ' MMDDYYYY format
            Dim counter As Integer = GetNextProductCounter(datePart)

            ' Format counter to be 4 digits (e.g., 0001, 0025, 0150)
            Dim counterPart As String = counter.ToString("D4")

            ' Concatenate to get full ProductCode
            Return prefix & datePart & counterPart
        End Function
        ' Function to get the next Product counter (last 4 digits) with reset condition
        Private Shared Function GetNextProductCounter(datePart As String) As Integer
            Dim query As String = "SELECT MAX(CAST(SUBSTRING(productID, 11, 4) AS UNSIGNED)) FROM product " &
                  "WHERE productID LIKE '20" & datePart & "%'"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Dim result As Object = cmd.ExecuteScalar()

                        ' If no previous records exist for today, start with 0001
                        If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                            Return Convert.ToInt32(result) + 1
                        Else
                            Return 1
                        End If
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error generating Product Code: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return 1
                End Try
            End Using
        End Function

        ' Add Row Function
        Public Shared Sub BtnAddRow_Click(sender As Object, e As RoutedEventArgs, Optional skipStockUpdate As Boolean = False)
            If MainContainer Is Nothing OrElse TxtStockUnits Is Nothing Then
                MessageBox.Show("MainContainer or TxtStockUnits is not initialized.")
                Return
            End If

            Dim outerStackPanel As New StackPanel With {.Margin = New Thickness(25, 20, 10, 20)}

            Dim grid As New Grid()
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

            ' Access the named component using Application.Current.TryFindResource
            Dim textBox As New TextBox With {.Style = CType(Application.Current.TryFindResource("RoundedTextboxStyle"), Style)}
            SerialNumbers.Add(textBox)

            Dim textBoxBorder As New Border With {.Style = CType(Application.Current.TryFindResource("RoundedBorderStyle"), Style), .Child = textBox}
            Grid.SetColumn(textBoxBorder, 0)
            grid.Children.Add(textBoxBorder)

            Dim buttonPanel As New StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(10, 0, 0, 0)}

            ' Add Row Button
            Dim addRowButton As New Button With {.Background = Brushes.White, .BorderThickness = New Thickness(0), .Name = "BtnAddRow"}
            Dim addIcon As New MaterialDesignThemes.Wpf.PackIcon With {.Kind = MaterialDesignThemes.Wpf.PackIconKind.TableRowAddAfter, .Width = 40, .Height = 30, .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#456B2E"))}
            addRowButton.Content = addIcon
            AddHandler addRowButton.Click, Sub(s, ev) BtnAddRow_Click(s, ev)
            buttonPanel.Children.Add(addRowButton)

            ' Remove Row Button
            Dim removeRowButton As New Button With {.Background = Brushes.White, .BorderThickness = New Thickness(0), .Name = "BtnRemoveRow"}
            Dim removeIcon As New MaterialDesignThemes.Wpf.PackIcon With {.Kind = MaterialDesignThemes.Wpf.PackIconKind.TableRowRemove, .Width = 40, .Height = 30, .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636"))}
            removeRowButton.Content = removeIcon
            AddHandler removeRowButton.Click, Sub(s, ev) BtnRemoveRow_Click(s, ev)
            buttonPanel.Children.Add(removeRowButton)

            ' Separator Border
            Dim separatorBorder As New Border With {.BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE")), .BorderThickness = New Thickness(1), .Height = 30}
            buttonPanel.Children.Add(separatorBorder)

            ' Row Controller Button
            Dim rowControllerButton As New Button With {.Background = Brushes.White, .BorderThickness = New Thickness(0), .Name = "BtnRowController"}
            Dim menuIcon As New MaterialDesignThemes.Wpf.PackIcon With {.Kind = MaterialDesignThemes.Wpf.PackIconKind.MenuDown, .Width = 30, .Height = 30, .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE"))}
            rowControllerButton.Content = menuIcon
            AddHandler rowControllerButton.Click, AddressOf BtnRowController_Click
            buttonPanel.Children.Add(rowControllerButton)

            Grid.SetColumn(buttonPanel, 1)
            grid.Children.Add(buttonPanel)
            outerStackPanel.Children.Add(grid)

            MainContainer.Children.Add(outerStackPanel)

            ' Update TxtStockUnits value only if not skipped
            If Not skipStockUpdate Then
                Dim currentValue As Integer
                If Integer.TryParse(TxtStockUnits.Text, currentValue) Then
                    TxtStockUnits.Text = (currentValue + 1).ToString()
                Else
                    TxtStockUnits.Text = "1"
                End If
            End If
        End Sub

        ' Remove Row Function
        Public Shared Sub BtnRemoveRow_Click(sender As Object, e As RoutedEventArgs)
            If MainContainer Is Nothing OrElse TxtStockUnits Is Nothing Then
                MessageBox.Show("MainContainer or TxtStockUnits is not initialized.")
                Return
            End If

            Dim button As Button = CType(sender, Button)
            Dim parentGrid As Grid = FindParentGrid(button)

            If parentGrid IsNot Nothing Then
                Dim parentStackPanel As StackPanel = TryCast(parentGrid.Parent, StackPanel)

                ' Check the current value of TxtStockUnits
                Dim currentValue As Integer
                If Integer.TryParse(TxtStockUnits.Text, currentValue) Then
                    ' If the current value is 1, stop execution
                    If currentValue = 1 Then Return
                End If

                ' Find the TextBox inside the grid and remove it from SerialNumbers
                Dim serialTextBox As TextBox = TryCast(parentGrid.Children.OfType(Of TextBox)().FirstOrDefault(), TextBox)
                If serialTextBox IsNot Nothing AndAlso SerialNumbers.Contains(serialTextBox) Then
                    SerialNumbers.Remove(serialTextBox)
                End If

                ' Remove the row from MainContainer
                If parentStackPanel IsNot Nothing AndAlso MainContainer.Children.Contains(parentStackPanel) Then
                    MainContainer.Children.Remove(parentStackPanel)

                    ' Update TxtStockUnits only if the value is greater than 1
                    TxtStockUnits.Text = Math.Max(currentValue - 1, 0).ToString()
                End If
            End If
        End Sub

        ' Helper function to find the parent grid
        Private Shared Function FindParentGrid(element As DependencyObject) As Grid
            While element IsNot Nothing AndAlso Not (TypeOf element Is Grid)
                element = VisualTreeHelper.GetParent(element)
            End While
            Return TryCast(element, Grid)
        End Function
        ' Row Controller Handler (Placeholder)
        Public Shared Sub BtnRowController_Click(sender As Object, e As RoutedEventArgs)
            Dim clickedButton As Button = TryCast(sender, Button)
            If clickedButton Is Nothing Then Return

            ' Prevent reopening if the popup was just closed
            If recentlyClosed Then
                recentlyClosed = False
                Return
            End If

            ' If the popup exists and is open, close it
            If popup IsNot Nothing AndAlso popup.IsOpen Then
                popup.IsOpen = False
                recentlyClosed = True
                Return
            End If

            ' Ensure the popup is only created once
            popup = New Popup With {
            .PlacementTarget = clickedButton,
            .Placement = PlacementMode.Bottom,
            .StaysOpen = False,
            .AllowsTransparency = True
        }

            Dim popOutContent As New DPC.Components.Forms.RowControllerPopout()
            popup.Child = popOutContent

            ' Handle popup closure
            AddHandler popup.Closed, Sub()
                                         recentlyClosed = True
                                         Task.Delay(100).ContinueWith(Sub() recentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
                                     End Sub

            ' Open the popup
            popup.IsOpen = True
        End Sub
        ' Remove Latest Row Function
        Public Shared Sub RemoveLatestRow()
            Dim parentPanel As StackPanel = MainContainer

            ' Check if there is only one row left
            If parentPanel IsNot Nothing AndAlso parentPanel.Children.Count = 1 Then
                MessageBox.Show("Cannot remove the last remaining row.")
                Return
            End If

            ' Proceed to remove the latest row if there are multiple rows
            If parentPanel IsNot Nothing AndAlso parentPanel.Children.Count > 0 Then
                ' Find the latest row
                Dim latestStackPanel As StackPanel = TryCast(parentPanel.Children(parentPanel.Children.Count - 1), StackPanel)

                ' Remove the TextBox from SerialNumbers if it exists
                If latestStackPanel IsNot Nothing Then
                    Dim latestTextBox As TextBox = latestStackPanel.Children.OfType(Of TextBox)().FirstOrDefault()
                    If latestTextBox IsNot Nothing AndAlso SerialNumbers.Contains(latestTextBox) Then
                        SerialNumbers.Remove(latestTextBox)
                    End If
                End If

                ' Remove the row
                parentPanel.Children.RemoveAt(parentPanel.Children.Count - 1)
            Else
                MessageBox.Show("No rows available to remove.")
            End If
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


        Public Shared Sub ImportSerialNumbers_Click()
            Dim openFileDialog As New Microsoft.Win32.OpenFileDialog With {
                .Filter = "CSV Files (*.csv)|*.csv"
            }


            If openFileDialog.ShowDialog() = True Then
                Try
                    Dim filePath As String = openFileDialog.FileName
                    Dim lines As List(Of String) = IO.File.ReadAllLines(filePath).ToList()

                    ' Ensure there are serial numbers available
                    If lines.Count <= 1 Then
                        MessageBox.Show("No serial numbers found in the file.")
                        Return
                    End If

                    ' Remove the first line (title "Serial Number") and read the rest
                    lines.RemoveAt(0)

                    ' Clear existing rows if any
                    MainContainer.Children.Clear()
                    SerialNumbers.Clear()

                    ' Process serial numbers
                    Dim serialNumbersData As New List(Of String)
                    For Each line In lines
                        Dim serialNumber As String = line.Trim()
                        If Not String.IsNullOrWhiteSpace(serialNumber) Then
                            serialNumbersData.Add(serialNumber)
                        End If
                    Next

                    If serialNumbersData.Any() Then
                        ' Update stock units once
                        TxtStockUnits.Text = serialNumbersData.Count.ToString()

                        ' Dynamically add rows for each serial number
                        For Each serialNumber In serialNumbersData
                            BtnAddRow_Click(Nothing, Nothing, True)
                            If SerialNumbers.Count > 0 Then
                                SerialNumbers.Last().Text = serialNumber
                            End If
                        Next

                        MessageBox.Show($"Successfully imported {serialNumbersData.Count} serial numbers.")
                    Else
                        MessageBox.Show("No valid serial numbers found.")
                    End If

                Catch ex As Exception
                    MessageBox.Show($"Error importing serial numbers: {ex.Message}")
                End Try
            End If
        End Sub

        Private Shared Function ValidateProductFields(Checkbox As Controls.CheckBox, ProductName As TextBox, Category As ComboBox,
                                              SubCategory As ComboBox, Warehouse As ComboBox, Brand As ComboBox,
                                              Supplier As ComboBox, RetailPrice As TextBox, PurchaseOrder As TextBox,
                                              DefaultTax As TextBox, DiscountRate As TextBox, StockUnits As TextBox,
                                              AlertQuantity As TextBox, MeasurementUnit As ComboBox, Description As TextBox,
                                              ValidDate As DatePicker, SerialNumbers As List(Of TextBox)) As Boolean

            ' Check if any of the required fields are empty (except SubCategory, which can be Nothing)
            If String.IsNullOrWhiteSpace(ProductName.Text) OrElse
       Category.SelectedItem Is Nothing OrElse
       Warehouse.SelectedItem Is Nothing OrElse
       Brand.SelectedItem Is Nothing OrElse
       Supplier.SelectedItem Is Nothing OrElse
       String.IsNullOrWhiteSpace(RetailPrice.Text) OrElse
       String.IsNullOrWhiteSpace(PurchaseOrder.Text) OrElse
       String.IsNullOrWhiteSpace(DefaultTax.Text) OrElse
       String.IsNullOrWhiteSpace(DiscountRate.Text) OrElse
       String.IsNullOrWhiteSpace(StockUnits.Text) OrElse
       String.IsNullOrWhiteSpace(AlertQuantity.Text) OrElse
       MeasurementUnit.SelectedItem Is Nothing OrElse
       String.IsNullOrWhiteSpace(Description.Text) OrElse
       ValidDate.SelectedDate Is Nothing OrElse
       (Checkbox.IsChecked = True AndAlso SerialNumbers.Any(Function(txt) String.IsNullOrWhiteSpace(txt.Text))) Then
                Return False
            End If

            ' ✅ If SubCategory is Nothing, set it to 0 when saving later
            Return True
        End Function


        Public Shared Sub InsertNewProduct(Toggle As System.Windows.Controls.Primitives.ToggleButton, Checkbox As Controls.CheckBox,
    ProductName As TextBox, Category As ComboBox, SubCategory As ComboBox, Warehouse As ComboBox,
    Brand As ComboBox, Supplier As ComboBox,
    RetailPrice As TextBox, PurchaseOrder As TextBox, DefaultTax As TextBox,
    DiscountRate As TextBox, StockUnits As TextBox, AlertQuantity As TextBox,
    MeasurementUnit As ComboBox, Description As TextBox, ValidDate As DatePicker,
    SerialNumbers As List(Of TextBox), ProductImage As String)

            ' Determine if the product is a variation
            Dim variation As Integer = If(Toggle.IsChecked = True, 1, 0)

            ' Validate required fields
            If Not ValidateProductFields(Checkbox, ProductName, Category, SubCategory, Warehouse, Brand, Supplier,
                  RetailPrice, PurchaseOrder, DefaultTax, DiscountRate, StockUnits,
                  AlertQuantity, MeasurementUnit, Description, ValidDate, SerialNumbers) Then
                MessageBox.Show("Please fill in all required fields!", "Input Error", MessageBoxButton.OK)
                Exit Sub
            End If

            ' Generate product ID
            Dim productID As String = GenerateProductCode()

            ' ✅ Handle SubCategory when it's Nothing
            Dim subCategoryId As Integer = If(SubCategory.SelectedItem IsNot Nothing, CType(SubCategory.SelectedItem, ComboBoxItem).Tag, 0)

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                conn.Open()
                Using transaction = conn.BeginTransaction()
                    ' Insert into product table first
                    Dim productQuery As String = "INSERT INTO product (productID, productName, categoryID, subcategoryID, supplierID, brandID, dateCreated, productVariation, productImage, measurementUnit, productDescription) 
                                          VALUES (@productID, @ProductName, @Category, @SubCategory, @SupplierID, @BrandID, @DateCreated, @variation, @ProductImage, @Description, @MeasurementUnit);"
                    Using productCmd As New MySqlCommand(productQuery, conn, transaction)
                        productCmd.Parameters.AddWithValue("@productID", productID)
                        productCmd.Parameters.AddWithValue("@ProductName", ProductName.Text)
                        productCmd.Parameters.AddWithValue("@Category", CType(Category.SelectedItem, ComboBoxItem).Tag)
                        productCmd.Parameters.AddWithValue("@SubCategory", subCategoryId) ' ✅ Now using 0 if Nothing
                        productCmd.Parameters.AddWithValue("@SupplierID", CType(Supplier.SelectedItem, ComboBoxItem).Tag)
                        productCmd.Parameters.AddWithValue("@BrandID", CType(Brand.SelectedItem, ComboBoxItem).Tag)
                        productCmd.Parameters.AddWithValue("@DateCreated", ValidDate.SelectedDate)
                        productCmd.Parameters.AddWithValue("@variation", variation)
                        productCmd.Parameters.AddWithValue("@ProductImage", ProductImage)
                        productCmd.Parameters.AddWithValue("@Description", Description.Text)
                        productCmd.Parameters.AddWithValue("@MeasurementUnit", CType(MeasurementUnit.SelectedItem, ComboBoxItem).Tag)
                        productCmd.ExecuteNonQuery()
                    End Using

                    ' Call the appropriate insertion function based on variation flag
                    If variation = 0 Then
                        InsertNonVariationProduct(conn, transaction, productID, Warehouse, RetailPrice, PurchaseOrder, DefaultTax,
                  DiscountRate, StockUnits, AlertQuantity, ValidDate, SerialNumbers, Checkbox)
                    Else
                        InsertVariationProduct(conn, transaction, productID)
                    End If

                    transaction.Commit()
                    MessageBox.Show($"Product {ProductName.Text} with Product Code {productID} has been inserted successfully.")
                End Using
            End Using
        End Sub

        'no variation insert
        Private Shared Sub InsertNonVariationProduct(conn As MySqlConnection, transaction As MySqlTransaction, productID As String,
                                     Warehouse As ComboBox, SellingPrice As TextBox, BuyingPrice As TextBox,
                                     DefaultTax As TextBox, DiscountRate As TextBox,
                                     StockUnits As TextBox, AlertQuantity As TextBox,
                                     ValidDate As DatePicker, SerialNumbers As List(Of TextBox),
                                     Checkbox As Controls.CheckBox)

            ' Insert into productnovariation table
            Dim query As String = "INSERT INTO productnovariation (productID, warehouseID, sellingPrice, buyingPrice, defaultTax, taxType, 
                                discountRate, discountType, stockUnit, alertQuantity, dateCreated, dateModified) 
                           VALUES (@productID, @WarehouseID, @SellingPrice, @BuyingPrice, @DefaultTax, NULL, 
                                @DiscountRate, NULL, @StockUnits, @AlertQuantity, @DateCreated, NULL);"
            Using cmd As New MySqlCommand(query, conn, transaction)
                cmd.Parameters.AddWithValue("@productID", productID)
                cmd.Parameters.AddWithValue("@WarehouseID", CType(Warehouse.SelectedItem, ComboBoxItem).Tag)
                cmd.Parameters.AddWithValue("@SellingPrice", SellingPrice.Text)
                cmd.Parameters.AddWithValue("@BuyingPrice", BuyingPrice.Text)
                cmd.Parameters.AddWithValue("@DefaultTax", DefaultTax.Text)
                cmd.Parameters.AddWithValue("@DiscountRate", DiscountRate.Text)
                cmd.Parameters.AddWithValue("@StockUnits", StockUnits.Text)
                cmd.Parameters.AddWithValue("@AlertQuantity", AlertQuantity.Text)
                cmd.Parameters.AddWithValue("@DateCreated", ValidDate.SelectedDate)
                cmd.ExecuteNonQuery()
            End Using

            ' Check if the product has serial numbers and insert them
            If Checkbox.IsChecked = True Then
                InsertSerialNumbersForProduct(conn, transaction, SerialNumbers, productID)
            End If
        End Sub

        'variation insert
        Private Shared Sub InsertVariationProduct(conn As MySqlConnection, transaction As MySqlTransaction, productID As String)

            ' Insert into productvariation table
            Dim query As String = "INSERT INTO productvariation (productID, dateCreated) 
                           VALUES (@productID, @DateCreated);"
            Using cmd As New MySqlCommand(query, conn, transaction)
                cmd.Parameters.AddWithValue("@productID", productID)
                cmd.Parameters.AddWithValue("@DateCreated", DateTime.Now)
                cmd.ExecuteNonQuery()
            End Using
        End Sub

        Private Shared Sub InsertSerialNumbersForProduct(conn As MySqlConnection, transaction As MySqlTransaction,
                                          SerialNumbers As List(Of TextBox), productID As String)
            Dim query As String = "INSERT INTO serialnumberproduct (SerialNumber, ProductID) VALUES (@SerialNumber, @ProductID)"
            Using cmd As New MySqlCommand(query, conn, transaction)
                cmd.Parameters.AddWithValue("@ProductID", productID)

                For Each serialNumberTextBox As TextBox In SerialNumbers
                    If Not String.IsNullOrWhiteSpace(serialNumberTextBox.Text) Then
                        cmd.Parameters.Clear()
                        cmd.Parameters.AddWithValue("@SerialNumber", serialNumberTextBox.Text)
                        cmd.Parameters.AddWithValue("@ProductID", productID)
                        cmd.ExecuteNonQuery()
                    End If
                Next
            End Using
        End Sub

        Public Function GetProductVariations() As List(Of ProductVariation)
            Return DPC.Components.Forms.AddVariation.SavedVariations
        End Function
    End Class
End Namespace
