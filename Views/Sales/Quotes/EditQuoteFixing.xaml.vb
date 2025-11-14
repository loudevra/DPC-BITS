Imports System.Collections.ObjectModel
Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports System.Windows.Input
Imports System.Windows.Threading
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Views.Sales.Quotes.Quote
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json
Imports SharpCompress.Readers.Tar

Namespace DPC.Views.Sales.Quotes
    Public Class EditQuoteFixing
        Inherits UserControl

        Private _typingTimer As DispatcherTimer
        Private _orderItems As New ObservableCollection(Of OrderItemModel)()
        Private _quoteDateVM As New CalendarController.SingleCalendar()
        Private _selectedClientID As String = ""
        Private _selectedWarehouseID As String = ""
        Private _vatDisplayEnabled As Boolean = False
        Private _productTextBoxes As New Dictionary(Of String, TextBox)()
        Private _productPopups As New Dictionary(Of String, Popup)()
        Private _productListBoxes As New Dictionary(Of String, ListBox)()
        Private _productTypingTimers As New Dictionary(Of String, DispatcherTimer)()
        Private _TaxSelection As Boolean = True
        Private _selectedClient As Client = Nothing
        Private WarehouseID As Integer = 0
        Private WarehouseName As String = ""
        Private rowCount As Integer = 0
        Private CEtaxSelection As Boolean = True
        Private OrderDateVM As New CalendarController.SingleCalendar()
        Private _clients As List(Of Client) = Nothing

        Public Sub New()
            InitializeComponent()
            _typingTimer = New DispatcherTimer With {.Interval = TimeSpan.FromMilliseconds(300)}
            AddHandler _typingTimer.Tick, AddressOf OnSearchTimerTick
            AddHandler Me.Loaded, AddressOf EditQuote_Loaded
        End Sub

        Private Sub InitializeCalendar()
            _quoteDateVM.SelectedDate = DateTime.Today
            QuoteDate.DataContext = _quoteDateVM
            QuoteDateButton.DataContext = _quoteDateVM
        End Sub

        Private Sub LoadQuoteData()
            Try
                Dim cacheModule = GetCacheModule()
                txtQuoteNumber.Text = cacheModule.QuoteNumber
                TxtClientDetails.Text = cacheModule.TxtClientDetails

                If Not String.IsNullOrEmpty(cacheModule.WarehouseID) Then
                    For i As Integer = 0 To ComboBoxWarehouse.Items.Count - 1
                        Dim item = DirectCast(ComboBoxWarehouse.Items(i), ComboBoxItem)
                        If item.Tag?.ToString() = cacheModule.WarehouseID OrElse item.Content.ToString() = cacheModule.WarehouseName Then
                            ComboBoxWarehouse.SelectedIndex = i
                            WarehouseID = Convert.ToInt32(cacheModule.WarehouseID)
                            WarehouseName = cacheModule.WarehouseName
                            Exit For
                        End If
                    Next
                End If

                If Not String.IsNullOrEmpty(cacheModule.QuoteDate) AndAlso cacheModule.QuoteDate <> "-" Then
                    Try
                        _quoteDateVM.SelectedDate = DateTime.Parse(cacheModule.QuoteDate)
                        QuoteDate.SelectedDate = _quoteDateVM.SelectedDate
                    Catch ex As Exception
                        Debug.WriteLine("Error parsing date: " & ex.Message)
                    End Try
                End If

                If Not String.IsNullOrEmpty(cacheModule.Validity) Then
                    cmbCostEstimateValidty.Text = cacheModule.Validity
                End If

                If Not String.IsNullOrEmpty(cacheModule.QuoteNote) Then
                    txtQuoteNote.Text = cacheModule.QuoteNote
                End If

                If cacheModule.OrderItems IsNot Nothing Then
                    LoadOrderItems(cacheModule.OrderItems)
                End If

            Catch ex As Exception
                MessageBox.Show("Error loading quote data: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Function GetCacheModule() As QuoteCacheData
            If Not Application.Current.Properties.Contains("QuoteCache") Then
                Application.Current.Properties("QuoteCache") = New QuoteCacheData()
            End If
            Return DirectCast(Application.Current.Properties("QuoteCache"), QuoteCacheData)
        End Function

        Private Sub InitializeControls()
            InitializeClientAutoComplete()
        End Sub

        Private Sub InitializeClientAutoComplete()
            Try
            Catch ex As Exception
                Debug.WriteLine("Error initializing autocomplete: " & ex.Message)
            End Try
        End Sub

        Private Sub LoadWarehouses()
            Try
                ProductController.GetWarehouse(ComboBoxWarehouse)
            Catch ex As Exception
                Debug.WriteLine("Error loading warehouses: " & ex.Message)
            End Try
        End Sub

        Private Sub InitializeProductUI()
            If HasCachedItems() Then
                If _typingTimer Is Nothing Then
                    _typingTimer = New DispatcherTimer()
                    _typingTimer.Interval = TimeSpan.FromMilliseconds(300)
                    AddHandler _typingTimer.Tick, AddressOf OnSearchTimerTick
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

        Private Function GetLatestInputPanel() As StackPanel
            If MainContainer.Children.Count = 0 Then Return Nothing

            Dim lastBorder = TryCast(MainContainer.Children(MainContainer.Children.Count - 1), Border)
            If lastBorder Is Nothing Then Return Nothing

            Dim outerStack = TryCast(lastBorder.Child, StackPanel)
            If outerStack Is Nothing OrElse outerStack.Children.Count = 0 Then Return Nothing

            Return TryCast(outerStack.Children(0), StackPanel)
        End Function

        Private Sub FillClientsField()
            Try
                RemoveHandler txtSearchCustomer.TextChanged, AddressOf TxtSearchCustomer_TextChanged

                ' Set client name in search box
                If Not String.IsNullOrWhiteSpace(CEClientName) Then
                    txtSearchCustomer.Text = CEClientName

                    ' Search for client in database
                    Try
                        Dim searchResults = ClientController.SearchClient(CEClientName)
                        _clients = New List(Of Client)(searchResults)

                        ' Find exact match and get full client data
                        If _clients.Count > 0 Then
                            Dim match = _clients.FirstOrDefault(Function(c) c.Name = CEClientName)

                            If match IsNot Nothing Then
                                _selectedClient = match
                                UpdateSupplierDetails(_selectedClient)
                            End If
                        End If
                    Catch ex As Exception
                        Debug.WriteLine($"ERROR searching clients: {ex.Message}")
                    End Try
                End If

                ' Set quote details from cache
                If Not String.IsNullOrWhiteSpace(CEQuoteNumberCache) Then
                    txtQuoteNumber.Text = CEQuoteNumberCache
                End If

                If Not String.IsNullOrWhiteSpace(CEReferenceNumber) Then
                    txtReferenceNumber.Text = CEReferenceNumber
                End If

                If Not String.IsNullOrWhiteSpace(CEnoteTxt) Then
                    txtQuoteNote.Text = CEnoteTxt
                End If

                ' Set client details
                If _selectedClient IsNot Nothing Then
                    UpdateSupplierDetails(_selectedClient)
                ElseIf Not String.IsNullOrWhiteSpace(CEClientDetailsCache) Then
                    TxtClientDetails.Text = CEClientDetailsCache
                End If

                AddHandler txtSearchCustomer.TextChanged, AddressOf TxtSearchCustomer_TextChanged

            Catch ex As Exception
                MessageBox.Show("Error filling client fields: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                AddHandler txtSearchCustomer.TextChanged, AddressOf TxtSearchCustomer_TextChanged
            End Try
        End Sub

        Private Sub LoadCachedQuoteItems()
            If CEQuoteItemsCache Is Nothing OrElse CEQuoteItemsCache.Count = 0 Then
                AddProductInputUI()
                Return
            End If

            For Each item In CEQuoteItemsCache
                rowCount += 1
                AddProductInputUI()

                Dim inputPanel = GetLatestInputPanel()
                If inputPanel Is Nothing Then Continue For

                FillProductFields(item, rowCount)
                FillDescriptionField(inputPanel, item)
                CalculateAmount(rowCount - 1)
            Next

            UpdateGrandTotal()
            UpdateTotalTax()
            UpdateTotalDiscount()
        End Sub

        Private Sub FillProductFields(item As Dictionary(Of String, String), row As Integer)
            System.Threading.Thread.Sleep(100)

            Try
                Dim productFields = New Dictionary(Of String, String) From {
            {"txtProductName_", "ProductName"},
            {"txtQuantity_", "Quantity"},
            {"txtRate_", "Rate"},
            {"txtTaxPercent_", "TaxPercent"},
            {"txtTaxValue_", "Tax"},
            {"txtDiscountPercent_", "DiscountPercent"},  ' ← FIXED: was "Discount"
            {"txtDiscount_", "DiscountAmount"},          ' ← FIXED: was "DiscountAmount"
            {"txtAmount_", "Amount"}
        }

                For Each field In productFields
                    Dim controlName = field.Key & row

                    If _productTextBoxes.ContainsKey(controlName) Then
                        If item.ContainsKey(field.Value) Then
                            Dim value = item(field.Value).ToString()

                            Select Case field.Key
                                Case "txtQuantity_", "txtTaxPercent_", "txtDiscountPercent_"
                                    If String.IsNullOrWhiteSpace(value) Then value = "0"
                                Case "txtRate_", "txtTaxValue_", "txtDiscount_", "txtAmount_"
                                    Dim decVal As Decimal = 0
                                    If Decimal.TryParse(value, decVal) Then
                                        value = decVal.ToString("F2")
                                    Else
                                        value = "0.00"
                                    End If
                            End Select

                            _productTextBoxes(controlName).Text = value
                        End If
                    End If
                Next

            Catch ex As Exception
                Debug.WriteLine($"FillProductFields ERROR: {ex.Message}")
            End Try
        End Sub

        Private Sub FillDescriptionField(productPanel As StackPanel, item As Dictionary(Of String, String))
            Try
                Dim parentStack = TryCast(productPanel.Parent, StackPanel)
                If parentStack Is Nothing OrElse parentStack.Children.Count < 2 Then Return

                Dim descPanel = TryCast(parentStack.Children(1), StackPanel)
                If descPanel Is Nothing OrElse descPanel.Children.Count = 0 Then Return

                Dim descBorder = TryCast(descPanel.Children(0), Border)
                If descBorder Is Nothing Then Return

                Dim descTextBox = TryCast(descBorder.Child, TextBox)
                If descTextBox Is Nothing Then Return

                If item.ContainsKey("Description") Then
                    Dim description = item("Description").ToString()
                    If Not String.IsNullOrWhiteSpace(description) Then
                        descTextBox.Text = description
                        Return
                    End If
                End If

                descTextBox.Text = "Enter product description (Optional)"

            Catch ex As Exception
                Debug.WriteLine($"FillDescriptionField ERROR: {ex.Message}")
            End Try
        End Sub

        Private Sub LoadOrderItems(itemsJson As Object)
            Try
                _orderItems.Clear()

                If itemsJson Is Nothing OrElse String.IsNullOrEmpty(itemsJson.ToString()) Then
                    ProductInfoDataGrid.ItemsSource = _orderItems
                    Return
                End If

                Dim jsonString = itemsJson.ToString()
                If String.IsNullOrWhiteSpace(jsonString) OrElse jsonString = "-" Then
                    ProductInfoDataGrid.ItemsSource = _orderItems
                    Return
                End If

                Dim items As List(Of Dictionary(Of String, Object)) = JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, Object)))(jsonString)

                If items Is Nothing OrElse items.Count = 0 Then
                    ProductInfoDataGrid.ItemsSource = _orderItems
                    Return
                End If

                For Each item In items
                    Dim orderItem As New OrderItemModel With {
                        .ItemName = If(item.ContainsKey("ProductName"), item("ProductName")?.ToString(), ""),
                        .Quantity = SafeConvertToInt(item("Quantity")),
                        .Rate = SafeConvertToDecimal(item("Rate")),
                        .TaxtPercent = SafeConvertToDecimal(item("TaxPercent")),
                        .TaxValue = SafeConvertToDecimal(item("TaxValue")),
                        .DiscountPercent = SafeConvertToDecimal(item("DiscountPercent")),
                        .Discount = SafeConvertToDecimal(item("Discount")),
                        .Amount = SafeConvertToDecimal(item("Amount"))
                    }
                    _orderItems.Add(orderItem)
                Next

                ProductInfoDataGrid.ItemsSource = _orderItems
                ProductInfoDataGrid.Items.Refresh()
                CalculateTotals()

            Catch ex As Exception
                Debug.WriteLine("Error loading order items: " & ex.Message)
            End Try
        End Sub

        Private Function SafeConvertToInt(value As Object) As Integer
            If value Is Nothing Then Return 0
            Dim result As Integer = 0
            If Integer.TryParse(value.ToString(), result) Then Return result
            Return 0
        End Function

        Private Function SafeConvertToDecimal(value As Object) As Decimal
            If value Is Nothing Then Return 0
            Dim result As Decimal = 0
            If Decimal.TryParse(value.ToString(), result) Then Return result
            Return 0
        End Function

        Private Sub CalculateTotals()
            Try
                Dim totalTax As Decimal = _orderItems.Sum(Function(o) o.TaxValue)
                Dim totalDiscount As Decimal = _orderItems.Sum(Function(o) o.Discount)
                Dim totalAmount As Decimal = _orderItems.Sum(Function(o) o.Amount)

                txtTotalTax.Text = "₱ " & totalTax.ToString("F2")
                txtTotalDiscount.Text = "₱ " & totalDiscount.ToString("F2")
                txtGrandTotal.Text = "₱ " & totalAmount.ToString("F2")

            Catch ex As Exception
                Debug.WriteLine("Error calculating totals: " & ex.Message)
            End Try
        End Sub

        Private Sub TxtSearchCustomer_TextChanged(sender As Object, e As TextChangedEventArgs)
            _typingTimer.Stop()
            _typingTimer.Start()
        End Sub

        Private Sub OnSearchTimerTick(sender As Object, e As EventArgs)
            _typingTimer.Stop()
            Dim searchText = txtSearchCustomer.Text.Trim()

            If String.IsNullOrWhiteSpace(searchText) Then
                AutoCompletePopup.IsOpen = False
                Return
            End If

            Try
                Dim clients = ClientController.SearchClient(searchText)
                If clients IsNot Nothing AndAlso clients.Count > 0 Then
                    LstItems.ItemsSource = clients
                    AutoCompletePopup.IsOpen = True
                Else
                    AutoCompletePopup.IsOpen = False
                End If
            Catch ex As Exception
                Debug.WriteLine("Error searching clients: " & ex.Message)
            End Try
        End Sub

        Private Sub LstItems_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If LstItems.SelectedItem Is Nothing Then Return

            Try
                Dim selectedClient = CType(LstItems.SelectedItem, Client)
                _selectedClient = selectedClient

                UpdateSupplierDetails(selectedClient)

                txtSearchCustomer.Clear()
                AutoCompletePopup.IsOpen = False
                LstItems.SelectedItem = Nothing

            Catch ex As Exception
                Debug.WriteLine("Error in client selection: " & ex.Message)
            End Try
        End Sub

        Private Sub UpdateSupplierDetails(client As Client)
            If client Is Nothing Then Return

            Dim details As String =
                    $"Representative Name: {client.Representative}{Environment.NewLine}" &
                    $"Company: {client.Company}{Environment.NewLine}" &
                    $"Contact: {client.Phone}{Environment.NewLine}" &
                    $"Email: {client.Email}{Environment.NewLine}"

            If client.BillingAddress Is Nothing Then
                details &= $"{Environment.NewLine}{Environment.NewLine}Billing Address: (No data)"
            Else
                details &= String.Join(Environment.NewLine, $"Billing Address: {client.BillingAddress}")
            End If

            TxtClientDetails.Text = details
        End Sub

        Private Sub CmbCostEstimateType_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Try
                If cmbCostEstimateType.SelectedItem IsNot Nothing Then
                    Dim selectedType = DirectCast(cmbCostEstimateType.SelectedItem, ComboBoxItem).Content.ToString()
                    Debug.WriteLine($"Cost Estimate Type selected: {selectedType}")
                    ' Add any logic needed when type changes
                End If
            Catch ex As Exception
                Debug.WriteLine($"Error in CmbCostEstimateType_SelectionChanged: {ex.Message}")
            End Try
        End Sub

        Private Sub CmbCostEstimateValidty_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Try
                If cmbCostEstimateValidty.SelectedItem IsNot Nothing Then
                    Dim selectedValidity = DirectCast(cmbCostEstimateValidty.SelectedItem, ComboBoxItem).Content.ToString()
                    Debug.WriteLine($"Cost Estimate Validity selected: {selectedValidity}")

                    ' ✓ STORE the exact selected text from ComboBox
                    CEValidUntilDate = selectedValidity
                    Debug.WriteLine($"Stored in CEValidUntilDate: {CEValidUntilDate}")
                End If
            Catch ex As Exception
                Debug.WriteLine($"Error in CmbCostEstimateValidty_SelectionChanged: {ex.Message}")
            End Try
        End Sub

        Private Sub QuoteDateButton_Click(sender As Object, e As RoutedEventArgs)
            Try
                If QuoteDate IsNot Nothing Then
                    QuoteDate.IsDropDownOpen = True
                End If
            Catch ex As Exception
                Debug.WriteLine($"Error in QuoteDateButton_Click: {ex.Message}")
            End Try
        End Sub

        Private Sub TxtQuoteNumber_TextChanged(sender As Object, e As TextChangedEventArgs)
            Try
                ' Add validation or formatting logic if needed
                Debug.WriteLine($"Quote number changed: {txtQuoteNumber.Text}")
            Catch ex As Exception
                Debug.WriteLine($"Error in TxtQuoteNumber_TextChanged: {ex.Message}")
            End Try
        End Sub

        Private Sub TxtReferenceNumber_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            Try
                ' Allow alphanumeric characters only
                e.Handled = Not System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[a-zA-Z0-9-/]*$")
            Catch ex As Exception
                Debug.WriteLine($"Error in TxtReferenceNumber_PreviewTextInput: {ex.Message}")
            End Try
        End Sub

        Private Sub TxtTaxSelection_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            ' This method already exists in your code, but ensure it's properly implemented
            Try
                _TaxSelection = CType(txtTaxSelection.SelectedItem, ComboBoxItem).Content.ToString() = "Exclusive"
                Debug.WriteLine($"Tax Selection - {_TaxSelection}")

                For Each kvp In _productTextBoxes
                    If kvp.Key.StartsWith("txtTaxPercent_") Then
                        If _TaxSelection Then
                            kvp.Value.Text = "0"
                            kvp.Value.IsReadOnly = False
                            CEtaxSelection = True
                            TaxHeader.Header = "Tax(%)"
                            ShowVatExBtn.Visibility = Visibility.Visible
                        Else
                            kvp.Value.Text = ""
                            kvp.Value.IsReadOnly = True
                            CEtaxSelection = False
                            TaxHeader.Header = "Tax(12%)"
                            ShowVatExBtn.Visibility = Visibility.Collapsed
                            CEisVatExInclude = False
                        End If
                    End If
                Next

                For i As Integer = 0 To rowCount - 1
                    CalculateAmount(i)
                Next
            Catch ex As Exception
                Debug.WriteLine($"Error in TxtTaxSelection_SelectionChanged: {ex.Message}")
            End Try
        End Sub

        Private Sub Quantity_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim txt = TryCast(sender, TextBox)
            If txt Is Nothing Then Return
            UpdateRowCalculations(txt)
        End Sub

        Private Sub Quantity_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            e.Handled = Not IsNumeric(e.Text)
        End Sub

        Private Sub TaxPercent_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim txt = TryCast(sender, TextBox)
            If txt Is Nothing Then Return
            UpdateRowCalculations(txt)
        End Sub

        Private Sub TaxPercent_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            e.Handled = Not IsNumeric(e.Text)
        End Sub

        Private Sub DiscountPercent_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim txt = TryCast(sender, TextBox)
            If txt Is Nothing Then Return
            UpdateRowCalculations(txt)
        End Sub

        Private Sub DiscountPercent_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            e.Handled = Not IsNumeric(e.Text)
        End Sub

        Private Function IsNumeric(text As String) As Boolean
            Return System.Text.RegularExpressions.Regex.IsMatch(text, "^[0-9.]+$")
        End Function

        Private Sub UpdateRowCalculations(changedTextBox As TextBox)
            Try
                Dim nameParts = changedTextBox.Name.Split("_"c)
                If nameParts.Length < 2 Then Return

                Dim rowIndex As Integer
                If Not Integer.TryParse(nameParts(1), rowIndex) Then Return

                Dim qtyBox = TryCast(Me.FindName($"txtQuantity_{rowIndex}"), TextBox)
                Dim rateBox = TryCast(Me.FindName($"txtRate_{rowIndex}"), TextBox)
                Dim taxPercentBox = TryCast(Me.FindName($"txtTaxPercent_{rowIndex}"), TextBox)
                Dim taxValueBox = TryCast(Me.FindName($"txtTaxValue_{rowIndex}"), TextBox)
                Dim discountPercentBox = TryCast(Me.FindName($"txtDiscountPercent_{rowIndex}"), TextBox)
                Dim discountBox = TryCast(Me.FindName($"txtDiscount_{rowIndex}"), TextBox)
                Dim amountBox = TryCast(Me.FindName($"txtAmount_{rowIndex}"), TextBox)

                If qtyBox Is Nothing OrElse rateBox Is Nothing Then Return

                Dim quantity As Decimal = 0, rate As Decimal = 0, taxPercent As Decimal = 0, discountPercent As Decimal = 0

                Decimal.TryParse(qtyBox.Text, quantity)
                Decimal.TryParse(rateBox.Text, rate)
                If taxPercentBox IsNot Nothing Then Decimal.TryParse(taxPercentBox.Text, taxPercent)
                If discountPercentBox IsNot Nothing Then Decimal.TryParse(discountPercentBox.Text, discountPercent)

                Dim subtotal = quantity * rate
                Dim calcTaxValue = subtotal * (taxPercent / 100)
                Dim discountValue = subtotal * (discountPercent / 100)
                Dim amount = subtotal + calcTaxValue - discountValue

                If taxValueBox IsNot Nothing Then taxValueBox.Text = calcTaxValue.ToString("F2")
                If discountBox IsNot Nothing Then discountBox.Text = discountValue.ToString("F2")
                If amountBox IsNot Nothing Then amountBox.Text = amount.ToString("F2")

                UpdateGrandTotal()
                UpdateTotalTax()
                UpdateTotalDiscount()

            Catch ex As Exception
                Debug.WriteLine("Error updating row calculations: " & ex.Message)
            End Try
        End Sub

        Private Sub UpdateGrandTotal()
            Try
                Dim total As Decimal = 0
                For Each kvp In _productTextBoxes
                    If kvp.Key.StartsWith("txtAmount_") Then
                        Dim amount As Decimal = 0
                        Decimal.TryParse(kvp.Value.Text, amount)
                        total += amount
                    End If
                Next
                txtGrandTotal.Text = "₱ " & total.ToString("F2")
            Catch ex As Exception
                Debug.WriteLine("Error updating grand total: " & ex.Message)
            End Try
        End Sub

        Public Sub UpdateTotalTax()
            Dim totalTax As Decimal = 0

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

            txtTotalTax.Text = "₱ " & totalTax.ToString("N2")
        End Sub

        Private Sub UpdateTotalDiscount()
            Try
                Dim total As Decimal = 0
                For Each kvp In _productTextBoxes
                    If kvp.Key.StartsWith("txtDiscount_") Then
                        Dim discount As Decimal = 0
                        Decimal.TryParse(kvp.Value.Text, discount)
                        total += discount
                    End If
                Next
                txtTotalDiscount.Text = "₱ " & total.ToString("F2")
            Catch ex As Exception
                Debug.WriteLine("Error updating total discount: " & ex.Message)
            End Try
        End Sub

        Private Sub SetProductDetails(rowIndex As Integer, product As ProductDataModel)
            Try
                Dim rateBox = TryCast(Me.FindName($"txtRate_{rowIndex}"), TextBox)
                Dim taxPercentBox = TryCast(Me.FindName($"txtTaxPercent_{rowIndex}"), TextBox)

                If rateBox IsNot Nothing Then
                    rateBox.Text = product.SellingPrice.ToString("F2")
                End If

                If taxPercentBox IsNot Nothing AndAlso Not _TaxSelection Then
                    taxPercentBox.Text = product.DefaultTax.ToString("F2")
                End If

                If rateBox IsNot Nothing Then
                    UpdateRowCalculations(rateBox)
                End If

            Catch ex As Exception
                Debug.WriteLine("Error setting product details: " & ex.Message)
            End Try
        End Sub

        Private Shared Function FindVisualChildren(Of T As DependencyObject)(depObj As DependencyObject) As IEnumerable(Of T)
            Dim children As New List(Of T)()
            If depObj IsNot Nothing Then
                For i As Integer = 0 To VisualTreeHelper.GetChildrenCount(depObj) - 1
                    Dim child = VisualTreeHelper.GetChild(depObj, i)
                    If child IsNot Nothing AndAlso TypeOf child Is T Then
                        children.Add(CType(child, T))
                    End If
                    children.AddRange(FindVisualChildren(Of T)(child))
                Next
            End If
            Return children
        End Function

        Private Sub AddNewRow_Click(sender As Object, e As RoutedEventArgs)
            rowCount += 1
            AddProductInputUI()
        End Sub

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

            productPanel.Children.Add(CreateProductSearchBox(125, rowIndex))
            productPanel.Children.Add(CreateQuantityBox(rowIndex))
            productPanel.Children.Add(CreateRateBox(rowIndex))
            productPanel.Children.Add(CreateTaxPercentBox(rowIndex))
            productPanel.Children.Add(CreateTaxValueBox(rowIndex))
            productPanel.Children.Add(CreateDiscountPercentBox(rowIndex))
            productPanel.Children.Add(CreateDiscountBox(rowIndex))
            productPanel.Children.Add(CreateAmountBox("₱0.00", rowIndex))
            productPanel.Children.Add(CreateDeleteButton(mainBorder))

            mainStack.Children.Add(productPanel)

            Dim descriptionTextBox As New TextBox With {
                .Text = "Enter product description (Optional)",
                .BorderThickness = New Thickness(0),
                .Background = Brushes.Transparent,
                .FontFamily = New FontFamily("Lexend"),
                .FontSize = 12,
                .Foreground = Brushes.Black,
                .FontWeight = FontWeights.SemiBold,
                .Height = Double.NaN,
                .VerticalAlignment = VerticalAlignment.Top,
                .HorizontalAlignment = HorizontalAlignment.Left,
                .Width = Double.NaN,
                .TextWrapping = TextWrapping.Wrap
            }

            Dim descriptionBorder As New Border With {
                .Margin = New Thickness(10),
                .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                .BorderThickness = New Thickness(2),
                .CornerRadius = New CornerRadius(5),
                .Padding = New Thickness(10),
                .Width = Double.NaN,
                .Height = 120,
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

        Public Function CreateProductSearchBox(width As Double, rowIndex As Integer) As Border
            Dim textBoxName As String = $"txtProductName_{rowIndex}"
            Dim popupKey As String = $"ProductPopup_{rowIndex}"
            Dim listBoxKey As String = $"LstProducts_{rowIndex}"
            Dim timerKey As String = $"ProductTimer_{rowIndex}"

            Dim textBox As New TextBox With {
                .Name = textBoxName,
                .FontFamily = New FontFamily("Lexend"),
                .FontSize = 12,
                .Foreground = Brushes.Black,
                .FontWeight = FontWeights.SemiBold,
                .TextWrapping = TextWrapping.Wrap,
                .Padding = New Thickness(5),
                .BorderThickness = New Thickness(0),
                .Width = width
            }

            Dim suggestionList As New ListBox With {
                .Name = listBoxKey,
                .MaxHeight = 150,
                .MinWidth = width
            }

            Dim factory As New FrameworkElementFactory(GetType(TextBlock))
            factory.SetBinding(TextBlock.TextProperty, New Binding("ProductName"))
            suggestionList.ItemTemplate = New DataTemplate With {.VisualTree = factory}

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

            _productTextBoxes(textBoxName) = textBox
            _productListBoxes(listBoxKey) = suggestionList
            _productPopups(popupKey) = popup

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

            AddHandler textBox.TextChanged, Sub(s As Object, e As TextChangedEventArgs)
                                                typingTimer.Stop()
                                                typingTimer.Start()
                                            End Sub

            AddHandler suggestionList.SelectionChanged, Sub(s As Object, e As SelectionChangedEventArgs)
                                                            If suggestionList.SelectedItem IsNot Nothing Then
                                                                Dim selectedProduct = CType(suggestionList.SelectedItem, ProductDataModel)
                                                                Dim selectedProductName = selectedProduct.ProductName.Trim().ToLower()
                                                                Dim duplicateExists = _productTextBoxes.Values.Any(Function(tb) tb IsNot textBox AndAlso tb.Text.Trim().ToLower() = selectedProductName)

                                                                If duplicateExists Then
                                                                    MessageBox.Show("This product is already added in another row.", "Duplicate Product", MessageBoxButton.OK, MessageBoxImage.Warning)
                                                                    textBox.Clear()
                                                                    popup.IsOpen = False
                                                                    suggestionList.SelectedItem = Nothing
                                                                    Return
                                                                End If

                                                                textBox.Text = selectedProduct.ProductName
                                                                popup.IsOpen = False
                                                                suggestionList.SelectedItem = Nothing

                                                                Dim productInfo = QuotesController.GetProductDetailsByProductID(selectedProduct.ProductID, WarehouseID)
                                                                If productInfo.Count > 0 Then
                                                                    Dim p = productInfo.First()
                                                                    SetProductDetails(rowIndex, p)
                                                                End If
                                                            End If
                                                        End Sub

            AddHandler textBox.LostFocus, Sub(s As Object, e As RoutedEventArgs)
                                              Dim currentText = textBox.Text.Trim()
                                              If String.IsNullOrEmpty(currentText) Then Return
                                              Dim duplicates = _productTextBoxes.Where(Function(kvp) kvp.Value IsNot textBox AndAlso kvp.Value.Text.Trim().ToLower() = currentText.ToLower())
                                              If duplicates.Any() Then
                                                  MessageBox.Show("This product is already added in another row.", "Duplicate Product", MessageBoxButton.OK, MessageBoxImage.Warning)
                                                  textBox.Clear()
                                                  textBox.Focus()
                                              End If
                                          End Sub

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


        Private Function CreateQuantityBox(rowIndex As Integer) As Border
            Return CreateInputBox("1", 50, False, $"txtQuantity_{rowIndex}")
        End Function

        Private Function CreateRateBox(rowIndex As Integer) As Border
            Dim box = CreateInputBox("", 70, False, $"txtRate_{rowIndex}")
            Dim txt = TryCast(box.Child, TextBox)
            If txt IsNot Nothing Then
                AddHandler txt.TextChanged, AddressOf Quantity_TextChanged
                AddHandler txt.PreviewTextInput, AddressOf Quantity_PreviewTextInput
            End If
            Return box
        End Function

        Private Function CreateTaxPercentBox(rowIndex As Integer) As Border
            Dim defaultTaxPercent As String = If(Not CEtaxSelection, "", "0")
            Dim box = CreateInputBox(defaultTaxPercent, 60, Not _TaxSelection, $"txtTaxPercent_{rowIndex}")
            Dim txt = TryCast(box.Child, TextBox)
            If txt IsNot Nothing Then
                AddHandler txt.TextChanged, AddressOf TaxPercent_TextChanged
                AddHandler txt.PreviewTextInput, AddressOf TaxPercent_PreviewTextInput
            End If
            box.BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush)
            box.BorderThickness = New Thickness(2)
            Return box
        End Function

        Private Function CreateTaxValueBox(rowIndex As Integer) As Border
            Return CreateInputBox("", 60, True, $"txtTaxValue_{rowIndex}")
        End Function

        Private Function CreateDiscountPercentBox(rowIndex As Integer) As Border
            Dim box = CreateInputBox("", 60, False, $"txtDiscountPercent_{rowIndex}")
            Dim txt = TryCast(box.Child, TextBox)
            If txt IsNot Nothing Then
                AddHandler txt.TextChanged, AddressOf DiscountPercent_TextChanged
                AddHandler txt.PreviewTextInput, AddressOf DiscountPercent_PreviewTextInput
            End If
            Return box
        End Function

        Private Function CreateDiscountBox(rowIndex As Integer) As Border
            Return CreateInputBox("", 90, True, $"txtDiscount_{rowIndex}")
        End Function

        Private Function CreateAmountBox(text As String, rowIndex As Integer) As Border
            Return CreateInputBox(text, 70, True, $"txtAmount_{rowIndex}")
        End Function

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
                                               MainContainer.Children.Remove(containerToRemoveFrom)
                                               Dim allTextBoxes = FindVisualChildren(Of TextBox)(containerToRemoveFrom)

                                               For Each txt In allTextBoxes
                                                   If Not String.IsNullOrEmpty(txt.Name) Then
                                                       Try
                                                           UnregisterName(txt.Name)
                                                       Catch ex As ArgumentException
                                                       End Try
                                                       If _productTextBoxes.ContainsKey(txt.Name) Then
                                                           _productTextBoxes.Remove(txt.Name)
                                                       End If
                                                   End If
                                               Next

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

                                               UpdateGrandTotal()
                                               UpdateTotalTax()
                                               UpdateTotalDiscount()
                                           End Sub
            Return deleteButton
        End Function

        Public Sub CalculateAmount(rowIndex As Integer)
            Dim quantityBox = FindTextBoxByName($"txtQuantity_{rowIndex}")
            Dim rateBox = FindTextBoxByName($"txtRate_{rowIndex}")
            Dim amountBox = FindTextBoxByName($"txtAmount_{rowIndex}")
            Dim taxPercentBox = FindTextBoxByName($"txtTaxPercent_{rowIndex}")
            Dim taxValueBox = FindTextBoxByName($"txtTaxValue_{rowIndex}")
            Dim discountPercentBox = FindTextBoxByName($"txtDiscountPercent_{rowIndex}")
            Dim discountBox = FindTextBoxByName($"txtDiscount_{rowIndex}")

            If quantityBox Is Nothing OrElse rateBox Is Nothing OrElse amountBox Is Nothing Then
                Exit Sub
            End If

            Dim quantity As Decimal = 0, rate As Decimal = 0, taxPercent As Decimal = 0, discountPercent As Decimal = 0
            Decimal.TryParse(quantityBox.Text, quantity)
            Decimal.TryParse(rateBox.Text, rate)
            If taxPercentBox IsNot Nothing Then Decimal.TryParse(taxPercentBox.Text, taxPercent)
            If discountPercentBox IsNot Nothing Then Decimal.TryParse(discountPercentBox.Text, discountPercent)

            Dim baseAmount = quantity * rate
            Dim taxValue As Decimal = 0
            Dim amountBeforeDiscount As Decimal = baseAmount

            If _TaxSelection Then
                taxValue = baseAmount * (taxPercent / 100)
                amountBeforeDiscount = baseAmount + taxValue
            Else
                taxValue = baseAmount * 0.12D
                amountBeforeDiscount = baseAmount
            End If

            Dim discountValue = amountBeforeDiscount * (discountPercent / 100)
            Dim finalAmount = amountBeforeDiscount - discountValue

            If taxValueBox IsNot Nothing Then taxValueBox.Text = taxValue.ToString("N2")
            If discountBox IsNot Nothing Then discountBox.Text = discountValue.ToString("N2")
            amountBox.Text = "₱" & finalAmount.ToString("N2")

            UpdateGrandTotal()
            UpdateTotalTax()
            UpdateTotalDiscount()
        End Sub

        Private Function FindTextBoxByName(name As String) As TextBox
            If _productTextBoxes.ContainsKey(name) Then
                Return _productTextBoxes(name)
            End If
            Return Nothing
        End Function

        Private Sub ComboBoxWarehouse_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim selectedItem As ComboBoxItem = TryCast(ComboBoxWarehouse.SelectedItem, ComboBoxItem)
            If selectedItem IsNot Nothing Then
                WarehouseID = Convert.ToInt32(selectedItem.Tag)
                WarehouseName = selectedItem.Content.ToString()
            End If
        End Sub

        Private Sub UpdateCostEstimate_Click(sender As Object, e As RoutedEventArgs)
            Dim productItemsJson As String
            If _orderItems.Count > 0 Then
                productItemsJson = ExportDataGridToJson()
            Else
                productItemsJson = SubmitAllProductInputs()
            End If

            If productItemsJson Is Nothing Then Exit Sub

            Dim client As Client = _selectedClient
            If client Is Nothing Then
                MessageBox.Show("Please select a client.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            GetAllDataInQuoteProperties(client, productItemsJson)
        End Sub

        Private Function ExportDataGridToJson() As String
            Try
                Dim exportList As New List(Of Dictionary(Of String, Object))()

                For Each item As OrderItemModel In _orderItems
                    Dim itemDict As New Dictionary(Of String, Object) From {
                        {"ProductName", item.ItemName},
                        {"Quantity", item.Quantity},
                        {"Rate", item.Rate},
                        {"TaxPercent", item.TaxtPercent},
                        {"TaxValue", item.TaxValue},
                        {"DiscountPercent", item.DiscountPercent},
                        {"Discount", item.Discount},
                        {"Amount", item.Amount}
                    }
                    exportList.Add(itemDict)
                Next

                Return JsonConvert.SerializeObject(exportList, Formatting.Indented)

            Catch ex As Exception
                Debug.WriteLine("Error exporting DataGrid: " & ex.Message)
                Return Nothing
            End Try
        End Function

        Private Function SubmitAllProductInputs() As String
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
                        Dim grid = TryCast(borderInput.Child, Grid)
                        If grid IsNot Nothing AndAlso grid.Children.Count > 0 Then
                            Dim txtBox = TryCast(grid.Children(0), TextBox)
                            If txtBox IsNot Nothing Then value = txtBox.Text.Trim()
                        End If
                    Else
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

                If stack.Children.Count > 1 Then
                    Dim descriptionPanel = TryCast(stack.Children(1), StackPanel)
                    If descriptionPanel IsNot Nothing AndAlso descriptionPanel.Children.Count > 0 Then
                        Dim descBorder = TryCast(descriptionPanel.Children(0), Border)
                        If descBorder IsNot Nothing Then
                            Dim descTextBox = TryCast(descBorder.Child, TextBox)
                            If descTextBox IsNot Nothing Then
                                productData("Description") = descTextBox.Text
                            End If
                        End If
                    End If
                End If

                productArray.Add(productData)
            Next

            Return JsonConvert.SerializeObject(productArray, Formatting.None)
        End Function

        Private Sub GetAllDataInQuoteProperties(client As Client, productItemsJson As String)
            If Not ValidateQuoteSubmission(client, productItemsJson) Then Exit Sub
            Try
                Dim selectedTax As String = CType(txtTaxSelection.SelectedItem, ComboBoxItem).Content.ToString()
                Dim selectedDiscount As String = CType(txtDiscountSelection.SelectedItem, ComboBoxItem).Content.ToString()

                ' FIX: Use different variable name to avoid conflict with QuoteDate control
                Dim quoteDateValue = QuoteDate.SelectedDate.Value
                Dim selectedValidityOption = DirectCast(cmbCostEstimateValidty.SelectedItem, ComboBoxItem).Content.ToString()
                Dim actualValidityDate = GetValidityDate(selectedValidityOption, quoteDateValue)

                CEQuoteNumberCache = txtQuoteNumber.Text
                CEDiscountProperty = txtDiscountSelection.Text
                CETaxProperty = txtTaxSelection.Text
                CEQuoteDateCache = quoteDateValue.ToString("yyyy-MM-dd")
                CEValidUntilDate = selectedValidityOption
                CEQuoteValidityDateCache = actualValidityDate.ToString("yyyy-MM-dd")
                CETaxValueCache = txtTotalTax.Text
                CETotalDiscountValueCache = txtTotalDiscount.Text
                CETotalAmountCache = txtGrandTotal.Text
                CEnoteTxt = txtQuoteNote.Text
                CEReferenceNumber = txtReferenceNumber.Text
                CEQuoteItemsCache = JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(productItemsJson)
                CEsignature = False
                CEImageCache = ""
                CEPathCache = ""
                CECompanyName = client.Company
                Dim stringArray As List(Of String) = client.BillingAddress.Split(","c).Select(Function(s) s.Trim()).ToList()

                CostEstimateDetails.CEAddress = stringArray(0)
                CostEstimateDetails.CECity = stringArray(1)
                CostEstimateDetails.CERegion = stringArray(2)
                CostEstimateDetails.CECountry = stringArray(3)
                CostEstimateDetails.CEClientDetailsCache = TxtClientDetails.Text
                CEPhone = client.Phone
                CEClientName = client.Name
                CostEstimateDetails.CERepresentative = client.Representative

                ViewLoader.DynamicView.NavigateToView("costestimate", Me)
            Catch ex As Exception
                MessageBox.Show("Please Fill up all of the Fields: " & ex.Message)
            End Try
        End Sub


        ''' Loads the complete quote data from database and populates all fields
        Private Sub LoadCompleteQuoteData()
            Try
                Dim cacheModule = GetCacheModule()
                Dim quoteNumber = cacheModule.QuoteNumber

                If String.IsNullOrWhiteSpace(quoteNumber) Then
                    MessageBox.Show("No quote number found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Get full quote data from database
                Dim quote = QuotesController.GetQuoteByNumber(quoteNumber)

                If quote Is Nothing OrElse String.IsNullOrEmpty(quote.QuoteNumber) Then
                    MessageBox.Show("Quote not found in database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Populate all fields
                txtQuoteNumber.Text = quote.QuoteNumber
                txtReferenceNumber.Text = quote.Reference
                TxtClientDetails.Text = cacheModule.TxtClientDetails

                ' Set warehouse
                If Not String.IsNullOrEmpty(quote.WarehouseID) Then
                    For i As Integer = 0 To ComboBoxWarehouse.Items.Count - 1
                        Dim item = DirectCast(ComboBoxWarehouse.Items(i), ComboBoxItem)
                        If item.Tag?.ToString() = quote.WarehouseID Then
                            ComboBoxWarehouse.SelectedIndex = i
                            WarehouseID = Convert.ToInt32(quote.WarehouseID)
                            WarehouseName = quote.WarehouseName
                            Exit For
                        End If
                    Next
                End If

                ' Set dates - FIXED: Use QuoteDate instead of quoteDate
                Try
                    Dim quoteDateValue = DateTime.Parse(quote.QuoteDate)
                    _quoteDateVM.SelectedDate = quoteDateValue
                    QuoteDate.SelectedDate = quoteDateValue  ' FIXED: was 'quoteDate.SelectedDate'
                Catch ex As Exception
                    Debug.WriteLine("Error parsing quote date: " & ex.Message)
                End Try

                ' Set validity
                If Not String.IsNullOrEmpty(quote.Validity) AndAlso quote.Validity <> "-" Then
                    Debug.WriteLine($"quote.Validity from DB: '{quote.Validity}'")
                    Debug.WriteLine($"quote.QuoteDate from DB: '{quote.QuoteDate}'")

                    Try
                        ' Parse both dates
                        Dim quoteDateTime = DateTime.Parse(quote.QuoteDate)
                        Dim validityDateTime = DateTime.Parse(quote.Validity)

                        ' Calculate days difference to match against ComboBox options
                        Dim daysDiff = (validityDateTime - quoteDateTime).TotalDays
                        Dim hoursDiff = (validityDateTime - quoteDateTime).TotalHours

                        Dim matchingOption As String = "48 Hours" ' Default

                        ' Match based on calculated difference with tolerance
                        If Math.Abs(hoursDiff - 48) < 1 Then
                            matchingOption = "48 Hours"
                        ElseIf Math.Abs(daysDiff - 7) < 0.5 Then
                            matchingOption = "1 Week"
                        ElseIf Math.Abs(daysDiff - 14) < 0.5 Then
                            matchingOption = "2 Weeks"
                        ElseIf Math.Abs(daysDiff - 21) < 0.5 Then
                            matchingOption = "3 Weeks"
                        ElseIf Math.Abs(daysDiff - 30) < 1 Then
                            matchingOption = "1 Month"
                        ElseIf Math.Abs(daysDiff - 60) < 1 Then
                            matchingOption = "2 Months"
                        ElseIf Math.Abs(daysDiff - 180) < 1 Then
                            matchingOption = "6 Months"
                        ElseIf Math.Abs(daysDiff - 365) < 2 Then
                            matchingOption = "1 Year"
                        End If

                        Debug.WriteLine($"Calculated days difference: {daysDiff}, Matched to: '{matchingOption}'")

                        ' Find and select the matching ComboBoxItem
                        Dim found As Boolean = False
                        For i As Integer = 0 To cmbCostEstimateValidty.Items.Count - 1
                            Dim item = DirectCast(cmbCostEstimateValidty.Items(i), ComboBoxItem)
                            Dim itemContent = item.Content.ToString()

                            If itemContent = matchingOption Then
                                cmbCostEstimateValidty.SelectedIndex = i
                                found = True
                                Debug.WriteLine($"MATCH FOUND: '{matchingOption}' at index {i}")
                                Exit For
                            End If
                        Next

                        If Not found Then
                            Debug.WriteLine($"No match found - Setting default (48 Hours)")
                            If cmbCostEstimateValidty.Items.Count > 0 Then
                                cmbCostEstimateValidty.SelectedIndex = 0
                            End If
                        End If

                        CEValidUntilDate = quote.Validity

                    Catch ex As Exception
                        Debug.WriteLine($"Error setting validity: {ex.Message}")
                        If cmbCostEstimateValidty.Items.Count > 0 Then
                            cmbCostEstimateValidty.SelectedIndex = 0
                        End If
                    End Try
                Else
                    Debug.WriteLine("No validity date in database - using default 48 Hours")
                    If cmbCostEstimateValidty.Items.Count > 0 Then
                        cmbCostEstimateValidty.SelectedIndex = 0
                    End If
                    CEValidUntilDate = cmbCostEstimateValidty.Text
                End If

                ' Set quote note
                txtQuoteNote.Text = quote.QuoteNote

                ' Set tax and discount selections
                If Not String.IsNullOrEmpty(quote.Tax) Then
                    txtTaxSelection.Text = quote.Tax
                End If

                If Not String.IsNullOrEmpty(quote.Discount) Then
                    txtDiscountSelection.Text = quote.Discount
                End If

                ' Load order items from OrderItems JSON
                If Not String.IsNullOrEmpty(quote.OrderItems) Then
                    LoadOrderItemsFromJson(quote.OrderItems)
                Else
                    AddProductInputUI()
                End If

                ' Store data for later save
                CEQuoteNumberCache = quote.QuoteNumber
                CEReferenceNumber = quote.Reference
                CEClientIDCache = quote.ClientID
                CEClientName = quote.ClientName
                CEWarehouseIDCache = quote.WarehouseID
                CEWarehouseNameCache = quote.WarehouseName
                CEQuoteDateCache = quote.QuoteDate
                CEQuoteValidityDateCache = quote.Validity
                CEnoteTxt = quote.QuoteNote
                CETaxProperty = quote.Tax
                CEDiscountProperty = quote.Discount
                CEClientDetailsCache = TxtClientDetails.Text

                ' Load client info
                Try
                    Dim searchResults = ClientController.SearchClient(quote.ClientName)
                    If searchResults IsNot Nothing AndAlso searchResults.Count > 0 Then
                        Dim matchedClient = searchResults.FirstOrDefault(Function(c) c.Name = quote.ClientName)
                        If matchedClient IsNot Nothing Then
                            _selectedClient = matchedClient
                            txtSearchCustomer.Text = quote.ClientName
                        End If
                    End If
                Catch ex As Exception
                    Debug.WriteLine("Error loading client info: " & ex.Message)
                End Try

                MessageBox.Show("Quote data loaded successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

            Catch ex As Exception
                MessageBox.Show("Error loading quote data: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Debug.WriteLine("LoadCompleteQuoteData Error: " & ex.Message)
            End Try
        End Sub


        ''' Loads order items from JSON string and populates the form
        Private Sub LoadOrderItemsFromJson(itemsJson As String)
            Try
                ClearAllRows()

                If String.IsNullOrWhiteSpace(itemsJson) OrElse itemsJson = "-" Then
                    AddProductInputUI()
                    Return
                End If

                Dim items As List(Of Dictionary(Of String, Object)) =
            JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, Object)))(itemsJson)

                If items Is Nothing OrElse items.Count = 0 Then
                    AddProductInputUI()
                    Return
                End If

                For Each item In items
                    rowCount += 1
                    AddProductInputUI()

                    Dim inputPanel = GetLatestInputPanel()
                    If inputPanel Is Nothing Then Continue For

                    ' Fill product fields
                    FillProductRow(item, rowCount)
                    CalculateAmount(rowCount - 1)
                Next

                UpdateGrandTotal()
                UpdateTotalTax()
                UpdateTotalDiscount()

            Catch ex As Exception
                Debug.WriteLine("Error loading items from JSON: " & ex.Message)
            End Try
        End Sub


        ''' Fills a single product row with data from dictionary
        Private Sub FillProductRow(item As Dictionary(Of String, Object), row As Integer)
            Try
                System.Threading.Thread.Sleep(50)

                Dim productName = SafeGetString(item, "ProductName")
                Dim quantity = SafeGetString(item, "Quantity")
                Dim rate = SafeGetString(item, "Rate")
                Dim taxPercent = SafeGetString(item, "TaxPercent")
                Dim taxValue = SafeGetString(item, "TaxValue")
                Dim discountPercent = SafeGetString(item, "DiscountPercent")
                Dim discount = SafeGetString(item, "Discount")
                Dim amount = SafeGetString(item, "Amount")
                Dim description = SafeGetString(item, "Description")

                ' Set product name
                If _productTextBoxes.ContainsKey($"txtProductName_{row}") Then
                    _productTextBoxes($"txtProductName_{row}").Text = productName
                End If

                ' Set quantity
                If _productTextBoxes.ContainsKey($"txtQuantity_{row}") Then
                    _productTextBoxes($"txtQuantity_{row}").Text = If(String.IsNullOrWhiteSpace(quantity), "0", quantity)
                End If

                ' Set rate
                If _productTextBoxes.ContainsKey($"txtRate_{row}") Then
                    Dim rateVal As Decimal = 0
                    Decimal.TryParse(rate, rateVal)
                    _productTextBoxes($"txtRate_{row}").Text = rateVal.ToString("F2")
                End If

                ' Set tax percent
                If _productTextBoxes.ContainsKey($"txtTaxPercent_{row}") Then
                    Dim taxPercentVal As Decimal = 0
                    Decimal.TryParse(taxPercent, taxPercentVal)
                    _productTextBoxes($"txtTaxPercent_{row}").Text = taxPercentVal.ToString("F2")
                End If

                ' Set discount percent
                If _productTextBoxes.ContainsKey($"txtDiscountPercent_{row}") Then
                    Dim discountPercentVal As Decimal = 0
                    Decimal.TryParse(discountPercent, discountPercentVal)
                    _productTextBoxes($"txtDiscountPercent_{row}").Text = discountPercentVal.ToString("F2")
                End If

                ' Set description - get the description from the latest row
                If row > 0 Then
                    Dim latestInputPanel = GetLatestInputPanel()
                    If latestInputPanel IsNot Nothing Then
                        Dim parentStack = TryCast(latestInputPanel.Parent, StackPanel)
                        If parentStack IsNot Nothing AndAlso parentStack.Children.Count > 1 Then
                            Dim descPanel = TryCast(parentStack.Children(1), StackPanel)
                            If descPanel IsNot Nothing AndAlso descPanel.Children.Count > 0 Then
                                Dim descBorder = TryCast(descPanel.Children(0), Border)
                                If descBorder IsNot Nothing Then
                                    Dim descTextBox = TryCast(descBorder.Child, TextBox)
                                    If descTextBox IsNot Nothing Then
                                        descTextBox.Text = If(String.IsNullOrWhiteSpace(description), "Enter product description (Optional)", description)
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If

            Catch ex As Exception
                Debug.WriteLine($"FillProductRow ERROR: {ex.Message}")
            End Try
        End Sub


        ''' Helper function to safely get string value from dictionary

        ' SafeGetString helper - ENSURE THIS EXISTS
        Private Function SafeGetString(dict As Dictionary(Of String, Object), key As String) As String
            If dict Is Nothing OrElse Not dict.ContainsKey(key) Then
                Return ""
            End If

            Dim value = dict(key)
            If value Is Nothing Then
                Return ""
            End If

            Return value.ToString().Trim()
        End Function

        ' Update the EditQuote_Loaded method to call the new function

        Private Sub EditQuote_Loaded(sender As Object, e As RoutedEventArgs)
            InitializeCalendar()
            LoadWarehouses()
            InitializeControls()
            LoadCompleteQuoteData()  ' Changed from LoadQuoteData() to LoadCompleteQuoteData()
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

            If String.IsNullOrWhiteSpace(cmbCostEstimateValidty.Text) Then
                MessageBox.Show("Select a Cost Estimate Validity Date.")
                Return False
            End If

            If Not QuoteDate.SelectedDate.HasValue Then
                MessageBox.Show("Quote Date is required.")
                Return False
            End If

            If txtTaxSelection.SelectedItem Is Nothing Then
                MessageBox.Show("Tax selection is required.")
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

        Private Sub ClearAllRows()
            Dim rowsToRemove As New List(Of Integer)

            For i As Integer = 0 To MainContainer.Children.Count - 1
                rowsToRemove.Add(i)
            Next

            rowsToRemove.Sort()
            rowsToRemove.Reverse()

            For Each rowIndex As Integer In rowsToRemove
                If rowIndex < MainContainer.Children.Count Then
                    Dim element = MainContainer.Children(rowIndex)
                    MainContainer.Children.Remove(element)
                End If
            Next

            rowCount = 0
        End Sub

        Private Sub IncExVatinExclusive_Click(sender As Object, e As RoutedEventArgs)
            If VatExShowVat.Text = "Show VAT 12%" Then
                CEisVatExInclude = True
                VatExShowVat.Text = "Hide VAT 12%"
            Else
                CEisVatExInclude = False
                VatExShowVat.Text = "Show VAT 12%"
            End If
        End Sub

        Private Sub NavigateToCostEstimate(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("salesquote", Me)
        End Sub

    End Class

    Public Class OrderItemModel
        Implements System.ComponentModel.INotifyPropertyChanged

        Private _itemName As String
        Private _quantity As Integer
        Private _rate As Decimal
        Private _taxtPercent As Decimal
        Private _taxValue As Decimal
        Private _discountPercent As Decimal
        Private _discount As Decimal
        Private _amount As Decimal

        Public Property ItemName As String
            Get
                Return _itemName
            End Get
            Set(value As String)
                If _itemName <> value Then
                    _itemName = value
                    OnPropertyChanged(NameOf(ItemName))
                End If
            End Set
        End Property

        Public Property Quantity As Integer
            Get
                Return _quantity
            End Get
            Set(value As Integer)
                If _quantity <> value Then
                    _quantity = value
                    OnPropertyChanged(NameOf(Quantity))
                End If
            End Set
        End Property

        Public Property Rate As Decimal
            Get
                Return _rate
            End Get
            Set(value As Decimal)
                If _rate <> value Then
                    _rate = value
                    OnPropertyChanged(NameOf(Rate))
                End If
            End Set
        End Property

        Public Property TaxtPercent As Decimal
            Get
                Return _taxtPercent
            End Get
            Set(value As Decimal)
                If _taxtPercent <> value Then
                    _taxtPercent = value
                    OnPropertyChanged(NameOf(TaxtPercent))
                End If
            End Set
        End Property

        Public Property TaxValue As Decimal
            Get
                Return _taxValue
            End Get
            Set(value As Decimal)
                If _taxValue <> value Then
                    _taxValue = value
                    OnPropertyChanged(NameOf(TaxValue))
                End If
            End Set
        End Property

        Public Property DiscountPercent As Decimal
            Get
                Return _discountPercent
            End Get
            Set(value As Decimal)
                If _discountPercent <> value Then
                    _discountPercent = value
                    OnPropertyChanged(NameOf(DiscountPercent))
                End If
            End Set
        End Property

        Public Property Discount As Decimal
            Get
                Return _discount
            End Get
            Set(value As Decimal)
                If _discount <> value Then
                    _discount = value
                    OnPropertyChanged(NameOf(Discount))
                End If
            End Set
        End Property

        Public Property Amount As Decimal
            Get
                Return _amount
            End Get
            Set(value As Decimal)
                If _amount <> value Then
                    _amount = value
                    OnPropertyChanged(NameOf(Amount))
                End If
            End Set
        End Property

        Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

        Protected Sub OnPropertyChanged(propertyName As String)
            RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(propertyName))
        End Sub
    End Class

End Namespace
