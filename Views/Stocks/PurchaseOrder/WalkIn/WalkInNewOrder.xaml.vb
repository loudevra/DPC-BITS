Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Linq
Imports System.Web.UI.WebControls.Expressions
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DocumentFormat.OpenXml.Bibliography
Imports DocumentFormat.OpenXml.Math
Imports DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Views.Stocks
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json
Imports SharpCompress.Readers.Tar

Namespace DPC.Views.Stocks.PurchaseOrder.WalkIn
    Public Class WalkInNewOrder
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
        Private OrderDueDateVM As New CalendarController.SingleCalendar()
        ' Variable to store the data from warehouse
        Public WarehouseID As Integer
        Public WarehouseName As String
        Dim QuantityTotal
        ' Tax Combobox Variables
        Dim _TaxSelection As Boolean
        Dim _SelectedTax As Decimal

#Region "Initializiation once loaded the form"
        Public Sub New()
            InitializeComponent()

            InitializeProductUI()
            rowCount += 1

            ' Set a default date today and tomorrow
            OrderDateVM.SelectedDate = DateTime.Today

            ' Set Date to bind
            billingDate.DataContext = OrderDateVM
            BillingDateButton.DataContext = OrderDateVM

            ' Autocomplete part
            _typingTimer = New DispatcherTimer With {
                .Interval = TimeSpan.FromMilliseconds(300)
            }

            ' For Tax Selection
            If _TaxSelection Then
                txtTaxSelection.SelectedItem = txtTaxSelection.Items.Cast(Of ComboBoxItem)().FirstOrDefault(Function(i) i.Content.ToString() = "Exclusive")
            Else
                txtTaxSelection.SelectedItem = txtTaxSelection.Items.Cast(Of ComboBoxItem)().FirstOrDefault(Function(i) i.Content.ToString() = "Inclusive")
            End If

            AddHandler _typingTimer.Tick, AddressOf OnTypingTimerTick
            AddHandler txtSearchCustomer.TextChanged, AddressOf txtSearchCustomer_TextChanged
            AddHandler LstItems.SelectionChanged, AddressOf LstItems_SelectionChanged

            ' Event for Checking the Billing number
            'AddHandler txtBillingNumber.TextChanged, AddressOf txtBillingNumber_TextChanged

            ' Load warehouse options
            ProductController.GetWarehouse(ComboBoxWarehouse)

            Dim selectedWarehouse As ComboBoxItem = TryCast(ComboBoxWarehouse.SelectedItem, ComboBoxItem)
            If selectedWarehouse IsNot Nothing Then
                CEWarehouseIDCache = Convert.ToInt32(selectedWarehouse.Tag)
                CEWarehouseNameCache = selectedWarehouse.Content.ToString()
            End If

            'LoadCachedBillingData()
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

            Dim details As String = $"Name: {client.Name}{Environment.NewLine}" &
                    $"Company: {client.Company}{Environment.NewLine}" &
                    $"Contact: {client.Phone}{Environment.NewLine}" &
                    $"Email: {client.Email}{Environment.NewLine}" &
                    $"Customer Group: {client.CustomerGroup}{Environment.NewLine}" &
                    $"Language: {client.ClientLanguage}"

            If client.BillingAddress Is Nothing Then
                details &= $"{Environment.NewLine}{Environment.NewLine}Billing Address: (No data)"
            Else
                Dim billing = client.BillingAddress
                details &= Environment.NewLine & Environment.NewLine & String.Join(Environment.NewLine, client.BillingAddress.Split(","c))
            End If

            If client.ShippingAddress Is Nothing Then
                details &= $"{Environment.NewLine}{Environment.NewLine}Shipping Address: (No data)"
            Else
                Dim shipping = client.ShippingAddress
                details &= Environment.NewLine & Environment.NewLine & String.Join(Environment.NewLine, client.BillingAddress.Split(","c))
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

