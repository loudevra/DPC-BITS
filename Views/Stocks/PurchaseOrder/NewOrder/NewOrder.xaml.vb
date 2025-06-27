Imports System.Collections.ObjectModel
Imports System.Text.Json.Nodes
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DocumentFormat.OpenXml.Office2010.ExcelAc
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Controllers.CalendarController
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Components.ConfirmationModals
Imports Newtonsoft.Json
Imports NuGet.Protocol.Core.Types

Namespace DPC.Views.Stocks.PurchaseOrder.NewOrder
    Public Class NewOrder

#Region "Initialization and Properties"
        Private namesList, quantityList, rateList, taxRateList, taxList, discountList, priceList, descriptionList As New List(Of String)

        Private rowCount As Integer = 0
        Private MyDynamicGrid As Grid
        Private _suppliers As New ObservableCollection(Of SupplierDataModel)
        Private _selectedSupplier As SupplierDataModel
        Private _typingTimer As DispatcherTimer
        Private _products As New ObservableCollection(Of ProductDataModel)
        Private _selectedProduct As ProductDataModel
        Private _productTypingTimers As New Dictionary(Of String, DispatcherTimer)
        Private _productPopups As New Dictionary(Of String, Popup)
        Private _productListBoxes As New Dictionary(Of String, ListBox)
        Public Property OrderDate As New CalendarController.SingleCalendar()
        Public Property OrderDueDate As New CalendarController.SingleCalendar()
        Public Sub New()
            InitializeComponent()
            OrderDate.SelectedDate = Date.Today
            OrderDueDate.SelectedDate = Date.Today.AddDays(1)

            DataContext = Me

            ' Initialize typing timer for search delay
            _typingTimer = New DispatcherTimer With {
                .Interval = TimeSpan.FromMilliseconds(300)
            }
            AddHandler _typingTimer.Tick, AddressOf OnTypingTimerTick
            AddHandler TxtSupplier.TextChanged, AddressOf TxtSupplier_TextChanged
            AddHandler LstItems.SelectionChanged, AddressOf LstItems_SelectionChanged

            MyDynamicGrid = CType(TableGridPanel.Children(0), Grid)
            AddNewRow()

            ClearFields()


            InvoiceNumber.Text = PurchaseOrderController.GenerateInvoice()
            ProductController.GetWarehouse(ComboBoxWarehouse)
        End Sub
#End Region

#Region "Calendar Controls"
        Private Sub OrderDate_Click(sender As Object, e As RoutedEventArgs)
            OrderDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub OrderDueDate_Click(sender As Object, e As RoutedEventArgs)
            OrderDueDatePicker.IsDropDownOpen = True
        End Sub

#End Region

#Region "Supplier Autocomplete"
        ' ========== Supplier Autocomplete Methods ==========
        Private Sub TxtSupplier_TextChanged(sender As Object, e As TextChangedEventArgs)
            ' Reset the timer
            _typingTimer.Stop()

            ' If text is empty, close popup
            If String.IsNullOrWhiteSpace(TxtSupplier.Text) Then
                AutoCompletePopup.IsOpen = False
                Return
            End If

            ' Start the timer
            _typingTimer.Start()

            HandleSuppliers()
        End Sub

        Private Sub HandleSuppliers()
            ' Configure popup if needed
            If AutoCompletePopup.PlacementTarget Is Nothing Then
                AutoCompletePopup.PlacementTarget = TxtSupplier
                AutoCompletePopup.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom
                AutoCompletePopup.StaysOpen = False
                AutoCompletePopup.AllowsTransparency = True
                AutoCompletePopup.IsOpen = True
            End If
        End Sub


        Private Sub OnTypingTimerTick(sender As Object, e As EventArgs)
            ' Stop the timer
            _typingTimer.Stop()

            ' Search for suppliers
            _suppliers = SupplierController.SearchSuppliers(TxtSupplier.Text)

            ' Update the list
            LstItems.ItemsSource = _suppliers

            ' Show popup if we have results
            AutoCompletePopup.IsOpen = _suppliers.Count > 0

            ' Adjust popup width to match the textbox
            AutoCompletePopup.Width = TxtSupplier.ActualWidth
        End Sub


        Private Sub LstItems_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If LstItems.SelectedItem IsNot Nothing Then
                Dim previousSupplier As SupplierDataModel = _selectedSupplier
                _selectedSupplier = CType(LstItems.SelectedItem, SupplierDataModel)
                TxtSupplier.Text = _selectedSupplier.SupplierName
                UpdateSupplierDetails(_selectedSupplier)
                AutoCompletePopup.IsOpen = False

                ' Clear existing rows and create a fresh one when supplier changes
                If previousSupplier Is Nothing OrElse previousSupplier.SupplierID <> _selectedSupplier.SupplierID Then
                    ClearAllRows()
                    namesList.Clear()
                    quantityList.Clear()
                    rateList.Clear()
                    taxRateList.Clear()
                    taxList.Clear()
                    discountList.Clear()
                    priceList.Clear()
                    AddNewRow()
                End If
            End If
        End Sub




        ' Add a new method to clear all rows
        Private Sub ClearAllRows()
            ' Create a list of row indices to remove
            Dim rowsToRemove As New List(Of Integer)

            ' Collect all row indices
            For i As Integer = 1 To rowCount
                rowsToRemove.Add(i * 2) ' Each item occupies 2 rows (main row + notes row)
            Next

            ' Sort in descending order to avoid index shifting issues when removing
            rowsToRemove.Sort()
            rowsToRemove.Reverse()

            ' Remove each row (starting from the last one)
            For Each rowIndex As Integer In rowsToRemove
                RemoveRow(rowIndex)
            Next

            ' Reset row count
            rowCount = 0
        End Sub

        ' Update supplier details section
        Private Sub UpdateSupplierDetails(supplier As SupplierDataModel)
            ' Find the readonly TextBox for supplier details
            Dim txtSupplierDetails As TextBox = TryCast(FindName("TxtSupplierDetails"), TextBox)
            If txtSupplierDetails IsNot Nothing AndAlso supplier IsNot Nothing Then
                ' Format supplier details
                Dim details As String = $"Name: {supplier.SupplierName}{Environment.NewLine}" &
                                     $"Company: {supplier.SupplierCompany}{Environment.NewLine}" &
                                     $"Contact: {supplier.SupplierPhone}{Environment.NewLine}" &
                                     $"Email: {supplier.SupplierEmail}{Environment.NewLine}" &
                                     $"Address: {supplier.OfficeAddress}, {supplier.City}, {supplier.Region}, " &
                                     $"{supplier.Country} {supplier.PostalCode}{Environment.NewLine}" &
                                     $"TIN: {supplier.TinID}"

                txtSupplierDetails.Text = details
            End If
        End Sub
