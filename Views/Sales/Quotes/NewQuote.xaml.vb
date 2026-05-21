Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DocumentFormat.OpenXml.Bibliography
Imports DocumentFormat.OpenXml.Math
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Views.Sales.Quotes.NewQuoteGovernment
Imports DPC.DPC.Views.Stocks
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json
Imports NuGet.Protocol.Plugins

Namespace DPC.Views.Sales.Quotes
    ' Note - The value of defau
    Public Class NewQuote
        Private _originalGrandTotal As Decimal = 0
        Private _isTaxApplied As Boolean = False
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

        ' *** NEW: Stores ProductID keyed by "txtProductName_{rowIndex}" ***
        Private _selectedProductIDs As New Dictionary(Of String, String)

        ' Controllers for calendar to shows the date by binding
        Private OrderDateVM As New CalendarController.SingleCalendar()
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
        Private _isInitialized As Boolean = False
        Private categoryCount As Integer = 0

#Region "Initializiation once loaded the form"
        Public Sub New()
            InitializeComponent()

            ' Autocomplete part
            _typingTimer = New DispatcherTimer With {
                .Interval = TimeSpan.FromMilliseconds(300)
            }

            AddHandler _typingTimer.Tick, AddressOf OnTypingTimerTick
            AddHandler txtSearchCustomer.TextChanged, AddressOf txtSearchCustomer_TextChanged
            AddHandler LstItems.SelectionChanged, AddressOf LstItems_SelectionChanged

            ' Event for Checking the Quotenumber
            AddHandler txtQuoteNumber.TextChanged, AddressOf txtQuoteNumber_TextChanged

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

            txtTaxSelection_SelectionChanged(txtTaxSelection, Nothing)

            _isInitialized = True
            InitializeProductUI()
            rowCount += 1

            OrderDateVM.SelectedDate = DateTime.Today

            ' Set Date to bind
            QuoteDate.DataContext = OrderDateVM
            QuoteDateButton.DataContext = OrderDateVM

            ' Load warehouse options
            ProductController.GetWarehouse(ComboBoxWarehouse)

            Dim selectedWarehouse As ComboBoxItem = TryCast(ComboBoxWarehouse.SelectedItem, ComboBoxItem)
            If selectedWarehouse IsNot Nothing Then
                CEWarehouseIDCache = Convert.ToInt32(selectedWarehouse.Tag)
                CEWarehouseNameCache = selectedWarehouse.Content.ToString()
            End If

            ' Checks the value of CEType
            Dim model = TransactionState.ActiveRecord
            Dim isEditing As Boolean = (model IsNot Nothing AndAlso model.IsEditMode)

            If isEditing Then
                txtQuoteNumber.IsReadOnly = True
                txtQuoteNumber.Background = New SolidColorBrush(Color.FromRgb(240, 240, 240))

                cmbCostEstimateType.IsEnabled = False
                cmbCostEstimateType.SelectedIndex = -1

                LoadingCEType = False
            Else
                If CostEstimateDetails.CEType > 4 Then
                    cmbCostEstimateType.SelectedIndex = 0
                    CEType = 0
                Else
                    cmbCostEstimateType.SelectedIndex = CostEstimateDetails.CEType
                    CEType = CostEstimateDetails.CEType
                End If

                ' Generate a BRAND NEW Quote ID
                Dim quoteID As String = QuotesController.GenerateQuoteID(CEType)
                txtQuoteNumber.Text = quoteID

                Dim _prefix As String
                Select Case CEType
                    Case 0 : _prefix = "WICE #:"
                    Case 1 : _prefix = "HHCE #:"
                    Case 2 : _prefix = "GPCE #:"
                    Case 3 : _prefix = "BCCE #:"
                    Case Else : _prefix = "CE #:"
                End Select

                CostEstimateDetails.CECNIndetifier = _prefix
                LoadingCEType = False
            End If

            If txtTaxSelection.Text = "Inclusive" Then
                ShowVatExBtn.Visibility = Visibility.Collapsed
            Else
                ShowVatExBtn.Visibility = Visibility.Collapsed
            End If

            ' Visibility for the Show/Hide VAT 12% button
            VatExShowVat.Text = If(CEisVatExInclude, "Hide VAT 12%", "Show VAT 12%")

            cmbCostEstimateValidty.Text = CostEstimateDetails.CEValidUntilDate

            TaxHeader.Header = If(_TaxSelection, "TAX(%)", "TAX(12%)")
        End Sub
#End Region

#Region "Quote Number Validation & Cost Estimate Validity"
        Private Sub cmbCostEstimateType_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If LoadingCEType Then Exit Sub

            CostEstimateDetails.CEType = cmbCostEstimateType.SelectedIndex
            Dim _prefix As String

            Select Case CEType
                Case 0
                    _prefix = "WICE #:"
                Case 1
                    _prefix = "HHCE #:"
                Case 2
                    _prefix = "GPCE #:"
                Case 3
                    _prefix = "BCCE #:"
                Case Else
                    _prefix = "CE #:"
            End Select

            CostEstimateDetails.CECNIndetifier = _prefix

            Dim quoteID As String = QuotesController.GenerateQuoteID(CEType)
            txtQuoteNumber.Text = quoteID
        End Sub

        Private Sub cmbCostEstimateValidty_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If LoadingCEType Then Exit Sub
            CostEstimateDetails.CEValidUntilDate = cmbCostEstimateValidty.SelectedIndex
        End Sub
#End Region

#Region "Autocomplete for Clients"
        Private Sub txtSearchCustomer_TextChanged(sender As Object, e As TextChangedEventArgs)
            _typingTimer.Stop()

            If String.IsNullOrWhiteSpace(txtSearchCustomer.Text) Then
                AutoCompletePopup.IsOpen = False
                Return
            End If

            _typingTimer.Start()
        End Sub

        Private Sub OnTypingTimerTick(sender As Object, e As EventArgs)
            _typingTimer.Stop()
            _clients = ClientController.SearchClient(txtSearchCustomer.Text)
            LstItems.ItemsSource = _clients
            AutoCompletePopup.IsOpen = _clients.Count > 0
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
            Dim rowsToRemove As New List(Of Integer)

            For i As Integer = 0 To rowCount - 1
                rowsToRemove.Add(i * 2)
            Next

            rowsToRemove.Sort()
            rowsToRemove.Reverse()

            For Each rowIndex As Integer In rowsToRemove
                RemoveRow(rowIndex)
            Next

            rowCount = 0
        End Sub

        Private Sub RemoveRow(row As Integer)
            Dim elementsToRemove As New List(Of UIElement)

            For Each element As UIElement In elementsToRemove
                If TypeOf element Is StackPanel Then
                End If
            Next

            For Each element As UIElement In elementsToRemove
                MyDynamicGrid.Children.Remove(element)
            Next

            Dim timerKey As String = $"ProductTimer_{row}"
            Dim popupKey As String = $"ProductPopup_{row}"
            Dim listBoxKey As String = $"LstProducts_{row}"

            If _productTypingTimers.ContainsKey(timerKey) Then
                _productTypingTimers(timerKey).Stop()
                _productTypingTimers.Remove(timerKey)
            End If

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
                        kvp.Value.Text = "0"
                        kvp.Value.IsReadOnly = False
                        CEtaxSelection = True
                        TaxHeader.Header = "TAX(%)"
                        If border IsNot Nothing Then
                            border.BorderThickness = New Thickness(1)
                            border.BorderBrush = CType(New BrushConverter().ConvertFrom("#AEAEAE"), Brush)
                        End If
                    Else
                        kvp.Value.Text = ""
                        kvp.Value.IsReadOnly = True
                        CEtaxSelection = False
                        TaxHeader.Header = "TAX(12%)"
                        CEisVatExInclude = False
                        If border IsNot Nothing Then
                            border.BorderThickness = New Thickness(0)
                            border.BorderBrush = Brushes.Transparent
                        End If
                    End If
                End If
            Next

            For i As Integer = 0 To rowCount - 1
                CalculateAmount(i)
            Next
        End Sub
