Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Linq
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DocumentFormat.OpenXml.Bibliography
Imports DocumentFormat.OpenXml.Math
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Views.Stocks
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json
Imports SharpCompress.Readers.Tar

Namespace DPC.Views.Sales.Quotes
    ' Note - The value of defau
    Public Class NewQuoteGovernment
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
        'New Category Variables()
        Private _categoryRowCounters As New Dictionary(Of String, Integer)
        Private _categories As New Dictionary(Of Integer, CategorySection)
        Private _categoryIdCounter As Integer = 0

        'Private txtCategoryInput As TextBox
        'Private TaxHeader As DataGridTextColumn

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

        Public Class CategorySection
            Public CategoryId As Integer
            Public CategoryName As String
            Public CategoryBorder As Border
            Public CategoryNameTextBox As TextBox
            Public CategoryNamePanel As StackPanel  ' NEW
            Public ControlsPanel As StackPanel      ' NEW
            Public Separator As Border              ' NEW
            Public ProductsPanel As StackPanel
            Public SubtotalTextBlock As TextBlock
            Public AddRowButton As Button
            Public RowCount As Integer = 0
        End Class


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

            'TaxHeader.Header = If(_TaxSelection, "Tax(%)", "Tax(12%)")

            Debug.WriteLine($"Tax Selection - {_TaxSelection}")
            Debug.WriteLine($"Tax Value In Quote Properties - {_SelectedTax}")
        End Sub

        Private Sub NewQuoteGovernment_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        End Sub

        Private Sub UpdateAllCategoryTaxHeaders()
            ' When Inclusive (_TaxSelection = False) → "Tax(12%)"
            ' When Exclusive (_TaxSelection = True) → "Tax(%)"
            Dim taxHeaderText As String = If(_TaxSelection, "Tax(%)", "Tax(12%)")

            For Each kvp In _categories
                Dim category = kvp.Value

                ' Find the header grid in the category
                If category.CategoryBorder IsNot Nothing Then
                    Dim productPanelStack = TryCast(category.CategoryBorder.Child, StackPanel)
                    If productPanelStack IsNot Nothing AndAlso productPanelStack.Children.Count > 0 Then
                        Dim headerBorder = TryCast(productPanelStack.Children(0), Border)
                        If headerBorder IsNot Nothing Then
                            Dim headerGrid = TryCast(headerBorder.Child, Grid)
                            If headerGrid IsNot Nothing Then
                                ' Find the 4th column header (index 3) which is Tax column
                                For Each child In headerGrid.Children
                                    Dim headerText = TryCast(child, TextBlock)
                                    If headerText IsNot Nothing Then
                                        Dim colIndex As Integer = Grid.GetColumn(headerText)
                                        If colIndex = 3 Then ' Tax percent column
                                            headerText.Text = taxHeaderText
                                            Exit For
                                        End If
                                    End If
                                Next
                            End If
                        End If
                    End If
                End If
            Next
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
            ViewLoader.DynamicView.NavigateToView("costestimategovernment", Me)
        End Sub

        Private Sub NavigateToCostEstimate1(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("costestimategovernment", Me)
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

            ' Update all category headers (Tax(12%) for Inclusive, Tax(%) for Exclusive)
            UpdateAllCategoryTaxHeaders()

            ' Update all tax percent textboxes
            For Each kvp In _productTextBoxes
                If kvp.Key.StartsWith("txtTaxPercent_") Then
                    If _TaxSelection Then
                        ' Exclusive: Allow user to edit
                        kvp.Value.Text = "0"
                        kvp.Value.IsReadOnly = False
                        CEtaxSelection = True
                        ShowVatExBtn.Visibility = Visibility.Visible
                    Else
                        ' Inclusive: Make readonly and clear value
                        kvp.Value.Text = ""
                        kvp.Value.IsReadOnly = True
                        CEtaxSelection = False
                        ShowVatExBtn.Visibility = Visibility.Collapsed
                        CEisVatExInclude = False
                    End If
                End If
            Next

            ' Recalculate all amounts in all categories
            For Each categoryKvp In _categories
                Dim category = categoryKvp.Value
                If category.ProductsPanel IsNot Nothing Then
                    For i As Integer = 1 To category.RowCount
                        Dim textboxes As New Dictionary(Of String, TextBox) From {
                    {"quantity", FindTextBoxByName($"txtQuantity_{category.CategoryId}_{i}")},
                    {"price", FindTextBoxByName($"txtUnitPrice_{category.CategoryId}_{i}")},
                    {"taxPercent", FindTextBoxByName($"txtTaxPercent_{category.CategoryId}_{i}")},
                    {"taxValue", FindTextBoxByName($"txtTaxValue_{category.CategoryId}_{i}")},
                    {"discountPercent", FindTextBoxByName($"txtDiscountPercent_{category.CategoryId}_{i}")},
                    {"discount", FindTextBoxByName($"txtDiscount_{category.CategoryId}_{i}")},
                    {"amount", FindTextBoxByName($"txtAmount_{category.CategoryId}_{i}")}
                }

                        If textboxes.Values.All(Function(tb) tb IsNot Nothing) Then
                            CalculateCategoryAmount(category.CategoryId, i, textboxes)
                        End If
                    Next
                End If
            Next
        End Sub
#End Region

#Region "This Loads every data if its available for updating"
        Private Sub InitializeProductUI()
            RestoreProjectIDAndSubject()
            If HasCachedItems() Then
                If _typingTimer Is Nothing Then
                    _typingTimer = New DispatcherTimer()
                    _typingTimer.Interval = TimeSpan.FromMilliseconds(300)
                    AddHandler _typingTimer.Tick, AddressOf OnTypingTimerTick
                End If

                FillClientsField()
                LoadCachedQuoteItems()
            Else
                AddNewCategory_Click(Nothing, Nothing)
            End If
        End Sub

        Private Sub RestoreProjectIDAndSubject()
            Try
                ' Restore Project ID
                If Not String.IsNullOrWhiteSpace(CostEstimateDetails.CEProjectID) Then
                    TxtProjectID.Text = CostEstimateDetails.CEProjectID
                    Debug.WriteLine($"✓ Restored Project ID: {CostEstimateDetails.CEProjectID}")
                End If

                ' Restore Subject
                If Not String.IsNullOrWhiteSpace(CostEstimateDetails.CESubject) Then
                    TxtSubjectDetails.Text = CostEstimateDetails.CESubject
                    Debug.WriteLine($"✓ Restored Subject: {CostEstimateDetails.CESubject}")
                End If

            Catch ex As Exception
                Debug.WriteLine($"Error restoring Project ID and Subject: {ex.Message}")
            End Try
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

        Private Sub AddProductInputUI()
            ' This method is deprecated - use AddNewCategory_Click instead
            AddNewCategory_Click(Nothing, Nothing)
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
        

        ' Create the complete category UI section
#Region "Category Management - FIXED"

        Private Sub AddNewCategory_Click(sender As Object, e As RoutedEventArgs)
            If MainContainer.Visibility = Visibility.Collapsed Then
                MainContainer.Visibility = Visibility.Visible
            End If

            _categoryIdCounter += 1
            Dim categoryId As Integer = _categoryIdCounter

            Dim newCategory As New CategorySection With {
            .CategoryId = categoryId,
            .CategoryName = "",
            .RowCount = 0
        }

            _categories.Add(categoryId, newCategory)
            CreateCategoryUI(categoryId, newCategory)
        End Sub

        Private Sub CreateCategoryUI(categoryId As Integer, categoryData As CategorySection)
            ' ===== CATEGORY NAME INPUT (OUTSIDE THE BOX) =====
            Dim categoryNamePanel As New StackPanel With {
        .Orientation = Orientation.Vertical,
        .Margin = New Thickness(0, 0, 0, 10)
    }

            Dim categoryLabel As New TextBlock With {
        .Text = "Category:",
        .FontWeight = FontWeights.SemiBold,
        .FontFamily = New FontFamily("Lexend"),
        .FontSize = 16,
        .Foreground = CType(New BrushConverter().ConvertFrom("#555555"), Brush),
        .Margin = New Thickness(0, 0, 0, 10)
    }
            categoryNamePanel.Children.Add(categoryLabel)

            Dim categoryNameBorder As New Border With {
        .Margin = New Thickness(0, 0, 0, 15),
        .Padding = New Thickness(10),
        .Height = 40,
        .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
        .BorderThickness = New Thickness(2),
        .CornerRadius = New CornerRadius(20),
        .Background = Brushes.White
    }

            Dim categoryNameTextBox As New TextBox With {
        .Name = $"txtCategory_{categoryId}",
        .BorderThickness = New Thickness(0),
        .Background = Brushes.Transparent,
        .FontFamily = New FontFamily("Lexend"),
        .FontSize = 14,
        .Foreground = Brushes.Black,
        .TextWrapping = TextWrapping.Wrap,
        .Padding = New Thickness(5),
        .Height = 22,
        .VerticalAlignment = VerticalAlignment.Center,
        .HorizontalAlignment = HorizontalAlignment.Stretch
    }

            categoryNameBorder.Child = categoryNameTextBox
            categoryNamePanel.Children.Add(categoryNameBorder)

            ' ===== PRODUCT PANEL BORDER (THE BOX) =====
            Dim productPanelBorder As New Border With {
        .BorderBrush = CType(New BrushConverter().ConvertFrom("#474747"), Brush),
        .BorderThickness = New Thickness(2),
        .Background = Brushes.White,
        .CornerRadius = New CornerRadius(15),
        .Padding = New Thickness(10),
        .Margin = New Thickness(0, 0, 0, 15)
    }

            Dim productPanelStack As New StackPanel With {
        .Orientation = Orientation.Vertical
    }

            ' ===== HEADER ROW =====
            Dim headerBorder As New Border With {
        .Background = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
        .CornerRadius = New CornerRadius(10),
        .Margin = New Thickness(-10, -10, -10, 10)
    }

            Dim headerGrid As New Grid With {
        .Height = 40
    }

            Dim columns As New List(Of ColumnDefinition) From {
        New ColumnDefinition With {.Width = New GridLength(2, GridUnitType.Star)},
        New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)},
        New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)},
        New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)},
        New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)},
        New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)},
        New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)},
        New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)},
        New ColumnDefinition With {.Width = New GridLength(80, GridUnitType.Pixel)}
    }

            For Each col In columns
                headerGrid.ColumnDefinitions.Add(col)
            Next

            Dim headers = {"Item Name", "Quantity", "Unit Price", "Tax(%)", "Tax", "Discount(%)", "Discount", "Amount(₱)", ""}

            For i As Integer = 0 To headers.Length - 1
                Dim headerText As New TextBlock With {
            .Text = headers(i),
            .FontFamily = New FontFamily("Lexend"),
            .FontSize = 14,
            .FontWeight = FontWeights.SemiBold,
            .Foreground = CType(New BrushConverter().ConvertFrom("#FDFDFD"), Brush),
            .VerticalAlignment = VerticalAlignment.Center,
            .HorizontalAlignment = HorizontalAlignment.Center,
            .Padding = New Thickness(10)
        }
                Grid.SetColumn(headerText, i)
                headerGrid.Children.Add(headerText)
            Next

            headerBorder.Child = headerGrid
            productPanelStack.Children.Add(headerBorder)

            ' ===== PRODUCTS CONTAINER =====
            Dim productsPanel As New StackPanel With {
        .Orientation = Orientation.Vertical,
        .Margin = New Thickness(0, 0, 0, 0),
        .Name = $"ProductsPanel_{categoryId}"
    }

            categoryData.ProductsPanel = productsPanel
            AddProductRowToUI(categoryId, productsPanel)
            productPanelStack.Children.Add(productsPanel)

            productPanelBorder.Child = productPanelStack

            ' ===== BUTTONS AND SUBTOTAL ROW (OUTSIDE THE BOX) =====
            Dim controlsPanel As New StackPanel With {
        .Orientation = Orientation.Horizontal,
        .HorizontalAlignment = HorizontalAlignment.Stretch,
        .Margin = New Thickness(0, 0, 0, 15),
        .Height = 40
    }

            ' Add Row Button
            Dim buttonBorder As New Border With {
        .Background = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
        .CornerRadius = New CornerRadius(10),
        .Padding = New Thickness(0),
        .Margin = New Thickness(0, 0, 20, 0),
        .Width = 130,
        .Height = 40
    }

            Dim addRowButton As New Button With {
        .Background = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
        .Width = 130,
        .Height = 40,
        .BorderThickness = New Thickness(0),
        .Cursor = Cursors.Hand,
        .Foreground = Brushes.White,
        .Margin = New Thickness(20, 0, 20, 0)
    }

            Dim addRowPanel As New StackPanel With {
        .Orientation = Orientation.Horizontal,
        .HorizontalAlignment = HorizontalAlignment.Center
    }

            Dim addIcon As New MaterialDesignThemes.Wpf.PackIcon With {
        .Kind = MaterialDesignThemes.Wpf.PackIconKind.PlaylistAdd,
        .Foreground = Brushes.White,
        .Width = 30,
        .Height = 30,
        .Margin = New Thickness(-50, 0, 5, 0)
    }
            addRowPanel.Children.Add(addIcon)

            Dim addText As New TextBlock With {
        .Text = "Row",
        .FontSize = 16,
        .VerticalAlignment = VerticalAlignment.Center,
        .FontWeight = FontWeights.SemiBold,
        .Foreground = Brushes.White,
        .Margin = New Thickness(-30, 0, 5, 0)
    }
            addRowPanel.Children.Add(addText)

            addRowButton.Content = addRowPanel
            buttonBorder.Child = addRowButton

            AddHandler addRowButton.Click, Sub(s As Object, args As RoutedEventArgs)
                                               AddProductRowToUI(categoryId, productsPanel)
                                           End Sub

            controlsPanel.Children.Add(buttonBorder)

            ' Subtotal section
            Dim subtotalPanel As New StackPanel With {
        .Orientation = Orientation.Horizontal,
        .HorizontalAlignment = HorizontalAlignment.Right,
        .Margin = New Thickness(30, 0, 0, 0)
    }

            Dim subtotalLabel As New TextBlock With {
        .Text = "Subtotal (₱): ",
        .FontSize = 16,
        .FontWeight = FontWeights.SemiBold,
        .Foreground = CType(New BrushConverter().ConvertFrom("#474747"), Brush),
        .VerticalAlignment = VerticalAlignment.Center,
        .Margin = New Thickness(0, 0, 20, 0)
    }
            subtotalPanel.Children.Add(subtotalLabel)

            Dim subtotalValue As New TextBlock With {
        .Text = "₱ 0.00",
        .FontSize = 16,
        .FontWeight = FontWeights.Bold,
        .Foreground = CType(New BrushConverter().ConvertFrom("#090909"), Brush),
        .VerticalAlignment = VerticalAlignment.Center,
        .Name = $"Subtotal_{categoryId}"
    }
            subtotalPanel.Children.Add(subtotalValue)
            categoryData.SubtotalTextBlock = subtotalValue

            controlsPanel.Children.Add(subtotalPanel)

            ' Separator line between categories
            Dim separator As New Border With {
        .Height = 2,
        .Background = CType(New BrushConverter().ConvertFrom("#E0E0E0"), Brush),
        .Margin = New Thickness(0, 0, 0, 20)
    }

            ' Store references
            categoryData.CategoryBorder = productPanelBorder
            categoryData.CategoryNameTextBox = categoryNameTextBox
            categoryData.CategoryNamePanel = categoryNamePanel
            categoryData.ControlsPanel = controlsPanel
            categoryData.Separator = separator

            ' Add all elements directly to MainContainer
            MainContainer.Children.Add(categoryNamePanel)
            MainContainer.Children.Add(productPanelBorder)
            MainContainer.Children.Add(controlsPanel)
            MainContainer.Children.Add(separator)
        End Sub

        Private Sub AddProductRowToUI(categoryId As Integer, productsPanel As StackPanel)
            If Not _categories.ContainsKey(categoryId) Then Return

            Dim category = _categories(categoryId)
            category.RowCount += 1
            Dim rowIndex As Integer = category.RowCount

            ' Create main row container
            Dim rowBorder As New Border With {
                .Margin = New Thickness(0, 0, 0, 10),
                .BorderBrush = CType(New BrushConverter().ConvertFrom("#E0E0E0"), Brush),
                .BorderThickness = New Thickness(1),
                .CornerRadius = New CornerRadius(5),
                .Padding = New Thickness(0)
            }

            ' Product row horizontal stack panel
            Dim productPanel As New StackPanel With {
                .Orientation = Orientation.Horizontal,
                .HorizontalAlignment = HorizontalAlignment.Stretch
            }

            ' Dictionary to store textboxes for calculations
            Dim textboxes As New Dictionary(Of String, TextBox)

            ' 1. Item Name
            Dim itemNameBorder = CreateProductSearchBoxForCategory(150, categoryId, rowIndex)
            productPanel.Children.Add(itemNameBorder)

            ' 2. Quantity Box
            Dim quantityBox = CreateInputBox("1", 50, False, $"txtQuantity_{categoryId}_{rowIndex}")
            Dim quantityTxt = TryCast(quantityBox.Child, TextBox)
            textboxes("quantity") = quantityTxt
            productPanel.Children.Add(quantityBox)

            ' 3. Unit Price
            Dim priceBox = CreateInputBox("", 70, False, $"txtUnitPrice_{categoryId}_{rowIndex}")
            Dim priceTxt = TryCast(priceBox.Child, TextBox)
            textboxes("price") = priceTxt
            AddHandler priceTxt.TextChanged, AddressOf Quantity_TextChanged
            AddHandler priceTxt.PreviewTextInput, AddressOf Quantity_PreviewTextInput
            productPanel.Children.Add(priceBox)

            ' 4. Tax Percent
            Dim defaultTaxPercent As String = If(Not CEtaxSelection, "", "0")
            Dim taxPercentBox = CreateInputBox(defaultTaxPercent, 40, Not _TaxSelection, $"txtTaxPercent_{categoryId}_{rowIndex}")
            Dim taxPercentTxt = TryCast(taxPercentBox.Child, TextBox)
            textboxes("taxPercent") = taxPercentTxt
            taxPercentBox.BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush)
            taxPercentBox.BorderThickness = New Thickness(2)
            AddHandler taxPercentTxt.TextChanged, AddressOf TaxPercent_TextChanged
            AddHandler taxPercentTxt.PreviewTextInput, AddressOf TaxPercent_PreviewTextInput
            productPanel.Children.Add(taxPercentBox)

            ' 5. Tax Value (Read-Only)
            Dim taxValueBox = CreateInputBox("0.00", 75, True, $"txtTaxValue_{categoryId}_{rowIndex}")
            Dim taxValueTxt = TryCast(taxValueBox.Child, TextBox)
            textboxes("taxValue") = taxValueTxt
            productPanel.Children.Add(taxValueBox)

            ' 6. Discount Percent
            Dim discountPercentBox = CreateInputBox("0", 40, False, $"txtDiscountPercent_{categoryId}_{rowIndex}")
            Dim discountPercentTxt = TryCast(discountPercentBox.Child, TextBox)
            textboxes("discountPercent") = discountPercentTxt
            AddHandler discountPercentTxt.TextChanged, AddressOf DiscountPercent_TextChanged
            AddHandler discountPercentTxt.PreviewTextInput, AddressOf DiscountPercent_PreviewTextInput
            productPanel.Children.Add(discountPercentBox)

            ' 7. Discount (Read-Only)
            Dim discountBox = CreateInputBox("0.00", 75, True, $"txtDiscount_{categoryId}_{rowIndex}")
            Dim discountTxt = TryCast(discountBox.Child, TextBox)
            textboxes("discount") = discountTxt
            productPanel.Children.Add(discountBox)

            ' 8. Amount (Read-Only)
            Dim amountBox = CreateInputBox("₱0.00", 80, True, $"txtAmount_{categoryId}_{rowIndex}")
            Dim amountTxt = TryCast(amountBox.Child, TextBox)
            textboxes("amount") = amountTxt
            productPanel.Children.Add(amountBox)

            ' 9. Delete Button
            Dim deleteBtn As New Button With {
                .Content = "✕",
                .Background = Brushes.Transparent,
                .Foreground = CType(New BrushConverter().ConvertFrom("#D23636"), Brush),
                .FontSize = 18,
                .FontWeight = FontWeights.Bold,
                .Cursor = Cursors.Hand,
                .Padding = New Thickness(0),
                .BorderThickness = New Thickness(0),
                .Width = 40,
                .Height = 40,
                .VerticalAlignment = VerticalAlignment.Center,
                .HorizontalAlignment = HorizontalAlignment.Center,
                .FontFamily = New FontFamily("Lexend"),
                .Margin = New Thickness(5, 0, 0, 0)
            }
            productPanel.Children.Add(deleteBtn)

            rowBorder.Child = productPanel

            ' Add change handlers for calculation
            For Each textbox In textboxes.Values
                AddHandler textbox.TextChanged, Sub(s As Object, e As TextChangedEventArgs)
                                                    CalculateCategoryAmount(categoryId, rowIndex, textboxes)
                                                End Sub
            Next

            ' Add the product row
            productsPanel.Children.Add(rowBorder)

            ' ===== DESCRIPTION BOX SECTION =====
            Dim descriptionTextBox As New TextBox With {
                .Name = $"txtDescription_{categoryId}_{rowIndex}",
                .Text = "Enter product description (Optional)",
                .BorderThickness = New Thickness(0),
                .Background = Brushes.Transparent,
                .FontFamily = New FontFamily("Lexend"),
                .FontSize = 12,
                .Foreground = CType(New BrushConverter().ConvertFrom("#666666"), Brush),
                .FontWeight = FontWeights.Normal,
                .Height = Double.NaN,
                .MinHeight = 80,
                .MaxHeight = 150,
                .VerticalAlignment = VerticalAlignment.Top,
                .HorizontalAlignment = HorizontalAlignment.Stretch,
                .Width = Double.NaN,
                .TextWrapping = TextWrapping.Wrap,
                .AcceptsReturn = True,
                .VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                .MaxLength = 2000,
                .Padding = New Thickness(8),
                .Margin = New Thickness(0, 0, 0, 10)
            }

            Dim descriptionBorder As New Border With {
                .BorderBrush = CType(New BrushConverter().ConvertFrom("#E0E0E0"), Brush),
                .BorderThickness = New Thickness(1),
                .CornerRadius = New CornerRadius(5),
                .Padding = New Thickness(0),
                .Width = Double.NaN,
                .MinHeight = 80,
                .MaxHeight = 150,
                .Background = Brushes.White,
                .Child = descriptionTextBox
            }

            AddHandler descriptionTextBox.GotFocus, Sub(sender As Object, e As RoutedEventArgs)
                                                        If descriptionTextBox.Text = "Enter product description (Optional)" Then
                                                            descriptionTextBox.Text = ""
                                                            descriptionTextBox.Foreground = Brushes.Black
                                                        End If
                                                    End Sub

            AddHandler descriptionTextBox.LostFocus, Sub(sender As Object, e As RoutedEventArgs)
                                                         If String.IsNullOrEmpty(descriptionTextBox.Text) Then
                                                             descriptionTextBox.Text = "Enter product description (Optional)"
                                                             descriptionTextBox.Foreground = CType(New BrushConverter().ConvertFrom("#666666"), Brush)
                                                         End If
                                                     End Sub

            productsPanel.Children.Add(descriptionBorder)

            ' Delete handler
            AddHandler deleteBtn.Click, Sub(s As Object, e As RoutedEventArgs)
                                            productsPanel.Children.Remove(rowBorder)
                                            productsPanel.Children.Remove(descriptionBorder)
                                            UpdateCategorySubtotal(categoryId)
                                        End Sub
        End Sub

        Private Sub DeleteCategory(categoryId As Integer)
            If Not _categories.ContainsKey(categoryId) Then Return

            Dim category = _categories(categoryId)

            ' Remove all UI elements from MainContainer
            If category.CategoryNamePanel IsNot Nothing Then
                MainContainer.Children.Remove(category.CategoryNamePanel)
            End If

            If category.CategoryBorder IsNot Nothing Then
                MainContainer.Children.Remove(category.CategoryBorder)
            End If

            If category.ControlsPanel IsNot Nothing Then
                MainContainer.Children.Remove(category.ControlsPanel)
            End If

            If category.Separator IsNot Nothing Then
                MainContainer.Children.Remove(category.Separator)
            End If

            ' Remove from dictionary
            _categories.Remove(categoryId)

            ' Update totals
            UpdateCategoryGrandTotal()
        End Sub

        Public Function CreateProductSearchBoxForCategory(width As Double, categoryId As Integer, rowIndex As Integer) As Border
            Dim textBoxName As String = $"txtProductName_{categoryId}_{rowIndex}"
            Dim popupKey As String = $"ProductPopup_{categoryId}_{rowIndex}"
            Dim listBoxKey As String = $"LstProducts_{categoryId}_{rowIndex}"
            Dim timerKey As String = $"ProductTimer_{categoryId}_{rowIndex}"

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
                                                     Try
                                                         Dim results = QuotesController.SearchProductsByNameAllWarehouses(keyword)
                                                         Debug.WriteLine($"[Cat {categoryId} Row {rowIndex}] Search returned {results.Count} results")

                                                         suggestionList.ItemsSource = results
                                                         suggestionList.Visibility = If(results.Count > 0, Visibility.Visible, Visibility.Collapsed)
                                                         popup.IsOpen = results.Count > 0
                                                     Catch ex As Exception
                                                         Debug.WriteLine($"[Cat {categoryId} Row {rowIndex}] ERROR: {ex.Message}")
                                                         popup.IsOpen = False
                                                     End Try
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

            AddHandler textBox.TextChanged, Sub(sender As Object, e As TextChangedEventArgs)
                                                typingTimer.Stop()
                                                typingTimer.Start()
                                            End Sub

            AddHandler suggestionList.SelectionChanged, Sub(sender As Object, e As SelectionChangedEventArgs)
                                                            If suggestionList.SelectedItem IsNot Nothing Then
                                                                Dim selectedProduct = CType(suggestionList.SelectedItem, ProductDataModel)
                                                                textBox.Text = selectedProduct.ProductName
                                                                popup.IsOpen = False
                                                                suggestionList.SelectedItem = Nothing

                                                                Try
                                                                    Dim productInfo = QuotesController.GetProductDetailsByProductIDAllWarehouses(selectedProduct.ProductID)
                                                                    If productInfo.Count > 0 Then
                                                                        Dim p = productInfo.First()
                                                                        SetProductDetailsForCategory(categoryId, rowIndex, p)
                                                                    End If
                                                                Catch ex As Exception
                                                                    Debug.WriteLine($"Error getting product details: {ex.Message}")
                                                                End Try
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


        Private Sub SetProductDetailsForCategory(categoryId As Integer, rowIndex As Integer, product As ProductDataModel)
            Dim rateBox = TryCast(FindTextBoxByName($"txtUnitPrice_{categoryId}_{rowIndex}"), TextBox)
            Dim taxPercentBox = TryCast(FindTextBoxByName($"txtTaxPercent_{categoryId}_{rowIndex}"), TextBox)

            If rateBox IsNot Nothing Then
                rateBox.Text = product.SellingPrice.ToString("F2")

                If taxPercentBox IsNot Nothing Then
                    If Not _TaxSelection Then
                        taxPercentBox.IsReadOnly = True
                    Else
                        taxPercentBox.Text = "0"
                        taxPercentBox.IsReadOnly = False
                    End If
                End If

                ' Get the textboxes dictionary for this row
                Dim textboxes As New Dictionary(Of String, TextBox) From {
            {"quantity", FindTextBoxByName($"txtQuantity_{categoryId}_{rowIndex}")},
            {"price", rateBox},
            {"taxPercent", taxPercentBox},
            {"taxValue", FindTextBoxByName($"txtTaxValue_{categoryId}_{rowIndex}")},
            {"discountPercent", FindTextBoxByName($"txtDiscountPercent_{categoryId}_{rowIndex}")},
            {"discount", FindTextBoxByName($"txtDiscount_{categoryId}_{rowIndex}")},
            {"amount", FindTextBoxByName($"txtAmount_{categoryId}_{rowIndex}")}
        }

                CalculateCategoryAmount(categoryId, rowIndex, textboxes)
            End If
        End Sub

        Private Sub CalculateCategoryAmount(categoryId As Integer, rowIndex As Integer, textboxes As Dictionary(Of String, TextBox))
            Try
                Dim quantity As Decimal = 0
                Dim unitPrice As Decimal = 0
                Dim taxPercent As Decimal = 0
                Dim discountPercent As Decimal = 0

                Decimal.TryParse(textboxes("quantity").Text, quantity)
                Decimal.TryParse(textboxes("price").Text, unitPrice)
                Decimal.TryParse(textboxes("taxPercent").Text, taxPercent)
                Decimal.TryParse(textboxes("discountPercent").Text, discountPercent)

                Dim baseAmount = quantity * unitPrice
                Dim taxValue As Decimal = 0
                Dim amountBeforeDiscount As Decimal = baseAmount

                If _TaxSelection Then
                    taxValue = baseAmount * (taxPercent / 100)
                    amountBeforeDiscount = baseAmount + taxValue
                Else
                    taxValue = baseAmount * 0.12D
                    amountBeforeDiscount = baseAmount + taxValue
                End If

                Dim discountValue = amountBeforeDiscount * (discountPercent / 100)
                Dim finalAmount = amountBeforeDiscount - discountValue

                textboxes("taxValue").Text = taxValue.ToString("N2")
                textboxes("discount").Text = discountValue.ToString("N2")
                textboxes("amount").Text = "₱ " & finalAmount.ToString("N2")

                UpdateCategorySubtotal(categoryId)

            Catch ex As Exception
                Debug.WriteLine($"Error in CalculateCategoryAmount: {ex.Message}")
            End Try
        End Sub

        Private Sub UpdateCategorySubtotal(categoryId As Integer)
            If Not _categories.ContainsKey(categoryId) Then Return

            Dim category = _categories(categoryId)
            Dim subtotal As Decimal = 0

            If category.ProductsPanel IsNot Nothing Then
                For Each border As Border In category.ProductsPanel.Children.OfType(Of Border)().Where(Function(b) b.BorderThickness.Left > 0)
                    Dim stackPanel = TryCast(border.Child, StackPanel)
                    If stackPanel IsNot Nothing Then
                        For Each fieldBorder As Border In stackPanel.Children.OfType(Of Border)()
                            Dim txtBox = TryCast(fieldBorder.Child, TextBox)
                            If txtBox IsNot Nothing AndAlso txtBox.Name.StartsWith("txtAmount_") Then
                                Dim amountText = txtBox.Text.Replace("₱", "").Trim()
                                Dim amount As Decimal
                                If Decimal.TryParse(amountText, amount) Then
                                    subtotal += amount
                                End If
                            End If
                        Next
                    End If
                Next
            End If

            If category.SubtotalTextBlock IsNot Nothing Then
                category.SubtotalTextBlock.Text = "₱ " & subtotal.ToString("N2")
            End If

            UpdateCategoryGrandTotal()
        End Sub

        Private Sub UpdateCategoryGrandTotal()
            Dim grandTotal As Decimal = 0

            For Each kvp In _categories
                Dim category = kvp.Value
                If category.SubtotalTextBlock IsNot Nothing Then
                    Dim subtotalText = category.SubtotalTextBlock.Text.Replace("₱", "").Trim()
                    Dim subtotal As Decimal
                    If Decimal.TryParse(subtotalText, subtotal) Then
                        grandTotal += subtotal
                    End If
                End If
            Next

            txtGrandTotal.Text = "₱ " & grandTotal.ToString("N2")
        End Sub