#Region "Validation and Other Function of the Billing Properties"
        Private Sub BillingDateButton_Click(sender As Object, e As RoutedEventArgs)
            billingDate.IsDropDownOpen = True
        End Sub

        Private Sub txtReferenceNumber_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            If Not e.Text.All(AddressOf Char.IsDigit) Then
                e.Handled = True
            End If
        End Sub

        Private Sub txtTaxSelection_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            _TaxSelection = CType(txtTaxSelection.SelectedItem, ComboBoxItem).Content.ToString() = "Exclusive"
            Debug.WriteLine($"Tax Selection - {_TaxSelection}")

            For Each kvp In _productTextBoxes
                If kvp.Key.StartsWith("txtTaxPercent_") Then
                    Dim txt = kvp.Value
                    Dim border = TryCast(txt.Parent, Border)

                    If _TaxSelection Then
                        ' Exclusive: Allow user to edit and clear the value
                        kvp.Value.Text = "0" '
                        kvp.Value.IsReadOnly = False
                        CEtaxSelection = True
                        TaxHeader.Header = "TAX(%)"
                        If border IsNot Nothing Then
                            border.BorderThickness = New Thickness(1)
                            border.BorderBrush = CType(New BrushConverter().ConvertFrom("#AEAEAE"), Brush)
                        End If
                    Else
                        ' Inclusive: Set to 12 and make it readonly
                        kvp.Value.Text = ""
                        kvp.Value.IsReadOnly = True
                        CEtaxSelection = False
                        TaxHeader.Header = "TAX(12%)"
                        CEisVatExInclude = False
                        If Border IsNot Nothing Then
                            Border.BorderThickness = New Thickness(0)
                            Border.BorderBrush = Brushes.Transparent
                        End If
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
            Dim hasAppCache As Boolean = Application.Current.Properties.Contains("BillingCache")
            Dim hasItemsInList As Boolean = (BLItemsCache IsNot Nothing AndAlso BLItemsCache.Count > 0)

            If hasAppCache OrElse hasItemsInList Then
                If _typingTimer Is Nothing Then
                    _typingTimer = New DispatcherTimer()
                    _typingTimer.Interval = TimeSpan.FromMilliseconds(300)
                    AddHandler _typingTimer.Tick, AddressOf OnTypingTimerTick
                End If

                LoadCachedBillingData()
            Else
                rowCount = 0
                MainContainer.Children.Clear()
                AddProductInputUI()

                Dim billingID As String = WalkInController.GenerateBillingID()
                txtBillingNumber.Text = billingID
            End If
        End Sub

        Private Sub LoadCachedBillingItems()
            ClearAllRows()
            rowCount = 0

            For Each item In BLItemsCache
                rowCount += 1
                AddProductInputUI()

                Dim inputPanel = GetLatestInputPanel()
                If inputPanel Is Nothing Then Continue For

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
            If Not String.IsNullOrWhiteSpace(BLClientDetailsCache) Then TxtClientDetails.Text = BLClientDetailsCache
            'If Not String.IsNullOrWhiteSpace(BLNumberCache) Then txtBillingNumber.Text = BLNumberCache
            'If Not String.IsNullOrWhiteSpace(CEReferenceNumber) Then txtReferenceNumber.Text = CEReferenceNumber
            If Not String.IsNullOrWhiteSpace(BLnoteTxt) Then txtBillingNote.Text = BLnoteTxt

            AddHandler txtSearchCustomer.TextChanged, AddressOf txtSearchCustomer_TextChanged
        End Sub

        Private Sub FillProductFields(item As Dictionary(Of String, String), row As Integer)
            Dim productFields = New Dictionary(Of String, String) From {
        {"txtProductName_", "ProductName"},
        {"txtQuantity_", "Quantity"},
        {"delivered", "delivered"},
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

            Dim scrollViewer As ScrollViewer = TryCast(MainContainer.Parent, ScrollViewer)

            If scrollViewer IsNot Nothing Then
                MainContainer.Dispatcher.BeginInvoke(Sub()
                                                         scrollViewer.ScrollToBottom()
                                                     End Sub, Windows.Threading.DispatcherPriority.Background)
            End If
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
        .Margin = New Thickness(0, 5, 0, 5),
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
            productPanel.Children.Add(CreateProductSearchBox(125, rowIndex))
            productPanel.Children.Add(CreateQuantityBox(rowIndex))
            productPanel.Children.Add(CreateRateBox(rowIndex))
            productPanel.Children.Add(CreateTaxPercentBox(rowIndex))
            productPanel.Children.Add(CreateTaxValueBox(rowIndex))
            productPanel.Children.Add(CreateDiscountPercentBox(rowIndex))
            productPanel.Children.Add(CreateDiscountBox(rowIndex))
            productPanel.Children.Add(CreateAmountBox("₱ 0.00", rowIndex))

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
            .MaxWidth = width
        }

            ' ListBox for suggestions
            Dim suggestionList As New ListBox With {
            .Name = listBoxKey,
            .MaxHeight = 150,
            .MinWidth = width
        }

            ' Template to show product name
            Dim factory As New FrameworkElementFactory(GetType(TextBlock))
            factory.SetBinding(TextBlock.TextProperty, New Binding("ProductName"))
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
                                                                'Dim duplicateExists = _productTextBoxes.Values.Any(Function(tb) tb IsNot textBox AndAlso tb.Text.Trim().ToLower() = selectedProductName)

                                                                'If duplicateExists Then
                                                                '    MessageBox.Show("This product is already added in another row.", "Duplicate Product", MessageBoxButton.OK, MessageBoxImage.Warning)
                                                                '    textBox.Clear()
                                                                '    popup.IsOpen = False
                                                                '    suggestionList.SelectedItem = Nothing
                                                                '    Return
                                                                'End If

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
                                              'Dim duplicates = _productTextBoxes.Where(Function(kvp) kvp.Value IsNot currentTextBox AndAlso kvp.Value.Text.Trim().ToLower() = currentText.ToLower())

                                              'If duplicates.Any() Then
                                              '    MessageBox.Show("This product is already added in another row.", "Duplicate Product", MessageBoxButton.OK, MessageBoxImage.Warning)
                                              '    currentTextBox.Clear()
                                              '    currentTextBox.Focus()
                                              'End If
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
        Public Function CreateInputBox(text As String, width As Double, Optional isReadOnly As Boolean = False, Optional name As String = "", Optional alignment As HorizontalAlignment = HorizontalAlignment.Left) As Border
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
        .Width = width,
        .HorizontalContentAlignment = alignment
    }

            If Not String.IsNullOrWhiteSpace(name) Then
                txt.Name = name
                _productTextBoxes(name) = txt

                Dim existingElement As Object = Me.FindName(name)
                If existingElement IsNot Nothing Then
                    Me.UnregisterName(name)
                End If

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
        .BorderBrush = If(isReadOnly, Brushes.Transparent, CType(New BrushConverter().ConvertFrom("#AEAEAE"), Brush)),
        .BorderThickness = If(isReadOnly, New Thickness(0), New Thickness(1)),
        .Background = CType(New BrushConverter().ConvertFrom("#FDFDFD"), Brush),
        .CornerRadius = New CornerRadius(5),
        .Padding = New Thickness(2),
        .Margin = New Thickness(2, 0, 2, 0),
        .Child = txt
    }

            Return border
        End Function

        ' Quantity Textbox
        Private Function CreateQuantityBox(rowIndex As Integer) As Border
            Return CreateInputBox("1", 50, False, $"txtQuantity_{rowIndex}", HorizontalAlignment.Center)
        End Function

        ' Rate Textbox
        Private Function CreateRateBox(rowIndex As Integer) As Border
            Dim box = CreateInputBox("", 90, False, $"txtRate_{rowIndex}", HorizontalAlignment.Center)
            Dim txt = TryCast(box.Child, TextBox)
            If txt IsNot Nothing Then
                AddHandler txt.TextChanged, AddressOf Quantity_TextChanged
                AddHandler txt.PreviewTextInput, AddressOf Quantity_PreviewTextInput
            End If
            Return box
        End Function

        ' Tax Percent Textbox
        Private Function CreateTaxPercentBox(rowIndex As Integer) As Border
            Dim defaultTaxPercent As String = If(Not CEtaxSelection, "", "0")

            ' Create the textbox with the default value and readonly behavior
            Dim box = CreateInputBox(defaultTaxPercent, 60, Not _TaxSelection, $"txtTaxPercent_{rowIndex}", HorizontalAlignment.Center)
            Dim txt = TryCast(box.Child, TextBox)
            If txt IsNot Nothing Then
                AddHandler txt.TextChanged, AddressOf TaxPercent_TextChanged
                AddHandler txt.PreviewTextInput, AddressOf TaxPercent_PreviewTextInput
            End If

            Return box
        End Function

        ' Tax Value Box
        Private Function CreateTaxValueBox(rowIndex As Integer) As Border
            Return CreateInputBox("0.00", 70, True, $"txtTaxValue_{rowIndex}", HorizontalAlignment.Center)
        End Function

        ' Discount Percent
        Private Function CreateDiscountPercentBox(rowIndex As Integer) As Border
            Dim box = CreateInputBox("", 75, False, $"txtDiscountPercent_{rowIndex}", HorizontalAlignment.Center)
            Dim txt = TryCast(box.Child, TextBox)
            If txt IsNot Nothing Then
                AddHandler txt.TextChanged, AddressOf DiscountPercent_TextChanged
                AddHandler txt.PreviewTextInput, AddressOf DiscountPercent_PreviewTextInput
            End If
            Return box
        End Function

        ' Discount Box
        Private Function CreateDiscountBox(rowIndex As Integer) As Border
            Return CreateInputBox("0.00", 75, True, $"txtDiscount_{rowIndex}", HorizontalAlignment.Center)
        End Function

        ' Amount Box
        Private Function CreateAmountBox(text As String, rowIndex As Integer) As Border
            Return CreateInputBox(text, 90, True, $"txtAmount_{rowIndex}", HorizontalAlignment.Center)
        End Function

        ' Deleting the buttons
        Private Function CreateDeleteButton(containerToRemoveFrom As UIElement) As Button
            Dim deleteButton As New Button With {
            .Background = Brushes.Transparent,
            .BorderBrush = Brushes.Transparent,
            .Padding = New Thickness(0),
            .Width = 50,
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
                Dim buyingPrice As Decimal = product.BuyingPrice
                rateBox.Text = buyingPrice.ToString("F2")

                If taxPercentBox IsNot Nothing Then
                    If _TaxSelection Then
                        ' Inclusive: set to default and lock
                        taxPercentBox.Text = (_SelectedTax * 100).ToString()
                        taxPercentBox.IsReadOnly = True
                    Else
                        ' Exclusive: let user type
                        taxPercentBox.Text = ""
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
            'Dim amountWithTax As Decimal

            If _TaxSelection Then
                ' Tax Exclusive: add tax to amount
                'amountWithTax = baseAmount + taxValue
                taxValue = baseAmount * (taxPercent / 100)
            Else
                ' Tax Inclusive: 12% is already in the base amount, calculate for display only
                taxValue = baseAmount * 0.12D
                'amountWithTax = baseAmount + taxValue

                ' Update tax value display
                If taxValueBox IsNot Nothing Then taxValueBox.Text = taxValue.ToString("N2")
            End If

            Dim discountValue = baseAmount * (discountPercent / 100)
            Dim finalAmount = baseAmount - discountValue

            ' Update all display boxes
            If taxValueBox IsNot Nothing Then taxValueBox.Text = taxValue.ToString("N2")
            If discountBox IsNot Nothing Then discountBox.Text = discountValue.ToString("N2")
            amountBox.Text = "₱ " & finalAmount.ToString("N2")

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
            Dim subtotalAmount As Decimal = 0
            Dim totalTaxAmount As Decimal = 0

            ' 1. Get all Amount TextBoxes and sanitize them
            Dim amountTextBoxNames = LogicalTreeHelper.GetChildren(MainContainer).OfType(Of UIElement)().
        SelectMany(Function(border) FindVisualChildren(Of TextBox)(border)).
        Where(Function(txt) txt.Name IsNot Nothing AndAlso txt.Name.StartsWith("txtAmount_")).
        Select(Function(txt) txt.Name).Distinct()

            For Each name As String In amountTextBoxNames
                Dim txtBox As TextBox = TryCast(Me.FindName(name), TextBox)
                If txtBox IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtBox.Text) Then
                    ' CLEANING: Remove Peso sign and Commas
                    Dim rawText = txtBox.Text.Replace("₱", "").Replace(",", "").Trim()
                    Dim amount As Decimal
                    If Decimal.TryParse(rawText, amount) Then
                        subtotalAmount += amount
                    End If
                End If
            Next

            ' 2. Ensure Tax is also sanitized
            UpdateTotalTax()
            Dim rawTax = txtTotalTax.Text.Replace("₱", "").Replace(",", "").Trim()
            Decimal.TryParse(rawTax, totalTaxAmount)

            Dim finalGrandTotal As Decimal = 0

            ' 3. Calculate based on Tax Selection
            If _TaxSelection Then
                ' Tax Exclusive logic: Total = Subtotal + Tax
                finalGrandTotal = subtotalAmount + totalTaxAmount
            Else
                ' Tax Inclusive logic: Total = Subtotal (Tax is already inside)
                finalGrandTotal = subtotalAmount
            End If

            BLSubtotalAmountCache = (subtotalAmount).ToString("F2")
            ' 4. Format outputs for UI display
            txtGrandTotal.Text = "₱ " & finalGrandTotal.ToString("N2")

            ' 5. Pass CLEAN values to Cache (It's better to store as Decimal or clean String)
            StatementDetails.TotalCostCache = finalGrandTotal.ToString("F2")
        End Sub

        ' This function is for updating the value of tax whenever there is changes
        Public Sub UpdateTotalTax()
            Dim totalTax As Decimal = 0
            Dim taxValueNames = LogicalTreeHelper.GetChildren(MainContainer).OfType(Of UIElement)().
        SelectMany(Function(border) FindVisualChildren(Of TextBox)(border)).
        Where(Function(txt) txt.Name IsNot Nothing AndAlso txt.Name.StartsWith("txtTaxValue_")).
        Select(Function(txt) txt.Name).Distinct()

            For Each name As String In taxValueNames
                Dim txtBox As TextBox = TryCast(Me.FindName(name), TextBox)
                If txtBox IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtBox.Text) Then
                    ' CLEANING
                    Dim rawText = txtBox.Text.Replace("₱", "").Replace(",", "").Trim()
                    Dim tax As Decimal
                    If Decimal.TryParse(rawText, tax) Then
                        totalTax += tax
                    End If
                End If
            Next

            txtTotalTax.Text = "₱ " & totalTax.ToString("N2")
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

            txtTotalDiscount.Text = "₱ " & totalDiscount.ToString("N2")
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
            If Not Char.IsDigit(e.Text, 0) OrElse tb.Text.Length >= 3 Then
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

        Private Function ValidateBillingSubmission(client As Client, productItemsJson As String) As Boolean
            If client Is Nothing Then
                MessageBox.Show("Client is required.")
                Return False
            End If

            If String.IsNullOrWhiteSpace(txtBillingNumber.Text) Then
                MessageBox.Show("Billing Number is required.")
                Return False
            End If

            If OrderDateVM.SelectedDate = DateTime.MinValue OrElse Not OrderDateVM.SelectedDate.HasValue Then
                MessageBox.Show("Billing Date is required.")
                Return False
            End If

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
                MessageBox.Show("No products found in the Billing.")
                Return False
            End If

            Return True
        End Function