#End Region

#Region "This Loads every data if its available for updating"
        Private Sub InitializeProductUI()
            Dim model = TransactionState.ActiveRecord

            If model IsNot Nothing AndAlso model.IsEditMode Then
                LoadFromUniversalPreview(model)
            Else
                AddNewCategoryUI()
            End If
        End Sub

        Private Sub BtnAddClient_Click(sender As Object, e As RoutedEventArgs) Handles BtnAddClient.Click
            ViewLoader.DynamicView.NavigateToView("newwalkinclient", Me)
        End Sub

        Private Sub BtnReset_Click(sender As Object, e As RoutedEventArgs) Handles BtnAddClient.Click
            TransactionState.ResetRecord()
            lblPageTitle.Text = "Cost Estimate"
            lblButton.Text = "Generate Cost Estimate"
            ViewLoader.DynamicView.NavigateToView("salesnewquote", Me)
        End Sub

        Private Sub LoadFromUniversalPreview(model As UniversalTransactionModel)
            _isInitialized = False
            lblPageTitle.Text = model.EditLabel
            lblButton.Text = model.EditButtonLabel
            txtQuoteNumber.Text = model.DocumentNumber
            txtQuoteNote.Text = model.Notes
            txtDeliveryFee.Text = model.FeeValue
            txtInstallationFee.Text = model.InstallationFee

            If model.WarehouseID > 0 Then
                ComboBoxWarehouse.SelectedValue = model.WarehouseID
                WarehouseID = model.WarehouseID
            End If

            FillClientsFieldFromModel(model)

            MainContainer.Children.Clear()
            rowCount = 0

            Dim currentTargetPanel As StackPanel = Nothing

            For Each item As OrderItems In model.OrderItems
                If item.IsHeaderRow = True Then
                    AddNewCategoryWithSpecificName(item.ProductName)
                    currentTargetPanel = GetLatestItemsPanel()
                Else
                    If currentTargetPanel Is Nothing Then
                        AddNewCategoryWithSpecificName("")
                        currentTargetPanel = GetLatestItemsPanel()
                    End If

                    rowCount += 1
                    AddProductInputUI(currentTargetPanel)

                    ' *** NEW: Restore ProductID when loading from edit mode ***
                    If Not String.IsNullOrEmpty(item.ProductID) Then
                        _selectedProductIDs($"txtProductName_{rowCount}") = item.ProductID
                    End If

                    PopulateDynamicRow(rowCount, item)
                End If
            Next

            UpdateGrandTotal()
        End Sub

        Private Sub FillClientsFieldFromModel(model As UniversalTransactionModel)
            RemoveHandler txtSearchCustomer.TextChanged, AddressOf txtSearchCustomer_TextChanged

            Dim foundClients = ClientController.SearchClient(model.ClientId)
            Dim match = foundClients.FirstOrDefault(Function(c) c.ClientID.ToString() = model.ClientId.ToString())

            If match IsNot Nothing Then
                _selectedClient = match
                txtSearchCustomer.Text = _selectedClient.Name
                UpdateSupplierDetails(_selectedClient)
            Else
                Dim matchByName = foundClients.FirstOrDefault(Function(c) c.Name = model.ClientName OrElse c.Company = model.ClientName)

                If matchByName IsNot Nothing Then
                    _selectedClient = matchByName
                    txtSearchCustomer.Text = _selectedClient.Name
                    UpdateSupplierDetails(_selectedClient)
                Else
                    txtSearchCustomer.Text = model.ClientName
                    TxtClientDetails.Text = $"Name: {model.ClientName}{Environment.NewLine}" &
                                   $"Contact: {model.ClientContact}{Environment.NewLine}" &
                                   $"Email: {model.ClientEmail}{Environment.NewLine}" &
                                   $"Address: {model.ClientAddress}"
                End If
            End If

            AddHandler txtSearchCustomer.TextChanged, AddressOf txtSearchCustomer_TextChanged
        End Sub

        Private Sub PopulateDynamicRow(rowIdx As Integer, item As OrderItems)
            Dim nameKey = $"txtProductName_{rowIdx}"
            Dim qtyKey = $"txtQuantity_{rowIdx}"
            Dim rateKey = $"txtRate_{rowIdx}"

            If _productTextBoxes.ContainsKey(nameKey) Then
                _productTextBoxes(nameKey).Text = item.ProductName
            End If

            If _productTextBoxes.ContainsKey(qtyKey) Then
                _productTextBoxes(qtyKey).Text = item.Quantity
            End If

            If _productTextBoxes.ContainsKey(rateKey) Then
                _productTextBoxes(rateKey).Text = item.UnitPrice.Replace("₱", "").Replace(",", "").Trim()
            End If

            Dim itemsPanel = GetLatestItemsPanel()
            If itemsPanel IsNot Nothing AndAlso itemsPanel.Children.Count > 0 Then
                Dim rowBorder = TryCast(itemsPanel.Children(itemsPanel.Children.Count - 1), Border)

                If rowBorder IsNot Nothing Then
                    Dim allTextBoxes = FindVisualChildren(Of TextBox)(rowBorder)
                    Dim descBox = allTextBoxes.FirstOrDefault(Function(t) t.Text.Contains("Optional") OrElse String.IsNullOrEmpty(t.Name))

                    If descBox IsNot Nothing Then
                        descBox.Text = item.ProductDescription
                    End If
                End If
            End If

            CalculateAmount(rowIdx)
        End Sub

        Private Sub AddNewCategoryWithSpecificName(catName As String)
            AddNewCategoryUI()

            Dim lastWrapper = TryCast(MainContainer.Children(MainContainer.Children.Count - 1), StackPanel)
            If lastWrapper IsNot Nothing Then
                Dim headerBorder = TryCast(lastWrapper.Children(0), Border)
                Dim headerGrid = TryCast(headerBorder?.Child, Grid)
                Dim nameTxt = TryCast(headerGrid?.Children(0), TextBox)

                If nameTxt IsNot Nothing Then
                    nameTxt.Text = If(String.IsNullOrWhiteSpace(catName), "New Category Group", catName)
                End If
            End If
        End Sub

        Private Function GetLatestItemsPanel() As StackPanel
            If MainContainer.Children.Count = 0 Then Return Nothing

            Dim lastWrapper = TryCast(MainContainer.Children(MainContainer.Children.Count - 1), StackPanel)
            If lastWrapper Is Nothing OrElse lastWrapper.Children.Count < 2 Then Return Nothing

            Return TryCast(lastWrapper.Children(1), StackPanel)
        End Function
#End Region

