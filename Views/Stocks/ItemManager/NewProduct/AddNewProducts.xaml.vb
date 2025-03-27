Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Model
Imports MaterialDesignThemes.Wpf.Theme

Namespace DPC.Views.Stocks.ItemManager.NewProduct
    Public Class AddNewProducts
        Inherits Window

        Private productController As New ProductController()
        Private WithEvents AddRowPopoutControl As AddRowPopout
        Private popup As Popup

        Public Sub New()
            InitializeComponent()

            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Content = sidebar

            ' Load Top Navigation Bar
            Dim topNav As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Content = topNav

            Toggle.IsChecked = False
            VariationChecker(Toggle)

            ProductController.GetProductCategory(ComboBoxCategory)

            If ComboBoxCategory.SelectedItem IsNot Nothing Then
                CategoryComboBox_SelectionChanged(ComboBoxCategory, Nothing)
            End If

            ProductController.GetWarehouse(ComboBoxWarehouse)

            Dim calendarViewModel As New CalendarController.SingleCalendar()
            Me.DataContext = calendarViewModel

            ProductController.MainContainer = MainContainer
            ProductController.TxtStockUnits = TxtStockUnits
            'MessageBox.Show("MainContainer and TxtStockUnits have been initialized.")

            ProductController.BtnAddRow_Click(Nothing, Nothing)
        End Sub

        Private Sub Toggle_Click(sender As Object, e As RoutedEventArgs)
            VariationChecker(Toggle)
        End Sub

        Private Sub VariationChecker(Toggle As System.Windows.Controls.Primitives.ToggleButton)
            Try
                If Toggle Is Nothing Then
                    Throw New Exception("ToggleButton is not initialized.")
                End If

                ' Update UI based on IsChecked state
                If Toggle.IsChecked = True Then
                    StackPanelVariation.Visibility = Visibility.Visible
                    StackPanelWarehouse.Visibility = Visibility.Collapsed
                    StackPanelRetailPrice.Visibility = Visibility.Collapsed
                    StackPanelOrderPrice.Visibility = Visibility.Collapsed
                    BorderSeperator.Visibility = Visibility.Collapsed
                    StackPanelTaxRate.Visibility = Visibility.Collapsed
                    StackPanelDiscountRate.Visibility = Visibility.Collapsed
                    StackPanelStockUnits.Visibility = Visibility.Collapsed
                    StackPanelAlertQuantity.Visibility = Visibility.Collapsed
                Else
                    StackPanelVariation.Visibility = Visibility.Collapsed
                    StackPanelWarehouse.Visibility = Visibility.Visible
                    StackPanelRetailPrice.Visibility = Visibility.Visible
                    StackPanelOrderPrice.Visibility = Visibility.Visible
                    BorderSeperator.Visibility = Visibility.Visible
                    StackPanelTaxRate.Visibility = Visibility.Visible
                    StackPanelDiscountRate.Visibility = Visibility.Visible
                    StackPanelStockUnits.Visibility = Visibility.Visible
                    StackPanelAlertQuantity.Visibility = Visibility.Visible
                End If

            Catch ex As Exception
                MessageBox.Show($"Error: {ex.Message}")
            End Try
        End Sub


        ' Start of inserting function for add product button
        Private Sub BtnAddProduct_Click(sender As Object, e As RoutedEventArgs)
            ProductController.InsertNewProduct(
                TxtProductName, TxtProductCode, ComboBoxCategory, ComboBoxSubCategory,
                ComboBoxWarehouse, TxtRetailPrice, TxtPurchaseOrder, TxtDefaultTax,
                TxtDiscountRate, TxtStockUnits, TxtAlertQuantity, ComboBoxMeasurementUnit,
                TxtDescription, SingleDatePicker, ProductController.SerialNumbers)

            ClearInputFields()

            'Current Workload
            Dim ProductName As String = TxtProductCode.Text
            Dim CategoryID As String = ComboBoxCategory.Tag
            Dim newProduct As New Product With {
                .ProductName = ProductName,
                .CategoryID = CategoryID
            }
        End Sub

        Private Sub ClearInputFields()
            ' Clear TextBoxes
            TxtProductName.Clear()
            TxtProductCode.Clear()
            TxtRetailPrice.Clear()
            TxtPurchaseOrder.Clear()
            TxtDefaultTax.Clear()
            TxtDiscountRate.Clear()
            TxtStockUnits.Text = "1"
            TxtAlertQuantity.Clear()
            TxtDescription.Clear()

            ' Reset ComboBoxes to first item (index 0)
            ComboBoxCategory.SelectedIndex = 0
            ComboBoxSubCategory.SelectedIndex = 0
            ComboBoxWarehouse.SelectedIndex = 0
            ComboBoxMeasurementUnit.SelectedIndex = 0

            ' Set DatePicker to current date
            SingleDatePicker.SelectedDate = DateTime.Now

            ' Clear Serial Numbers and reset to one row
            If ProductController.SerialNumbers IsNot Nothing Then
                ProductController.SerialNumbers.Clear()
            End If

            If MainContainer IsNot Nothing Then
                MainContainer.Children.Clear()
            End If

            ' Add back one row for Serial Number input
            ProductController.BtnAddRow_Click(Nothing, Nothing)
            TxtStockUnits.Text = "1"
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
                ProductController.GetProductSubcategory(selectedCategory, ComboBoxSubCategory, SubCategoryLabel, StackPanelSubCategory)
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
            ProductController.BtnAddRow_Click(Nothing, Nothing)
        End Sub

        Private Sub BtnRemoveRow_Click(sender As Object, e As RoutedEventArgs)
            ProductController.BtnRemoveRow_Click(Nothing, Nothing)
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

                        ' Clear SerialNumbers to remove old references
                        ProductController.SerialNumbers.Clear()

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


        Private recentlyClosed As Boolean = False

        Private Sub OpenAddVariation(sender As Object, e As RoutedEventArgs)
            Dim clickedButton As System.Windows.Controls.Button = CType(sender, System.Windows.Controls.Button)
            If clickedButton Is Nothing Then Return

            ' Reuse or create a new Popup if it doesn't exist
            If popup Is Nothing Then
                popup = New Popup With {
            .PlacementTarget = clickedButton,
            .Placement = PlacementMode.Relative,
            .StaysOpen = False,
            .AllowsTransparency = True
        }
            End If

            Dim popOutContent As New DPC.Components.Forms.AddVariation()
            popup.Child = popOutContent

            ' Adjust position once the content is loaded
            AddHandler popOutContent.Loaded, Sub()
                                                 Dim targetElement As FrameworkElement = TryCast(popup.PlacementTarget, FrameworkElement)
                                                 If targetElement IsNot Nothing Then
                                                     ' Center horizontally and position below the parent
                                                     popup.HorizontalOffset = -(popup.Child.DesiredSize.Width - targetElement.ActualWidth) / 2
                                                     popup.VerticalOffset = targetElement.ActualHeight
                                                 End If
                                             End Sub

            ' Handle popup closure
            AddHandler popup.Closed, Sub()
                                         recentlyClosed = True
                                         Task.Delay(100).ContinueWith(Sub() recentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
                                     End Sub

            ' Open the popup
            popup.IsOpen = True
        End Sub
    End Class
End Namespace
