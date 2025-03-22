Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web

Namespace DPC.Data.Controllers
    Public Class ProductController

        Public Shared Sub GetProductCategory(comboBox As ComboBox)
            Dim query As String = "
    SELECT TRIM(CONCAT(
        IF(category = 'fdas', 
           UPPER(category), 
           IF(CHAR_LENGTH(SUBSTRING_INDEX(category, ' ', 1)) <= 3,
              UPPER(SUBSTRING_INDEX(category, ' ', 1)),
              CONCAT(UPPER(LEFT(SUBSTRING_INDEX(category, ' ', 1), 1)),
                     LOWER(SUBSTRING(SUBSTRING_INDEX(category, ' ', 1), 2)))
           )
        ),
        IF(LOCATE(' ', category) > 0, CONCAT(' ',
            IF(category = 'fdas', 
               UPPER(category),
               IF(CHAR_LENGTH(SUBSTRING_INDEX(category, ' ', -1)) <= 3,
                  UPPER(SUBSTRING_INDEX(category, ' ', -1)),
                  CONCAT(UPPER(LEFT(SUBSTRING_INDEX(category, ' ', -1), 1)),
                         LOWER(SUBSTRING(SUBSTRING_INDEX(category, ' ', -1), 2)))
               )
            )
        ), '')
    )) AS category
    FROM productcategory
    ORDER BY category ASC;
    "

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            comboBox.Items.Clear()
                            While reader.Read()
                                Dim categoryName As String = reader("category").ToString()
                                comboBox.Items.Add(New ComboBoxItem With {.Content = categoryName})
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}")
                End Try
            End Using
        End Sub

        Public Shared Sub GetProductSubcategory(categoryName As String, comboBox As ComboBox, label As TextBlock)
            Dim query As String = "SELECT subcategory FROM productcategory WHERE LOWER(category) = LOWER(@categoryName)"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@categoryName", categoryName)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            comboBox.Items.Clear()

                            If reader.Read() Then
                                Dim subcategoryData As String = reader("subcategory").ToString().Trim()

                                ' Remove unwanted characters and check for empty data
                                subcategoryData = subcategoryData.Replace("""", "").Replace("[", "").Replace("]", "").Trim()

                                If String.IsNullOrWhiteSpace(subcategoryData) Then
                                    comboBox.Visibility = Visibility.Collapsed
                                    label.Visibility = Visibility.Collapsed
                                    comboBox.SelectedIndex = -1
                                Else
                                    label.Visibility = Visibility.Visible
                                    comboBox.Visibility = Visibility.Visible

                                    ' Format and add subcategories to ComboBox
                                    Dim subcategories As String() = subcategoryData.Split(","c).
                                                Select(Function(s) StrConv(s.Trim(), VbStrConv.ProperCase)).
                                                ToArray()

                                    For Each subcategory As String In subcategories
                                        comboBox.Items.Add(New ComboBoxItem With {.Content = subcategory})
                                    Next

                                    comboBox.SelectedIndex = 0
                                End If
                            Else
                                comboBox.Visibility = Visibility.Collapsed
                                label.Visibility = Visibility.Collapsed
                                comboBox.SelectedIndex = -1
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}")
                End Try
            End Using
        End Sub

        Public Shared Sub GetWarehouse(comboBox As ComboBox)
            Dim query As String = "SELECT name FROM warehouse ORDER BY name ASC"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            comboBox.Items.Clear()

                            If reader.HasRows Then
                                While reader.Read()
                                    Dim warehouseName As String = reader("name").ToString().Trim()
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

        Public Shared Sub InsertNewProduct(ProductName As TextBox, ProductCode As TextBox,
                                    Category As ComboBox, SubCategory As ComboBox, Warehouse As ComboBox,
                                    RetailPrice As TextBox, PurchaseOrder As TextBox, DefaultTax As TextBox,
                                    DiscountRate As TextBox, StockUnits As TextBox, AlertQuantity As TextBox,
                                    MeasurementUnit As ComboBox, Description As TextBox, ValidDate As DatePicker,
                                    SerialNumbers As List(Of TextBox))

            ' Validate required fields
            If String.IsNullOrWhiteSpace(ProductName.Text) OrElse
               String.IsNullOrWhiteSpace(ProductCode.Text) OrElse
               String.IsNullOrWhiteSpace(Category.Text) OrElse
               String.IsNullOrWhiteSpace(SubCategory.Text) OrElse
               String.IsNullOrWhiteSpace(Warehouse.Text) OrElse
               String.IsNullOrWhiteSpace(RetailPrice.Text) OrElse
               String.IsNullOrWhiteSpace(PurchaseOrder.Text) OrElse
               String.IsNullOrWhiteSpace(DefaultTax.Text) OrElse
               String.IsNullOrWhiteSpace(DiscountRate.Text) OrElse
               String.IsNullOrWhiteSpace(StockUnits.Text) OrElse
               String.IsNullOrWhiteSpace(AlertQuantity.Text) OrElse
               String.IsNullOrWhiteSpace(MeasurementUnit.Text) OrElse
               String.IsNullOrWhiteSpace(Description.Text) OrElse
               String.IsNullOrWhiteSpace(ValidDate.Text) OrElse
               SerialNumbers.Any(Function(txt) String.IsNullOrWhiteSpace(txt.Text)) Then

                MessageBox.Show("Please fill in all required fields!", "Input Error", MessageBoxButton.OK)
                Exit Sub
            End If

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Using transaction = conn.BeginTransaction()
                        ' Insert into storedproduct table
                        Dim query1 As String = "INSERT INTO storedproduct 
                                                (ProductID, ProductName, ProductCode, Category, SubCategory, Warehouse,
                                                RetailPrice, PurchaseOrder, DefaultTax, DiscountRate, StockUnits,
                                                AlertQuantity, MeasurementUnit, Description, DateAdded)
                                                VALUES 
                                                (DEFAULT, @ProductName, @ProductCode, @Category, @SubCategory,
                                                @Warehouse, @RetailPrice, @PurchaseOrder, @DefaultTax, @DiscountRate,
                                                @StockUnits, @AlertQuantity, @MeasurementUnit, @Description, @DateAdded);"

                        Using cmd1 As New MySqlCommand(query1, conn, transaction)
                            cmd1.Parameters.AddWithValue("@ProductName", ProductName.Text)
                            cmd1.Parameters.AddWithValue("@ProductCode", ProductCode.Text)
                            cmd1.Parameters.AddWithValue("@Category", Category.Text)
                            cmd1.Parameters.AddWithValue("@SubCategory", SubCategory.Text)
                            cmd1.Parameters.AddWithValue("@Warehouse", Warehouse.Text)
                            cmd1.Parameters.AddWithValue("@RetailPrice", RetailPrice.Text)
                            cmd1.Parameters.AddWithValue("@PurchaseOrder", PurchaseOrder.Text)
                            cmd1.Parameters.AddWithValue("@DefaultTax", DefaultTax.Text)
                            cmd1.Parameters.AddWithValue("@DiscountRate", DiscountRate.Text)
                            cmd1.Parameters.AddWithValue("@StockUnits", StockUnits.Text)
                            cmd1.Parameters.AddWithValue("@AlertQuantity", AlertQuantity.Text)
                            cmd1.Parameters.AddWithValue("@MeasurementUnit", MeasurementUnit.Text)
                            cmd1.Parameters.AddWithValue("@Description", Description.Text)
                            cmd1.Parameters.AddWithValue("@DateAdded", ValidDate.Text)

                            ' After the first query execution
                            cmd1.ExecuteNonQuery()

                            ' Retrieve the last inserted ProductID
                            Dim productIDQuery As String = "SELECT LAST_INSERT_ID();"
                            Using cmdGetID As New MySqlCommand(productIDQuery, conn, transaction)
                                Dim productID As Integer = Convert.ToInt32(cmdGetID.ExecuteScalar())

                                ' Insert into serialnumberproduct table
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

                                transaction.Commit()
                                MessageBox.Show($"Product {ProductName.Text} has been inserted successfully.")
                            End Using
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show($"An error occurred: {ex.Message}")
            End Try
        End Sub
    End Class

End Namespace