#End Region

#Region "Generate the Billing Before saving"
        ' Once Done All of the Data Will Be pass to another form for generating invoice
        Private Sub GenerateBilling_Click(sender As Object, e As RoutedEventArgs)
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

            GetAllDataInBillingProperties(client, productItemsJson)
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
                productData("Delivered") = "0"
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
                                Dim rawDescription As String = descTextBox.Text.Trim()
                                If rawDescription = "Enter product description (Optional)" OrElse String.IsNullOrWhiteSpace(rawDescription) Then
                                    productData("Description") = "No Description Available"
                                Else
                                    productData("Description") = rawDescription
                                End If
                            End If
                        End If
                    End If
                End If

                productArray.Add(productData)
            Next

            Return JsonConvert.SerializeObject(productArray, Formatting.None)
        End Function
#End Region

#Region "Clearing all of the fields"
        Public Sub ClearAllFields()
            If Application.Current.Properties.Contains("BillingCache") Then
                Application.Current.Properties.Remove("BillingCache")
            End If

            ' Clear the shared list of items
            If BLItemsCache IsNot Nothing Then
                BLItemsCache.Clear()
            End If

            txtBillingNumber.Clear()
            Dim billingID As String = BillingController.GenerateBillingID(False)
            txtBillingNumber.Text = billingID
            'txtReferenceNumber.Text = "Reference #"
            txtSearchCustomer.Clear()
            txtBillingNote.Text = "None"
            txtTaxSelection.SelectedIndex = 0
            txtDiscountSelection.SelectedIndex = 0
            txtTotalTax.Text = "₱ 0.00"
            txtTotalDiscount.Text = "₱ 0.00"
            txtGrandTotal.Text = ""
            TxtClientDetails.Clear()
            ' Clear the client details
            _selectedClient = Nothing
            UpdateSupplierDetails(Nothing)
            ' Clear all product input UIs
            ClearAllRows()
            ' Reset date pickers
            OrderDateVM.SelectedDate = DateTime.Today
            OrderDueDateVM.SelectedDate = DateTime.Today.AddDays(1)

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

