Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers

Namespace DPC.Views.Stocks.ItemManager.NewProduct
    Public Class AddNewProducts
        Inherits Window

        Private productController As New ProductController()
        Private WithEvents AddRowPopoutControl As AddRowPopout

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

            ProductController.MainContainer = MainContainer
            ProductController.TxtStockUnits = TxtStockUnits
            MessageBox.Show("MainContainer and TxtStockUnits have been initialized.")

            productController.BtnAddRow_Click(Nothing, Nothing)
        End Sub

        ' Start of inserting function for add product button
        Private Sub BtnAddProduct_Click(sender As Object, e As RoutedEventArgs)
            productController.InsertNewProduct(
                TxtProductName, TxtProductCode, ComboBoxCategory, ComboBoxSubCategory,
                ComboBoxWarehouse, TxtRetailPrice, TxtPurchaseOrder, TxtDefaultTax,
                TxtDiscountRate, TxtStockUnits, TxtAlertQuantity, ComboBoxMeasurementUnit,
                TxtDescription, SingleDatePicker, productController.SerialNumbers)
        End Sub

        Private TxtSerialNumber As TextBox

        ' Function to handle integer only input on textboxes
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

        ' Handles the combobox for categories and subcategories
        Private Sub CategoryComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboBoxCategory.SelectionChanged
            Dim selectedCategory As String = TryCast(ComboBoxCategory.SelectedItem, ComboBoxItem)?.Content?.ToString()
            If Not String.IsNullOrEmpty(selectedCategory) Then
                productController.GetProductSubcategory(selectedCategory, ComboBoxSubCategory, SubCategoryLabel)
            Else
                ComboBoxSubCategory.Items.Clear()
            End If
        End Sub

        ' Handles the date picker component
        Public Sub StartDate_Click(sender As Object, e As RoutedEventArgs)
            SingleDatePicker.IsDropDownOpen = True
        End Sub

        ' Handles the serial table components
        Private Sub BtnAddRow_Click(sender As Object, e As RoutedEventArgs)
            productController.BtnAddRow_Click(Nothing, Nothing)
        End Sub

        Private Sub BtnRemoveRow_Click(sender As Object, e As RoutedEventArgs)
            productController.BtnRemoveRow_Click(Nothing, Nothing)
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
