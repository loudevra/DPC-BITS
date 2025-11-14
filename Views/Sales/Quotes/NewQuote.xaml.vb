Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DocumentFormat.OpenXml.Bibliography
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Views.Stocks
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json
Imports System.Linq
Imports DocumentFormat.OpenXml.Math

Namespace DPC.Views.Sales.Quotes
    ' Note - The value of defau
    Public Class NewQuote
        ' Autocomplete
        Private rowCount As Integer = 0
        Private MyDynamicGrid As Grid
        ' Autocomplete Popup for clients
        Private _typingTimer As DispatcherTimer
        Private _clients As New ObservableCollection(Of Client)
        Private _selectedClient As Client
        ' Autocomplete Popup for products
        Private _products As New ObservableCollection(Of ProductDataModel)
        Private _selectedProduct As ProductDataModel
        Private _productTypingTimers As New Dictionary(Of String, DispatcherTimer)
        Private _productPopups As New Dictionary(Of String, Popup)
        Private _productListBoxes As New Dictionary(Of String, ListBox)
        Private _productTextBoxes As New Dictionary(Of String, TextBox)
        ' Controllers for calendar to shows the date by binding
        Private OrderDateVM As New CalendarController.SingleCalendar()
        'Private OrderDueDateVM As New CalendarController.SingleCalendar()
        ' Variable to store the data from warehouse
        Public WarehouseID As Integer
        Public WarehouseName As String
        Dim QuantityTotal
        ' Tax Combobox Variables
        Dim _TaxSelection As Boolean
        Dim _SelectedTax As Decimal
        ' Avoid runnng the code of Cost Estimate Type Combobox while loadng the page
        Dim LoadingCEType As Boolean = True
        ' Set a fixed length for Cost Estimate
        Dim _FixedPrefixLength As Integer = 14


#Region "Initializiation once loaded the form"
        Public Sub New()
            InitializeComponent()

            ' For Tax Selection
            If String.IsNullOrWhiteSpace(CEtaxSelection) Then
                _TaxSelection = False
            Else
                _TaxSelection = CBool(CEtaxSelection)
            End If
            CEtaxSelection = _TaxSelection

            ' Set the ComboBox selection to match _TaxSelection
            If _TaxSelection Then
                txtTaxSelection.SelectedItem = txtTaxSelection.Items.Cast(Of ComboBoxItem)().FirstOrDefault(Function(i) i.Content.ToString() = "Exclusive")
            Else
                txtTaxSelection.SelectedItem = txtTaxSelection.Items.Cast(Of ComboBoxItem)().FirstOrDefault(Function(i) i.Content.ToString() = "Inclusive")
            End If

            ' Run the selection changed logic to update all rates/tax fields
            txtTaxSelection_SelectionChanged(txtTaxSelection, Nothing)

            InitializeProductUI()
            rowCount += 1

            ' Set a default date today and tomorrow
            OrderDateVM.SelectedDate = DateTime.Today
            'OrderDueDateVM.SelectedDate = DateTime.Today.AddDays(1)

            ' Set Date to bind
            QuoteDate.DataContext = OrderDateVM
            QuoteDateButton.DataContext = OrderDateVM
            'QuoteValidityDate.DataContext = OrderDueDateVM
            'QuoteValidityButton.DataContext = OrderDueDateVM

            ' Autocomplete part
            _typingTimer = New DispatcherTimer With {
                .Interval = TimeSpan.FromMilliseconds(300)
            }


            ' Hide/Show the button for vat12% if exclusive
            ' Visibility of the Vat Text
            'If CEVatShow Then
            '    ChangeVATColumn.Text = "Hide Vat 12%"
            'Else
            '    ChangeVATColumn.Text = "Show Vat 12%"
            'End If


            AddHandler _typingTimer.Tick, AddressOf OnTypingTimerTick
            AddHandler txtSearchCustomer.TextChanged, AddressOf txtSearchCustomer_TextChanged
            AddHandler LstItems.SelectionChanged, AddressOf LstItems_SelectionChanged

            ' Event for Checking the Quotenumber
            AddHandler txtQuoteNumber.TextChanged, AddressOf txtQuoteNumber_TextChanged

            ' Load warehouse options
            ProductController.GetWarehouse(ComboBoxWarehouse)

            Dim selectedWarehouse As ComboBoxItem = TryCast(ComboBoxWarehouse.SelectedItem, ComboBoxItem)
            If selectedWarehouse IsNot Nothing Then
                CEWarehouseIDCache = Convert.ToInt32(selectedWarehouse.Tag)
                CEWarehouseNameCache = selectedWarehouse.Content.ToString()
            End If

            Console.WriteLine($"Selected Cost Estimate Type : {cmbCostEstimateType.SelectedIndex}")
            ' Checks the value of CEType
            If CostEstimateDetails.CEType > 4 Then
                cmbCostEstimateType.SelectedIndex = 0
                CEType = 0
                ' Generate Quote ID
                Dim quoteID As String = QuotesController.GenerateQuoteID(CEType)
                txtQuoteNumber.Text = quoteID

                Dim _prefix As String

                Select Case CEType
                    Case 0
                        _prefix = "GPCE #:"
                    Case 1
                        _prefix = "BCCE #:"
                    Case 2
                        _prefix = "HHCE #:"
                    Case 3
                        _prefix = "WICE #:"
                    Case Else
                        _prefix = "CE #:" ' Fail Safe if doesnt work
                End Select

                CostEstimateDetails.CECNIndetifier = _prefix

                ' Make the loading CEtype false so it can now run smoothly
                LoadingCEType = False
            Else
                cmbCostEstimateType.SelectedIndex = CostEstimateDetails.CEType
                CEType = CostEstimateDetails.CEType
                ' Generate Quote ID
                Dim quoteID As String = QuotesController.GenerateQuoteID(CEType)
                txtQuoteNumber.Text = quoteID

                Dim _prefix As String

                Select Case CEType
                    Case 0
                        _prefix = "GPCE #:"
                    Case 1
                        _prefix = "BCCE #:"
                    Case 2
                        _prefix = "HHCE #:"
                    Case 3
                        _prefix = "WICE #:"
                    Case Else
                        _prefix = "CE #:" ' Fail Safe if doesnt work
                End Select

                CostEstimateDetails.CECNIndetifier = _prefix

                ' Make the loading CEtype false so it can now run smoothly
                LoadingCEType = False
            End If

            If txtTaxSelection.Text = "Inclusive" Then
                ShowVatExBtn.Visibility = Visibility.Collapsed
            Else
                ShowVatExBtn.Visibility = Visibility.Visible
            End If

            ' Visibility for the Show/Hide VAT 12% button
            VatExShowVat.Text = If(CEisVatExInclude, "Hide VAT 12%", "Show VAT 12%")

            cmbCostEstimateValidty.Text = CostEstimateDetails.CEValidUntilDate

            TaxHeader.Header = If(_TaxSelection, "Tax(%)", "Tax(12%)")

            Debug.WriteLine($"Tax Selection - {_TaxSelection}")
            Debug.WriteLine($"Tax Value In Quote Properties - {_SelectedTax}")
        End Sub
#End Region

#Region "Quote Number Validation & Cost Estimate Validity"
        Private Sub cmbCostEstimateType_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            ' Failsafe for loading
            If LoadingCEType Then Exit Sub

            CostEstimateDetails.CEType = cmbCostEstimateType.SelectedIndex
            Dim _prefix As String

            Select Case CEType
                Case 0
                    _prefix = "GPCE #:"
                Case 1
                    _prefix = "BCCE #:"
                Case 2
                    _prefix = "HHCE #:"
                Case 3
                    _prefix = "WICE #:"
                Case Else
                    _prefix = "CE #:" ' Fail Safe if doesnt work
            End Select

            CostEstimateDetails.CECNIndetifier = _prefix

            ' Generate Quote ID
            Dim quoteID As String = QuotesController.GenerateQuoteID(CEType)
            txtQuoteNumber.Text = quoteID
        End Sub

        Private Sub cmbCostEstimateValidty_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If LoadingCEType Then Exit Sub

            CostEstimateDetails.CEValidUntilDate = cmbCostEstimateValidty.SelectedIndex

            'CostEstimateDetails.CEQuoteValidityDateCache = cmbCostEstimateValidty.Text ' Fail Safe 
            'Console.WriteLine($"Newly Selected Index in Valid Until Date - {CostEstimateDetails.CEValidUntilDate}")
            'Console.WriteLine($"Newly Text in Valid Until Date - {CostEstimateDetails.CEQuoteValidityDateCache}")
        End Sub