#Region "Getting All of the Data and Insert of this Billing"
        ' Whenever there is a change in WarehosueCombobox will also update the data
        Private Sub ComboBoxWarehouse_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim selectedItem As ComboBoxItem = TryCast(ComboBoxWarehouse.SelectedItem, ComboBoxItem)

            If selectedItem IsNot Nothing Then
                WarehouseID = Convert.ToInt32(selectedItem.Tag)
                WarehouseName = selectedItem.Content.ToString()
            End If
        End Sub

        Private Sub GetAllDataInBillingProperties(client As Client, productItemsJson As String)
            If Not ValidateBillingSubmission(client, productItemsJson) Then Exit Sub
            Try
                Dim selectedTax As String = CType(txtTaxSelection.SelectedItem, ComboBoxItem).Content.ToString()
                Dim selectedDiscount As String = CType(txtDiscountSelection.SelectedItem, ComboBoxItem).Content.ToString()



                BLNumberCache = txtBillingNumber.Text
                BLDiscountProperty = txtDiscountSelection.Text
                BLTaxProperty = txtTaxSelection.Text
                BLDateCache = OrderDateVM.SelectedDate.Value.ToString("yyyy-MM-dd")
                BLTotalTaxValueCache = txtTotalTax.Text
                BLTotalDiscountValueCache = txtTotalDiscount.Text
                BLTotalAmountCache = txtGrandTotal.Text
                BLnoteTxt = txtBillingNote.Text
                BLpaymentTerms = "None"
                BLItemsCache = JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(productItemsJson)
                BLsignature = False ' Assuming no signature for now
                BLImageCache = "" ' Assuming no image for now
                BLPathCache = "" ' Assuming no path for now
                'ils
                BLClientIDCache = client.ClientID
                BLClientName = client.Name
                Dim stringArray As List(Of String) = client.BillingAddress.Split(","c).Select(Function(s) s.Trim()).ToList()

                WalkinBillingStatementDetails.BLAddress = stringArray(0)
                WalkinBillingStatementDetails.BLCity = stringArray(1)
                WalkinBillingStatementDetails.BLRegion = stringArray(2)
                WalkinBillingStatementDetails.BLCountry = stringArray(3)
                BLPhone = client.Phone
                BLEmail = client.Email
                Dim Warehouse As ComboBoxItem = CType(ComboBoxWarehouse.SelectedItem, ComboBoxItem)
                Dim selectedWarehouse As String = Warehouse.Content.ToString()
                WalkinBillingStatementDetails.BLWarehouseNameCache = selectedWarehouse

                Dim selectedTaxType As String = CType(txtTaxSelection.SelectedItem, ComboBoxItem).Content.ToString()

                If selectedTaxType = "Exclusive" Then
                    WalkinBillingStatementDetails.BLVatLabel = $"VAT EXCLUSIVE"
                    WalkinBillingStatementDetails.BLSubtotalLabel = "SUBTOTAL VAT EX."
                ElseIf selectedTaxType = "Inclusive" Then
                    WalkinBillingStatementDetails.BLVatLabel = "VAT 12%"
                    WalkinBillingStatementDetails.BLSubtotalLabel = "SUBTOTAL VAT IN."
                End If

                ViewLoader.DynamicView.NavigateToView("navigatetobillingstatement", Me)
            Catch ex As Exception
                MessageBox.Show("Please Fill up all of the Fields")
            End Try
        End Sub

        Private Sub BtnAddClient_Click(sender As Object, e As RoutedEventArgs) Handles BtnAddClient.Click
            ViewLoader.DynamicView.NavigateToView("newwalkinclient", Me)
        End Sub
        Private Sub BtnReset_Click(sender As Object, e As RoutedEventArgs) Handles BtnReset.Click
            ClearAllFields()
            ViewLoader.DynamicView.NavigateToView("walkinorder", Me)
        End Sub

        Private Sub LoadCachedBillingData()
            If Application.Current.Properties.Contains("BillingCache") Then
                Dim cachedData As BillingModel = DirectCast(Application.Current.Properties("BillingCache"), BillingModel)

                txtBillingNumber.Text = cachedData.BillingNumber
                RemoveHandler txtSearchCustomer.TextChanged, AddressOf txtSearchCustomer_TextChanged

                If Not String.IsNullOrEmpty(cachedData.ClientID) Then
                    Dim clientList = ClientController.SearchClient(cachedData.ClientID)
                    Dim targetClient = clientList.FirstOrDefault(Function(c) c.ClientID = cachedData.ClientID)

                    If targetClient IsNot Nothing Then
                        txtSearchCustomer.Text = targetClient.Name

                        _selectedClient = targetClient
                        UpdateSupplierDetails(_selectedClient)
                    End If
                End If
                AddHandler txtSearchCustomer.TextChanged, AddressOf txtSearchCustomer_TextChanged

                If Not String.IsNullOrEmpty(cachedData.OrderItems) Then
                    ClearAllRows()
                    BLItemsCache = JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(cachedData.OrderItems)
                    LoadCachedBillingItems()
                End If

                Application.Current.Properties.Remove("BillingCache")

                AutoCompletePopup.IsOpen = False
            End If
        End Sub
#End Region
    End Class
End Namespace
