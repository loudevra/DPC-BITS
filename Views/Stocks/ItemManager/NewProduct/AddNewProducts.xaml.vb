Imports System.Windows
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Components
Imports System.Windows.Controls.Primitives
Imports MySql.Data.MySqlClient
Imports System.Diagnostics.Metrics

Namespace DPC.Views.Stocks.ItemManager.NewProduct
    Public Class AddNewProducts
        Inherits Window

        Public Sub New()
            InitializeComponent()

            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Content = sidebar

            ' Load Top Navigation Bar
            Dim topNav As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Content = topNav

            ProductController.GetProductCategory(ComboBoxCategory)

            If ComboBoxCategory.SelectedItem IsNot Nothing Then
                CategoryComboBox_SelectionChanged(ComboBoxCategory, Nothing)
            End If

            ProductController.GetWarehouse(ComboBoxWarehouse)

            Dim calendarViewModel As New CalendarController.SingleCalendar()
            Me.DataContext = calendarViewModel

            AddHandler BtnRowController.Click, AddressOf BtnRowController_Click
        End Sub

        'start of inserting function for add product button
        Private Sub BtnAddProduct_Click(sender As Object, e As RoutedEventArgs)
            InsertNewProducts()
        End Sub

        Private TxtSerialNumber As TextBox


        'funtion to insert data to the database
        Private Sub InsertNewProducts()
            Dim connectionString As String = "server=localhost;userid=root;password=;database=dpc"

            ' Validate all required fields
            If String.IsNullOrWhiteSpace(TxtProductName.Text) OrElse
String.IsNullOrWhiteSpace(TxtProductCode.Text) OrElse
String.IsNullOrWhiteSpace(ComboBoxCategory.Text) OrElse
String.IsNullOrWhiteSpace(ComboBoxSubCategory.Text) OrElse
String.IsNullOrWhiteSpace(ComboBoxWarehouse.Text) OrElse
String.IsNullOrWhiteSpace(TxtRetailPrice.Text) OrElse
String.IsNullOrWhiteSpace(TxtPurchaseOrder.Text) OrElse
String.IsNullOrWhiteSpace(TxtDefaultTax.Text) OrElse
String.IsNullOrWhiteSpace(TxtDiscountRate.Text) OrElse
String.IsNullOrWhiteSpace(TxtStockUnits.Text) OrElse
String.IsNullOrWhiteSpace(TxtAlertQuantity.Text) OrElse
String.IsNullOrWhiteSpace(ComboBoxMeasurementUnit.Text) OrElse
String.IsNullOrWhiteSpace(TxtDescription.Text) OrElse
String.IsNullOrWhiteSpace(SingleDatePicker.Text) OrElse
String.IsNullOrWhiteSpace(TxtSerialNumber.Text) Then

                MessageBox.Show("Please fill in all required fields!", "Input Error", MessageBoxButton.OK)
                Exit Sub
            End If

            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim transaction As MySqlTransaction = conn.BeginTransaction()
                ' Start transaction

                Try
                    ' Insert into storedproduct table
                    Dim query1 As String = "INSERT INTO storedproduct
         (ProductName, ProductCode, Category, SubCategory, Warehouse,
RetailPrice, PurchaseOrder, DefaultTax, DiscountRate, StockUnits,
AlertQuantity, MeasurementUnit, Description, DateAdded)
         VALUES (@ProductName, @ProductCode, @Category, @SubCategory,
@Warehouse, @RetailPrice, @PurchaseOrder, @DefaultTax, @DiscountRate,
@StockUnits, @AlertQuantity, @MeasurementUnit, @Description,
@DateAdded);
         SELECT LAST_INSERT_ID();"

                    Using cmd1 As New MySqlCommand(query1, conn, transaction)
                        cmd1.Parameters.AddWithValue("@ProductName", TxtProductName.Text)
                        cmd1.Parameters.AddWithValue("@ProductCode", TxtProductCode.Text)
                        cmd1.Parameters.AddWithValue("@Category", ComboBoxCategory.Text)
                        cmd1.Parameters.AddWithValue("@SubCategory", ComboBoxSubCategory.Text)
                        cmd1.Parameters.AddWithValue("@Warehouse", ComboBoxWarehouse.Text)
                        cmd1.Parameters.AddWithValue("@RetailPrice", TxtRetailPrice.Text)
                        cmd1.Parameters.AddWithValue("@PurchaseOrder", TxtPurchaseOrder.Text)
                        cmd1.Parameters.AddWithValue("@DefaultTax", TxtDefaultTax.Text)
                        cmd1.Parameters.AddWithValue("@DiscountRate", TxtDiscountRate.Text)
                        cmd1.Parameters.AddWithValue("@StockUnits", TxtStockUnits.Text)
                        cmd1.Parameters.AddWithValue("@AlertQuantity", TxtAlertQuantity.Text)
                        cmd1.Parameters.AddWithValue("@MeasurementUnit", ComboBoxMeasurementUnit.Text)
                        cmd1.Parameters.AddWithValue("@Description", TxtDescription.Text)
                        cmd1.Parameters.AddWithValue("@DateAdded", SingleDatePicker.Text)

                        Dim productID As Integer = Convert.ToInt32(cmd1.ExecuteScalar()) ' Get the inserted ID

                        ' Insert into serialnumberproduct table
                        Dim query2 As String = "INSERT INTO serialnumberproduct (SerialNumber, ProductID) VALUES (@SerialNumber,