#End Region

#Region "Autocomplete for Clients"
        ' Autocomplete Section
        Private Sub txtSearchCustomer_TextChanged(sender As Object, e As TextChangedEventArgs)
            ' Reset the timer
            _typingTimer.Stop()

            ' If text is empty, close popup
            If String.IsNullOrWhiteSpace(txtSearchCustomer.Text) Then
                AutoCompletePopup.IsOpen = False
                Return
            End If

            ' Start the timer
            _typingTimer.Start()
        End Sub

        Private Sub OnTypingTimerTick(sender As Object, e As EventArgs)
            ' Stop the timer
            _typingTimer.Stop()

            ' Search for suppliers
            _clients = ClientController.SearchClient(txtSearchCustomer.Text)

            ' Update the list
            LstItems.ItemsSource = _clients

            ' Show popup if we have results
            AutoCompletePopup.IsOpen = _clients.Count > 0

            ' Adjust popup width to match the textbox
            AutoCompletePopup.Width = txtSearchCustomer.ActualWidth
        End Sub

        Private Sub LstItems_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If LstItems.SelectedItem IsNot Nothing Then
                Dim previousSupplier As Client = _selectedClient
                _selectedClient = CType(LstItems.SelectedItem, Client)
                txtSearchCustomer.Text = _selectedClient.Name
                CEClientIDCache = _selectedClient.ClientID
                UpdateSupplierDetails(_selectedClient)
                AutoCompletePopup.IsOpen = False

                ' Clear existing rows and create a fresh one when supplier changes
                If previousSupplier Is Nothing OrElse previousSupplier.ClientID <> _selectedClient.ClientID Then
                    ClearAllRows()
                End If
            End If
        End Sub

        Private Sub UpdateSupplierDetails(client As Client)
            Dim txtClientDetails As TextBox = TryCast(FindName("TxtClientDetails"), TextBox)
            If txtClientDetails Is Nothing OrElse client Is Nothing Then Return

            Dim details As String =
                    $"Representative Name: {client.Representative}{Environment.NewLine}" &
                    $"Company: {client.Company}{Environment.NewLine}" &
                    $"Contact: {client.Phone}{Environment.NewLine}" &
                    $"Email: {client.Email}{Environment.NewLine}"

            If client.BillingAddress Is Nothing Then
                details &= $"{Environment.NewLine}{Environment.NewLine}Billing Address: (No data)"
            Else
                details &= String.Join(Environment.NewLine, $"Billing Address : {client.BillingAddress}")
            End If

            txtClientDetails.Text = details
        End Sub


        Private Sub ClearAllRows()
            ' Create a list of row indices to remove
            Dim rowsToRemove As New List(Of Integer)

            ' Collect all row indices
            For i As Integer = 0 To rowCount - 1
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


        Private Sub RemoveRow(row As Integer)
            ' Find all elements in the specified row and the corresponding note row
            Dim elementsToRemove As New List(Of UIElement)

            For Each element As UIElement In elementsToRemove
                ' Same logic as before to unregister, etc...
                If TypeOf element Is StackPanel Then
                    ' (same code as your original to clean up textbox names...)
                End If
            Next

            ' Remove elements *after* loop
            For Each element As UIElement In elementsToRemove
                MyDynamicGrid.Children.Remove(element)
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
        End Sub
#End Region