#Region "Product Autocomplete"
        Public Sub AddNewCategory_Click(sender As Object, e As RoutedEventArgs)
            AddNewCategoryUI()
        End Sub

        Private Sub CategoryAddRow_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            If btn IsNot Nothing Then
                Dim targetPanel = TryCast(btn.Tag, StackPanel)

                If targetPanel IsNot Nothing Then
                    rowCount += 1
                    AddProductInputUI(targetPanel)
                End If
            End If
        End Sub

        Private Sub DeleteCategory_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            If btn IsNot Nothing Then
                Dim wrapperToDelete = TryCast(btn.Tag, StackPanel)

                If wrapperToDelete IsNot Nothing Then
                    MainContainer.Children.Remove(wrapperToDelete)

                    Dim allTextBoxes = FindVisualChildren(Of TextBox)(wrapperToDelete)
                    For Each txt In allTextBoxes
                        If Not String.IsNullOrEmpty(txt.Name) Then
                            If Me.FindName(txt.Name) IsNot Nothing Then Me.UnregisterName(txt.Name)
                            If _productTextBoxes.ContainsKey(txt.Name) Then _productTextBoxes.Remove(txt.Name)
                            ' *** NEW: Also clean up the ProductID cache for deleted rows ***
                            If _selectedProductIDs.ContainsKey(txt.Name) Then _selectedProductIDs.Remove(txt.Name)
                        End If
                    Next

                    UpdateGrandTotal()
                End If
            End If
        End Sub

        Private Function CreateAddButtonContent() As StackPanel
            Dim sp As New StackPanel With {.Orientation = Orientation.Horizontal}
            Dim icon As New MaterialDesignThemes.Wpf.PackIcon With {
                .Kind = MaterialDesignThemes.Wpf.PackIconKind.PlaylistAdd,
                .Margin = New Thickness(0, 0, 5, 0)
            }
            Dim txt As New TextBlock With {.Text = "Add New Item Row"}
            sp.Children.Add(icon)
            sp.Children.Add(txt)
            Return sp
        End Function

        Private Sub AddNewCategoryUI()
            categoryCount += 1
            Dim currentCatId = categoryCount

            Dim categoryWrapper As New StackPanel With {
                .Margin = New Thickness(0, 10, 0, 20)
            }

            Dim headerBorder As New Border With {
                .Background = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                .CornerRadius = New CornerRadius(10, 10, 0, 0),
                .Padding = New Thickness(15, 8, 15, 8),
                .Margin = New Thickness(0, 5, 0, 0)
            }

            Dim headerGrid As New Grid()
            headerGrid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(1, GridUnitType.Star)})
            headerGrid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = GridLength.Auto})

            Dim categoryHeader As New TextBox With {
                .Text = "New Category Group",
                .FontSize = 14,
                .FontWeight = FontWeights.SemiBold,
                .Foreground = Brushes.White,
                .Background = Brushes.Transparent,
                .BorderThickness = New Thickness(0),
                .FontFamily = New FontFamily("Lexend"),
                .VerticalAlignment = VerticalAlignment.Center
            }

            Dim closeIcon As New MaterialDesignThemes.Wpf.PackIcon With {
                .Kind = MaterialDesignThemes.Wpf.PackIconKind.Close,
                .Foreground = Brushes.White
            }

            Dim removeGroupBtn As New Button With {
                .Content = closeIcon,
                .Background = Brushes.Transparent,
                .BorderThickness = New Thickness(0),
                .Cursor = Cursors.Hand,
                .Width = 30,
                .Height = 30,
                .Tag = categoryWrapper
            }
            AddHandler removeGroupBtn.Click, AddressOf DeleteCategory_Click

            headerGrid.Children.Add(categoryHeader)
            Grid.SetColumn(categoryHeader, 0)
            headerGrid.Children.Add(removeGroupBtn)
            Grid.SetColumn(removeGroupBtn, 1)
            headerBorder.Child = headerGrid

            Dim categoryItemsPanel As New StackPanel()

            Dim addRowBtn As New Button With {
                .Content = CreateAddButtonContent(),
                .HorizontalAlignment = HorizontalAlignment.Center,
                .Margin = New Thickness(0, 10, 0, 0),
                .Style = DirectCast(Me.FindResource("MaterialDesignOutlinedButton"), System.Windows.Style),
                .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                .Foreground = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                .Height = 35,
                .Tag = categoryItemsPanel
            }
            AddHandler addRowBtn.Click, AddressOf CategoryAddRow_Click

            categoryWrapper.Children.Add(headerBorder)
            categoryWrapper.Children.Add(categoryItemsPanel)
            categoryWrapper.Children.Add(addRowBtn)

            MainContainer.Children.Add(categoryWrapper)
        End Sub

        Private Sub AddProductInputUI(targetPanel)
            If targetPanel Is Nothing Then Exit Sub

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

            Dim productPanel As New Grid With {
        .Margin = New Thickness(10, 12, 10, 8),
        .HorizontalAlignment = HorizontalAlignment.Stretch,
        .VerticalAlignment = VerticalAlignment.Center
    }

            productPanel.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(135)}) ' Item Description
            productPanel.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(55)})  ' Qty
            productPanel.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(105)}) ' Unit Price
            productPanel.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(70)})  ' Tax %
            productPanel.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(95)}) ' Tax Value
            productPanel.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(70)})  ' Disc %
            productPanel.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(95)}) ' Discount
            productPanel.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(115)}) ' Total
            productPanel.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(45)})      ' Delete

            Dim productSearch = CreateProductSearchBox(125, rowIndex)
            Dim qtyBox = CreateQuantityBox(rowIndex)
            Dim rateBox = CreateRateBox(rowIndex)
            Dim taxPercentBox = CreateTaxPercentBox(rowIndex)
            Dim taxValueBox = CreateTaxValueBox(rowIndex)
            Dim discountPercentBox = CreateDiscountPercentBox(rowIndex)
            Dim discountBox = CreateDiscountBox(rowIndex)
            Dim amountBox = CreateAmountBox("₱ 0.00", rowIndex)
            Dim deleteBtn = CreateDeleteButton(mainBorder, targetPanel)

            productSearch.VerticalAlignment = VerticalAlignment.Center
            qtyBox.VerticalAlignment = VerticalAlignment.Center
            rateBox.VerticalAlignment = VerticalAlignment.Center
            taxPercentBox.VerticalAlignment = VerticalAlignment.Center
            taxValueBox.VerticalAlignment = VerticalAlignment.Center
            discountPercentBox.VerticalAlignment = VerticalAlignment.Center
            discountBox.VerticalAlignment = VerticalAlignment.Center
            amountBox.VerticalAlignment = VerticalAlignment.Center
            deleteBtn.VerticalAlignment = VerticalAlignment.Center

            productSearch.Margin = New Thickness(0, 0, 8, 0)
            qtyBox.Margin = New Thickness(0, 0, 8, 0)
            rateBox.Margin = New Thickness(0, 0, 8, 0)
            taxPercentBox.Margin = New Thickness(0, 0, 8, 0)
            taxValueBox.Margin = New Thickness(0, 0, 8, 0)
            discountPercentBox.Margin = New Thickness(0, 0, 8, 0)
            discountBox.Margin = New Thickness(0, 0, 8, 0)
            amountBox.Margin = New Thickness(0, 0, 8, 0)

            Grid.SetColumn(productSearch, 0)
            Grid.SetColumn(qtyBox, 1)
            Grid.SetColumn(rateBox, 2)
            Grid.SetColumn(taxPercentBox, 3)
            Grid.SetColumn(taxValueBox, 4)
            Grid.SetColumn(discountPercentBox, 5)
            Grid.SetColumn(discountBox, 6)
            Grid.SetColumn(amountBox, 7)
            Grid.SetColumn(deleteBtn, 8)

            productPanel.Children.Add(productSearch)
            productPanel.Children.Add(qtyBox)
            productPanel.Children.Add(rateBox)
            productPanel.Children.Add(taxPercentBox)
            productPanel.Children.Add(taxValueBox)
            productPanel.Children.Add(discountPercentBox)
            productPanel.Children.Add(discountBox)
            productPanel.Children.Add(amountBox)
            productPanel.Children.Add(deleteBtn)

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
        .HorizontalAlignment = HorizontalAlignment.Stretch,
        .Width = Double.NaN,
        .TextWrapping = TextWrapping.Wrap,
        .AcceptsReturn = True,
        .VerticalScrollBarVisibility = ScrollBarVisibility.Auto
    }

            Dim descriptionBorder As New Border With {
        .Margin = New Thickness(10, 0, 10, 10),
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
            targetPanel.Children.Add(mainBorder)

            UpdateGrandTotal()
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
                .TextWrapping = TextWrapping.NoWrap,
                .Padding = New Thickness(5),
                .BorderThickness = New Thickness(0),
                .MinWidth = width,
                .MaxWidth = width,
                .Width = width,
                .Height = 34,
                .MinHeight = 34,
                .MaxHeight = 34,
                .AcceptsReturn = False,
                .VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                .HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                .MaxLength = 1000,
                .VerticalAlignment = VerticalAlignment.Top
            }

            Dim suggestionList As New ListBox With {
                .Name = listBoxKey,
                .MaxHeight = 150,
                .MinWidth = width
            }

            Dim factory As New FrameworkElementFactory(GetType(TextBlock))
            factory.SetBinding(TextBlock.TextProperty, New Binding("ProductName"))
            suggestionList.ItemTemplate = New DataTemplate() With {.VisualTree = factory}

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

            AddHandler textBox.TextChanged, Sub(sender As Object, e As TextChangedEventArgs)
                                                typingTimer.Stop()
                                                typingTimer.Start()
                                            End Sub

            ' *** UPDATED: Save ProductID when user selects from suggestion list ***
            AddHandler suggestionList.SelectionChanged, Sub(sender As Object, e As SelectionChangedEventArgs)
                                                            If suggestionList.SelectedItem IsNot Nothing Then
                                                                Dim selectedProduct = CType(suggestionList.SelectedItem, ProductDataModel)

                                                                textBox.Text = selectedProduct.ProductName
                                                                popup.IsOpen = False
                                                                suggestionList.SelectedItem = Nothing

                                                                ' *** NEW: Cache the ProductID for this row ***
                                                                _selectedProductIDs(textBoxName) = selectedProduct.ProductID

                                                                Dim productInfo = QuotesController.GetProductDetailsByProductID(selectedProduct.ProductID, WarehouseID)

                                                                If productInfo.Count > 0 Then
                                                                    Dim p = productInfo.First()
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
                .Margin = New Thickness(0)
            }

            Return border
        End Function

        Public Function CreateInputBox(text As String, width As Double, Optional isReadOnly As Boolean = False, Optional name As String = "", Optional alignment As HorizontalAlignment = HorizontalAlignment.Left) As Border
            Dim txt As New TextBox With {
        .Text = text,
        .FontFamily = New FontFamily("Lexend"),
        .FontSize = 12,
        .Foreground = Brushes.Black,
        .FontWeight = FontWeights.SemiBold,
        .TextWrapping = TextWrapping.NoWrap,
        .TextAlignment = TextAlignment.Center,
        .Padding = New Thickness(5),
        .BorderThickness = New Thickness(0),
        .IsReadOnly = isReadOnly,
        .Width = width,
        .MinWidth = width,
        .HorizontalContentAlignment = HorizontalAlignment.Center,
        .VerticalContentAlignment = VerticalAlignment.Center,
        .Height = 34,
        .MaxLines = 1,
        .AcceptsReturn = False,
        .VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
        .HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
    }

            If alignment = HorizontalAlignment.Right Then
                txt.TextAlignment = TextAlignment.Right
                txt.HorizontalContentAlignment = HorizontalAlignment.Right
            ElseIf alignment = HorizontalAlignment.Center Then
                txt.TextAlignment = TextAlignment.Center
                txt.HorizontalContentAlignment = HorizontalAlignment.Center
            Else
                txt.TextAlignment = TextAlignment.Left
                txt.HorizontalContentAlignment = HorizontalAlignment.Left
            End If

            If Not String.IsNullOrWhiteSpace(name) Then
                txt.Name = name
                _productTextBoxes(name) = txt

                Dim existingElement As Object = Me.FindName(name)
                If existingElement IsNot Nothing Then
                    Me.UnregisterName(name)
                End If

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
        .BorderBrush = If(isReadOnly, Brushes.Transparent, CType(New BrushConverter().ConvertFrom("#AEAEAE"), Brush)),
        .BorderThickness = If(isReadOnly, New Thickness(0), New Thickness(1)),
        .Background = CType(New BrushConverter().ConvertFrom("#FDFDFD"), Brush),
        .CornerRadius = New CornerRadius(5),
        .Padding = New Thickness(2),
        .Margin = New Thickness(0),
        .Child = txt
    }

            Return border
        End Function

        Private Function CreateQuantityBox(rowIndex As Integer) As Border
            Return CreateInputBox("1", 50, False, $"txtQuantity_{rowIndex}", HorizontalAlignment.Center)
        End Function

        Private Function CreateRateBox(rowIndex As Integer) As Border
            Dim box = CreateInputBox("", 95, False, $"txtRate_{rowIndex}", HorizontalAlignment.Right)
            Dim txt = TryCast(box.Child, TextBox)
            If txt IsNot Nothing Then
                AddHandler txt.TextChanged, AddressOf Rate_TextChanged
                AddHandler txt.PreviewTextInput, AddressOf Rate_PreviewTextInput
                DataObject.AddPastingHandler(txt, New DataObjectPastingEventHandler(AddressOf Rate_Pasting))
            End If
            Return box
        End Function

        Private Function CreateTaxPercentBox(rowIndex As Integer) As Border
            Dim defaultTaxPercent As String = If(Not CEtaxSelection, "", "0")
            Dim box = CreateInputBox(defaultTaxPercent, 70, Not _TaxSelection, $"txtTaxPercent_{rowIndex}", HorizontalAlignment.Center)
            Dim txt = TryCast(box.Child, TextBox)
            If txt IsNot Nothing Then
                AddHandler txt.TextChanged, AddressOf TaxPercent_TextChanged
                AddHandler txt.PreviewTextInput, AddressOf TaxPercent_PreviewTextInput
            End If
            Return box
        End Function

        Private Function CreateTaxValueBox(rowIndex As Integer) As Border
            Return CreateInputBox("0.00", 90, True, $"txtTaxValue_{rowIndex}", HorizontalAlignment.Right)
        End Function

        Private Function CreateDiscountPercentBox(rowIndex As Integer) As Border
            Dim box = CreateInputBox("", 70, False, $"txtDiscountPercent_{rowIndex}", HorizontalAlignment.Center)
            Dim txt = TryCast(box.Child, TextBox)
            If txt IsNot Nothing Then
                AddHandler txt.TextChanged, AddressOf DiscountPercent_TextChanged
                AddHandler txt.PreviewTextInput, AddressOf DiscountPercent_PreviewTextInput
            End If
            Return box
        End Function

        Private Function CreateDiscountBox(rowIndex As Integer) As Border
            Return CreateInputBox("0.00", 90, True, $"txtDiscount_{rowIndex}", HorizontalAlignment.Right)
        End Function

        Private Function CreateAmountBox(text As String, rowIndex As Integer) As Border
            Return CreateInputBox(text, 110, True, $"txtAmount_{rowIndex}", HorizontalAlignment.Right)
        End Function

        Private Function CreateDeleteButton(containerToRemoveFrom As UIElement, targetPanel As StackPanel) As Button
            Dim deleteButton As New Button With {
        .Background = Brushes.Transparent,
        .BorderBrush = Brushes.Transparent,
        .Padding = New Thickness(0),
        .Width = 40,
        .Height = 40,
        .MinWidth = 40,
        .HorizontalAlignment = HorizontalAlignment.Center,
        .VerticalAlignment = VerticalAlignment.Center,
        .Cursor = Cursors.Hand
    }

            Dim icon As New MaterialDesignThemes.Wpf.PackIcon With {
        .Kind = MaterialDesignThemes.Wpf.PackIconKind.PlaylistRemove,
        .Foreground = CType(New BrushConverter().ConvertFrom("#D23636"), Brush),
        .Width = 28,
        .Height = 28,
        .HorizontalAlignment = HorizontalAlignment.Center,
        .VerticalAlignment = VerticalAlignment.Center
    }

            deleteButton.Content = icon

            AddHandler deleteButton.Click, Sub(sender As Object, e As RoutedEventArgs)
                                               targetPanel.Children.Remove(containerToRemoveFrom)

                                               Dim allTextBoxes = FindVisualChildren(Of TextBox)(containerToRemoveFrom)
                                               For Each txt In allTextBoxes
                                                   If Not String.IsNullOrEmpty(txt.Name) Then
                                                       If Me.FindName(txt.Name) IsNot Nothing Then Me.UnregisterName(txt.Name)
                                                       _productTextBoxes.Remove(txt.Name)
                                                       If _selectedProductIDs.ContainsKey(txt.Name) Then
                                                           _selectedProductIDs.Remove(txt.Name)
                                                       End If
                                                   End If
                                               Next

                                               UpdateGrandTotal()
                                           End Sub

            Return deleteButton
        End Function