#End Region

        Private Sub ClearFields()
            StatementDetails.InvoiceNumberCache = Nothing
            StatementDetails.InvoiceDateCache = Nothing
            StatementDetails.DueDateCache = Nothing
            StatementDetails.TaxCache = Nothing
            StatementDetails.TotalCostCache = Nothing
            StatementDetails.OrderItemsCache = Nothing
            StatementDetails.SupplierName = Nothing
            StatementDetails.City = Nothing
            StatementDetails.Region = Nothing
            StatementDetails.Country = Nothing
            StatementDetails.Phone = Nothing
            StatementDetails.Email = Nothing
            _selectedSupplier = Nothing
            _selectedProduct = Nothing
            ClearAllRows()
            namesList.Clear()
            quantityList.Clear()
            rateList.Clear()
            taxRateList.Clear()
            taxList.Clear()
            discountList.Clear()
            priceList.Clear()
            OrderNote.Clear()
            TxtSupplier.Clear()
            TxtSupplierDetails.Clear()
            InvoiceNumber.Clear()
            TotalDiscount.Text = 0
            TotalTax.Text = 0
            TotalPrice.Text = 0
            AddNewRow()
            InvoiceNumber.Text = PurchaseOrderController.GenerateInvoice()
        End Sub

#Region "Product Autocomplete"
        ' Create product popup for a specific row
        Private Function CreateProductAutoCompletePopup(row As Integer) As Popup
            ' Create a ListBox for product items
            Dim lstProducts As New ListBox()
            lstProducts.Name = $"LstProducts_{row}"

            ' Create a ItemTemplate for the ListBox
            Dim template As DataTemplate = CreateProductItemTemplate()
            lstProducts.ItemTemplate = template

            ' Create Border to contain the ListBox
            Dim border As New Border With {
        .Background = Brushes.White,
        .BorderBrush = Brushes.LightGray,
        .BorderThickness = New Thickness(1),
        .MaxHeight = 150
    }
            border.Child = lstProducts

            ' Create Popup
            Dim popup As New Popup With {
        .StaysOpen = False,
        .IsOpen = False,
        .AllowsTransparency = True,
        .PopupAnimation = PopupAnimation.Fade,
        .Child = border
    }

            ' Store references for later use - check if keys already exist first
            Dim popupKey As String = $"ProductPopup_{row}"
            Dim listBoxKey As String = $"LstProducts_{row}"

            ' Remove existing entries if they exist
            If _productPopups.ContainsKey(popupKey) Then
                _productPopups.Remove(popupKey)
            End If

            If _productListBoxes.ContainsKey(listBoxKey) Then
                _productListBoxes.Remove(listBoxKey)
            End If

            ' Now safely add the new entries
            _productPopups.Add(popupKey, popup)
            _productListBoxes.Add(listBoxKey, lstProducts)

            ' Add handlers
            AddHandler lstProducts.SelectionChanged, AddressOf ProductList_SelectionChanged

            Return popup
        End Function

        Private Function CreateProductItemTemplate() As DataTemplate
            ' Create a DataTemplate in code
            Dim template As New DataTemplate()

            ' Create the root FrameworkElementFactory
            Dim stackPanelFactory As New FrameworkElementFactory(GetType(StackPanel))
            stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal)
            stackPanelFactory.SetValue(StackPanel.MarginProperty, New Thickness(5))

            ' Add product ID and name TextBlock
            Dim productTextFactory As New FrameworkElementFactory(GetType(TextBlock))
            ' Use a multi-binding to combine ProductID and ProductName
            Dim multiBinding As New MultiBinding()
            multiBinding.StringFormat = "{0} - {1}" ' Format as "ProductID - ProductName"

            ' Add the ProductID binding
            Dim idBinding As New Binding("ProductID")
            multiBinding.Bindings.Add(idBinding)

            ' Add the ProductName binding
            Dim nameBinding As New Binding("ProductName")
            multiBinding.Bindings.Add(nameBinding)

            ' Set the combined binding to the Text property
            productTextFactory.SetBinding(TextBlock.TextProperty, multiBinding)
            productTextFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Normal)

            stackPanelFactory.AppendChild(productTextFactory)

            ' Set the root of the DataTemplate
            template.VisualTree = stackPanelFactory

            Return template
        End Function

        ' TextChanged event handler for product TextBox
        Private Sub ProductTextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim textBox As TextBox = CType(sender, TextBox)
            Dim parts As String() = textBox.Name.Split("_"c)
            If parts.Length < 3 Then Return

            Dim row As Integer = parts(1)
            'If Not Integer.TryParse(parts(1), row) Then Return

            Dim timerKey As String = $"ProductTimer_{row}"

            ' If timer doesn't exist for this row, create one
            If Not _productTypingTimers.ContainsKey(timerKey) Then
                Dim timer As New DispatcherTimer With {
                    .Interval = TimeSpan.FromMilliseconds(300)
                }



                ' Add a closure to capture the row
                Dim rowCaptured As Integer = row
                AddHandler timer.Tick, Sub(s, args)
                                           OnProductTypingTimerTick(s, args, rowCaptured)
                                       End Sub

                _productTypingTimers.Add(timerKey, timer)
            End If

            ' Reset and start timer
            _productTypingTimers(timerKey).Stop()

            Dim popupKey As String = $"ProductPopup_{row}"

            ' Close popup if textbox is empty
            If String.IsNullOrWhiteSpace(textBox.Text) Then

                If _productPopups.ContainsKey(popupKey) Then
                    _productPopups(popupKey).IsOpen = False
                End If
                Return
            End If

            ' Start timer
            _productTypingTimers(timerKey).Start()

        End Sub

        ' Typing timer tick handler for product search
        Private Sub OnProductTypingTimerTick(sender As Object, e As EventArgs, row As Integer)


            ' Stop the timer
            Dim timerKey As String = $"ProductTimer_{row}"
            If _productTypingTimers.ContainsKey(timerKey) Then
                _productTypingTimers(timerKey).Stop()
            End If

            ' Get the textbox
            Dim textBoxName As String = $"txt_{row}_0"
            Dim textBox As TextBox = GetTextBoxFromStackPanel(textBoxName)
            Dim rateBoxName As String = $"txt_{row}_2"
            Dim rate As TextBox = GetTextBoxFromStackPanel(rateBoxName)

            If textBox Is Nothing Then Return

            ' Get the popup and listbox
            Dim popupKey As String = $"ProductPopup_{row}"
            Dim listBoxKey As String = $"LstProducts_{row}"

            If Not _productPopups.ContainsKey(popupKey) Or Not _productListBoxes.ContainsKey(listBoxKey) Then Return

            Dim popup As Popup = _productPopups(popupKey)
            Dim listBox As ListBox = _productListBoxes(listBoxKey)
            AddHandler listBox.SelectionChanged, Sub()
                                                     _selectedProduct = listBox.SelectedItem

                                                     If _selectedProduct IsNot Nothing Then
                                                         textBox.Text = _selectedProduct.ProductName
                                                         rate.Text = _selectedProduct.BuyingPrice
                                                     End If
                                                 End Sub

            ' Search for products from the supplier
            If _selectedSupplier IsNot Nothing Then
                ' Call product controller to search products by supplier ID and search text
                _products = PurchaseOrderController.SearchProductsBySupplier(_selectedSupplier.SupplierID, textBox.Text)

                ' Update the ListBox
                listBox.ItemsSource = _products



                ' Show popup if we have results
                If _products IsNot Nothing Then
                    popup.PlacementTarget = textBox
                    popup.IsOpen = _products.Count > 0
                    popup.PlacementTarget = textBox
                    popup.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom
                End If

                CType(popup.Child, Border).Width = textBox.ActualWidth
            End If
        End Sub

        Private Function IsProductAlreadyAdded(productID As String, currentRow As Integer) As Boolean
            Dim isDuplicate As Boolean = False

            ' Loop through all product rows to check if this product exists elsewhere
            For row As Integer = 0 To rowCount - 1
                ' Skip checking the current row
                If row = currentRow Then Continue For

                ' Get the product textbox from this row
                Dim productTextBox As TextBox = GetTextBoxFromBorder($"txt_{row}_0")
                If productTextBox IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(productTextBox.Text) Then
                    ' Check if this textbox contains the same product ID
                    ' The format is usually "ProductID - ProductName"
                    If productTextBox.Text.StartsWith(productID & " - ") Then
                        isDuplicate = True
                        Exit For
                    End If
                End If
            Next

            Return isDuplicate
        End Function

        ' Update the ProductList_SelectionChanged method to include duplicate checking
        Private Sub ProductList_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim listBox As ListBox = CType(sender, ListBox)

            If listBox Is Nothing Or listBox.SelectedItem Is Nothing Then Return

            ' Extract the row from the ListBox name
            Dim parts As String() = listBox.Name.Split("_"c)
            If parts.Length < 2 Then Return

            Dim row As Integer
            If Not Integer.TryParse(parts(1), row) Then Return

            ' Get selected product
            _selectedProduct = CType(listBox.SelectedItem, ProductDataModel)

            ' Define popupKey once for reuse
            Dim popupKey As String = $"ProductPopup_{row}"

            ' Check if this product is already added in another row
            If IsProductAlreadyAdded(_selectedProduct.ProductID, row) Then
                ' Show error message
                MessageBox.Show($"The product '{_selectedProduct.ProductName}' is already added to this purchase order.",
                        "Duplicate Product",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning)

                ' Close popup without adding the product
                If _productPopups.ContainsKey(popupKey) Then
                    _productPopups(popupKey).IsOpen = False
                End If

                Return ' Exit the method without updating the row
            End If

            ' Update product details
            UpdateProductRow(row, _selectedProduct)

            ' Close popup
            If _productPopups.ContainsKey(popupKey) Then
                _productPopups(popupKey).IsOpen = False
            End If
        End Sub
        ' Update product row with selected product details
        Private Sub UpdateProductRow(row As Integer, product As ProductDataModel)
            ' Update product name to include the ID
            Dim nameTextBox As TextBox = GetTextBoxFromBorder($"txt_{row}_0")
            If nameTextBox IsNot Nothing Then
                nameTextBox.Text = $"{product.ProductID} - {product.ProductName}"
            End If

            ' Update price/rate
            Dim rateTextBox As TextBox = GetTextBoxFromBorder($"txt_{row}_2")
            If rateTextBox IsNot Nothing AndAlso product.BuyingPrice > 0 Then
                rateTextBox.Text = product.BuyingPrice.ToString("0.00")
            End If

            ' Update tax percentage if available
            Dim taxTextBox As TextBox = GetTextBoxFromBorder($"txt_{row}_3")
            If taxTextBox IsNot Nothing AndAlso product.DefaultTax > 0 Then
                taxTextBox.Text = product.DefaultTax.ToString("0.00")
            End If

            ' Set default quantity to 1 if it's empty
            Dim quantityTextBox As TextBox = GetTextBoxFromBorder($"txt_{row}_1")
            If quantityTextBox IsNot Nothing AndAlso String.IsNullOrWhiteSpace(quantityTextBox.Text) Then
                quantityTextBox.Text = "1"
            End If

            ' Trigger calculations
            UpdateTaxAndAmount(row)
        End Sub