#Region "Navigation of the Forms"
        Private Sub NavigateToCostEstimate(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("costestimate", Me)
        End Sub

        Private Sub NavigateToCostEstimate1(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("costestimate", Me)
        End Sub
#End Region

#Region "Validation and Other Function of the Quote Properties"
        Private Sub QuoteDateButton_Click(sender As Object, e As RoutedEventArgs)
            QuoteDate.IsDropDownOpen = True
        End Sub

        'Private Sub QuoteValidityButton_Click(sender As Object, e As RoutedEventArgs)
        '    QuoteValidityDate.IsDropDownOpen = True
        'End Sub

        Private Sub txtReferenceNumber_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            If Not e.Text.All(AddressOf Char.IsDigit) Then
                e.Handled = True ' block the input
            End If
        End Sub

        Private Sub txtTaxSelection_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            _TaxSelection = CType(txtTaxSelection.SelectedItem, ComboBoxItem).Content.ToString() = "Exclusive"
            Debug.WriteLine($"Tax Selection - {_TaxSelection}")

            For Each kvp In _productTextBoxes
                If kvp.Key.StartsWith("txtTaxPercent_") Then
                    If _TaxSelection Then
                        ' Exclusive: Allow user to edit and clear the value
                        kvp.Value.Text = "0" ' Let user type any percent
                        kvp.Value.IsReadOnly = False
                        CEtaxSelection = True
                        TaxHeader.Header = "Tax(%)"
                        ShowVatExBtn.Visibility = Visibility.Visible
                    Else
                        ' Inclusive: Set to 12 and make it readonly
                        kvp.Value.Text = ""
                        kvp.Value.IsReadOnly = True
                        CEtaxSelection = False
                        TaxHeader.Header = "Tax(12%)"
                        ShowVatExBtn.Visibility = Visibility.Collapsed
                        CEisVatExInclude = False
                    End If
                End If
            Next

            ' Call CalculateAmount method for each row
            For i As Integer = 0 To rowCount - 1
                CalculateAmount(i)
            Next
        End Sub
#End Region

#Region "This Loads every data if its available for updating"
        Private Sub InitializeProductUI()
            If HasCachedItems() Then
                If _typingTimer Is Nothing Then
                    _typingTimer = New DispatcherTimer()
                    _typingTimer.Interval = TimeSpan.FromMilliseconds(300)
                    AddHandler _typingTimer.Tick, AddressOf OnTypingTimerTick
                End If

                FillClientsField()
                LoadCachedQuoteItems()
            Else
                AddProductInputUI()
            End If
        End Sub

        Private Function HasCachedItems() As Boolean
            Return CEQuoteItemsCache IsNot Nothing AndAlso CEQuoteItemsCache.Count > 0
        End Function

        Private Sub LoadCachedQuoteItems()
            For Each item In CEQuoteItemsCache
                rowCount += 1
                AddProductInputUI()

                Dim inputPanel = GetLatestInputPanel()
                If inputPanel Is Nothing Then Continue For

                FillClientsField()
                FillProductFields(item, rowCount)
                FillDescriptionField(inputPanel, item)
            Next
        End Sub

        Private Function GetLatestInputPanel() As StackPanel
            If MainContainer.Children.Count = 0 Then Return Nothing

            Dim lastBorder = TryCast(MainContainer.Children(MainContainer.Children.Count - 1), Border)
            If lastBorder Is Nothing Then Return Nothing

            Dim outerStack = TryCast(lastBorder.Child, StackPanel)
            If outerStack Is Nothing OrElse outerStack.Children.Count = 0 Then Return Nothing

            Return TryCast(outerStack.Children(0), StackPanel)
        End Function

        Private Sub FillClientsField()
            RemoveHandler txtSearchCustomer.TextChanged, AddressOf txtSearchCustomer_TextChanged

            ' Fill client name first
            If Not String.IsNullOrWhiteSpace(CEClientName) Then
                txtSearchCustomer.Text = CEClientName
            End If

            ' Load clients manually before trying to match
            If _clients Is Nothing OrElse _clients.Count = 0 Then
                _clients = ClientController.SearchClient(txtSearchCustomer.Text)
            End If

            ' Now we can match safely
            If _clients IsNot Nothing AndAlso _clients.Count > 0 Then
                Dim match = _clients.FirstOrDefault(Function(c) c.Name = txtSearchCustomer.Text)
                If match IsNot Nothing Then
                    _selectedClient = match
                    UpdateSupplierDetails(_selectedClient)
                End If
            End If

            ' Continue setting other fields
            If Not String.IsNullOrWhiteSpace(CEClientDetailsCache) Then TxtClientDetails.Text = CEClientDetailsCache
            If Not String.IsNullOrWhiteSpace(CEQuoteNumberCache) Then txtQuoteNumber.Text = CEQuoteNumberCache
            If Not String.IsNullOrWhiteSpace(CEReferenceNumber) Then txtReferenceNumber.Text = CEReferenceNumber
            If Not String.IsNullOrWhiteSpace(CEnoteTxt) Then txtQuoteNote.Text = CEnoteTxt

            Dim parsedDate As DateTime
            If DateTime.TryParse(CEQuoteDateCache, parsedDate) Then QuoteDate.SelectedDate = parsedDate
            'If DateTime.TryParse(CEQuoteValidityDateCache, parsedDate) Then QuoteValidityDate.SelectedDate = parsedDate

            cmbCostEstimateValidty.Text = CEValidUntilDate

            AddHandler txtSearchCustomer.TextChanged, AddressOf txtSearchCustomer_TextChanged
        End Sub

        Private Sub FillProductFields(item As Dictionary(Of String, String), row As Integer)
            Dim productFields = New Dictionary(Of String, String) From {
        {"txtProductName_", "ProductName"},
        {"txtQuantity_", "Quantity"},
        {"txtRate_", "Rate"},
        {"txtTaxPercent_", "TaxPercent"},
        {"txtTaxValue_", "Tax"},
        {"txtDiscountPercent_", "Discount"},
        {"txtDiscount_", "DiscountAmount"},
        {"txtAmount_", "Amount"}
    }

            For Each field In productFields
                Dim controlName = field.Key & row
                If _productTextBoxes.ContainsKey(controlName) AndAlso item.ContainsKey(field.Value) Then
                    _productTextBoxes(controlName).Text = item(field.Value)
                End If
            Next
        End Sub

        Private Sub FillDescriptionField(productPanel As StackPanel, item As Dictionary(Of String, String))
            Dim parentStack = TryCast(productPanel.Parent, StackPanel)
            If parentStack Is Nothing OrElse parentStack.Children.Count < 2 Then Return

            Dim descPanel = TryCast(parentStack.Children(1), StackPanel)
            If descPanel Is Nothing OrElse descPanel.Children.Count = 0 Then Return

            Dim descBorder = TryCast(descPanel.Children(0), Border)
            If descBorder Is Nothing Then Return

            Dim descTextBox = TryCast(descBorder.Child, TextBox)
            If descTextBox IsNot Nothing AndAlso item.ContainsKey("Description") Then
                descTextBox.Text = item("Description")
            End If
        End Sub

#End Region

#Region "Product Autocomplete"
        ' Add New Row Button Click Event in the UI to be able to put new product input
        Private Sub AddNewRow_Click(sender As Object, e As RoutedEventArgs)
            rowCount += 1 ' Make sure to increment rowCount here so new rows get unique names
            AddProductInputUI()
        End Sub

        ' The UI will Add ProductUI to the Interface
        Private Sub AddProductInputUI()
            Dim rowIndex As Integer = rowCount
            Dim mainBorder As New Border With {
        .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
        .BorderThickness = New Thickness(2),
        .Background = CType(New BrushConverter().ConvertFrom("#FDFDFD"), Brush),
        .CornerRadius = New CornerRadius(15),
        .Padding = New Thickness(0),
        .Margin = New Thickness(5),
        .HorizontalAlignment = HorizontalAlignment.Stretch,
        .MinWidth = 300
    }

            Dim mainStack As New StackPanel With {
        .Orientation = Orientation.Vertical,
        .Width = Double.NaN
    }

            Dim productPanel As New StackPanel With {
        .Orientation = Orientation.Horizontal,
        .Margin = New Thickness(10),
        .HorizontalAlignment = HorizontalAlignment.Left,
        .VerticalAlignment = VerticalAlignment.Top
    }

            ' This function will add all of the textbox to the MainContainer
            productPanel.Children.Add(CreateProductSearchBox(125, rowIndex))   ' Item Name
            productPanel.Children.Add(CreateQuantityBox(rowIndex))             ' Quantity
            productPanel.Children.Add(CreateRateBox(rowIndex))                 ' Rate
            productPanel.Children.Add(CreateTaxPercentBox(rowIndex))           ' Tax (%)
            productPanel.Children.Add(CreateTaxValueBox(rowIndex))             ' Tax (readonly)
            productPanel.Children.Add(CreateDiscountPercentBox(rowIndex))      ' Discount (%)
            productPanel.Children.Add(CreateDiscountBox(rowIndex))             ' Discount
            productPanel.Children.Add(CreateAmountBox("₱0.00", rowIndex))      ' Amount

            productPanel.Children.Add(CreateDeleteButton(mainBorder))
            mainStack.Children.Add(productPanel)

            ' Description remains the same
            Dim descriptionTextBox As New TextBox With {
            .Text = "Enter product description (Optional)",
            .BorderThickness = New Thickness(0),
            .Background = Brushes.Transparent,
            .FontFamily = New FontFamily("Lexend"),
            .FontSize = 12,
            .Foreground = Brushes.Black,
            .FontWeight = FontWeights.SemiBold,
            .Height = Double.NaN,
            .MinHeight = 120,
            .MaxHeight = 300,
            .VerticalAlignment = VerticalAlignment.Top,
            .HorizontalAlignment = HorizontalAlignment.Left,
            .Width = Double.NaN,
            .TextWrapping = TextWrapping.Wrap,
            .AcceptsReturn = True,
            .VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            .MaxLength = 2000
             }

            Dim descriptionBorder As New Border With {
            .Margin = New Thickness(10),
            .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
            .BorderThickness = New Thickness(2),
            .CornerRadius = New CornerRadius(5),
            .Padding = New Thickness(10),
            .Width = Double.NaN,
            .MinHeight = 120,
            .MaxHeight = 300,
            .Background = Brushes.Transparent,
            .Child = descriptionTextBox
             }

            Dim descriptionStack As New StackPanel With {
        .Width = Double.NaN
             }
            descriptionStack.Children.Add(descriptionBorder)

            mainStack.Children.Add(descriptionStack)
            mainBorder.Child = mainStack
            MainContainer.Children.Add(mainBorder)
        End Sub

        ' Textbox for Product Search It also included inside the Popup Function
        Public Function CreateProductSearchBox(width As Double, rowIndex As Integer) As Border
            Dim textBoxName As String = $"txtProductName_{rowIndex}"
            Dim popupKey As String = $"ProductPopup_{rowIndex}"
            Dim listBoxKey As String = $"LstProducts_{rowIndex}"
            Dim timerKey As String = $"ProductTimer_{rowIndex}"

            ' TextBox
            Dim textBox As New TextBox With {
            .Name = textBoxName,
            .FontFamily = New FontFamily("Lexend"),
            .FontSize = 12,
            .Foreground = Brushes.Black,
            .FontWeight = FontWeights.SemiBold,
            .TextWrapping = TextWrapping.Wrap,
            .Padding = New Thickness(5),
            .BorderThickness = New Thickness(0),
            .MinWidth = width,
            .MaxWidth = 150,
            .Width = Double.NaN,
            .Height = Double.NaN,
            .MinHeight = 30,
            .MaxHeight = 150,
            .AcceptsReturn = True,
            .VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            .HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            .MaxLength = 1000,
            .VerticalAlignment = VerticalAlignment.Top
        }

            ' ListBox for suggestions
            Dim suggestionList As New ListBox With {
            .Name = listBoxKey,
            .MaxHeight = 150,
            .MinWidth = width
        }

            ' Template to show product name
            Dim factory As New FrameworkElementFactory(GetType(TextBlock))
            factory.SetBinding(TextBlock.TextProperty, New Binding("ProductName")) ' Bind to property of ProductDataModel
            suggestionList.ItemTemplate = New DataTemplate() With {.VisualTree = factory}

            ' Popup setup
            Dim popup As New Popup With {
            .Name = popupKey,
            .StaysOpen = False,
            .AllowsTransparency = True,
            .PopupAnimation = PopupAnimation.Fade,
            .PlacementTarget = textBox,
            .Placement = PlacementMode.Bottom,
            .Child = New Border With {
                .Background = Brushes.White,
                .BorderBrush = Brushes.LightGray,
                .BorderThickness = New Thickness(1),
                .Child = suggestionList
            }
        }

            ' Store for cleanup or reference
            _productTextBoxes(textBoxName) = textBox
            _productListBoxes(listBoxKey) = suggestionList
            _productPopups(popupKey) = popup

            ' Typing debounce timer
            Dim typingTimer As New DispatcherTimer With {.Interval = TimeSpan.FromMilliseconds(300)}
            _productTypingTimers(timerKey) = typingTimer

            AddHandler typingTimer.Tick, Sub()
                                             typingTimer.Stop()

                                             Dim keyword = textBox.Text.Trim()

                                             If keyword.Length >= 2 Then
                                                 If _selectedClient IsNot Nothing Then
                                                     If WarehouseID <= 0 Then
                                                         MessageBox.Show("Please select a warehouse before searching products.")
                                                         popup.IsOpen = False
                                                         suggestionList.Visibility = Visibility.Collapsed
                                                         Return
                                                     End If

                                                     Dim results = QuotesController.SearchProductsByName(keyword, WarehouseID)

                                                     suggestionList.ItemsSource = results
                                                     suggestionList.Visibility = If(results.Count > 0, Visibility.Visible, Visibility.Collapsed)
                                                     popup.IsOpen = results.Count > 0
                                                 Else
                                                     popup.IsOpen = False
                                                     suggestionList.Visibility = Visibility.Collapsed
                                                     MessageBox.Show("Please select a client before searching products.")
                                                 End If
                                             Else
                                                 popup.IsOpen = False
                                                 suggestionList.Visibility = Visibility.Collapsed
                                             End If
                                         End Sub

            ' Trigger timer on text change
            AddHandler textBox.TextChanged, Sub(sender As Object, e As TextChangedEventArgs)
                                                typingTimer.Stop()
                                                typingTimer.Start()
                                            End Sub

            ' Handle selection
            AddHandler suggestionList.SelectionChanged, Sub(sender As Object, e As SelectionChangedEventArgs)
                                                            If suggestionList.SelectedItem IsNot Nothing Then
                                                                Dim selectedProduct = CType(suggestionList.SelectedItem, ProductDataModel)
                                                                Dim selectedProductName = selectedProduct.ProductName.Trim().ToLower()

                                                                ' Check duplicates in other TextBoxes BEFORE setting the text
                                                                Dim duplicateExists = _productTextBoxes.Values.Any(Function(tb) tb IsNot textBox AndAlso tb.Text.Trim().ToLower() = selectedProductName)

                                                                If duplicateExists Then
                                                                    MessageBox.Show("This product is already added in another row.", "Duplicate Product", MessageBoxButton.OK, MessageBoxImage.Warning)
                                                                    textBox.Clear()
                                                                    popup.IsOpen = False
                                                                    suggestionList.SelectedItem = Nothing
                                                                    Return
                                                                End If

                                                                ' No duplicate - now safe to proceed
                                                                textBox.Text = selectedProduct.ProductName
                                                                popup.IsOpen = False
                                                                suggestionList.SelectedItem = Nothing

                                                                ' Call the warehouse-specific function
                                                                Dim productInfo = QuotesController.GetProductDetailsByProductID(selectedProduct.ProductID, WarehouseID)

                                                                If productInfo.Count > 0 Then
                                                                    Dim p = productInfo.First()
                                                                    ' UI For setting the details
                                                                    SetProductDetails(rowIndex, p)
                                                                    Debug.WriteLine("== Product Info Retrieved ==")
                                                                    Debug.WriteLine("Name: " & p.ProductName)
                                                                    Debug.WriteLine("Buying Price: " & p.BuyingPrice)
                                                                    Debug.WriteLine("Tax: " & p.DefaultTax)
                                                                    Debug.WriteLine("Stock Unit: " & p.StockUnits)
                                                                Else
                                                                    Debug.WriteLine("No matching product found in PNV or PVS.")
                                                                End If
                                                            End If
                                                        End Sub

            AddHandler textBox.LostFocus, Sub(sender As Object, e As RoutedEventArgs)
                                              Dim currentTextBox = CType(sender, TextBox)
                                              Dim currentText = currentTextBox.Text.Trim()

                                              If String.IsNullOrEmpty(currentText) Then Return

                                              ' Check if any other product TextBox already has this text (ignore current one)
                                              Dim duplicates = _productTextBoxes.Where(Function(kvp) kvp.Value IsNot currentTextBox AndAlso kvp.Value.Text.Trim().ToLower() = currentText.ToLower())

                                              If duplicates.Any() Then
                                                  MessageBox.Show("This product is already added in another row.", "Duplicate Product", MessageBoxButton.OK, MessageBoxImage.Warning)
                                                  currentTextBox.Clear()
                                                  currentTextBox.Focus()
                                              End If
                                          End Sub

            ' Assemble UI
            Dim grid As New Grid()
            grid.Children.Add(textBox)
            grid.Children.Add(popup)

            Dim border As New Border With {
            .Child = grid,
            .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
            .BorderThickness = New Thickness(2),
            .Background = CType(New BrushConverter().ConvertFrom("#FDFDFD"), Brush),
            .CornerRadius = New CornerRadius(15),
            .Padding = New Thickness(5),
            .Margin = New Thickness(0, 0, 5, 0)
        }

            Return border
        End Function

        ' Based Textbox for the other textbox 
        Public Function CreateInputBox(text As String, width As Double, Optional isReadOnly As Boolean = False, Optional name As String = "") As Border
            Dim txt As New TextBox With {
        .Text = text,
        .FontFamily = New FontFamily("Lexend"),
        .FontSize = 12,
        .Foreground = Brushes.Black,
        .FontWeight = FontWeights.SemiBold,
        .TextWrapping = TextWrapping.Wrap,
        .Padding = New Thickness(5),
        .BorderThickness = New Thickness(0),
        .IsReadOnly = isReadOnly,
        .Width = width
    }

            If Not String.IsNullOrWhiteSpace(name) Then
                txt.Name = name
                _productTextBoxes(name) = txt
                Me.RegisterName(txt.Name, txt)
                ' 🔌 Attach Quantity_TextChanged if this is a Quantity TextBox
                If name.StartsWith("txtQuantity_") Then
                    AddHandler txt.TextChanged, AddressOf Quantity_TextChanged
                    AddHandler txt.PreviewTextInput, AddressOf Quantity_PreviewTextInput
                End If
                If name.StartsWith("txtDiscountPercent_") Then
                    AddHandler txt.TextChanged, AddressOf DiscountPercent_TextChanged
                    AddHandler txt.PreviewTextInput, AddressOf DiscountPercent_PreviewTextInput
                End If
            End If

            Dim border As New Border With {
        .BorderBrush = If(isReadOnly, Brushes.Transparent, CType(New BrushConverter().ConvertFrom("#1D3242"), Brush)),
        .BorderThickness = If(isReadOnly, New Thickness(0), New Thickness(2)),
        .Background = CType(New BrushConverter().ConvertFrom("#FDFDFD"), Brush),
        .CornerRadius = New CornerRadius(15),
        .Padding = New Thickness(5),
        .Margin = New Thickness(0, 0, 5, 0),
        .Child = txt
    }

            Return border
        End Function

        ' Quantity Textbox
        Private Function CreateQuantityBox(rowIndex As Integer) As Border
            Return CreateInputBox("1", 50, False, $"txtQuantity_{rowIndex}")
        End Function

        ' Rate Textbox
        Private Function CreateRateBox(rowIndex As Integer) As Border
            Dim box = CreateInputBox("", 70, False, $"txtRate_{rowIndex}")
            Dim txt = TryCast(box.Child, TextBox)
            If txt IsNot Nothing Then
                AddHandler txt.TextChanged, AddressOf Quantity_TextChanged
                AddHandler txt.PreviewTextInput, AddressOf Quantity_PreviewTextInput
            End If
            Return box
        End Function

        ' Tax Percent Textbox
        Private Function CreateTaxPercentBox(rowIndex As Integer) As Border
            ' Set the default value to "12" if the tax is inclusive
            Dim defaultTaxPercent As String = If(Not CEtaxSelection, "", "0")

            ' Create the textbox with the default value and readonly behavior
            Dim box = CreateInputBox(defaultTaxPercent, 60, Not _TaxSelection, $"txtTaxPercent_{rowIndex}")
            Dim txt = TryCast(box.Child, TextBox)
            If txt IsNot Nothing Then
                AddHandler txt.TextChanged, AddressOf TaxPercent_TextChanged
                AddHandler txt.PreviewTextInput, AddressOf TaxPercent_PreviewTextInput
            End If

            ' Ensure the border is always visible
            box.BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush)
            box.BorderThickness = New Thickness(2)

            Return box
        End Function

        ' Tax Value Box
        Private Function CreateTaxValueBox(rowIndex As Integer) As Border
            Return CreateInputBox("", 60, True, $"txtTaxValue_{rowIndex}")
        End Function

        ' Discount Percent
        Private Function CreateDiscountPercentBox(rowIndex As Integer) As Border
            Dim box = CreateInputBox("", 60, False, $"txtDiscountPercent_{rowIndex}")
            Dim txt = TryCast(box.Child, TextBox)
            If txt IsNot Nothing Then
                AddHandler txt.TextChanged, AddressOf DiscountPercent_TextChanged
                AddHandler txt.PreviewTextInput, AddressOf DiscountPercent_PreviewTextInput
            End If
            Return box
        End Function

        ' Discount Box
        Private Function CreateDiscountBox(rowIndex As Integer) As Border
            Return CreateInputBox("", 90, True, $"txtDiscount_{rowIndex}")
        End Function

        ' Amount Box
        Private Function CreateAmountBox(text As String, rowIndex As Integer) As Border
            Return CreateInputBox(text, 70, True, $"txtAmount_{rowIndex}")
        End Function

        ' Deleting the buttons
        Private Function CreateDeleteButton(containerToRemoveFrom As UIElement) As Button
            Dim deleteButton As New Button With {
            .Background = Brushes.Transparent,
            .BorderBrush = Brushes.Transparent,
            .Padding = New Thickness(0),
            .Width = 35,
            .Height = 40,
            .Cursor = Cursors.Hand,
            .VerticalAlignment = VerticalAlignment.Center
        }

            Dim icon As New MaterialDesignThemes.Wpf.PackIcon With {
            .Kind = MaterialDesignThemes.Wpf.PackIconKind.PlaylistRemove,
            .Foreground = CType(New BrushConverter().ConvertFrom("#D23636"), Brush),
            .Width = 35,
            .Height = 35,
            .HorizontalAlignment = HorizontalAlignment.Center,
            .VerticalAlignment = VerticalAlignment.Center
        }

            deleteButton.Content = icon

            AddHandler deleteButton.Click, Sub(sender As Object, e As RoutedEventArgs)
                                               ' Remove the UI container
                                               MainContainer.Children.Remove(containerToRemoveFrom)

                                               ' Clean up any registered names (e.g., txtAmount_0, txtQuantity_0, etc.)
                                               Dim allTextBoxes = FindVisualChildren(Of TextBox)(containerToRemoveFrom)

                                               For Each txt In allTextBoxes
                                                   If Not String.IsNullOrEmpty(txt.Name) Then
                                                       Try
                                                           UnregisterName(txt.Name)
                                                       Catch ex As ArgumentException
                                                           ' Ignore if already unregistered
                                                       End Try
                                                       If _productTextBoxes.ContainsKey(txt.Name) Then
                                                           _productTextBoxes.Remove(txt.Name)
                                                       End If
                                                   End If
                                               Next

                                               ' Optionally remove popup/listbox from dictionaries
                                               Dim amountBox = allTextBoxes.FirstOrDefault(Function(t) t.Name IsNot Nothing AndAlso t.Name.StartsWith("txtAmount_"))
                                               If amountBox IsNot Nothing Then
                                                   Dim rowIndex As Integer
                                                   If Integer.TryParse(amountBox.Name.Split("_"c).Last(), rowIndex) Then
                                                       Dim timerKey = $"ProductTimer_{rowIndex}"
                                                       Dim popupKey = $"ProductPopup_{rowIndex}"
                                                       Dim listKey = $"LstProducts_{rowIndex}"

                                                       If _productTypingTimers.ContainsKey(timerKey) Then _productTypingTimers.Remove(timerKey)
                                                       If _productPopups.ContainsKey(popupKey) Then _productPopups.Remove(popupKey)
                                                       If _productListBoxes.ContainsKey(listKey) Then _productListBoxes.Remove(listKey)
                                                   End If
                                               End If

                                               ' Update the grand total after removing a row
                                               UpdateGrandTotal()
                                               UpdateTotalTax()
                                               UpdateTotalDiscount()
                                           End Sub
            Return deleteButton
        End Function
#End Region

#Region "Calculation Per Row"
        ' This will set the Rate based on buyingPrice of the product and set a value to the Rate TextBox
        Private Sub SetProductDetails(rowIndex As Integer, product As ProductDataModel)
            Dim rateBox = TryCast(FindTextBoxByName($"txtRate_{rowIndex}"), TextBox)
            Dim taxPercentBox = TryCast(FindTextBoxByName($"txtTaxPercent_{rowIndex}"), TextBox)
            Dim taxValueBox = TryCast(FindTextBoxByName($"txtTaxValue_{rowIndex}"), TextBox)

            If rateBox IsNot Nothing Then
                Dim buyingPrice As Decimal

                buyingPrice = product.SellingPrice

                rateBox.Text = buyingPrice.ToString("F2")

                If taxPercentBox IsNot Nothing Then
                    If Not _TaxSelection Then
                        ' Inclusive: set to default and lock
                        ' taxPercentBox.Text = 12 ' Removed for testing 
                        taxPercentBox.IsReadOnly = True
                    Else
                        ' Exclusive: let user type
                        taxPercentBox.Text = "0"
                        taxPercentBox.IsReadOnly = False
                    End If
                End If

                ' Always recalculate using CalculateAmount so all logic is consistent
                CalculateAmount(rowIndex)
            End If
        End Sub

        ' This function will find the TextBox by name in the _productTextBoxes dictionary
        Private Function FindTextBoxByName(name As String) As TextBox
            If _productTextBoxes.ContainsKey(name) Then
                Return _productTextBoxes(name)
            End If
            Return Nothing
        End Function

        ' This function will find the Amount Textbox for all of the UI that is generated dynamically
        Private Function TryFindAmountTextBlock(rowIndex As Integer) As TextBlock
            ' Optional: store it in a dictionary if needed.
            For Each container As Border In MainContainer.Children.OfType(Of Border)()
                Dim amountTextBlock = container.FindName($"txtAmount_{rowIndex}")
                If TypeOf amountTextBlock Is TextBlock Then Return CType(amountTextBlock, TextBlock)
            Next
            Return Nothing
        End Function

        ' Calculate Amount
        Public Sub CalculateAmount(rowIndex As Integer)
            Dim quantityBox = FindTextBoxByName($"txtQuantity_{rowIndex}")
            Dim rateBox = FindTextBoxByName($"txtRate_{rowIndex}")
            Dim amountBox = FindTextBoxByName($"txtAmount_{rowIndex}")
            Dim taxPercentBox = FindTextBoxByName($"txtTaxPercent_{rowIndex}")
            Dim taxValueBox = FindTextBoxByName($"txtTaxValue_{rowIndex}")
            Dim discountPercentBox = FindTextBoxByName($"txtDiscountPercent_{rowIndex}")
            Dim discountBox = FindTextBoxByName($"txtDiscount_{rowIndex}")

            If quantityBox Is Nothing OrElse rateBox Is Nothing OrElse amountBox Is Nothing Then
                Debug.WriteLine($"[Row {rowIndex}] One or more required boxes not found.")
                Exit Sub
            End If

            ' Parse input values
            Dim quantity As Decimal = 0, rate As Decimal = 0, taxPercent As Decimal = 0, discountPercent As Decimal = 0
            Decimal.TryParse(quantityBox.Text, quantity)
            Decimal.TryParse(rateBox.Text, rate)
            If taxPercentBox IsNot Nothing Then Decimal.TryParse(taxPercentBox.Text, taxPercent)
            If discountPercentBox IsNot Nothing Then Decimal.TryParse(discountPercentBox.Text, discountPercent)

            Dim baseAmount = quantity * rate
            Dim taxValue As Decimal = 0
            Dim amountBeforeDiscount As Decimal = baseAmount

            If _TaxSelection Then
                ' Tax Exclusive: add tax to amount
                taxValue = baseAmount * (taxPercent / 100)
                amountBeforeDiscount = baseAmount + taxValue
            Else
                ' Tax Inclusive: always use 12% for display, do NOT add to amount
                taxValue = baseAmount * 0.12D
                amountBeforeDiscount = baseAmount

                ' Update txtTaxValueBox and amount value
                Dim taxValueBoxVal = FindTextBoxByName($"txtTaxValue_{rowIndex}")
                taxValueBox.Text = taxValue.ToString("N2")
                Dim amountBoxVal = FindTextBoxByName($"txtAmount_{rowIndex}")
                amountBox.Text = "₱" & amountBeforeDiscount.ToString("N2")
            End If

            Dim discountValue = amountBeforeDiscount * (discountPercent / 100)
            Dim finalAmount = amountBeforeDiscount - discountValue

            If taxValueBox IsNot Nothing Then taxValueBox.Text = taxValue.ToString("N2")
            If discountBox IsNot Nothing Then discountBox.Text = discountValue.ToString("N2")
            amountBox.Text = "₱" & finalAmount.ToString("N2")

            Debug.WriteLine($"[Row {rowIndex}] Base: {baseAmount}, Tax: {taxValue}, Discount: {discountValue}, Total: {finalAmount}")

            UpdateGrandTotal()
            UpdateTotalTax()
            UpdateTotalDiscount()
        End Sub

        ' This function is a helper to find all visual children of amount textboxes in the MainContainer (Don't touch this)
        Private Iterator Function FindVisualChildren(Of T As DependencyObject)(depObj As DependencyObject) As IEnumerable(Of T)
            If depObj IsNot Nothing Then
                For i As Integer = 0 To VisualTreeHelper.GetChildrenCount(depObj) - 1
                    Dim child As DependencyObject = VisualTreeHelper.GetChild(depObj, i)
                    If child IsNot Nothing AndAlso TypeOf child Is T Then
                        Yield CType(child, T)
                    End If

                    For Each childOfChild In FindVisualChildren(Of T)(child)
                        Yield childOfChild
                    Next
                Next
            End If
        End Function

        Public Sub UpdateGrandTotal()
            Dim grandTotal As Decimal = 0

            ' Loop through all entries in the dynamic amount textboxes
            For Each name As String In LogicalTreeHelper.GetChildren(MainContainer).OfType(Of UIElement)().
            SelectMany(Function(border) FindVisualChildren(Of TextBox)(border)).
            Where(Function(txt) txt.Name IsNot Nothing AndAlso txt.Name.StartsWith("txtAmount_")).
            Select(Function(txt) txt.Name).Distinct()

                Dim txtBox As TextBox = TryCast(Me.FindName(name), TextBox)
                If txtBox IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtBox.Text) Then
                    Dim rawText = txtBox.Text.Replace("₱", "").Trim()
                    Dim amount As Decimal
                    If Decimal.TryParse(rawText, amount) Then
                        grandTotal += amount
                    End If
                End If
            Next

            ' Update the grand total display
            txtGrandTotal.Text = "₱" & grandTotal.ToString("N2")
        End Sub

        ' This function is for updating the value of tax whenever there is changes
        Public Sub UpdateTotalTax()
            Dim totalTax As Decimal = 0

            ' Loop through all textboxes with names starting with txtTaxValue_
            For Each name As String In LogicalTreeHelper.GetChildren(MainContainer).OfType(Of UIElement)().
        SelectMany(Function(border) FindVisualChildren(Of TextBox)(border)).
        Where(Function(txt) txt.Name IsNot Nothing AndAlso txt.Name.StartsWith("txtTaxValue_")).
        Select(Function(txt) txt.Name).Distinct()

                Dim txtBox As TextBox = TryCast(Me.FindName(name), TextBox)
                If txtBox IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtBox.Text) Then
                    Dim rawText = txtBox.Text.Replace("₱", "").Trim()
                    Dim tax As Decimal
                    If Decimal.TryParse(rawText, tax) Then
                        totalTax += tax
                    End If
                End If
            Next

            ' Example output target: you should declare this in your XAML like you did with txtGrandTotal
            txtTotalTax.Text = "₱" & totalTax.ToString("N2")
        End Sub

        Public Sub UpdateTotalDiscount()
            Dim totalDiscount As Decimal = 0

            For Each name As String In LogicalTreeHelper.GetChildren(MainContainer).OfType(Of UIElement)().
        SelectMany(Function(border) FindVisualChildren(Of TextBox)(border)).
        Where(Function(txt) txt.Name IsNot Nothing AndAlso txt.Name.StartsWith("txtDiscount_")).
        Select(Function(txt) txt.Name).Distinct()

                Dim txtBox As TextBox = TryCast(Me.FindName(name), TextBox)
                If txtBox IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtBox.Text) Then
                    Dim rawText = txtBox.Text.Replace("₱", "").Trim()
                    Dim discount As Decimal
                    If Decimal.TryParse(rawText, discount) Then
                        totalDiscount += discount
                    End If
                End If
            Next

            txtTotalDiscount.Text = "₱" & totalDiscount.ToString("N2")
        End Sub


        ' Quantity Textbox for Dynamic Product Input UI
        Private Sub Quantity_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim textBox = TryCast(sender, TextBox)
            If textBox Is Nothing Then Exit Sub

            textBox.Dispatcher.BeginInvoke(Sub()
                                               Dim parts = textBox.Name.Split("_"c)
                                               If parts.Length < 2 Then Exit Sub

                                               Dim rowIndex As Integer
                                               If Not Integer.TryParse(parts(1), rowIndex) Then Exit Sub

                                               CalculateAmount(rowIndex)
                                           End Sub, DispatcherPriority.Background)
        End Sub

        Private Sub Quantity_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            If Not e.Text.All(AddressOf Char.IsDigit) Then
                e.Handled = True
            End If
        End Sub

        ' Tax Percent Textbox for Dynamic Product Input UI
        Private Sub TaxPercent_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim textBox = TryCast(sender, TextBox)
            If textBox Is Nothing Then Exit Sub

            textBox.Dispatcher.BeginInvoke(Sub()
                                               Dim parts = textBox.Name.Split("_"c)
                                               If parts.Length < 2 Then Exit Sub

                                               Dim rowIndex As Integer
                                               If Not Integer.TryParse(parts(1), rowIndex) Then Exit Sub

                                               CalculateAmount(rowIndex)
                                           End Sub, DispatcherPriority.Background)
        End Sub

        Private Sub TaxPercent_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            Dim tb = DirectCast(sender, TextBox)

            ' Block if input is not a digit or if new text would be longer than 3 chars
            If Not Char.IsDigit(e.Text, 0) OrElse tb.Text.Length >= 6 Then
                e.Handled = True
            End If
        End Sub

        Private Sub DiscountPercent_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim textBox = TryCast(sender, TextBox)
            If textBox Is Nothing Then Exit Sub

            textBox.Dispatcher.BeginInvoke(Sub()
                                               Dim parts = textBox.Name.Split("_"c)
                                               If parts.Length < 2 Then Exit Sub

                                               Dim rowIndex As Integer
                                               If Not Integer.TryParse(parts(1), rowIndex) Then Exit Sub

                                               CalculateAmount(rowIndex)
                                           End Sub, DispatcherPriority.Background)
        End Sub

        Private Sub DiscountPercent_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            Dim tb = DirectCast(sender, TextBox)

            ' Block if input is not a digit or if new text would be longer than 3 chars
            If Not Char.IsDigit(e.Text, 0) OrElse tb.Text.Length >= 3 Then
                e.Handled = True
            End If
        End Sub

        Private Function ValidateQuoteSubmission(client As Client, productItemsJson As String) As Boolean
            If client Is Nothing Then
                MessageBox.Show("Client is required.")
                Return False
            End If

            If String.IsNullOrWhiteSpace(txtQuoteNumber.Text) Then
                MessageBox.Show("Quote Number is required.")
                Return False
            End If

            'If String.IsNullOrWhiteSpace(txtReferenceNumber.Text) Then
            '    MessageBox.Show("Reference Number is required.")
            '    Return False
            'End If

            If String.IsNullOrWhiteSpace(cmbCostEstimateValidty.Text) Then
                MessageBox.Show("Select an Cost Estimate Validity Date.")
                Return False
            End If

            If Not QuoteDate.SelectedDate.HasValue Then
                MessageBox.Show("Quote Date is required.")
                Return False
            End If

            'If Not QuoteValidityDate.SelectedDate.HasValue Then
            'MessageBox.Show("Validity Date is required.")
            'Return False
            'End If

            If txtTaxSelection.SelectedItem Is Nothing Then
                MessageBox.Show("Tax selection is required.")
                Return False
            End If

            If txtDiscountSelection.SelectedItem Is Nothing Then
                MessageBox.Show("Discount selection is required.")
                Return False
            End If

            If WarehouseID <= 0 Then
                MessageBox.Show("Please select a valid warehouse.")
                Return False
            End If

            If String.IsNullOrWhiteSpace(productItemsJson) Then
                MessageBox.Show("No products found in the quote.")
                Return False
            End If

            Return True
        End Function

        Private Sub txtQuoteNumber_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim currentQuoteID = txtQuoteNumber.Text.Trim()

            If String.IsNullOrEmpty(currentQuoteID) Then Exit Sub

            If QuotesController.QuoteNumberExists(currentQuoteID) Then
                ' If the quote number already exists, generate a new one and set it
                txtQuoteNumber.Text = QuotesController.GenerateQuoteID()
                txtQuoteNumber.CaretIndex = txtQuoteNumber.Text.Length ' Keep cursor at end
            End If
        End Sub
