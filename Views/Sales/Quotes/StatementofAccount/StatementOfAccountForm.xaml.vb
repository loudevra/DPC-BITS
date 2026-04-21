Imports System.Collections.ObjectModel
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports System.Windows.Media
Imports System.Windows.Threading
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Views.SOA

Public Class StatementOfAccountForm
    Private lineItemCount As Integer = 0
    Private paymentItemCount As Integer = 0
    Public WarehouseID As Integer = 12
    Private _dynamicTextBoxes As New Dictionary(Of String, TextBox)
    Private _dynamicDatePickers As New Dictionary(Of String, DatePicker)
    Private _typingTimer As DispatcherTimer
    Private _clients As New ObservableCollection(Of Client)
    Private _selectedClient As Client
    Private _productPopups As New Dictionary(Of String, Popup)
    Private _productListBoxes As New Dictionary(Of String, ListBox)
    Private _productTypingTimers As New Dictionary(Of String, DispatcherTimer)

    Public Sub New()
        InitializeComponent()
        _typingTimer = New DispatcherTimer With {.Interval = TimeSpan.FromMilliseconds(300)}
        AddHandler _typingTimer.Tick, AddressOf OnTypingTimerTick

        AddLineItemUI()
        AddPaymentDetailUI()

        ' Set the automatic number on initialization
        GenerateAutoSOANumber()
    End Sub

#Region "Form Reset Logic"
    Private Sub BtnReset_Click(sender As Object, e As RoutedEventArgs)
        Dim result = MessageBox.Show("Are you sure you want to reset the entire form?", "Confirm Reset", MessageBoxButton.YesNo, MessageBoxImage.Warning)
        If result = MessageBoxResult.No Then Return
        txtSearchCustomer.Text = ""
        TxtClientDetails.Text = ""
        _selectedClient = Nothing
        If txtProjectTitle IsNot Nothing Then txtProjectTitle.Text = ""
        If txtPONo IsNot Nothing Then txtPONo.Text = ""
        If txtSINo IsNot Nothing Then txtSINo.Text = ""
        If txtDRNo IsNot Nothing Then txtDRNo.Text = ""
        If txtBSNo IsNot Nothing Then txtBSNo.Text = ""
        If dpStatementDate IsNot Nothing Then dpStatementDate.SelectedDate = Nothing
        If dpPODate IsNot Nothing Then dpPODate.SelectedDate = Nothing
        If txtDeliveryPeriod IsNot Nothing Then txtDeliveryPeriod.Text = ""
        If dpRequiredDate IsNot Nothing Then dpRequiredDate.SelectedDate = Nothing
        If dpCompletionDate IsNot Nothing Then dpCompletionDate.SelectedDate = Nothing
        If txtContractAmount IsNot Nothing Then txtContractAmount.Text = ""
        If txtLDRate IsNot Nothing Then txtLDRate.Text = "0.1"
        If txtDaysDelayed IsNot Nothing Then txtDaysDelayed.Text = "0"
        ClearDynamicContainer(LineItemsContainer)
        ClearDynamicContainer(PaymentDetailsContainer)
        lineItemCount = 0
        paymentItemCount = 0
        AddLineItemUI()
        AddPaymentDetailUI()
        UpdateSummaryTotals()
        GenerateAutoSOANumber()
    End Sub

    Private Sub ClearDynamicContainer(container As StackPanel)
        If container Is Nothing Then Return
        For Each child As UIElement In container.Children
            For Each txt In FindVisualChildren(Of TextBox)(child)
                If _dynamicTextBoxes.ContainsKey(txt.Name) Then
                    Me.UnregisterName(txt.Name)
                    _dynamicTextBoxes.Remove(txt.Name)
                End If
            Next
            For Each dp In FindVisualChildren(Of DatePicker)(child)
                If _dynamicDatePickers.ContainsKey(dp.Name) Then
                    Me.UnregisterName(dp.Name)
                    _dynamicDatePickers.Remove(dp.Name)
                End If
            Next
        Next
        container.Children.Clear()
    End Sub
    Private Sub StatementDateButton_Click(sender As Object, e As RoutedEventArgs)
        dpStatementDate.IsDropDownOpen = True
    End Sub

    Private Sub PODateButton_Click(sender As Object, e As RoutedEventArgs)
        dpPODate.IsDropDownOpen = True
    End Sub

    Private Sub RequiredDateButton_Click(sender As Object, e As RoutedEventArgs)
        dpRequiredDate.IsDropDownOpen = True
    End Sub

    Private Sub CompletionDateButton_Click(sender As Object, e As RoutedEventArgs)
        dpCompletionDate.IsDropDownOpen = True
    End Sub