#End Region

#Region "Dynamic Grid Manipulation"
        ' ➜ Add a New Row
        Private Sub AddNewRow()
            rowCount += 1
            Dim row1 As New RowDefinition() With {.Height = GridLength.Auto}
            Dim row2 As New RowDefinition() With {.Height = GridLength.Auto}

            MyDynamicGrid.RowDefinitions.Add(row1)
            MyDynamicGrid.RowDefinitions.Add(row2)

            Dim newRowStart As Integer = MyDynamicGrid.RowDefinitions.Count - 2

            ' Create UI Elements (Matches XAML Layout)
            CreateStackPanelWithTextBox(newRowStart, 0, True) ' Item Name with autocomplete
            CreateStackPanelWithTextBox(newRowStart, 1) ' Quantity
            CreateStackPanelWithTextBox(newRowStart, 2) ' Rate
            CreateStackPanelWithTextBox(newRowStart, 3) ' Tax %
            CreateStackPanelWithTextBox(newRowStart, 4) ' Tax (Editable)
            CreateStackPanelWithTextBox(newRowStart, 5) ' Discount
            CreateStackPanelWithTextBox(newRowStart, 6) ' Amount (Editable)
            CreateDeleteButtonStack(newRowStart, 7)
            CreateFullWidthStackPanel(newRowStart)
        End Sub

        ' ➜ Create TextBox wrapped in StackPanel - Updated to support product autocomplete
        Private Sub CreateStackPanelWithTextBox(row As Integer, column As Integer, Optional isProductSearch As Boolean = False)
            Dim txtName As String = $"txt_{row}_{column}"

            ' Check if the name already exists and unregister it
            Dim existingElement As Object = Me.FindName(txtName)
            If existingElement IsNot Nothing Then
                Me.UnregisterName(txtName)
            End If

            ' Create TextBox with adjustments for product name
            Dim txt As New TextBox With {
                .Name = txtName,
                .Style = CType(Me.FindResource("RoundedTextboxStyle"), Style),
                .Margin = New Thickness(2, 0, 0, 0),
                .Background = Brushes.Transparent
            }

            ' For product name field, adjust to allow more text
            If column = 0 Then
                txt.TextWrapping = TextWrapping.NoWrap
                txt.MaxWidth = 500 ' Increase maximum width for product names
                txt.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto ' Enable horizontal scrolling if needed


            End If

            Select Case column
                Case 0
                    namesList.Add(txtName)
                Case 1
                    quantityList.Add(txtName)
                Case 2
                    rateList.Add(txtName)
                Case 3
                    taxRateList.Add(txtName)
                Case 4
                    taxList.Add(txtName)
                Case 5
                    discountList.Add(txtName)
                Case 6
                    priceList.Add(txtName)
            End Select

            ' Create a Border and apply the style
            ' Wrap TextBox inside the Border
            Dim border As New Border With {
                .Style = CType(Me.FindResource("RoundedBorderStyle"), Style),
                .Child = txt
            }

            ' Create StackPanel to contain the Border
            Dim stackPanel As New StackPanel With {
                .Margin = New Thickness(2.5, 10, 2.5, 0)
            }

            stackPanel.Children.Add(border)

            ' Adjust border width for product name column
            If column = 0 Then
                border.MaxWidth = 500 ' Increase max width for the border too
            End If

            ' Attach numeric validation and event handlers
            If column = 1 Or column = 2 Or column = 3 Or column = 4 Or column = 5 Or column = 6 Then
                AddHandler txt.PreviewTextInput, AddressOf ValidateNumericInput
                AddHandler txt.TextChanged, AddressOf TextBoxValueChanged
            End If

            ' If this is the product search field, add autocomplete functionality
            If isProductSearch Then
                ' Create autocomplete popup for this row
                Dim productPopup As Popup = CreateProductAutoCompletePopup(row)

                ' Add it to the grid at same position
                Grid.SetRow(productPopup, row)
                Grid.SetColumn(productPopup, column)
                MyDynamicGrid.Children.Add(productPopup)

                ' Add TextChanged event for autocomplete
                AddHandler txt.TextChanged, AddressOf ProductTextBox_TextChanged
            End If

            ' Set Grid position
            Grid.SetRow(stackPanel, row)
            Grid.SetColumn(stackPanel, column)

            ' Register the StackPanel with the TextBox name for reference
            Me.RegisterName(txtName, stackPanel)

            ' Add to Grid
            MyDynamicGrid.Children.Add(stackPanel)
        End Sub

        Private Sub CreateFullWidthStackPanel(row As Integer)
            Dim txtName As String = $"txt_full_{row}"

            ' Check if the name already exists and unregister it
            Dim existingElement As Object = Me.FindName(txtName)
            If existingElement IsNot Nothing Then
                Me.UnregisterName(txtName)
            End If

            ' Create TextBox
            ' Apply the existing TextBox style
            Dim fullWidthTextBox As New TextBox With {
                .Name = txtName,
                .Style = CType(Me.FindResource("RoundedTextboxStyle"), Style),
                .Margin = New Thickness(2, 0, 0, 0),
                .Background = Brushes.Transparent,
                .Height = 75
            }

            ' Create a Border and apply the existing style
            ' Wrap TextBox inside the Border
            Dim border As New Border With {
                .Style = CType(Me.FindResource("RoundedBorderStyle"), Style),
                .Height = 75,
                .Child = fullWidthTextBox
            }

            ' Create StackPanel to contain the Border
            Dim stackPanel As New StackPanel With {
                .Margin = New Thickness(2.5, 2.5, 2.5, 0)
            }

            stackPanel.Children.Add(border)

            ' Set Grid position
            Grid.SetRow(stackPanel, row + 1) ' Place it in the next row
            Grid.SetColumn(stackPanel, 0)
            Grid.SetColumnSpan(stackPanel, 8) ' Span across all columns

            ' Register the StackPanel
            Me.RegisterName(txtName, stackPanel)

            ' Add to Grid
            MyDynamicGrid.Children.Add(stackPanel)

            descriptionList.Add(txtName)
        End Sub

        ' ➜ Create Delete Button in a StackPanel
        Private Sub CreateDeleteButtonStack(row As Integer, column As Integer)
            ' Create StackPanel container
            Dim stackPanel As New StackPanel()

            ' Create Button
            Dim btn As New Button() With {
                .Style = CType(Me.FindResource("RoundedButtonStyle"), Style),
                .Background = Brushes.Transparent,
                .BorderBrush = Brushes.Transparent
            }

            Dim icon As New MaterialDesignThemes.Wpf.PackIcon() With {
                .Kind = MaterialDesignThemes.Wpf.PackIconKind.PlaylistRemove,
                .Foreground = Brushes.Red,
                .Width = 35,
                .Height = 35,
                .VerticalAlignment = VerticalAlignment.Center,
                .HorizontalAlignment = HorizontalAlignment.Center
            }

            btn.Content = icon

            ' Attach delete functionality
            AddHandler btn.Click, Sub(sender As Object, e As RoutedEventArgs)
                                      Dim parentRow As Integer = Grid.GetRow(CType(sender, Button).Parent)
                                      RemoveRow(parentRow)
                                  End Sub

            ' Add button to stack panel
            stackPanel.Children.Add(btn)

            ' Set Grid position
            Grid.SetRow(stackPanel, row)
            Grid.SetColumn(stackPanel, column)

            ' Add to Grid
            MyDynamicGrid.Children.Add(stackPanel)
        End Sub

        ' Check if entered text is a number
        Private Sub ValidateNumericInput(sender As Object, e As TextCompositionEventArgs)
            Dim allowedPattern As String = "^[0-9.]+$" ' Allow only digits and decimal point
            If Not System.Text.RegularExpressions.Regex.IsMatch(e.Text, allowedPattern) Then
                e.Handled = True ' Reject input if it doesn't match
            End If
        End Sub

        ' Remove Row Functionality - Updated to clean up product autocomplete resources
        Private Sub RemoveRow(row As Integer)
            ' Find all elements in the specified row and the corresponding note row
            Dim elementsToRemove As New List(Of UIElement)

            For Each element As UIElement In MyDynamicGrid.Children
                If Grid.GetRow(element) = row Or Grid.GetRow(element) = row + 1 Then
                    elementsToRemove.Add(element)
                    namesList.Remove($"txt_{row}_0")
                    quantityList.Remove($"txt_{row}_1")
                    priceList.Remove($"txt_{row}_2")
                    taxRateList.Remove($"txt_{row}_3")
                    taxList.Remove($"txt_{row}_4")
                    discountList.Remove($"txt_{row}_5")
                    priceList.Remove($"txt_{row}_6")
                End If
            Next

            ' Clean up product autocomplete resources
            Dim timerKey As String = $"ProductTimer_{row}"
            Dim popupKey As String = $"ProductPopup_{row}"
            Dim listBoxKey As String = $"LstProducts_{row}"

            ' Remove timer
            If _productTypingTimers.ContainsKey(timerKey) Then
                _productTypingTimers(timerKey).Stop()
                _productTypingTimers.Remove(timerKey)
            End If

            ' Remove popup and listbox references
            If _productPopups.ContainsKey(popupKey) Then
                _productPopups.Remove(popupKey)
            End If

            If _productListBoxes.ContainsKey(listBoxKey) Then
                _productListBoxes.Remove(listBoxKey)
            End If

            ' Unregister names and remove elements from the grid
            For Each element As UIElement In elementsToRemove
                If TypeOf element Is StackPanel Then
                    Dim stackPanel As StackPanel = CType(element, StackPanel)
                    If stackPanel.Children.Count > 0 AndAlso TypeOf stackPanel.Children(0) Is Border Then
                        Dim border As Border = CType(stackPanel.Children(0), Border)
                        If border.Child IsNot Nothing AndAlso TypeOf border.Child Is TextBox Then
                            Dim txtBox As TextBox = CType(border.Child, TextBox)
                            If Not String.IsNullOrEmpty(txtBox.Name) AndAlso Me.FindName(txtBox.Name) IsNot Nothing Then
                                Try
                                    Me.UnregisterName(txtBox.Name)
                                Catch ex As ArgumentException
                                    ' Ignore error if the name is already unregistered
                                End Try
                            End If
                        End If
                    End If

                    ' Also try to unregister the stack panel itself if it has a name in our format
                    For i As Integer = 0 To 7 ' Column index
                        Dim txtName As String = $"txt_{row}_{i}"
                        If Me.FindName(txtName) IsNot Nothing Then
                            Try
                                Me.UnregisterName(txtName)
                            Catch ex As ArgumentException
                                ' Ignore error if the name is already unregistered
                            End Try
                        End If
                    Next

                    Dim fullRowTxtName As String = $"txt_full_{row}"
                    If Me.FindName(fullRowTxtName) IsNot Nothing Then
                        Try
                            Me.UnregisterName(fullRowTxtName)
                        Catch ex As ArgumentException
                            ' Ignore error if the name is already unregistered
                        End Try
                    End If
                End If
                MyDynamicGrid.Children.Remove(element)
            Next

            ' Remove the corresponding RowDefinitions (ensure valid index before removing)
            If row < MyDynamicGrid.RowDefinitions.Count Then
                MyDynamicGrid.RowDefinitions.RemoveAt(row)
                If row < MyDynamicGrid.RowDefinitions.Count Then
                    MyDynamicGrid.RowDefinitions.RemoveAt(row) ' Remove note row as well
                End If
            End If

            ' Only adjust remaining rows if this isn't a complete clear operation
            If rowCount > 1 Then
                ' Adjust remaining rows (shift everything above the deleted row)
                Dim elementsToShift As New List(Of UIElement)
                For Each element As UIElement In MyDynamicGrid.Children
                    Dim currentRow As Integer = Grid.GetRow(element)
                    If currentRow > row Then
                        elementsToShift.Add(element)
                    End If
                Next

                For Each element As UIElement In elementsToShift
                    Grid.SetRow(element, Grid.GetRow(element) - 2)
                Next
            End If

            ' Reduce row count
            If rowCount > 0 Then
                rowCount -= 1
            End If

            ' Update totals if any rows remain
            If rowCount > 0 Then
                UpdateOrderTotals()
            End If
        End Sub

        ' Function to retrieve the TextBox from a StackPanel > Border structure
        Private Function GetTextBoxFromStackPanel(stackPanelName As String) As TextBox
            Dim stackPanel As StackPanel = TryCast(Me.FindName(stackPanelName), StackPanel)
            If stackPanel IsNot Nothing AndAlso stackPanel.Children.Count > 0 AndAlso TypeOf stackPanel.Children(0) Is Border Then
                Dim border As Border = CType(stackPanel.Children(0), Border)
                If border.Child IsNot Nothing AndAlso TypeOf border.Child Is TextBox Then
                    Return CType(border.Child, TextBox)
                End If
            End If
            Return Nothing
        End Function