#End Region

#Region "Generate the Quote Before saving"
        ' Once Done All of the Data Will Be pass to another form for generating invoice
        Private Sub GenerateCostEstimate_Click(sender As Object, e As RoutedEventArgs)
            Dim productItemsJson As String = SubmitAllProductInputs()

            If productItemsJson Is Nothing Then
                ' Validation failed inside SubmitAllProductInputs
                Exit Sub
            End If

            Dim client As Client = _selectedClient

            ' Optional: check if client is nothing
            If client Is Nothing Then
                MessageBox.Show("Please select a client.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            GetAllDataInQuoteProperties(client, productItemsJson)
        End Sub

        ' Function for converting all of the product inputs to JSON Format before saving it and print it
        Private Function SubmitAllProductInputs() As String
            ' Ensure all amounts are up-to-date
            For i As Integer = 0 To MainContainer.Children.Count - 1
                CalculateAmount(i)
            Next

            Dim productArray As New List(Of Dictionary(Of String, Object))()

            For i As Integer = 0 To MainContainer.Children.Count - 1
                Dim border = TryCast(MainContainer.Children(i), Border)
                If border Is Nothing Then Continue For

                Dim stack = TryCast(border.Child, StackPanel)
                If stack Is Nothing OrElse stack.Children.Count < 1 Then Continue For

                Dim productPanel = TryCast(stack.Children(0), StackPanel)
                If productPanel Is Nothing OrElse productPanel.Children.Count < 8 Then Continue For

                Dim productData As New Dictionary(Of String, Object)
                Dim fieldNames = {"ProductName", "Quantity", "Rate", "TaxPercent", "Tax", "Discount"}

                For j As Integer = 0 To 5
                    If j >= productPanel.Children.Count Then Exit For

                    Dim borderInput = TryCast(productPanel.Children(j), Border)
                    If borderInput Is Nothing Then Continue For

                    Dim value As String = ""

                    If j = 0 Then
                        ' ProductName: might be inside a Grid
                        Dim grid = TryCast(borderInput.Child, Grid)
                        If grid IsNot Nothing AndAlso grid.Children.Count > 0 Then
                            Dim txtBox = TryCast(grid.Children(0), TextBox)
                            If txtBox IsNot Nothing Then value = txtBox.Text.Trim()
                        End If
                    Else
                        ' Other fields: Border -> TextBox
                        Dim txtBox = TryCast(borderInput.Child, TextBox)
                        If txtBox IsNot Nothing Then value = txtBox.Text.Trim()
                    End If

                    If (fieldNames(j) = "ProductName" OrElse fieldNames(j) = "Quantity" OrElse fieldNames(j) = "Rate") AndAlso
       String.IsNullOrWhiteSpace(value) Then

                        MessageBox.Show($"Please fill in all required fields in row {i + 1}.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                        Return Nothing
                    End If

                    productData(fieldNames(j)) = value
                Next

                ' Amount (child index 7)
                If productPanel.Children.Count > 7 Then
                    Dim amountBorder = TryCast(productPanel.Children(7), Border)
                    If amountBorder IsNot Nothing Then
                        Dim amountTextBox = TryCast(amountBorder.Child, TextBox)
                        If amountTextBox IsNot Nothing Then
                            Dim amountValue = amountTextBox.Text.Replace("₱", "").Trim()
                            productData("Amount") = amountValue
                        End If
                    End If
                End If

                ' Description field
                If stack.Children.Count > 1 Then
                    Dim descriptionPanel = TryCast(stack.Children(1), StackPanel)
                    If descriptionPanel IsNot Nothing AndAlso descriptionPanel.Children.Count > 0 Then
                        Dim descBorder = TryCast(descriptionPanel.Children(0), Border)
                        If descBorder IsNot Nothing Then
                            Dim descTextBox = TryCast(descBorder.Child, TextBox)
                            If descTextBox IsNot Nothing Then
                                ' Store the description, filtering out placeholder text
                                Dim descText = descTextBox.Text.Trim()
                                If descText = "Enter product description (Optional)" Then
                                    descText = ""
                                End If
                                productData("Description") = descText
                            End If
                        End If
                    End If
                End If

                ' ========== ADD THIS NEW SECTION ==========
                ' Get product image from database
                Dim productName As String = productData("ProductName").ToString()
                Dim imageBase64 As String = Nothing

                Try
                    ' Use GetProduct controller to retrieve image
                    imageBase64 = GetProduct.GetProductImageBase64(productName)
                Catch ex As Exception
                    Debug.WriteLine($"Error getting image for {productName}: {ex.Message}")
                End Try

                productData("ProductImageBase64") = If(String.IsNullOrEmpty(imageBase64), "", imageBase64)
                ' ========== END OF NEW SECTION ==========

                productArray.Add(productData)
            Next

            Return JsonConvert.SerializeObject(productArray, Formatting.None)
        End Function
#End Region

#Region "Clearing all of the fields"
        Public Sub ClearAllFields()
            Me.UnregisterName(txtDiscountSelection.Name)
            'Clear all fields in the quote form
            txtQuoteNumber.Clear()
            Dim quoteID As String = QuotesController.GenerateQuoteID()
            txtQuoteNumber.Text = quoteID
            txtReferenceNumber.Text = "Reference #"
            txtSearchCustomer.Clear()
            txtQuoteNote.Text = "None"
            txtTaxSelection.SelectedIndex = 0
            txtDiscountSelection.SelectedIndex = 0
            txtTotalTax.Text = "₱0.00"
            txtTotalDiscount.Text = "₱0.00"
            txtGrandTotal.Text = ""
            TxtClientDetails.Clear()
            'Do NOT clear _selectedClient, so autocomplete will not show the message
            'Do NOT call UpdateSupplierDetails(Nothing)
            ClearAllRows()
            OrderDateVM.SelectedDate = DateTime.Today
            'OrderDueDateVM.SelectedDate = DateTime.Today.AddDays(1)

            For Each child As UIElement In MainContainer.Children
                Dim allTextBoxes = FindVisualChildren(Of TextBox)(child)
                For Each txt In allTextBoxes
                    If Not String.IsNullOrWhiteSpace(txt.Name) Then
                        Try
                            UnregisterName(txt.Name)
                        Catch ex As ArgumentException
                            ' Already unregistered or not found, skip
                        End Try

                        If _productTextBoxes.ContainsKey(txt.Name) Then
                            _productTextBoxes.Remove(txt.Name)
                        End If
                    End If
                Next
            Next
        End Sub
#End Region

#Region "Getting All of the Data and Insert of this Quote"
        ' Whenever there is a change in WarehosueCombobox will also update the data
        Private Sub ComboBoxWarehouse_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim selectedItem As ComboBoxItem = TryCast(ComboBoxWarehouse.SelectedItem, ComboBoxItem)

            If selectedItem IsNot Nothing Then
                WarehouseID = Convert.ToInt32(selectedItem.Tag)
                WarehouseName = selectedItem.Content.ToString()
            End If
        End Sub

        Private Function GetValidityDate(validitySelection As String, baseDate As DateTime) As DateTime
            Try
                ' Normalize input
                Dim selection = validitySelection.Trim().ToLower()

                ' Direct mapping to AddMonths/AddDays
                Select Case selection
                    Case "48 hours"
                        Return baseDate.AddHours(48)
                    Case "1 week"
                        Return baseDate.AddDays(7)
                    Case "2 weeks"
                        Return baseDate.AddDays(14)
                    Case "3 weeks"
                        Return baseDate.AddDays(21)
                    Case "1 month"
                        Return baseDate.AddMonths(1)
                    Case "2 months"
                        Return baseDate.AddMonths(2)
                    Case "6 months"
                        Return baseDate.AddMonths(6)
                    Case "1 year"
                        Return baseDate.AddYears(1)
                    Case Else
                        Return baseDate.AddHours(48) ' Default
                End Select

            Catch ex As Exception
                Debug.WriteLine($"Error calculating validity date: {ex.Message}")
                Return baseDate.AddHours(48)
            End Try
        End Function

        ' Function for inserting the data into the quote table in the database
        Private Sub GetAllDataInQuoteProperties(client As Client, productItemsJson As String)
            If Not ValidateQuoteSubmission(client, productItemsJson) Then Exit Sub
            Try
                Dim selectedTax As String = CType(txtTaxSelection.SelectedItem, ComboBoxItem).Content.ToString()
                Dim selectedDiscount As String = CType(txtDiscountSelection.SelectedItem, ComboBoxItem).Content.ToString()
                ' Calculate actual validity date from selected option
                Dim selectedValidityOption = DirectCast(cmbCostEstimateValidty.SelectedItem, ComboBoxItem).Content.ToString()
                Dim actualValidityDate = GetValidityDate(selectedValidityOption, OrderDateVM.SelectedDate)
                CEValidUntilDate = selectedValidityOption
                CEQuoteValidityDateCache = actualValidityDate.ToString("yyyy-MM-dd")

                ' 07 - 04 - 2025 -- Moved the insert at the save and print button in previewprintquote.xaml.vb
                CEQuoteNumberCache = txtQuoteNumber.Text
                CEDiscountProperty = txtDiscountSelection.Text
                CETaxProperty = txtTaxSelection.Text
                CEQuoteDateCache = QuoteDate.SelectedDate.Value.ToString("yyyy-MM-dd")
                CEValidUntilDate = cmbCostEstimateValidty.Text ' Changed the value of the text instead
                'CETotalTaxValueCache = txtTotalTax.Text ' Doesnt need since im calculating it in the preview
                CETaxValueCache = txtTotalTax.Text
                CETotalDiscountValueCache = txtTotalDiscount.Text
                CETotalAmountCache = txtGrandTotal.Text
                CEnoteTxt = txtQuoteNote.Text
                CEReferenceNumber = txtReferenceNumber.Text
                'CEpaymentTerms = "None"
                CEQuoteItemsCache = JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(productItemsJson)
                CEsignature = False ' Assuming no signature for now
                CEImageCache = "" ' Assuming no image for now
                CEPathCache = "" ' Assuming no path for now
                'ils
                CECompanyName = client.Company ' Changed from Client Name to Company Name
                Dim stringArray As List(Of String) = client.BillingAddress.Split(","c).Select(Function(s) s.Trim()).ToList()

                CostEstimateDetails.CEAddress = stringArray(0)
                CostEstimateDetails.CECity = stringArray(1)
                CostEstimateDetails.CERegion = stringArray(2)
                CostEstimateDetails.CECountry = stringArray(3)
                CostEstimateDetails.CEClientDetailsCache = TxtClientDetails.Text
                CEPhone = client.Phone
                CEClientName = client.Name
                CostEstimateDetails.CERepresentative = client.Representative

                ' Debugging 
                Debug.WriteLine($"QuoteNumber: {CEQuoteNumberCache}, QuoteDate: {CEQuoteDateCache}, ValidityDate: {CEQuoteValidityDateCache}, Tax: {CETaxValueCache}, TotalAmount: {CETotalAmountCache}, Note: {CEnoteTxt}, Remarks: {CEremarksTxt}, Items: {JsonConvert.SerializeObject(CEQuoteItemsCache)}, Signature: {CEsignature}, Image: {CEImageCache}, Path: {CEPathCache}, ClientName: {CEClientName}, Phone: {CEPhone}, Email: {CEEmail}, Term1: {CETerm1}, Term2: {CETerm2}, Term3: {CETerm3}, Term4: {CETerm4}, Term5: {CETerm5}, Term6: {CETerm6}, Term7: {CETerm7}, Term8: {CETerm8}, Term9: {CETerm9}, Term10: {CETerm10}, Term11: {CETerm11}, Term12: {CETerm12}")
                ViewLoader.DynamicView.NavigateToView("costestimate", Me)
            Catch ex As Exception
                MessageBox.Show("Please Fill up all of the Fields: " & ex.Message)
            End Try
        End Sub

        ' If the user clicks the button to show or hide VAT in exclusive mode
        ' If the Show VAT button is clicked, toggle the VAT display
        ' If the Hide VAT button is clicked, toggle the collapsed VAT display
        Private Sub IncExVatinExclusive_Click(sender As Object, e As RoutedEventArgs)
            If VatExShowVat.Text = "Show VAT 12%" Then
                CEisVatExInclude = True
                VatExShowVat.Text = "Hide VAT 12%"
                MessageBox.Show($"Vat Selection - {CEisVatExInclude}")
            Else
                CEisVatExInclude = False
                VatExShowVat.Text = "Show VAT 12%"
                MessageBox.Show($"Vat Selection - {CEisVatExInclude}")
            End If
        End Sub

        'Private Sub btnExclusiveVatShow_Click(sender As Object, e As RoutedEventArgs)
        '    If ChangeVATColumn.Text = "Show Vat 12%" Then
        '        CEVatShow = True
        '        ChangeVATColumn.Text = "Hide Vat 12%"
        '        MessageBox.Show($"Vat Show - {CEVatShow}")
        '    Else
        '        CEVatShow = False
        '        ChangeVATColumn.Text = "Show Vat 12%"
        '        MessageBox.Show($"Vat Show - {CEVatShow}")
        '    End If
        'End Sub
#End Region
    End Class
End Namespace