#End Region

#Region "Autocomplete for Clients"
    Private Sub txtSearchCustomer_TextChanged(sender As Object, e As TextChangedEventArgs)
        _typingTimer.Stop()
        If String.IsNullOrWhiteSpace(txtSearchCustomer.Text) Then
            AutoCompletePopup.IsOpen = False
            TxtClientDetails.Clear()
            _selectedClient = Nothing
            Return
        End If
        _typingTimer.Start()
    End Sub
    Private Sub OnTypingTimerTick(sender As Object, e As EventArgs)
        _typingTimer.Stop()
        _clients = ClientController.SearchClient(txtSearchCustomer.Text)
        LstItems.ItemsSource = _clients
        AutoCompletePopup.IsOpen = _clients.Count > 0
        AutoCompletePopup.Width = SearchBorder.ActualWidth
    End Sub
    Private Sub LstItems_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If LstItems.SelectedItem IsNot Nothing Then
            _selectedClient = CType(LstItems.SelectedItem, Client)
            RemoveHandler txtSearchCustomer.TextChanged, AddressOf txtSearchCustomer_TextChanged
            txtSearchCustomer.Text = _selectedClient.Name
            AddHandler txtSearchCustomer.TextChanged, AddressOf txtSearchCustomer_TextChanged
            UpdateSupplierDetails(_selectedClient)
            AutoCompletePopup.IsOpen = False
            LstItems.SelectedItem = Nothing
        End If
    End Sub
    Private Sub UpdateSupplierDetails(client As Client)
        If TxtClientDetails Is Nothing OrElse client Is Nothing Then Return
        Dim details As String = $"Representative Name: {client.Representative}{Environment.NewLine}" &
                                $"Company: {client.Company}{Environment.NewLine}" &
                                $"Contact: {client.Phone}{Environment.NewLine}" &
                                $"Email: {client.Email}{Environment.NewLine}"
        If client.BillingAddress Is Nothing Then
            details &= $"{Environment.NewLine}Billing Address: (No data)"
        Else
            details &= $"{Environment.NewLine}Billing Address: {client.BillingAddress}"
        End If
        TxtClientDetails.Text = details
    End Sub
#End Region