#End Region

#Region "Calculations and Updates"
        Private Sub UpdateOrderTotals()
            ' Calculate and update subtotal, tax, discount, and grand total
            Dim subtotal As Decimal = 0
            Dim taxTotal As Decimal = 0
            Dim discountTotal As Decimal = 0

            ' Loop through all rows and sum up amounts
            For row As Integer = 0 To rowCount - 1
                Dim amountTxt As TextBox = GetTextBoxFromBorder($"txt_{row}_6")
                Dim taxTxt As TextBox = GetTextBoxFromBorder($"txt_{row}_4")
                Dim discountTxt As TextBox = GetTextBoxFromBorder($"txt_{row}_5")

                Dim amount As Decimal = 0
                Dim tax As Decimal = 0
                Dim discount As Decimal = 0

                If amountTxt IsNot Nothing AndAlso Decimal.TryParse(amountTxt.Text, amount) Then
                    subtotal += amount
                End If

                If taxTxt IsNot Nothing AndAlso Decimal.TryParse(taxTxt.Text, tax) Then
                    taxTotal += tax
                End If

                If discountTxt IsNot Nothing AndAlso Decimal.TryParse(discountTxt.Text, discount) Then
                    discountTotal += discount
                End If
            Next
        End Sub
        ' Function to retrieve the TextBox from a Border element
        Private Function GetTextBoxFromBorder(borderName As String) As TextBox
            Dim border As Border = TryCast(Me.FindName(borderName), Border)
            If border IsNot Nothing AndAlso TypeOf border.Child Is TextBox Then
                Return CType(border.Child, TextBox)
            End If
            Return Nothing
        End Function


        ' Update helper method references to use GetTextBoxFromStackPanel instead of GetTextBoxFromBorder
        Private Sub UpdateTaxAndAmount(row As Integer)
            ' Get references to the required textboxes inside their StackPanel > Border structure
            Dim quantityTxt As TextBox = GetTextBoxFromStackPanel($"txt_{row}_1")
            Dim rateTxt As TextBox = GetTextBoxFromStackPanel($"txt_{row}_2")
            Dim taxPercentTxt As TextBox = GetTextBoxFromStackPanel($"txt_{row}_3")
            Dim taxTxt As TextBox = GetTextBoxFromStackPanel($"txt_{row}_4")
            Dim discountTxt As TextBox = GetTextBoxFromStackPanel($"txt_{row}_5")
            Dim amountTxt As TextBox = GetTextBoxFromStackPanel($"txt_{row}_6")

            ' Ensure all fields have valid numeric input
            Dim quantity As Integer = If(quantityTxt IsNot Nothing AndAlso Integer.TryParse(quantityTxt.Text, quantity), quantity, 0)
            Dim rate As Single = If(rateTxt IsNot Nothing AndAlso Single.TryParse(rateTxt.Text, rate), rate, 0)
            Dim taxPercent As Single = If(taxPercentTxt IsNot Nothing AndAlso Single.TryParse(taxPercentTxt.Text, taxPercent), taxPercent, 0)
            Dim discount As Single = If(discountTxt IsNot Nothing AndAlso Single.TryParse(discountTxt.Text, discount), discount, 0)

            ' Calculate subtotal (Quantity × Rate)
            Dim subtotal As Single = quantity * rate

            ' Compute tax (Tax % of subtotal)
            Dim taxAmount As Single = subtotal * (taxPercent / 100)

            ' Update the Tax field dynamically
            If taxTxt IsNot Nothing Then
                taxTxt.Text = taxAmount.ToString("0.00")
            End If

            ' Calculate total amount (Subtotal + Tax - Discount)
            Dim totalAmount As Single = subtotal + taxAmount - discount

            ' Ensure the total amount is not negative
            If totalAmount < 0 Then totalAmount = 0

            ' Update the Amount field dynamically
            If amountTxt IsNot Nothing Then
                amountTxt.Text = totalAmount.ToString("0.00")
            End If

            ' Update Total Tax
            TotalTax.Text = 0
            Dim taxTotal As Double = Double.Parse(TotalTax.Text)

            For Each list In taxList
                Dim tax As TextBox = GetTextBoxFromStackPanel($"{list}")

                If String.IsNullOrWhiteSpace(tax.Text) Then
                    tax.Text = 0.00
                End If

                Dim taxAdded As Double = Double.Parse(tax.Text)

                If tax IsNot Nothing Then
                    taxTotal = taxTotal + taxAdded

                    TotalTax.Text = taxTotal
                End If
            Next


            ' Update Total Discount
            TotalDiscount.Text = 0
            Dim discountTotal As Double = Double.Parse(TotalDiscount.Text)

            For Each list In discountList
                Dim discountAmount As TextBox = GetTextBoxFromStackPanel($"{list}")

                If String.IsNullOrWhiteSpace(discountAmount.Text) Then
                    discountAmount.Text = 0.00
                End If

                Dim discountAdded As Double = Double.Parse(discountAmount.Text)

                If discountAmount IsNot Nothing Then
                    discountTotal = discountTotal + discountAdded

                    TotalDiscount.Text = discountTotal
                End If
            Next


            ' Update Total Price
            TotalPrice.Text = 0
            Dim priceTotal As Double = Double.Parse(TotalPrice.Text)

            For Each list In priceList
                Dim price As TextBox = GetTextBoxFromStackPanel($"{list}")

                If String.IsNullOrWhiteSpace(price.Text) Then
                    price.Text = 0.00
                End If

                Dim priceAdded As Double = Double.Parse(price.Text)

                If price IsNot Nothing Then
                    priceTotal = priceTotal + priceAdded

                    TotalPrice.Text = priceTotal
                End If
            Next
        End Sub

        Private Sub TextBoxValueChanged(sender As Object, e As TextChangedEventArgs)
            Dim txt As TextBox = CType(sender, TextBox)

            ' Extract row number from textbox name (e.g., "txt_2_1" → row 2)
            Dim parts As String() = txt.Name.Split("_"c)
            If parts.Length < 3 Then Exit Sub

            Dim row As Integer
            If Integer.TryParse(parts(1), row) Then
                UpdateTaxAndAmount(row) ' Call function to update tax and amount
            End If
        End Sub