#End Region


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
                                                     Try
                                                         ' Use the new AllWarehouses function for government quotes
                                                         Dim results = QuotesController.SearchProductsByNameAllWarehouses(keyword)

                                                         Debug.WriteLine($"[Row {rowIndex}] Search returned {results.Count} results")

                                                         suggestionList.ItemsSource = results
                                                         suggestionList.Visibility = If(results.Count > 0, Visibility.Visible, Visibility.Collapsed)
                                                         popup.IsOpen = results.Count > 0

                                                     Catch ex As Exception
                                                         Debug.WriteLine($"[Row {rowIndex}] ERROR in SearchProductsByNameAllWarehouses: {ex.Message}")
                                                         MessageBox.Show($"Error searching products: {ex.Message}", "Search Error", MessageBoxButton.OK, MessageBoxImage.Error)
                                                         popup.IsOpen = False
                                                     End Try
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

                                                                ' Use the new AllWarehouses function for government quotes
                                                                Try
                                                                    Dim productInfo = QuotesController.GetProductDetailsByProductIDAllWarehouses(selectedProduct.ProductID)

                                                                    Debug.WriteLine($"[Row {rowIndex}] GetProductDetails returned {productInfo.Count} results")

                                                                    If productInfo.Count > 0 Then
                                                                        Dim p = productInfo.First()
                                                                        ' UI For setting the details
                                                                        SetProductDetails(rowIndex, p)
                                                                        Debug.WriteLine("== Product Info Retrieved ==")
                                                                        Debug.WriteLine("Name: " & p.ProductName)
                                                                        Debug.WriteLine("Selling Price: " & p.SellingPrice)
                                                                        Debug.WriteLine("Tax: " & p.DefaultTax)
                                                                        Debug.WriteLine("Stock Unit: " & p.StockUnits)
                                                                    Else
                                                                        Debug.WriteLine($"[Row {rowIndex}] ERROR: No product details found for ProductID {selectedProduct.ProductID}")
                                                                        MessageBox.Show("Could not retrieve product details.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                                                                    End If
                                                                Catch ex As Exception
                                                                    Debug.WriteLine($"[Row {rowIndex}] ERROR in GetProductDetails: {ex.Message}")
                                                                    MessageBox.Show($"Error getting product details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                                                                End Try
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
                ' Tax Inclusive: 12% is already in the base amount, calculate for display only
                taxValue = baseAmount * 0.12D
                amountBeforeDiscount = baseAmount + taxValue ' Base amount already includes tax conceptually

                ' Update tax value display
                If taxValueBox IsNot Nothing Then taxValueBox.Text = taxValue.ToString("N2")
            End If

            Dim discountValue = amountBeforeDiscount * (discountPercent / 100)
            Dim finalAmount = amountBeforeDiscount - discountValue

            ' Update all display boxes
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
            Dim productArray As New List(Of Dictionary(Of String, Object))()

            ' Loop through all categories
            For Each kvp In _categories
                Dim category = kvp.Value
                Dim categoryId = kvp.Key

                If category.ProductsPanel Is Nothing Then Continue For

                ' Get category name
                Dim categoryName As String = ""
                If category.CategoryNameTextBox IsNot Nothing Then
                    categoryName = category.CategoryNameTextBox.Text.Trim()
                End If

                ' Loop through product rows in this category
                Dim childIndex = 0
                While childIndex < category.ProductsPanel.Children.Count
                    Dim rowBorder = TryCast(category.ProductsPanel.Children(childIndex), Border)

                    ' Skip description borders (they have no border thickness)
                    If rowBorder Is Nothing OrElse rowBorder.BorderThickness.Left = 0 Then
                        childIndex += 1
                        Continue While
                    End If

                    Dim productPanel = TryCast(rowBorder.Child, StackPanel)
                    If productPanel Is Nothing Then
                        childIndex += 1
                        Continue While
                    End If

                    Dim productData As New Dictionary(Of String, Object)
                    productData("Category") = categoryName

                    ' Extract product fields
                    Dim fieldIndex = 0
                    Dim fieldNames = {"ProductName", "Quantity", "Rate", "TaxPercent", "TaxValue", "DiscountPercent", "Discount", "Amount"}

                    For Each fieldBorder As Border In productPanel.Children.OfType(Of Border)().Take(8)
                        Dim value As String = ""

                        If fieldIndex = 0 Then
                            ' ProductName might be in a Grid
                            Dim grid = TryCast(fieldBorder.Child, Grid)
                            If grid IsNot Nothing AndAlso grid.Children.Count > 0 Then
                                Dim txtBox = TryCast(grid.Children(0), TextBox)
                                If txtBox IsNot Nothing Then value = txtBox.Text.Trim()
                            End If
                        Else
                            Dim txtBox = TryCast(fieldBorder.Child, TextBox)
                            If txtBox IsNot Nothing Then value = txtBox.Text.Trim()
                        End If

                        If (fieldNames(fieldIndex) = "ProductName" OrElse fieldNames(fieldIndex) = "Quantity" OrElse fieldNames(fieldIndex) = "Rate") AndAlso
                   String.IsNullOrWhiteSpace(value) Then
                            MessageBox.Show($"Please fill in all required fields in category '{categoryName}'.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                            Return Nothing
                        End If

                        productData(fieldNames(fieldIndex)) = value
                        fieldIndex += 1
                    Next

                    ' Get description (next child after rowBorder in ProductsPanel)
                    If childIndex + 1 < category.ProductsPanel.Children.Count Then
                        Dim descBorder = TryCast(category.ProductsPanel.Children(childIndex + 1), Border)
                        If descBorder IsNot Nothing AndAlso descBorder.BorderThickness.Left = 0 Then
                            Dim descTextBox = TryCast(descBorder.Child, TextBox)
                            If descTextBox IsNot Nothing Then
                                Dim descText = descTextBox.Text.Trim()
                                If descText = "Enter product description (Optional)" Then descText = ""
                                productData("Description") = descText
                            End If
                        End If
                    End If

                    ' Get product image
                    Dim productName As String = productData("ProductName").ToString()
                    Dim imageBase64 As String = Nothing
                    Try
                        imageBase64 = GetProduct.GetProductImageBase64(productName)
                    Catch ex As Exception
                        Debug.WriteLine($"Error getting image for {productName}: {ex.Message}")
                    End Try
                    productData("ProductImageBase64") = If(String.IsNullOrEmpty(imageBase64), "", imageBase64)

                    productArray.Add(productData)

                    ' Move to next row (skip description)
                    childIndex += 2
                End While
            Next

            If productArray.Count = 0 Then
                MessageBox.Show("No products found in any category.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return Nothing
            End If

            Return JsonConvert.SerializeObject(productArray, Formatting.None)
        End Function
#End Region

#Region "Clearing all of the fields"
        Public Sub ClearAllFields()
            ' Clear categories
            _categories.Clear()
            _categoryIdCounter = 0

            ' Clear main container
            MainContainer.Children.Clear()

            ' Clear all dictionaries
            _productTextBoxes.Clear()
            _productPopups.Clear()
            _productListBoxes.Clear()
            _productTypingTimers.Clear()

            ' Reset other fields
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

            OrderDateVM.SelectedDate = DateTime.Today

            ' Create first category
            AddNewCategory_Click(Nothing, Nothing)
        End Sub
#End Region

#Region "Getting All of the Data and Insert of this Quote"
        ' Whenever there is a change in WarehosueCombobox will also update the data

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
        ' Replace the entire GetAllDataInQuoteProperties method in NewQuoteGovernment.xaml.vb

        Private Sub GetAllDataInQuoteProperties(client As Client, productItemsJson As String)
            If Not ValidateQuoteSubmission(client, productItemsJson) Then Exit Sub
            If cmbCostEstimateValidty.SelectedItem Is Nothing Then
                MessageBox.Show("Please select a validity date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            Dim selectedValidityOption = DirectCast(cmbCostEstimateValidty.SelectedItem, ComboBoxItem).Content.ToString()
            Try
                Dim selectedTax As String = CType(txtTaxSelection.SelectedItem, ComboBoxItem).Content.ToString()
                Dim selectedDiscount As String = CType(txtDiscountSelection.SelectedItem, ComboBoxItem).Content.ToString()

                ' Calculate actual validity date from selected option
                Dim actualValidityDate = GetValidityDate(selectedValidityOption, OrderDateVM.SelectedDate)

                ' ✅ FIX: Use CostEstimateDetails.Property consistently
                CostEstimateDetails.CEValidUntilDate = selectedValidityOption
                CostEstimateDetails.CEQuoteValidityDateCache = actualValidityDate.ToString("yyyy-MM-dd")
                CostEstimateDetails.CEQuoteNumberCache = txtQuoteNumber.Text
                CostEstimateDetails.CEDiscountProperty = txtDiscountSelection.Text
                CostEstimateDetails.CETaxProperty = txtTaxSelection.Text
                CostEstimateDetails.CEQuoteDateCache = QuoteDate.SelectedDate.Value.ToString("yyyy-MM-dd")
                CostEstimateDetails.CETaxValueCache = txtTotalTax.Text
                CostEstimateDetails.CETotalDiscountValueCache = txtTotalDiscount.Text
                CostEstimateDetails.CETotalAmountCache = txtGrandTotal.Text
                CostEstimateDetails.CEnoteTxt = txtQuoteNote.Text
                CostEstimateDetails.CEReferenceNumber = txtReferenceNumber.Text

                ' ✅ SET PROJECT ID
                CostEstimateDetails.CEProjectID = TxtProjectID.Text.Trim()

                ' ✅ SET SUBJECT
                CostEstimateDetails.CESubject = TxtSubjectDetails.Text.Trim()

                CostEstimateDetails.CEQuoteItemsCache = JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(productItemsJson)
                CostEstimateDetails.CEsignature = False
                CostEstimateDetails.CEImageCache = ""
                CostEstimateDetails.CEPathCache = ""
                CostEstimateDetails.CECompanyName = client.Company

                ' Parse billing address
                Dim stringArray As List(Of String) = client.BillingAddress.Split(","c).Select(Function(s) s.Trim()).ToList()
                CostEstimateDetails.CEAddress = stringArray(0)
                CostEstimateDetails.CECity = stringArray(1)
                CostEstimateDetails.CERegion = stringArray(2)
                CostEstimateDetails.CECountry = stringArray(3)
                CostEstimateDetails.CEClientDetailsCache = TxtClientDetails.Text
                CostEstimateDetails.CEPhone = client.Phone
                CostEstimateDetails.CEEmail = client.Email
                CostEstimateDetails.CEClientName = client.Name
                CostEstimateDetails.CERepresentative = client.Representative

                ' Debugging 
                Debug.WriteLine("✓ All data saved to CostEstimateDetails")
                Debug.WriteLine($"QuoteNumber: {CostEstimateDetails.CEQuoteNumberCache}")
                Debug.WriteLine($"Subject: {CostEstimateDetails.CESubject}")
                Debug.WriteLine($"ProjectID: {CostEstimateDetails.CEProjectID}")
                Debug.WriteLine($"ClientName: {CostEstimateDetails.CEClientName}")
                Debug.WriteLine($"ValidityDate: {CostEstimateDetails.CEValidUntilDate}")

                ' Navigate to Cost Estimate view
                ViewLoader.DynamicView.NavigateToView("costestimategovernment", Me)

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

        Private Sub TxtSubjectDetails_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TxtSubjectDetails.TextChanged

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