#Region "Dynamic Line Items"
    Private Sub AddLineItem_Click(sender As Object, e As RoutedEventArgs)
        AddLineItemUI()
    End Sub
    Private Sub AddLineItemUI()
        lineItemCount += 1
        Dim rowIdx As Integer = lineItemCount
        Dim mainBorder As New Border With {.Margin = New Thickness(0, 2, 0, 2), .Padding = New Thickness(0, 5, 0, 5), .Background = Brushes.Transparent}
        Dim grid As New Grid()
        grid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(130)})
        grid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(1, GridUnitType.Star)})
        grid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(60)})
        grid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(110)})
        grid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(110)})
        grid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(110)})
        grid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(50)})

        Dim borderDate = CreateDatePickerWrapped($"txtLineDate_{rowIdx}")
        Dim borderDesc = CreateProductSearchBoxWrapped($"txtLineDesc_{rowIdx}")
        Dim borderQty = CreateInputBoxWrapped("1", $"txtLineQty_{rowIdx}", HorizontalAlignment.Center)
        Dim borderAmount = CreateInputBoxWrapped("0.00", $"txtLineAmount_{rowIdx}", HorizontalAlignment.Right)
        Dim borderPayment = CreateInputBoxWrapped("0.00", $"txtLinePayment_{rowIdx}", HorizontalAlignment.Right)
        Dim borderBalance = CreateInputBoxWrapped("0.00", $"txtLineBalance_{rowIdx}", HorizontalAlignment.Right, False, True)

        AddHandler _dynamicTextBoxes($"txtLineQty_{rowIdx}").TextChanged, AddressOf LineItemCalculation_TextChanged
        AddHandler _dynamicTextBoxes($"txtLineAmount_{rowIdx}").TextChanged, AddressOf LineItemCalculation_TextChanged
        AddHandler _dynamicTextBoxes($"txtLinePayment_{rowIdx}").TextChanged, AddressOf LineItemCalculation_TextChanged

        Grid.SetColumn(borderDate, 0)
        Grid.SetColumn(borderDesc, 1)
        Grid.SetColumn(borderQty, 2)
        Grid.SetColumn(borderAmount, 3)
        Grid.SetColumn(borderPayment, 4)
        Grid.SetColumn(borderBalance, 5)

        Dim btnDelete = CreateDeleteButton(mainBorder, LineItemsContainer)
        Grid.SetColumn(btnDelete, 6)

        grid.Children.Add(borderDate)
        grid.Children.Add(borderDesc)
        grid.Children.Add(borderQty)
        grid.Children.Add(borderAmount)
        grid.Children.Add(borderPayment)
        grid.Children.Add(borderBalance)
        grid.Children.Add(btnDelete)
        mainBorder.Child = grid
        LineItemsContainer.Children.Add(mainBorder)

        CalculateRowBalance(rowIdx)
        UpdateSummaryTotals()
    End Sub

    Private Sub LineItemCalculation_TextChanged(sender As Object, e As TextChangedEventArgs)
        Dim txt = CType(sender, TextBox)
        If txt.Name Is Nothing Then Return
        Dim parts = txt.Name.Split("_"c)
        If parts.Length < 2 Then Return
        Dim rowIndex As Integer
        If Integer.TryParse(parts(1), rowIndex) Then
            CalculateRowBalance(rowIndex)
            UpdateSummaryTotals()
        End If
    End Sub

    Private Sub CalculateRowBalance(rowIndex As Integer)
        Try
            Dim qtyBox = _dynamicTextBoxes($"txtLineQty_{rowIndex}")
            Dim amountBox = _dynamicTextBoxes($"txtLineAmount_{rowIndex}")
            Dim paymentBox = _dynamicTextBoxes($"txtLinePayment_{rowIndex}")
            Dim balanceBox = _dynamicTextBoxes($"txtLineBalance_{rowIndex}")
            Dim qty As Decimal = 0, amount As Decimal = 0, payment As Decimal = 0
            Decimal.TryParse(qtyBox.Text.Replace(",", ""), qty)
            Decimal.TryParse(amountBox.Text.Replace(",", ""), amount)
            Decimal.TryParse(paymentBox.Text.Replace(",", ""), payment)
            Dim totalAmount = qty * amount
            Dim balance = totalAmount - payment
            balanceBox.Text = balance.ToString("N2")
        Catch ex As Exception
        End Try
    End Sub
#End Region

#Region "Dynamic Payment Details"
    Private Sub AddPaymentRow_Click(sender As Object, e As RoutedEventArgs)
        AddPaymentDetailUI()
    End Sub
    Private Sub AddPaymentDetailUI()
        paymentItemCount += 1
        Dim rowIdx As Integer = paymentItemCount
        Dim mainBorder As New Border With {.Margin = New Thickness(0, 2, 0, 2), .Padding = New Thickness(0, 5, 0, 5), .Background = Brushes.Transparent}
        Dim grid As New Grid()
        grid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(130)})
        grid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(1, GridUnitType.Star)})
        grid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(150)})
        grid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(50)})
        Dim borderDate = CreateDatePickerWrapped($"txtPayDate_{rowIdx}")
        Dim borderRef = CreateInputBoxWrapped("", $"txtPayRef_{rowIdx}", HorizontalAlignment.Left)
        Dim borderAmount = CreateInputBoxWrapped("0.00", $"txtPayAmount_{rowIdx}", HorizontalAlignment.Right)
        AddHandler _dynamicTextBoxes($"txtPayAmount_{rowIdx}").TextChanged, AddressOf CalculationTrigger_TextChanged
        Grid.SetColumn(borderDate, 0)
        Grid.SetColumn(borderRef, 1)
        Grid.SetColumn(borderAmount, 2)
        Dim btnDelete = CreateDeleteButton(mainBorder, PaymentDetailsContainer)
        Grid.SetColumn(btnDelete, 3)
        grid.Children.Add(borderDate)
        grid.Children.Add(borderRef)
        grid.Children.Add(borderAmount)
        grid.Children.Add(btnDelete)
        mainBorder.Child = grid
        PaymentDetailsContainer.Children.Add(mainBorder)
        UpdateSummaryTotals()
    End Sub