@ProductID)"

                        Using cmd2 As New MySqlCommand(query2, conn, transaction)
                            cmd2.Parameters.AddWithValue("@SerialNumber", TxtSerialNumber.Text)
                            cmd2.Parameters.AddWithValue("@ProductID",
productID) ' Link to the stored product
                            cmd2.ExecuteNonQuery()
                        End Using
                    End Using

                    ' Commit Transaction if both inserts succeed
                    transaction.Commit()

                    MessageBox.Show("Product and Serial Number added
successfully!", "Success", MessageBoxButton.OK)

                Catch ex As MySqlException
                    transaction.Rollback() ' Rollback transaction if an error occurs
                    MessageBox.Show("Database Error: " & ex.Message, "Error",
MessageBoxButton.OK)

                Catch ex As Exception
                    transaction.Rollback() ' Ensure rollback on any failure
                    MessageBox.Show("Unexpected Error: " & ex.Message,
"Error", MessageBoxButton.OK)
                End Try
            End Using
        End Sub

        'Function to handle integer only input on textboxes
        Private Sub IntegerOnlyTextInputHandler(sender As Object, e As TextCompositionEventArgs)
            If Not IsNumeric(e.Text) OrElse Not Integer.TryParse(e.Text, New Integer()) Then
                e.Handled = True ' Block non-integer input
            End If
        End Sub

        ' Function to handle pasting
        Private Sub IntegerOnlyPasteHandler(sender As Object, e As DataObjectPastingEventArgs)
            If e.DataObject.GetDataPresent(GetType(String)) Then
                Dim pastedText As String = CStr(e.DataObject.GetData(GetType(String)))
                If Not Integer.TryParse(pastedText, New Integer()) Then
                    e.CancelCommand() ' Cancel if pasted data is not an integer
                End If
            Else
                e.CancelCommand()
            End If
        End Sub



        'handles the combobox for categories and subcategories
        Private Sub CategoryComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboBoxCategory.SelectionChanged
            Dim selectedCategory As String = TryCast(ComboBoxCategory.SelectedItem, ComboBoxItem)?.Content?.ToString()
            If Not String.IsNullOrEmpty(selectedCategory) Then
                ProductController.GetProductSubcategory(selectedCategory, ComboBoxSubCategory, SubCategoryLabel)
            Else
                ComboBoxSubCategory.Items.Clear()
            End If
        End Sub

        'handles the date picker componen
        Public Sub StartDate_Click(sender As Object, e As RoutedEventArgs)
            SingleDatePicker.IsDropDownOpen = True
        End Sub


        'handles the serial table components

        Private popup As Popup
        Private recentlyClosed As Boolean = False


        ' Function to handle the Row Controller Click
        Private Sub BtnRowController_Click(sender As Object, e As RoutedEventArgs)
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

            Dim popOutContent As New DPC.Components.Forms.AddRowPopOut()
            popup.Child = popOutContent

            ' Handle popup closure
            AddHandler popup.Closed, Sub()
                                         recentlyClosed = True
                                         Task.Delay(100).ContinueWith(Sub() recentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
                                     End Sub

            ' Open the popup
            popup.IsOpen = True
        End Sub


        Private Sub BtnAddRow_Click(sender As Object, e As RoutedEventArgs) Handles BtnAddRow.Click
            ' Create the outer StackPanel
            Dim outerStackPanel As New StackPanel With {
        .Margin = New Thickness(25, 20, 10, 20)
    }

            ' Create the Grid
            Dim grid As New Grid()
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

            ' Create TextBox with Border
            Dim textBox As New TextBox With {
    .Style = TryFindResource("RoundedTextboxStyle"),
    .Name = "TxtSerialNumber" ' Set the name
}
            TxtSerialNumber = textBox
            Dim textBoxBorder As New Border With {
    .Style = TryFindResource("RoundedBorderStyle"),
    .Child = textBox
}
            Grid.SetColumn(textBoxBorder, 0)
            grid.Children.Add(textBoxBorder)

            ' Create the Button Panel
            Dim buttonPanel As New StackPanel With {
        .Orientation = Orientation.Horizontal,
        .Margin = New Thickness(10, 0, 0, 0)
    }

            ' Add Button for Add Row
            Dim addRowButton As New Button With {
        .Background = Brushes.White,
        .BorderThickness = New Thickness(0),
        .Name = "BtnAddRow"
    }
            Dim addIcon As New MaterialDesignThemes.Wpf.PackIcon With {
        .Kind = MaterialDesignThemes.Wpf.PackIconKind.TableRowAddAfter,
        .Width = 40,
        .Height = 30,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#456B2E"))
    }
            addRowButton.Content = addIcon
            AddHandler addRowButton.Click, AddressOf BtnAddRow_Click
            buttonPanel.Children.Add(addRowButton)

            ' Add Button for Remove Row
            Dim removeRowButton As New Button With {
        .Background = Brushes.White,
        .BorderThickness = New Thickness(0),
        .Name = "BtnRemoveRow"
    }
            Dim removeIcon As New MaterialDesignThemes.Wpf.PackIcon With {
        .Kind = MaterialDesignThemes.Wpf.PackIconKind.TableRowRemove,
        .Width = 40,
        .Height = 30,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636"))
    }
            removeRowButton.Content = removeIcon
            AddHandler removeRowButton.Click, AddressOf BtnRemoveRow_Click
            buttonPanel.Children.Add(removeRowButton)

            ' Add Border between Remove Row and Row Controller
            Dim separatorBorder As New Border With {
        .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE")),
        .BorderThickness = New Thickness(1),
        .Height = 30,
        .VerticalAlignment = VerticalAlignment.Center
    }
            buttonPanel.Children.Add(separatorBorder)

            ' Add Button for Row Controller
            Dim rowControllerButton As New Button With {
        .Background = Brushes.White,
        .BorderThickness = New Thickness(0),
        .Name = "BtnRowController"
    }
            Dim rowControllerIcon As New MaterialDesignThemes.Wpf.PackIcon With {
        .Kind = MaterialDesignThemes.Wpf.PackIconKind.MenuDown,
        .Width = 30,
        .Height = 30,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE"))
    }
            rowControllerButton.Content = rowControllerIcon
            AddHandler rowControllerButton.Click, AddressOf BtnRowController_Click
            buttonPanel.Children.Add(rowControllerButton)

            Grid.SetColumn(buttonPanel, 1)
            grid.Children.Add(buttonPanel)

            ' Add everything to the outer StackPanel
            outerStackPanel.Children.Add(grid)

            ' Append to MainContainer
            MainContainer.Children.Add(outerStackPanel)

            ' Update TxtStockUnits value
            Dim currentValue As Integer
            If Integer.TryParse(TxtStockUnits.Text, currentValue) Then
                TxtStockUnits.Text = (currentValue + 1).ToString()
            Else
                TxtStockUnits.Text = "1"
            End If
        End Sub

        Private Sub BtnRemoveRow_Click(sender As Object, e As RoutedEventArgs)
            ' Find the parent StackPanel (the one representing the row) containing the clicked button
            Dim button As Button = CType(sender, Button)
            Dim parentGrid As Grid = FindParentGrid(button)

            ' Get the outermost StackPanel (MainContainer child)
            If parentGrid IsNot Nothing Then
                Dim parentStackPanel As StackPanel = TryCast(parentGrid.Parent, StackPanel)
                If parentStackPanel IsNot Nothing AndAlso MainContainer.Children.Contains(parentStackPanel) Then
                    MainContainer.Children.Remove(parentStackPanel)
                    ' Update TxtStockUnits value
                    Dim currentValue As Integer
                    If Integer.TryParse(TxtStockUnits.Text, currentValue) Then
                        TxtStockUnits.Text = Math.Max(currentValue - 1, 0).ToString()
                    End If
                End If
            End If
        End Sub


        Private Function FindParentGrid(element As DependencyObject) As Grid
            ' Traverse up the visual tree to find the Grid
            While element IsNot Nothing AndAlso Not TypeOf element Is Grid
                element = VisualTreeHelper.GetParent(element)
            End While
            Return TryCast(element, Grid)
        End Function

        Private Sub TxtStockUnits_KeyDown(sender As Object, e As KeyEventArgs)
            ' Check if Enter key is pressed
            If e.Key = Key.Enter Then
                Dim stockUnits As Integer

                ' Validate if input is a valid number and greater than zero
                If Integer.TryParse(TxtStockUnits.Text, stockUnits) Then
                    If stockUnits > 0 Then
                        ' Clear previous rows
                        MainContainer.Children.Clear()

                        ' Call BtnAddRow_Click the specified number of times
                        For i As Integer = 1 To stockUnits
                            BtnAddRow_Click(Nothing, Nothing)
                        Next

                        ' Ensure the textbox retains the correct value
                        TxtStockUnits.Text = stockUnits.ToString()
                    Else
                        MessageBox.Show("Please enter a number greater than zero.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning)
                    End If
                Else
                    MessageBox.Show("Please enter a valid number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning)
                End If

                ' Prevent further propagation of the event
                e.Handled = True
            End If
        End Sub





    End Class
End Namespace