#End Region

#Region "Calculation Per Row"
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
                        taxPercentBox.IsReadOnly = True
                    Else
                        taxPercentBox.Text = "0"
                        taxPercentBox.IsReadOnly = False
                    End If
                End If

                CalculateAmount(rowIndex)
            End If
        End Sub

        Private Function FindTextBoxByName(name As String) As TextBox
            If _productTextBoxes.ContainsKey(name) Then
                Return _productTextBoxes(name)
            End If
            Return Nothing
        End Function

        Private Function TryFindAmountTextBlock(rowIndex As Integer) As TextBlock
            For Each container As Border In MainContainer.Children.OfType(Of Border)()
                Dim amountTextBlock = container.FindName($"txtAmount_{rowIndex}")
                If TypeOf amountTextBlock Is TextBlock Then Return CType(amountTextBlock, TextBlock)
            Next
            Return Nothing
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
                Debug.WriteLine($"[Row {rowIndex}] One or more required boxes not found.")
                Exit Sub
            End If

            Dim quantity As Decimal = 0, rate As Decimal = 0, taxPercent As Decimal = 0, discountPercent As Decimal = 0
            Decimal.TryParse(quantityBox.Text, quantity)
            Decimal.TryParse(rateBox.Text, rate)
            If taxPercentBox IsNot Nothing Then Decimal.TryParse(taxPercentBox.Text, taxPercent)
            If discountPercentBox IsNot Nothing Then Decimal.TryParse(discountPercentBox.Text, discountPercent)

            Dim baseAmount = quantity * rate
            Dim taxValue As Decimal = 0

            If _TaxSelection Then
                taxValue = baseAmount * (taxPercent / 100)
            Else
                taxValue = baseAmount * 0.12D
                If taxValueBox IsNot Nothing Then taxValueBox.Text = taxValue.ToString("N2")
            End If

            Dim discountValue = baseAmount * (discountPercent / 100)
            Dim finalAmount = baseAmount - discountValue

            If taxValueBox IsNot Nothing Then taxValueBox.Text = taxValue.ToString("N2")
            If discountBox IsNot Nothing Then discountBox.Text = discountValue.ToString("N2")
            amountBox.Text = "₱" & finalAmount.ToString("N2")

            Debug.WriteLine($"[Row {rowIndex}] Base: {baseAmount}, Tax: {taxValue}, Discount: {discountValue}, Total: {finalAmount}")

            UpdateGrandTotal()
            UpdateTotalTax()
            UpdateTotalDiscount()
        End Sub

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
            Dim deliveryFee As Decimal = 0
            Dim installationFee As Decimal = 0

            Dim amountTextBoxNames = LogicalTreeHelper.GetChildren(MainContainer).OfType(Of UIElement)().
                SelectMany(Function(border) FindVisualChildren(Of TextBox)(border)).
                Where(Function(txt) txt.Name IsNot Nothing AndAlso txt.Name.StartsWith("txtAmount_")).
                Select(Function(txt) txt.Name).Distinct()

            For Each name As String In amountTextBoxNames
                Dim txtBox As TextBox = TryCast(Me.FindName(name), TextBox)
                If txtBox IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtBox.Text) Then
                    Dim rawText = txtBox.Text.Replace("₱", "").Replace(",", "").Trim()
                    Dim amount As Decimal
                    If Decimal.TryParse(rawText, amount) Then
                        subtotalAmount += amount
                    End If
                End If
            Next

            Decimal.TryParse(txtDeliveryFee.Text.Replace("₱", "").Replace(",", "").Trim(), deliveryFee)
            Decimal.TryParse(txtInstallationFee.Text.Replace("₱", "").Replace(",", "").Trim(), installationFee)

            Dim rawTax = txtTotalTax.Text.Replace("₱", "").Replace(",", "").Trim()
            Decimal.TryParse(rawTax, totalTaxAmount)

            Dim finalGrandTotal As Decimal = 0
            Dim baseForTaxCalculation As Decimal = subtotalAmount + deliveryFee + installationFee

            If _TaxSelection Then
                Dim calculatedTaxAmount As Decimal = baseForTaxCalculation * 0.12D
                finalGrandTotal = baseForTaxCalculation + calculatedTaxAmount
                CostEstimateDetails.CETotalAmountCache = "₱ " & finalGrandTotal.ToString("N2")
            Else
                Dim calculatedTaxAmount As Decimal = subtotalAmount * 0.12D
                finalGrandTotal = baseForTaxCalculation
                txtTotalTax.Text = "₱" & calculatedTaxAmount.ToString("N2")
                CostEstimateDetails.CETotalAmountCache = "₱ " & finalGrandTotal.ToString("N2")
            End If

            _originalGrandTotal = finalGrandTotal

            If _isTaxApplied Then
                Dim grandTotalWithTax As Decimal = _originalGrandTotal + totalTaxAmount
                txtGrandTotal.Text = "₱" & grandTotalWithTax.ToString("N2")
            Else
                txtGrandTotal.Text = "₱" & finalGrandTotal.ToString("N2")
            End If

            CostEstimateDetails.CETotalBaseAmount = "₱" & subtotalAmount.ToString("N2")

            If _isTaxApplied Then
                _isTaxApplied = False
                UpdateGrandTotalDisplay()
            End If
        End Sub

        Public Sub UpdateTotalTax()
            Dim totalTax As Decimal = 0
            Dim subtotalAmount As Decimal = 0
            Dim deliveryFee As Decimal = 0
            Dim installationFee As Decimal = 0

            For Each name As String In LogicalTreeHelper.GetChildren(MainContainer).OfType(Of UIElement)().
                SelectMany(Function(border) FindVisualChildren(Of TextBox)(border)).
                Where(Function(txt) txt.Name IsNot Nothing AndAlso txt.Name.StartsWith("txtAmount_")).
                Select(Function(txt) txt.Name).Distinct()

                Dim txtBox As TextBox = TryCast(Me.FindName(name), TextBox)
                If txtBox IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtBox.Text) Then
                    Dim rawText = txtBox.Text.Replace("₱", "").Replace(",", "").Trim()
                    Dim amount As Decimal
                    If Decimal.TryParse(rawText, amount) Then
                        subtotalAmount += amount
                    End If
                End If
            Next

            Decimal.TryParse(txtDeliveryFee.Text.Replace("₱", "").Replace(",", "").Trim(), deliveryFee)
            Decimal.TryParse(txtInstallationFee.Text.Replace("₱", "").Replace(",", "").Trim(), installationFee)

            Dim baseForTaxCalculation As Decimal = subtotalAmount + deliveryFee + installationFee
            totalTax = baseForTaxCalculation * 0.12D

            CostEstimateDetails.CETotalTaxValueCache = "₱ " & totalTax.ToString("N2")
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

            If _isTaxApplied Then
                _isTaxApplied = False

                Dim toggleButton = TryCast(ApplyTaxToggle, Button)
                If toggleButton IsNot Nothing Then
                    toggleButton.Background = CType(New BrushConverter().ConvertFrom("#AEAEAE"), Brush)
                    toggleButton.Margin = New Thickness(2, 2, 0, 0)

                    Dim icon = TryCast(toggleButton.Content, MaterialDesignThemes.Wpf.PackIcon)
                    If icon IsNot Nothing Then
                        icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Close
                    End If
                End If
            End If
        End Sub

        Private Sub Rate_TextChanged(sender As Object, e As TextChangedEventArgs)
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

        Private Sub Rate_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            Dim tb = DirectCast(sender, TextBox)

            Dim proposedText As String = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength).
        Insert(tb.SelectionStart, e.Text)

            Dim regex As New Regex("^\d*\.?\d{0,2}$")

            e.Handled = Not regex.IsMatch(proposedText)
        End Sub

        Private Sub Rate_Pasting(sender As Object, e As DataObjectPastingEventArgs)
            If e.DataObject.GetDataPresent(GetType(String)) Then
                Dim pastedText As String = CType(e.DataObject.GetData(GetType(String)), String)
                Dim regex As New Regex("^\d*\.?\d{0,2}$")

                If Not regex.IsMatch(pastedText.Trim()) Then
                    e.CancelCommand()
                End If
            Else
                e.CancelCommand()
            End If
        End Sub

        Private Sub txtDeliveryFee_TextChange(sender As Object, e As TextChangedEventArgs)
            If Not _isInitialized Then Return
            Dim tb = DirectCast(sender, TextBox)

            RemoveHandler tb.TextChanged, AddressOf txtDeliveryFee_TextChange

            Dim rawInput As String = tb.Text.Replace(",", "").Trim()
            Dim cleanInput As String = Regex.Replace(rawInput, "[^0-9.]", "")

            Dim parts = cleanInput.Split("."c)
            If parts.Length > 2 Then
                cleanInput = parts(0) & "." & parts(1)
            End If

            If String.IsNullOrEmpty(cleanInput) Then
                tb.Text = ""
                lblFee.Text = "₱ 0"
            Else
                Dim intPart As String = cleanInput.Split("."c)(0)
                Dim decPart As String = If(cleanInput.Contains("."), "." & cleanInput.Split("."c)(1), "")

                Dim intVal As Long = 0
                Long.TryParse(intPart, intVal)
                Dim formatted = intVal.ToString("N0") & decPart

                Dim caretPos = tb.CaretIndex
                Dim oldLen = tb.Text.Length
                tb.Text = formatted
                tb.CaretIndex = Math.Max(0, Math.Min(formatted.Length, caretPos + (formatted.Length - oldLen)))

                lblFee.Text = "₱ " & formatted
            End If

            AddHandler tb.TextChanged, AddressOf txtDeliveryFee_TextChange
            UpdateGrandTotal()
            UpdateTotalTax()
        End Sub

        Public Sub txtInstallationFee_TextChanged(sender As Object, e As TextChangedEventArgs)
            If Not _isInitialized Then Return
            Dim tb = DirectCast(sender, TextBox)

            RemoveHandler tb.TextChanged, AddressOf txtInstallationFee_TextChanged

            Dim rawInput As String = tb.Text.Replace(",", "").Trim()
            Dim cleanInput As String = Regex.Replace(rawInput, "[^0-9.]", "")

            Dim parts = cleanInput.Split("."c)
            If parts.Length > 2 Then
                cleanInput = parts(0) & "." & parts(1)
            End If

            If String.IsNullOrEmpty(cleanInput) Then
                tb.Text = ""
                lblInstallationFee.Text = "₱ 0"
            Else
                Dim intPart As String = cleanInput.Split("."c)(0)
                Dim decPart As String = If(cleanInput.Contains("."), "." & cleanInput.Split("."c)(1), "")

                Dim intVal As Long = 0
                Long.TryParse(intPart, intVal)
                Dim formatted = intVal.ToString("N0") & decPart

                Dim caretPos = tb.CaretIndex
                Dim oldLen = tb.Text.Length
                tb.Text = formatted
                tb.CaretIndex = Math.Max(0, Math.Min(formatted.Length, caretPos + (formatted.Length - oldLen)))

                lblInstallationFee.Text = "₱ " & formatted
            End If

            AddHandler tb.TextChanged, AddressOf txtInstallationFee_TextChanged
            UpdateGrandTotal()
            UpdateTotalTax()
        End Sub

        Private Sub cmbFeeType_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If lblFeeType Is Nothing OrElse cmbFeeType.SelectedIndex = -1 Then Return

            Select Case cmbFeeType.SelectedIndex
                Case 0
                    lblFeeType.Text = "Delivery Fee"
                Case 1
                    lblFeeType.Text = "Mobilization Fee"
            End Select
        End Sub

        Private Sub Quantity_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim textBox = TryCast(sender, TextBox)
            If textBox Is Nothing Then Exit Sub

            textBox.Dispatcher.BeginInvoke(Sub()
                                               Dim rawText As String = textBox.Text.Replace(",", "").Trim()

                                               If Not String.IsNullOrEmpty(rawText) Then
                                                   Dim val As Decimal = 0
                                                   If Decimal.TryParse(rawText, val) Then
                                                       Dim formattedText As String = val.ToString("N0")

                                                       If textBox.Text <> formattedText Then
                                                           Dim caretIndex = textBox.CaretIndex
                                                           Dim selectionStart = textBox.SelectionStart
                                                           Dim oldLength = textBox.Text.Length

                                                           textBox.Text = formattedText

                                                           Dim newLength = textBox.Text.Length
                                                           textBox.CaretIndex = Math.Max(0, caretIndex + (newLength - oldLength))
                                                       End If
                                                   End If
                                               End If

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

            If String.IsNullOrWhiteSpace(cmbCostEstimateValidty.Text) Then
                MessageBox.Show("Select an Cost Estimate Validity Date.")
                Return False
            End If

            If Not OrderDateVM.SelectedDate.HasValue Then
                MessageBox.Show("Quote Date is required.")
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
                MessageBox.Show("No products found in the quote.")
                Return False
            End If

            Return True
        End Function

        Private Sub txtQuoteNumber_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim model = TransactionState.ActiveRecord
            If model IsNot Nothing AndAlso model.IsEditMode Then Exit Sub

            Dim currentQuoteID = txtQuoteNumber.Text.Trim()
            If String.IsNullOrEmpty(currentQuoteID) Then Exit Sub

            If QuotesController.QuoteNumberExists(currentQuoteID) Then
                txtQuoteNumber.Text = QuotesController.GenerateQuoteID()
                txtQuoteNumber.CaretIndex = txtQuoteNumber.Text.Length
            End If
        End Sub

        Private Sub ApplyTaxToggle_Click(sender As Object, e As RoutedEventArgs)
            _isTaxApplied = Not _isTaxApplied
            UpdateGrandTotalDisplay()
        End Sub

        Private Sub UpdateGrandTotalDisplay()
            Dim subtotal As Decimal = 0
            Dim delivery As Decimal = 0
            Dim installation As Decimal = 0

            Dim subtotalText = CostEstimateDetails.CETotalBaseAmount.Replace("₱", "").Trim()
            Decimal.TryParse(subtotalText, subtotal)

            Decimal.TryParse(txtDeliveryFee.Text.Replace("₱", "").Replace(",", "").Trim(), delivery)
            Decimal.TryParse(txtInstallationFee.Text.Replace("₱", "").Replace(",", "").Trim(), installation)

            Dim baseAmount = subtotal + delivery + installation
            Dim calculatedTax As Decimal = baseAmount * 0.12D

            Dim toggleButton = TryCast(ApplyTaxToggle, Button)

            If _isTaxApplied Then
                Dim grandTotalWithTax As Decimal = baseAmount + calculatedTax
                txtGrandTotal.Text = "₱" & grandTotalWithTax.ToString("N2")
                txtTotalTax.Text = "₱" & calculatedTax.ToString("N2")

                If toggleButton IsNot Nothing Then
                    toggleButton.Background = CType(New BrushConverter().ConvertFrom("#1D5642"), Brush)
                    toggleButton.Margin = New Thickness(24, 2, 0, 0)

                    Dim icon = TryCast(toggleButton.Content, MaterialDesignThemes.Wpf.PackIcon)
                    If icon IsNot Nothing Then
                        icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Check
                    End If
                End If
            Else
                txtGrandTotal.Text = "₱" & baseAmount.ToString("N2")
                txtTotalTax.Text = "₱" & calculatedTax.ToString("N2")

                If toggleButton IsNot Nothing Then
                    toggleButton.Background = CType(New BrushConverter().ConvertFrom("#AEAEAE"), Brush)
                    toggleButton.Margin = New Thickness(2, 2, 0, 0)

                    Dim icon = TryCast(toggleButton.Content, MaterialDesignThemes.Wpf.PackIcon)
                    If icon IsNot Nothing Then
                        icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Close
                    End If
                End If
            End If
        End Sub
#End Region

#Region "Generate the Quote Before saving"
        Private Sub GenerateCostEstimate_Click(sender As Object, e As RoutedEventArgs)
            Dim productItemsJson As String = SubmitAllProductInputs()

            If productItemsJson Is Nothing Then
                Exit Sub
            End If

            Dim client As Client = _selectedClient

            If client Is Nothing Then
                MessageBox.Show("Please select a client.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            GetAllDataInQuoteProperties(client, productItemsJson)
        End Sub

        ' *** UPDATED: Now collects ProductID per row using a local counter ***
        Private Function SubmitAllProductInputs() As String
            Dim flatList As New List(Of Dictionary(Of String, String))()

            ' *** NEW: Local counter to match _selectedProductIDs keys ***
            Dim rowCount_local As Integer = 0

            For Each categoryWrapper As StackPanel In MainContainer.Children.OfType(Of StackPanel)()

                Dim headerBorder = TryCast(categoryWrapper.Children(0), Border)
                Dim headerGrid = TryCast(headerBorder?.Child, Grid)
                Dim categoryNameTxt = TryCast(headerGrid?.Children(0), TextBox)
                Dim currentCategoryName = If(categoryNameTxt IsNot Nothing, categoryNameTxt.Text.Trim(), "")

                Dim headerRow As New Dictionary(Of String, String)()
                headerRow("ProductName") = currentCategoryName
                headerRow("IsHeaderRow") = "True"
                headerRow("IsCategoryHeader") = "True"
                headerRow("ProductDescription") = ""
                headerRow("ProductID") = ""
                flatList.Add(headerRow)

                Dim itemsPanel = TryCast(categoryWrapper.Children(1), StackPanel)

                For Each productBorder As Border In itemsPanel.Children.OfType(Of Border)()
                    Dim outerStack = TryCast(productBorder.Child, StackPanel)
                    If outerStack Is Nothing Then Continue For

                    Dim productRow = TryCast(outerStack.Children(0), StackPanel)
                    If productRow Is Nothing OrElse productRow.Children.Count < 8 Then Continue For

                    ' *** NEW: Increment local row counter for each product row ***
                    rowCount_local += 1

                    Dim itemData As New Dictionary(Of String, String)()
                    itemData("IsHeaderRow") = "False"
                    itemData("IsCategoryHeader") = "False"

                    itemData("ProductName") = GetInputVal(productRow, 0)

                    ' *** NEW: Look up the saved ProductID for this row ***
                    Dim productNameKey = $"txtProductName_{rowCount_local}"
                    Dim savedProductID As String = ""
                    If _selectedProductIDs.ContainsKey(productNameKey) Then
                        savedProductID = _selectedProductIDs(productNameKey)
                    End If
                    itemData("ProductID") = savedProductID

                    itemData("Quantity") = GetInputVal(productRow, 1)
                    itemData("UnitPrice") = GetInputVal(productRow, 2)
                    itemData("TaxPercent") = GetInputVal(productRow, 3)
                    itemData("TaxValue") = GetInputVal(productRow, 4)
                    itemData("DiscountPercent") = GetInputVal(productRow, 5)
                    itemData("Discount") = GetInputVal(productRow, 6)
                    itemData("LinePrice") = GetInputVal(productRow, 7).Replace("₱", "").Trim()

                    Dim descStack = TryCast(outerStack.Children(1), StackPanel)
                    Dim descBorder = TryCast(descStack?.Children(0), Border)
                    Dim descTxt = TryCast(descBorder?.Child, TextBox)
                    Dim cleanDesc = If(descTxt IsNot Nothing, descTxt.Text.Trim(), "")

                    itemData("ProductDescription") = If(cleanDesc.Contains("Optional"), "", cleanDesc)
                    itemData("Description") = itemData("ProductDescription")

                    If String.IsNullOrWhiteSpace(itemData("ProductName")) OrElse
                       String.IsNullOrWhiteSpace(itemData("Quantity")) Then
                        MessageBox.Show("Please fill in the product name and quantity.")
                        Return Nothing
                    End If

                    flatList.Add(itemData)
                Next
            Next

            Return JsonConvert.SerializeObject(flatList, Formatting.None)
        End Function
#End Region

#Region "Clearing all of the fields"
        Public Sub ClearAllFields()
            Me.UnregisterName(txtDiscountSelection.Name)
            txtQuoteNumber.Clear()
            Dim quoteID As String = QuotesController.GenerateQuoteID()
            txtQuoteNumber.Text = quoteID
            txtSearchCustomer.Clear()
            txtQuoteNote.Text = "None"
            txtTaxSelection.SelectedIndex = 0
            txtDiscountSelection.SelectedIndex = 0
            txtTotalTax.Text = "₱0.00"
            txtTotalDiscount.Text = "₱0.00"
            txtGrandTotal.Text = ""
            TxtClientDetails.Clear()
            ClearAllRows()
            OrderDateVM.SelectedDate = DateTime.Today

            ' *** NEW: Also clear the ProductID cache on full reset ***
            _selectedProductIDs.Clear()

            For Each child As UIElement In MainContainer.Children
                Dim allTextBoxes = FindVisualChildren(Of TextBox)(child)
                For Each txt In allTextBoxes
                    If Not String.IsNullOrWhiteSpace(txt.Name) Then
                        Try
                            UnregisterName(txt.Name)
                        Catch ex As ArgumentException
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
        Private Sub ComboBoxWarehouse_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim selectedItem As ComboBoxItem = TryCast(ComboBoxWarehouse.SelectedItem, ComboBoxItem)

            If selectedItem IsNot Nothing Then
                WarehouseID = Convert.ToInt32(selectedItem.Tag)
                WarehouseName = selectedItem.Content.ToString()
            End If
        End Sub

        Private Function GetValidityDate(validitySelection As String, baseDate As DateTime) As DateTime
            Try
                Dim selection = validitySelection.Trim().ToLower()

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
                        Return baseDate.AddHours(48)
                End Select

            Catch ex As Exception
                Debug.WriteLine($"Error calculating validity date: {ex.Message}")
                Return baseDate.AddHours(48)
            End Try
        End Function

        Private Sub GetAllDataInQuoteProperties(client As Client, productItemsJson As String)
            If Not ValidateQuoteSubmission(client, productItemsJson) Then Exit Sub
            If cmbCostEstimateValidty.SelectedItem Is Nothing Then
                MessageBox.Show("Please select a validity date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            Try
                TransactionState.ResetRecord()
                Dim data = TransactionState.ActiveRecord

                Dim selectedValidityOption = DirectCast(cmbCostEstimateValidty.SelectedItem, ComboBoxItem).Content.ToString()
                Dim actualValidityDate = GetValidityDate(selectedValidityOption, OrderDateVM.SelectedDate.Value)

                data.DocumentTitle = "COST ESTIMATE"
                data.BackButtonLabel = "Back to Quote"
                data.CreatePath = "salesnewquote"

                data.DocumentNumber = txtQuoteNumber.Text
                data.DocumentDate = OrderDateVM.SelectedDate.Value.ToString("MMMM dd, yyyy")
                data.DocumentValidity = actualValidityDate.ToString("MMMM dd, yyyy")
                data.IsEditMode = QuotesController.QuoteNumberExists(txtQuoteNumber.Text)

                data.ClientId = client.ClientID
                data.ClientName = If(Not String.IsNullOrWhiteSpace(client.Company), client.Company, client.Name)
                data.ClientAddress = client.BillingAddress
                data.ClientContact = client.Phone
                data.ClientEmail = client.Email
                data.PreparedBy = CacheOnLoggedInName

                Dim rawItems = JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(productItemsJson)

                For Each dict In rawItems
                    Dim isHeader As Boolean = (dict("IsCategoryHeader") = "True")

                    ' *** UPDATED: ProductID now flows into OrderItems ***
                    Dim newItem As New OrderItems With {
                        .IsHeaderRow = isHeader,
                        .ProductName = If(dict.ContainsKey("ProductName"), dict("ProductName"), ""),
                        .ProductID = If(Not isHeader AndAlso dict.ContainsKey("ProductID"), dict("ProductID"), ""),
                        .ProductDescription = If(isHeader, "", If(dict.ContainsKey("Description"), dict("Description"), "")),
                        .Quantity = If(isHeader, "", If(dict.ContainsKey("Quantity"), dict("Quantity"), "0")),
                        .UnitPrice = If(isHeader, "", "₱ " & If(dict.ContainsKey("UnitPrice"), dict("UnitPrice"), "0.00")),
                        .LinePrice = If(isHeader, "", "₱ " & If(dict.ContainsKey("LinePrice"), dict("LinePrice"), "0.00")),
                        .ProductDescriptionVisibility = If(isHeader OrElse Not dict.ContainsKey("Description") OrElse String.IsNullOrEmpty(dict("Description")), Visibility.Collapsed, Visibility.Visible)
                    }

                    If Not isHeader AndAlso dict.ContainsKey("ProductImageBase64") Then
                        newItem.ProductImage = Base64ToBitmapImage(dict("ProductImageBase64"))
                    End If

                    data.OrderItems.Add(newItem)
                Next

                Dim selectedTaxType As String = CType(txtTaxSelection.SelectedItem, ComboBoxItem).Content.ToString()

                data.Subtotal = "₱" & CostEstimateDetails.CETotalBaseAmount.Replace("₱", "").Trim()
                data.VatValue = txtTotalTax.Text
                data.TotalCost = txtGrandTotal.Text
                data.DiscountValue = txtTotalDiscount.Text
                data.DiscountSelection = txtDiscountSelection.Text

                data.ApprovedBy = cmbApprovedBy.Text
                data.PaymentTerms = cmbPaymentTerm.Text

                data.InstallationFee = lblInstallationFee.Text
                data.FeeValue = lblFee.Text
                data.DeliveryMobilizationLabel = lblFeeType.Text.ToUpper()

                If selectedTaxType = "Exclusive" Then
                    data.VatLabel = "VAT EXCLUSIVE"
                    data.SubtotalLabel = "SUBTOTAL VAT EX."
                Else
                    data.VatLabel = "VAT 12%"
                    data.SubtotalLabel = "SUBTOTAL VAT IN."
                End If

                data.VatType = selectedTaxType
                data.Notes = txtQuoteNote.Text
                data.WarrantyText = "Dream PC Build and IT Solutions Inc. offers 1 year warranty for this cost estimate..."
                data.WarehouseName = ComboBoxWarehouse.Text

                If ComboBoxWarehouse.SelectedIndex >= 0 Then
                    data.WarehouseID = If(ComboBoxWarehouse.SelectedIndex = 0, 12, 13)
                Else
                    data.WarehouseID = data.WarehouseID
                End If

                ViewLoader.DynamicView.NavigateToView("universaleditablepreviewdocument", Me)

            Catch ex As Exception
                MessageBox.Show("Error generating preview: " & ex.Message)
            End Try
        End Sub

        Private Function Base64ToBitmapImage(b64 As String) As BitmapImage
            If String.IsNullOrEmpty(b64) Then Return Nothing
            Try
                If b64.Contains(",") Then b64 = b64.Split(","c)(1)

                Dim bytes As Byte() = Convert.FromBase64String(b64)
                Dim bmp As New BitmapImage()
                Using ms As New MemoryStream(bytes)
                    bmp.BeginInit()
                    bmp.CacheOption = BitmapCacheOption.OnLoad
                    bmp.StreamSource = ms
                    bmp.EndInit()
                End Using
                bmp.Freeze()
                Return bmp
            Catch ex As Exception
                Debug.WriteLine("Image conversion error: " & ex.Message)
                Return Nothing
            End Try
        End Function

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

        Private Function GetInputVal(parent As StackPanel, index As Integer) As String
            Dim borderInput = TryCast(parent.Children(index), Border)
            If borderInput Is Nothing Then Return ""

            Dim txt = TryCast(borderInput.Child, TextBox)
            If txt IsNot Nothing Then Return txt.Text.Trim()

            Dim grid = TryCast(borderInput.Child, Grid)
            If grid IsNot Nothing Then
                Dim gridTxt = TryCast(grid.Children(0), TextBox)
                Return If(gridTxt IsNot Nothing, gridTxt.Text.Trim(), "")
            End If
            Return ""
        End Function
#End Region
    End Class
End Namespace