#End Region

#Region "Calculations & Summary"
    Public Sub CalculationTrigger_TextChanged(sender As Object, e As TextChangedEventArgs)
        UpdateSummaryTotals()
    End Sub

    Private Sub UpdateSummaryTotals()
        Dim subtotal As Decimal = 0
        Dim totalPayment As Decimal = 0
        For Each key In _dynamicTextBoxes.Keys
            If key.StartsWith("txtLineAmount_") Then
                Dim rowIdxStr = key.Split("_"c)(1)
                Dim qtyBox = _dynamicTextBoxes($"txtLineQty_{rowIdxStr}")
                Dim amountBox = _dynamicTextBoxes($"txtLineAmount_{rowIdxStr}")
                Dim qty As Decimal = 0, amt As Decimal = 0
                Decimal.TryParse(qtyBox.Text, qty)
                Decimal.TryParse(amountBox.Text.Replace(",", ""), amt)
                subtotal += (qty * amt)
            End If
        Next
        For Each key In _dynamicTextBoxes.Keys
            If key.StartsWith("txtPayAmount_") Then
                Dim paymentVal As Decimal = 0
                Decimal.TryParse(_dynamicTextBoxes(key).Text.Replace(",", ""), paymentVal)
                totalPayment += paymentVal
            End If
        Next
        Dim outstandingBalance = subtotal - totalPayment
        Dim contractAmount As Decimal = 0
        Dim ldRate As Decimal = 0
        Dim daysDelayed As Decimal = 0
        Dim uiContractAmt As TextBox = TryCast(Me.FindName("txtContractAmount"), TextBox)
        Dim uiLdRate As TextBox = TryCast(Me.FindName("txtLDRate"), TextBox)
        Dim uiDaysDelayed As TextBox = TryCast(Me.FindName("txtDaysDelayed"), TextBox)
        If uiContractAmt IsNot Nothing Then Decimal.TryParse(uiContractAmt.Text.Replace(",", ""), contractAmount)
        If uiLdRate IsNot Nothing Then Decimal.TryParse(uiLdRate.Text, ldRate)
        If uiDaysDelayed IsNot Nothing Then Decimal.TryParse(uiDaysDelayed.Text, daysDelayed)
        Dim ldPerDay = contractAmount * (ldRate / 100)
        Dim totalLD = ldPerDay * daysDelayed
        Dim netAmountDue = outstandingBalance - totalLD
        Dim lblSub As TextBlock = TryCast(Me.FindName("lblSubtotal"), TextBlock)
        Dim lblPay As TextBlock = TryCast(Me.FindName("lblTotalPayment"), TextBlock)
        Dim lblOut As TextBlock = TryCast(Me.FindName("lblOutstandingBalance"), TextBlock)
        Dim lblLD As TextBlock = TryCast(Me.FindName("lblLiquidatedDamages"), TextBlock)
        Dim lblNet As TextBlock = TryCast(Me.FindName("lblNetAmountDue"), TextBlock)
        If lblSub IsNot Nothing Then lblSub.Text = "₱ " & subtotal.ToString("N2")
        If lblPay IsNot Nothing Then lblPay.Text = "₱ " & totalPayment.ToString("N2")
        If lblOut IsNot Nothing Then lblOut.Text = "₱ " & outstandingBalance.ToString("N2")
        If lblLD IsNot Nothing Then lblLD.Text = "₱ " & totalLD.ToString("N2")
        If lblNet IsNot Nothing Then lblNet.Text = "₱ " & netAmountDue.ToString("N2")
    End Sub

    Private Sub GenerateStatement_Click(sender As Object, e As RoutedEventArgs)
        ' ONLY proceed if the form is completely filled up
        If Not IsFormValid() Then Exit Sub

        ' 1. Collect all form data into the expanded model
        Dim newStatement As New StatementModel With {
            .SOANo = txtSOANo.Text,
            .ClientName = txtSearchCustomer.Text,
            .ClientDetails = TxtClientDetails.Text,
            .ProjectTitle = txtProjectTitle.Text,
            .StatementDate = If(dpStatementDate.SelectedDate.HasValue, dpStatementDate.SelectedDate.Value.ToString("MMMM dd, yyyy"), DateTime.Now.ToString("MMMM dd, yyyy")),
            .PONo = txtPONo.Text,
            .SINo = txtSINo.Text,
            .DRNo = txtDRNo.Text,
            .BSNo = txtBSNo.Text,
            .PODate = If(dpPODate.SelectedDate.HasValue, dpPODate.SelectedDate.Value.ToString("MMMM dd, yyyy"), ""),
            .DeliveryPeriod = txtDeliveryPeriod.Text,
            .RequiredDate = If(dpRequiredDate.SelectedDate.HasValue, dpRequiredDate.SelectedDate.Value.ToString("MMMM dd, yyyy"), ""),
            .CompletionDate = If(dpCompletionDate.SelectedDate.HasValue, dpCompletionDate.SelectedDate.Value.ToString("MMMM dd, yyyy"), ""),
            .ContractAmount = txtContractAmount.Text,
            .Subtotal = lblSubtotal.Text.Replace("₱", "").Trim(),
            .TotalPayment = lblTotalPayment.Text.Replace("₱", "").Trim(),
            .OutstandingBalance = lblOutstandingBalance.Text.Replace("₱", "").Trim(),
            .LiquidatedDamages = lblLiquidatedDamages.Text.Replace("₱", "").Trim(),
            .NetAmountDue = lblNetAmountDue.Text.Replace("₱", "").Trim(),
            .LDDaysDelayed = txtDaysDelayed.Text,
            .LDRate = txtLDRate.Text
        }

        ' Calculate LD per day for the print view
        Dim ldAmount As Decimal = 0
        Dim contractAmt As Decimal = 0
        Dim rate As Decimal = 0
        If Decimal.TryParse(txtContractAmount.Text.Replace(",", ""), contractAmt) AndAlso Decimal.TryParse(txtLDRate.Text, rate) Then
            ldAmount = contractAmt * (rate / 100)
        End If
        newStatement.LDPerDay = ldAmount.ToString("N2")

        ' Extract Dynamic Line Items
        For Each key In _dynamicTextBoxes.Keys
            If key.StartsWith("txtLineDesc_") Then
                Dim rowIdxStr = key.Split("_"c)(1)
                Dim item As New LineItemModel()
                item.Description = _dynamicTextBoxes($"txtLineDesc_{rowIdxStr}").Text
                item.Qty = _dynamicTextBoxes($"txtLineQty_{rowIdxStr}").Text
                item.Amount = _dynamicTextBoxes($"txtLineAmount_{rowIdxStr}").Text
                item.Payment = _dynamicTextBoxes($"txtLinePayment_{rowIdxStr}").Text
                item.Balance = _dynamicTextBoxes($"txtLineBalance_{rowIdxStr}").Text

                If _dynamicDatePickers.ContainsKey($"txtLineDate_{rowIdxStr}") AndAlso _dynamicDatePickers($"txtLineDate_{rowIdxStr}").SelectedDate.HasValue Then
                    item.DateStr = _dynamicDatePickers($"txtLineDate_{rowIdxStr}").SelectedDate.Value.ToString("MM/dd/yyyy")
                End If
                newStatement.LineItems.Add(item)
            End If
        Next

        ' Extract Dynamic Payments
        For Each key In _dynamicTextBoxes.Keys
            If key.StartsWith("txtPayRef_") Then
                Dim rowIdxStr = key.Split("_"c)(1)
                Dim pItem As New PaymentItemModel()
                pItem.Reference = _dynamicTextBoxes($"txtPayRef_{rowIdxStr}").Text
                pItem.AmountPaid = _dynamicTextBoxes($"txtPayAmount_{rowIdxStr}").Text

                If _dynamicDatePickers.ContainsKey($"txtPayDate_{rowIdxStr}") AndAlso _dynamicDatePickers($"txtPayDate_{rowIdxStr}").SelectedDate.HasValue Then
                    pItem.DateStr = _dynamicDatePickers($"txtPayDate_{rowIdxStr}").SelectedDate.Value.ToString("MM/dd/yyyy")
                End If
                newStatement.PaymentItems.Add(pItem)
            End If
        Next

        ' 2. Save
        ManageStatementOfAccount.StatementList.Add(newStatement)

        ' === POPUP THE PREVIEW IN THE SAME WINDOW ===
        ' Instantiate the preview with the data
        Dim printLayout As New SOAPreview(newStatement)

        ' Assign it to the container overlay
        PreviewContainer.Content = printLayout

        ' Show the overlay grid inside the same view
        PreviewOverlay.Visibility = Visibility.Visible

        ' 3. Refresh
        GenerateAutoSOANumber()
    End Sub

    Private Function IsFormValid() As Boolean
        If _selectedClient Is Nothing OrElse String.IsNullOrWhiteSpace(txtSearchCustomer.Text) Then
            MessageBox.Show("Please select a valid Client first.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return False
        End If
        If String.IsNullOrWhiteSpace(txtProjectTitle.Text) Then
            MessageBox.Show("Project Title is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return False
        End If
        If LineItemsContainer.Children.Count = 0 Then
            MessageBox.Show("Please add at least one Line Item.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return False
        End If
        If _dynamicTextBoxes.ContainsKey("txtLineDesc_1") Then
            If String.IsNullOrWhiteSpace(_dynamicTextBoxes("txtLineDesc_1").Text) OrElse
           Val(_dynamicTextBoxes("txtLineAmount_1").Text) <= 0 Then
                MessageBox.Show("Please fill in the Description and Amount for the line items.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return False
            End If
        End If
        Return True
    End Function
#End Region

#Region "UI Helpers"
    Private Function CreateProductSearchBoxWrapped(name As String) As Border
        Dim popupKey As String = $"Popup_{name}"
        Dim listBoxKey As String = $"Lst_{name}"
        Dim timerKey As String = $"Timer_{name}"

        Dim txt As New TextBox With {
        .Name = name,
        .Style = CType(Me.FindResource("RoundedTextboxStyle"), Style),
        .AcceptsReturn = True,
        .TextWrapping = TextWrapping.Wrap,
        .MinHeight = 35
    }
        _dynamicTextBoxes(name) = txt
        If Me.FindName(name) IsNot Nothing Then Me.UnregisterName(name)
        Me.RegisterName(txt.Name, txt)

        Dim suggestionList As New ListBox With {.Name = listBoxKey, .MaxHeight = 150, .Background = Brushes.White}
        Dim factory As New FrameworkElementFactory(GetType(TextBlock))
        factory.SetBinding(TextBlock.TextProperty, New Binding("ProductName"))
        factory.SetValue(TextBlock.PaddingProperty, New Thickness(10, 5, 10, 5))
        suggestionList.ItemTemplate = New DataTemplate() With {.VisualTree = factory}

        Dim popup As New Popup With {
        .Name = popupKey,
        .StaysOpen = False,
        .PlacementTarget = txt,
        .Placement = PlacementMode.Bottom,
        .Child = New Border With {.Background = Brushes.White, .BorderBrush = Brushes.LightGray, .BorderThickness = New Thickness(1), .Child = suggestionList}
    }
        _productListBoxes(listBoxKey) = suggestionList
        _productPopups(popupKey) = popup

        Dim typingTimer As New DispatcherTimer With {.Interval = TimeSpan.FromMilliseconds(300)}
        AddHandler typingTimer.Tick, Sub()
                                         typingTimer.Stop()
                                         Dim keyword = txt.Text.Trim()
                                         If keyword.Length >= 2 Then
                                             Dim results = QuotesController.SearchProductsByName(keyword, WarehouseID)
                                             suggestionList.ItemsSource = results
                                             popup.Width = txt.ActualWidth
                                             popup.IsOpen = results.Count > 0
                                         End If
                                     End Sub

        AddHandler txt.TextChanged, Sub()
                                        typingTimer.Stop()
                                        typingTimer.Start()
                                    End Sub

        AddHandler suggestionList.SelectionChanged, Sub()
                                                        If suggestionList.SelectedItem IsNot Nothing Then
                                                            Dim selectedProduct = CType(suggestionList.SelectedItem, ProductDataModel)
                                                            txt.Text = selectedProduct.ProductName

                                                            Dim parts = name.Split("_"c)
                                                            If parts.Length >= 2 Then
                                                                Dim rowIndex As Integer
                                                                If Integer.TryParse(parts(1), rowIndex) Then
                                                                    Dim rateBoxKey = $"txtLineAmount_{rowIndex}"
                                                                    If _dynamicTextBoxes.ContainsKey(rateBoxKey) Then
                                                                        _dynamicTextBoxes(rateBoxKey).Text = selectedProduct.SellingPrice.ToString("F2")
                                                                        CalculateRowBalance(rowIndex)
                                                                        UpdateSummaryTotals()
                                                                    End If
                                                                End If
                                                            End If
                                                            popup.IsOpen = False
                                                            suggestionList.SelectedItem = Nothing
                                                        End If
                                                    End Sub

        Dim grid As New Grid()
        grid.Children.Add(txt)
        grid.Children.Add(popup)

        Return New Border With {
        .BorderBrush = CType(New BrushConverter().ConvertFrom("#AEAEAE"), Brush),
        .BorderThickness = New Thickness(1),
        .CornerRadius = New CornerRadius(8),
        .Child = grid
    }
    End Function

    Private Function CreateInputBoxWrapped(defaultText As String, name As String, alignment As HorizontalAlignment, Optional multiLine As Boolean = False, Optional isReadOnly As Boolean = False) As Border
        Dim txt As New TextBox With {.Name = name, .Text = defaultText, .Style = CType(Me.FindResource("RoundedTextboxStyle"), Style), .HorizontalContentAlignment = alignment, .Margin = New Thickness(0), .BorderBrush = Brushes.Transparent, .BorderThickness = New Thickness(0), .Background = Brushes.Transparent, .IsReadOnly = isReadOnly}
        If multiLine Then
            txt.AcceptsReturn = True
            txt.TextWrapping = TextWrapping.Wrap
            txt.VerticalContentAlignment = VerticalAlignment.Top
            txt.MinHeight = 35
        End If
        _dynamicTextBoxes(name) = txt
        If Me.FindName(name) IsNot Nothing Then Me.UnregisterName(name)
        Me.RegisterName(txt.Name, txt)
        Dim border As New Border With {.BorderBrush = CType(New BrushConverter().ConvertFrom("#AEAEAE"), Brush), .BorderThickness = New Thickness(1), .Background = If(isReadOnly, CType(New BrushConverter().ConvertFrom("#F9F9F9"), Brush), CType(New BrushConverter().ConvertFrom("#FFFFFF"), Brush)), .CornerRadius = New CornerRadius(8), .Margin = New Thickness(2, 0, 2, 0), .Padding = New Thickness(2), .Child = txt}
        Return border
    End Function

    Private Function CreateDatePickerWrapped(name As String) As Border
        Dim dp As New DatePicker With {.Name = name, .Background = Brushes.Transparent, .BorderThickness = New Thickness(0), .Padding = New Thickness(5), .VerticalAlignment = VerticalAlignment.Center, .HorizontalAlignment = HorizontalAlignment.Stretch}
        If name.StartsWith("txtLineDate_") AndAlso name <> "txtLineDate_1" Then
            If _dynamicDatePickers.ContainsKey("txtLineDate_1") Then
                dp.SelectedDate = _dynamicDatePickers("txtLineDate_1").SelectedDate
            End If
        ElseIf name.StartsWith("txtPayDate_") AndAlso name <> "txtPayDate_1" Then
            If _dynamicDatePickers.ContainsKey("txtPayDate_1") Then
                dp.SelectedDate = _dynamicDatePickers("txtPayDate_1").SelectedDate
            End If
        End If
        _dynamicDatePickers(name) = dp
        If Me.FindName(name) IsNot Nothing Then Me.UnregisterName(name)
        Me.RegisterName(dp.Name, dp)
        AddHandler dp.SelectedDateChanged, AddressOf DatePicker_SelectedDateChanged
        Dim border As New Border With {.BorderBrush = CType(New BrushConverter().ConvertFrom("#AEAEAE"), Brush), .BorderThickness = New Thickness(1), .Background = CType(New BrushConverter().ConvertFrom("#FFFFFF"), Brush), .CornerRadius = New CornerRadius(8), .Margin = New Thickness(2, 0, 2, 0), .Padding = New Thickness(0), .Child = dp}
        Return border
    End Function

    Private Sub DatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
        Dim dp = CType(sender, DatePicker)
        If dp.Name Is Nothing OrElse Not dp.SelectedDate.HasValue Then Return
        If dp.Name = "txtLineDate_1" Then
            For Each key In _dynamicDatePickers.Keys
                If key.StartsWith("txtLineDate_") AndAlso key <> "txtLineDate_1" Then
                    _dynamicDatePickers(key).SelectedDate = dp.SelectedDate
                End If
            Next
        End If
        If dp.Name = "txtPayDate_1" Then
            For Each key In _dynamicDatePickers.Keys
                If key.StartsWith("txtPayDate_") AndAlso key <> "txtPayDate_1" Then
                    _dynamicDatePickers(key).SelectedDate = dp.SelectedDate
                End If
            Next
        End If
    End Sub

    Private Function CreateDeleteButton(containerToRemove As UIElement, targetPanel As StackPanel) As Button
        Dim deleteButton As New Button With {.Background = Brushes.Transparent, .BorderBrush = Brushes.Transparent, .Padding = New Thickness(0), .Cursor = Cursors.Hand, .Width = 35, .Height = 35, .ToolTip = "Remove Entry", .VerticalAlignment = VerticalAlignment.Center}
        Dim icon As New MaterialDesignThemes.Wpf.PackIcon With {.Kind = MaterialDesignThemes.Wpf.PackIconKind.PlaylistRemove, .Foreground = CType(New BrushConverter().ConvertFrom("#D23636"), Brush), .Width = 30, .Height = 30, .HorizontalAlignment = HorizontalAlignment.Center, .VerticalAlignment = VerticalAlignment.Center}
        deleteButton.Content = icon
        AddHandler deleteButton.Click, Sub(sender As Object, e As RoutedEventArgs) HandleRowDeletion(containerToRemove, targetPanel)
        Return deleteButton
    End Function

    Private Sub HandleRowDeletion(containerToRemove As UIElement, targetPanel As StackPanel)
        targetPanel.Children.Remove(containerToRemove)
        Dim keysToRemove As New List(Of String)
        For Each txt In FindVisualChildren(Of TextBox)(containerToRemove)
            If _dynamicTextBoxes.ContainsKey(txt.Name) Then
                keysToRemove.Add(txt.Name)
                Me.UnregisterName(txt.Name)
                Dim timerKey = $"Timer_{txt.Name}"
                If _productTypingTimers.ContainsKey(timerKey) Then
                    _productTypingTimers(timerKey).Stop()
                    _productTypingTimers.Remove(timerKey)
                End If
            End If
        Next
        For Each dp In FindVisualChildren(Of DatePicker)(containerToRemove)
            If _dynamicDatePickers.ContainsKey(dp.Name) Then
                Me.UnregisterName(dp.Name)
                _dynamicDatePickers.Remove(dp.Name)
            End If
        Next
        For Each k As String In keysToRemove
            _dynamicTextBoxes.Remove(k)
        Next
        UpdateSummaryTotals()
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
#End Region

    Private Sub GenerateAutoSOANumber()
        Try
            Dim prefix As String = "SOA-" & DateTime.Now.Year.ToString() & "-"
            Dim nextNumber As Integer = 1

            If ManageStatementOfAccount.StatementList IsNot Nothing AndAlso ManageStatementOfAccount.StatementList.Count > 0 Then
                Dim lastSOA = ManageStatementOfAccount.StatementList _
                    .Where(Function(s) s.SOANo.StartsWith(prefix)) _
                    .OrderByDescending(Function(s) s.SOANo) _
                    .FirstOrDefault()

                If lastSOA IsNot Nothing Then
                    Dim lastPart As String = lastSOA.SOANo.Split("-"c).Last()
                    Dim lastNumericValue As Integer
                    If Integer.TryParse(lastPart, lastNumericValue) Then
                        nextNumber = lastNumericValue + 1
                    End If
                End If
            End If
            txtSOANo.Text = prefix & nextNumber.ToString("D4")
        Catch ex As Exception
            txtSOANo.Text = "SOA-NEW"
        End Try
    End Sub
End Class