#End Region

#Region "Event Handlers"
        Private Sub BtnAddRow_Click(sender As Object, e As RoutedEventArgs) Handles btnAddRow.Click
            AddNewRow()
        End Sub

        Private Sub BtnAddSupplier_Click(sender As Object, e As RoutedEventArgs) Handles BtnAddSupplier.Click
            ViewLoader.DynamicView.NavigateToView("newsuppliers", Me)
        End Sub

        Private Sub BtnGenerateOrder_Click(sender As Object, e As RoutedEventArgs) Handles btnGenerateOrder.Click
            ' Validate required fields
            If _selectedSupplier Is Nothing Then
                MessageBox.Show("Please select a supplier for this order.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim confirmation As New ConfirmPurchaseOrder()
            confirmation.ShowDialog()

            If confirmation.UserConfirmed Then
                ConfirmOrder()
            End If

        End Sub

        Private Sub ConfirmOrder()
            ' Add your order generation code here
            ' This would include collecting all the data and saving to database

            ' Example implementation:
            Try
                ' Generate JSON for items
                Dim itemArray As New List(Of Dictionary(Of String, String))


                For i As Integer = 0 To namesList.Count - 1
                    Dim itemDetails As New Dictionary(Of String, String)

                    Dim name As TextBox = GetTextBoxFromStackPanel($"{namesList(i)}")
                    If name IsNot Nothing Then
                        itemDetails.Add("ItemName", name.Text)
                    End If

                    Dim quantity As TextBox = GetTextBoxFromStackPanel($"{quantityList(i)}")
                    If quantity IsNot Nothing Then
                        itemDetails.Add("Quantity", quantity.Text)
                    End If

                    Dim rate As TextBox = GetTextBoxFromStackPanel($"{rateList(i)}")
                    If rate IsNot Nothing Then
                        itemDetails.Add("Rate", rate.Text)
                    End If

                    Dim taxRate As TextBox = GetTextBoxFromStackPanel($"{taxRateList(i)}")
                    If taxRate IsNot Nothing Then
                        itemDetails.Add("TaxRate", taxRate.Text)
                    End If

                    Dim tax As TextBox = GetTextBoxFromStackPanel($"{taxList(i)}")
                    If tax IsNot Nothing Then
                        itemDetails.Add("Tax", tax.Text)
                    End If

                    Dim discount As TextBox = GetTextBoxFromStackPanel($"{discountList(i)}")
                    If discount IsNot Nothing Then
                        itemDetails.Add("Discount", discount.Text)
                    End If

                    Dim price As TextBox = GetTextBoxFromStackPanel($"{priceList(i)}")
                    If price IsNot Nothing Then
                        itemDetails.Add("Price", price.Text)
                    End If

                    Dim description As TextBox = GetTextBoxFromStackPanel($"{descriptionList(i + 1)}")
                    If description IsNot Nothing Then
                        itemDetails.Add("Description", description.Text)
                    End If

                    itemArray.Add(itemDetails)

                Next


                Dim itemsJSON As String = JsonConvert.SerializeObject(itemArray, Formatting.Indented)

                Dim selectedItem As ComboBoxItem = CType(ComboBoxWarehouse.SelectedItem, ComboBoxItem)
                Dim warehouseID As Integer = selectedItem.Tag

                Dim orderSelectedDate As Date = OrderDatePicker.SelectedDate
                Dim dueSelectedDate As Date = OrderDueDatePicker.SelectedDate

                Dim orderDate As String = orderSelectedDate.ToString("yyyy-MM-dd")
                Dim dueDate As String = dueSelectedDate.ToString("yyyy-MM-dd")

                Dim isSuccess = PurchaseOrderController.InsertPurchaseOrder(InvoiceNumber.Text, orderDate, dueDate, Tax.Text, Discount.Text,
                                                            _selectedSupplier.SupplierID, _selectedSupplier.SupplierName, warehouseID, ComboBoxWarehouse.Text, itemsJSON, TotalPrice.Text, TotalTax.Text, TotalDiscount.Text, OrderNote.Text)

                If isSuccess Then
                    MessageBox.Show("Purchase order created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

                    'Clears fields only after successfully adding

                    Dim TaxNumber As Decimal = TotalTax.Text
                    Dim TotalCostNumber As Decimal = TotalPrice.Text

                    Dim TaxFormatted As String = TaxNumber.ToString("N2")
                    Dim TotalFormatted As String = TotalCostNumber.ToString("N2")

                    StatementDetails.InvoiceNumberCache = InvoiceNumber.Text
                    StatementDetails.InvoiceDateCache = Date.Now.ToString("M/d/yyyy")
                    StatementDetails.DueDateCache = dueSelectedDate.ToString("M/d/yyyy")
                    StatementDetails.TaxCache = TaxFormatted
                    StatementDetails.TotalCostCache = TotalFormatted
                    StatementDetails.OrderItemsCache = itemArray
                    StatementDetails.SupplierName = _selectedSupplier.SupplierName
                    StatementDetails.City = _selectedSupplier.City
                    StatementDetails.Region = _selectedSupplier.Region
                    StatementDetails.Country = _selectedSupplier.Country
                    StatementDetails.Phone = _selectedSupplier.SupplierPhone
                    StatementDetails.Email = _selectedSupplier.SupplierEmail


                    ViewLoader.DynamicView.NavigateToView("purchaseorderstatement", Me)
                End If
            Catch ex As Exception
                MessageBox.Show($"Error creating purchase order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
#End Region
    End Class
End Namespace