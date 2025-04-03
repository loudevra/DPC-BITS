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
        INNER JOIN SupplierBrand sb ON s.supplierID = sb.supplierID
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
                                Dim subcategories As New List(Of Subcategory)()

                                While reader.Read()
                                    ' Create Subcategory object for each record
                                    Dim subcategory As New Subcategory() With {
.subcategoryID = Convert.ToInt32(reader("subcategoryID")),
                                .subcategoryName = reader("subcategoryName").ToString().Trim()
                            }

                                    ' Add subcategory to the list
                                    subcategories.Add(subcategory)
                                End While

                                If subcategories.Count > 0 Then
                                    label.Visibility = Visibility.Visible
                                    comboBox.Visibility = Visibility.Visible
                                    stackPanel.Visibility = Visibility.Visible

                                    ' Add subcategory items to ComboBox
                                    For Each subcategory In subcategories
                                        comboBox.Items.Add(New ComboBoxItem With {.Content = subcategory.subcategoryName, .Tag = subcategory.subcategoryID})
                                    Next

                                    ' Optionally, set default selected item (first subcategory)
                                    comboBox.SelectedIndex = 0
                                Else
                                    comboBox.Visibility = Visibility.Collapsed
                                    label.Visibility = Visibility.Collapsed
                                    comboBox.SelectedItem = Nothing
                                    stackPanel.Visibility = Visibility.Collapsed
                                End If
                            Else
                                comboBox.Visibility = Visibility.Collapsed
                                label.Visibility = Visibility.Collapsed
                                comboBox.SelectedItem = Nothing
                                stackPanel.Visibility = Visibility.Collapsed
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}")
                End Try
            End Using
        End Sub

        Public Shared Sub GetWarehouse(comboBox As ComboBox)
            Dim query As String = "SELECT * FROM warehouse ORDER BY warehouseName ASC"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            comboBox.Items.Clear()

                            If reader.HasRows Then
                                While reader.Read()
                                    Dim warehouseName As String = reader("warehouseName").ToString().Trim()
                                    comboBox.Items.Add(New ComboBoxItem With {.Content = warehouseName})
                                End While
                                comboBox.SelectedIndex = 0 ' Set first item as selected
                            Else
                                comboBox.Items.Add(New ComboBoxItem With {.Content = "No Warehouses Available"})
                                comboBox.SelectedIndex = 0
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
            Dim query As String = "SELECT MAX(CAST(SUBSTRING(ProductCode, 11, 4) AS UNSIGNED)) FROM storedproduct " &
                  "WHERE ProductCode LIKE '20" & datePart & "%'"

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
        Public Shared Sub InsertNewProduct(Toggle As System.Windows.Controls.Primitives.ToggleButton, Checkbox As Controls.CheckBox,
                            ProductName As TextBox, Category As ComboBox, SubCategory As ComboBox, Warehouse As ComboBox,
                            Brand As ComboBox, Supplier As ComboBox,
                            RetailPrice As TextBox, PurchaseOrder As TextBox, DefaultTax As TextBox,
                            DiscountRate As TextBox, StockUnits As TextBox, AlertQuantity As TextBox,
                            MeasurementUnit As ComboBox, Description As TextBox, ValidDate As DatePicker,
                            SerialNumbers As List(Of TextBox))

            'Product is not a variation
            If Toggle.IsChecked = False Then
                'Include Serial Numbers
                If Checkbox.IsChecked = True Then
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
                        SerialNumbers.Any(Function(txt) String.IsNullOrWhiteSpace(txt.Text)) Then

                        MessageBox.Show("Please fill in all required fields!", "Input Error", MessageBoxButton.OK)
                        Exit Sub
                    End If

                    'Do Not Include Serial Numbers
                Else
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
                        ValidDate.SelectedDate Is Nothing Then

                        MessageBox.Show("Please fill in all required fields!", "Input Error", MessageBoxButton.OK)
                        Exit Sub
                    End If
                End If
            End If

            Try
                Dim productCode As String = GenerateProductCode()
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Using transaction = conn.BeginTransaction()

                        Dim query1 As String = "INSERT INTO storedproduct 
                         (ProductID, ProductName, ProductCode, Category, SubCategory, Warehouse, BrandID, SupplierID,
                         RetailPrice, PurchaseOrder, DefaultTax, DiscountRate, StockUnits,
                         AlertQuantity, MeasurementUnit, Description, DateAdded)
                         VALUES 
                         (DEFAULT, @ProductName, @ProductCode, @Category, @SubCategory, @Warehouse, @BrandID, @SupplierID,
                         @RetailPrice, @PurchaseOrder, @DefaultTax, @DiscountRate, @StockUnits,
                         @AlertQuantity, @MeasurementUnit, @Description, @DateAdded);"

                        Using cmd1 As New MySqlCommand(query1, conn, transaction)
                            cmd1.Parameters.AddWithValue("@ProductName", ProductName.Text)
                            cmd1.Parameters.AddWithValue("@ProductCode", productCode)

                            Dim selectedCategoryItem As ComboBoxItem = TryCast(Category.SelectedItem, ComboBoxItem)
                            cmd1.Parameters.AddWithValue("@Category", selectedCategoryItem?.Tag)

                            Dim subCategoryText As String = If(SubCategory.SelectedItem IsNot Nothing, CType(SubCategory.SelectedItem, ComboBoxItem).Content.ToString(), "")
                            Dim subCategoryValue As Object = If(String.IsNullOrWhiteSpace(subCategoryText) OrElse subCategoryText.ToLower() = "none", DBNull.Value, subCategoryText)
                            cmd1.Parameters.AddWithValue("@SubCategory", subCategoryValue)

                            cmd1.Parameters.AddWithValue("@Warehouse", CType(Warehouse.SelectedItem, ComboBoxItem).Content.ToString())

                            ' Extract Brand and Supplier IDs
                            Dim brandID As Integer = Convert.ToInt32(TryCast(Brand.SelectedItem, ComboBoxItem)?.Tag)
                            Dim supplierID As String = Convert.ToString(TryCast(Supplier.SelectedItem, ComboBoxItem)?.Tag)

                            cmd1.Parameters.AddWithValue("@BrandID", brandID)
                            cmd1.Parameters.AddWithValue("@SupplierID", supplierID)

                            cmd1.Parameters.AddWithValue("@RetailPrice", RetailPrice.Text)
                            cmd1.Parameters.AddWithValue("@PurchaseOrder", PurchaseOrder.Text)
                            cmd1.Parameters.AddWithValue("@DefaultTax", DefaultTax.Text)
                            cmd1.Parameters.AddWithValue("@DiscountRate", DiscountRate.Text)
                            cmd1.Parameters.AddWithValue("@StockUnits", StockUnits.Text)
                            cmd1.Parameters.AddWithValue("@AlertQuantity", AlertQuantity.Text)
                            cmd1.Parameters.AddWithValue("@MeasurementUnit", CType(MeasurementUnit.SelectedItem, ComboBoxItem).Content.ToString())
                            cmd1.Parameters.AddWithValue("@Description", Description.Text)
                            cmd1.Parameters.AddWithValue("@DateAdded", ValidDate.SelectedDate)

                            ' Execute the query
                            cmd1.ExecuteNonQuery()

                            Dim productIDQuery As String = "SELECT LAST_INSERT_ID();"
                            Using cmdGetID As New MySqlCommand(productIDQuery, conn, transaction)
                                Dim productID As Integer = Convert.ToInt32(cmdGetID.ExecuteScalar())

                                Dim query2 As String = "INSERT INTO serialnumberproduct (SerialNumber, ProductID) VALUES (@SerialNumber, @ProductID)"
                                Using cmd2 As New MySqlCommand(query2, conn, transaction)
                                    cmd2.Parameters.AddWithValue("@ProductID", productID)

                                    For Each serialNumberTextBox As TextBox In SerialNumbers
                                        If Not String.IsNullOrWhiteSpace(serialNumberTextBox.Text) Then
                                            cmd2.Parameters.Clear()
                                            cmd2.Parameters.AddWithValue("@SerialNumber", serialNumberTextBox.Text)
                                            cmd2.Parameters.AddWithValue("@ProductID", productID)
                                            cmd2.ExecuteNonQuery()
                                        End If
                                    Next
                                End Using
                            End Using

                            transaction.Commit()
                            MessageBox.Show($"Product {ProductName.Text} with Product Code {productCode} has been inserted successfully.")
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show($"An error occurred: {ex.Message}")
            End Try
        End Sub
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
            Dim query As String = "SELECT productid AS ID, productname AS Name, stockunits AS StockQuantity, (retailprice * stockunits) AS Action FROM storedproduct;"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Dim adapter As New MySqlDataAdapter(query, conn)
                    Dim table As New DataTable()
                    adapter.Fill(table)

                    ' Calculate Total Products manually since SQL query doesn't return it
                    If Not table.Columns.Contains("TotaProducts") Then
                        table.Columns.Add("TotaProducts", GetType(Integer))
                    End If

                    For Each row As DataRow In table.Rows
                        row("TotaProducts") = 1 ' Since this is a product without variation
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

    End Class

End Namespace
