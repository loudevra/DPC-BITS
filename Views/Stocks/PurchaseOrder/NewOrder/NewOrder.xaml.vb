Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports Newtonsoft.Json
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Components.ConfirmationModals

Namespace DPC.Views.Stocks.PurchaseOrder.NewOrder
    Public Class NewOrder

        ' Autocomplete properties
        Private rowCount As Integer = 0
        Private categoryCount As Integer = 0
        Private _isInitialized As Boolean = False
        Private _TaxSelection As Boolean

        Private _typingTimer As DispatcherTimer
        Private _suppliers As New ObservableCollection(Of SupplierDataModel)
        Private _selectedSupplier As SupplierDataModel

        Private _productTypingTimers As New Dictionary(Of String, DispatcherTimer)
        Private _productPopups As New Dictionary(Of String, Popup)
        Private _productListBoxes As New Dictionary(Of String, ListBox)
        Private _productTextBoxes As New Dictionary(Of String, TextBox)

        ' Calendars
        Private OrderDateVM As New CalendarController.SingleCalendar()
        Private OrderDueDateVM As New CalendarController.SingleCalendar()

        Public WarehouseID As Integer
        Public WarehouseName As String

        Public Sub New()
            InitializeComponent()

            ' Autocomplete Setup
            _typingTimer = New DispatcherTimer With {
                .Interval = TimeSpan.FromMilliseconds(300)
            }
            AddHandler _typingTimer.Tick, AddressOf OnTypingTimerTick

            ' Dates Setup
            OrderDateVM.SelectedDate = DateTime.Today
            OrderDueDateVM.SelectedDate = DateTime.Today.AddDays(7)
            OrderDateButton.DataContext = OrderDateVM
            OrderDueDateButton.DataContext = OrderDueDateVM

            ' Load Warehouse and Invoice Number
            ProductController.GetWarehouse(ComboBoxWarehouse)
            txtInvoiceNumber.Text = PurchaseOrderController.GenerateInvoice()

            ' Tax Initialization
            _TaxSelection = False ' Default to Inclusive
            txtTaxSelection_SelectionChanged(txtTaxSelection, Nothing)

            _isInitialized = True
            AddNewCategoryUI()
        End Sub

#Region "Supplier Autocomplete"
        Private Sub txtSearchSupplier_TextChanged(sender As Object, e As TextChangedEventArgs)
            _typingTimer.Stop()
            If String.IsNullOrWhiteSpace(txtSearchSupplier.Text) Then
                AutoCompletePopup.IsOpen = False
                Return
            End If
            _typingTimer.Start()
        End Sub

        Private Sub OnTypingTimerTick(sender As Object, e As EventArgs)
            _typingTimer.Stop()
            _suppliers = SupplierController.SearchSuppliers(txtSearchSupplier.Text)
            LstItems.ItemsSource = _suppliers
            AutoCompletePopup.IsOpen = _suppliers.Count > 0
            AutoCompletePopup.Width = txtSearchSupplier.ActualWidth
        End Sub

        Private Sub LstItems_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If LstItems.SelectedItem IsNot Nothing Then
                Dim previousSupplier As SupplierDataModel = _selectedSupplier
                _selectedSupplier = CType(LstItems.SelectedItem, SupplierDataModel)
                txtSearchSupplier.Text = _selectedSupplier.SupplierName
                UpdateSupplierDetails(_selectedSupplier)
                AutoCompletePopup.IsOpen = False

                If previousSupplier Is Nothing OrElse previousSupplier.SupplierID <> _selectedSupplier.SupplierID Then
                    ClearAllRows()
                    AddNewCategoryUI()
                End If
            End If
        End Sub

        Private Sub UpdateSupplierDetails(supplier As SupplierDataModel)
            If supplier Is Nothing Then Return
            Dim details As String = $"Name: {supplier.SupplierName}{Environment.NewLine}" &
                                    $"Company: {supplier.SupplierCompany}{Environment.NewLine}" &
                                    $"Contact: {supplier.SupplierPhone}{Environment.NewLine}" &
                                    $"Email: {supplier.SupplierEmail}{Environment.NewLine}" &
                                    $"Address: {supplier.OfficeAddress}, {supplier.City}, {supplier.Country}{Environment.NewLine}" &
                                    $"TIN: {supplier.TinID}"
            TxtSupplierDetails.Text = details
        End Sub
#End Region

#Region "Calculation Settings & Properties"
        Private Sub ComboBoxWarehouse_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim selectedItem As ComboBoxItem = TryCast(ComboBoxWarehouse.SelectedItem, ComboBoxItem)
            If selectedItem IsNot Nothing Then
                WarehouseID = Convert.ToInt32(selectedItem.Tag)
                WarehouseName = selectedItem.Content.ToString()
            End If
        End Sub

        Private Sub OrderDateButton_Click(sender As Object, e As RoutedEventArgs)
            OrderDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub OrderDueDateButton_Click(sender As Object, e As RoutedEventArgs)
            OrderDueDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub txtTaxSelection_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If txtTaxSelection.SelectedItem Is Nothing Then Return
            _TaxSelection = CType(txtTaxSelection.SelectedItem, ComboBoxItem).Content.ToString() = "Exclusive"

            For Each kvp In _productTextBoxes
                If kvp.Key.StartsWith("txtTaxPercent_") Then
                    Dim border = TryCast(kvp.Value.Parent, Border)
                    If _TaxSelection Then
                        ' Exclusive (Editable)
                        kvp.Value.Text = "0"
                        kvp.Value.IsReadOnly = False
                        TaxHeader.Header = "TAX(%)"
                        If border IsNot Nothing Then
                            border.BorderThickness = New Thickness(1)
                            border.BorderBrush = CType(New BrushConverter().ConvertFrom("#AEAEAE"), Brush)
                        End If
                    Else
                        ' Inclusive (Locked at 12%)
                        kvp.Value.Text = ""
                        kvp.Value.IsReadOnly = True
                        TaxHeader.Header = "TAX(12%)"
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

        Private Sub txtDeliveryFee_TextChange(sender As Object, e As TextChangedEventArgs)
            If Not _isInitialized Then Return
            Dim tb = DirectCast(sender, TextBox)
            Dim rawInput As String = tb.Text.Replace(",", "").Trim()
            Dim cleanInput As String = Regex.Replace(rawInput, "[^0-9]", "")

            RemoveHandler tb.TextChanged, AddressOf txtDeliveryFee_TextChange

            If String.IsNullOrEmpty(cleanInput) Then
                tb.Text = ""
                lblFee.Text = "₱ 0"
            Else
                Dim val As Long = 0
                If Long.TryParse(cleanInput, val) Then
                    lblFee.Text = $"₱ {val:N0}"
                    Dim caretIndex = tb.CaretIndex
                    Dim oldLength = tb.Text.Length
                    tb.Text = val.ToString("N0")
                    tb.CaretIndex = Math.Max(0, caretIndex + (tb.Text.Length - oldLength))
                End If
            End If

            AddHandler tb.TextChanged, AddressOf txtDeliveryFee_TextChange
            UpdateGrandTotal()
        End Sub
#End Region

#Region "Dynamic UI Generation (Categories and Products)"
        Public Sub AddNewCategory_Click(sender As Object, e As RoutedEventArgs)
            AddNewCategoryUI()
        End Sub

        Private Sub AddNewCategoryUI()
            categoryCount += 1
            Dim currentCatId = categoryCount

            Dim categoryWrapper As New StackPanel With {.Margin = New Thickness(0, 10, 0, 20)}
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
                .FontSize = 14, .FontWeight = FontWeights.SemiBold,
                .Foreground = Brushes.White, .Background = Brushes.Transparent,
                .BorderThickness = New Thickness(0), .FontFamily = New FontFamily("Lexend"),
                .VerticalAlignment = VerticalAlignment.Center
            }

            Dim closeIcon As New MaterialDesignThemes.Wpf.PackIcon With {.Kind = MaterialDesignThemes.Wpf.PackIconKind.Close, .Foreground = Brushes.White}
            Dim removeGroupBtn As New Button With {
                .Content = closeIcon, .Background = Brushes.Transparent,
                .BorderThickness = New Thickness(0), .Cursor = Cursors.Hand,
                .Width = 30, .Height = 30, .Tag = categoryWrapper
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
                .HorizontalAlignment = HorizontalAlignment.Center, .Margin = New Thickness(0, 10, 0, 0),
                .Style = DirectCast(Me.FindResource("MaterialDesignOutlinedButton"), System.Windows.Style),
                .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                .Foreground = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                .Height = 35, .Tag = categoryItemsPanel
            }
            AddHandler addRowBtn.Click, AddressOf CategoryAddRow_Click

            categoryWrapper.Children.Add(headerBorder)
            categoryWrapper.Children.Add(categoryItemsPanel)
            categoryWrapper.Children.Add(addRowBtn)
            MainContainer.Children.Add(categoryWrapper)
        End Sub

        Private Function CreateAddButtonContent() As StackPanel
            Dim sp As New StackPanel With {.Orientation = Orientation.Horizontal}
            Dim icon As New MaterialDesignThemes.Wpf.PackIcon With {.Kind = MaterialDesignThemes.Wpf.PackIconKind.PlaylistAdd, .Margin = New Thickness(0, 0, 5, 0)}
            Dim txt As New TextBlock With {.Text = "Add New Item Row"}
            sp.Children.Add(icon)
            sp.Children.Add(txt)
            Return sp
        End Function

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
                        End If
                    Next
                    UpdateGrandTotal()
                End If
            End If
        End Sub

        Private Sub AddProductInputUI(targetPanel As StackPanel)
            Dim rowIndex As Integer = rowCount

            ' 1. The Outer Container matches the thin, dark, rounded border in your image
            Dim mainBorder As New Border With {
        .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
        .BorderThickness = New Thickness(1),
        .Background = Brushes.White,
        .CornerRadius = New CornerRadius(10),
        .Margin = New Thickness(0, 5, 0, 5),
        .Padding = New Thickness(0, 10, 0, 10)
    }

            Dim mainStack As New StackPanel With {.Orientation = Orientation.Vertical}

            ' 2. Use a Grid for the top row to align perfectly with DataGrid Headers
            Dim productGrid As New Grid()
            productGrid.Margin = New Thickness(0, 0, 0, 0)

            ' Match these GridLengths to your DataGrid column Widths!
            productGrid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(160)}) ' Desc
            productGrid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(60)})  ' Qty
            productGrid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(95)})  ' Rate
            productGrid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(75)})  ' Tax %
            productGrid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(75)})  ' Tax Val
            productGrid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(85)})  ' Disc %
            productGrid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(85)})  ' Disc
            productGrid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(100)}) ' Total
            productGrid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(50)})  ' Delete

            ' Instantiate elements and assign them to columns
            Dim searchBox = CreateProductSearchBox(140, rowIndex)
            Grid.SetColumn(searchBox, 0)

            Dim qtyBox = CreateQuantityBox(rowIndex)
            Grid.SetColumn(qtyBox, 1)

            Dim rateBox = CreateRateBox(rowIndex)
            Grid.SetColumn(rateBox, 2)

            Dim taxPBox = CreateTaxPercentBox(rowIndex)
            Grid.SetColumn(taxPBox, 3)

            Dim taxVBox = CreateTaxValueBox(rowIndex)
            Grid.SetColumn(taxVBox, 4)

            Dim discPBox = CreateDiscountPercentBox(rowIndex)
            Grid.SetColumn(discPBox, 5)

            Dim discVBox = CreateDiscountBox(rowIndex)
            Grid.SetColumn(discVBox, 6)

            Dim amountBox = CreateAmountBox("₱ 0.00", rowIndex)
            Grid.SetColumn(amountBox, 7)

            Dim delBtn = CreateDeleteButton(mainBorder, targetPanel)
            Grid.SetColumn(delBtn, 8)

            ' Add to grid
            productGrid.Children.Add(searchBox)
            productGrid.Children.Add(qtyBox)
            productGrid.Children.Add(rateBox)
            productGrid.Children.Add(taxPBox)
            productGrid.Children.Add(taxVBox)
            productGrid.Children.Add(discPBox)
            productGrid.Children.Add(discVBox)
            productGrid.Children.Add(amountBox)
            productGrid.Children.Add(delBtn)

            ' 3. Description Area Setup
            Dim descriptionTextBox As New TextBox With {
        .Text = "Enter product description (Optional)",
        .BorderThickness = New Thickness(0),
        .Background = Brushes.Transparent,
        .FontFamily = New FontFamily("Lexend"),
        .FontSize = 12,
        .FontWeight = FontWeights.SemiBold,
        .TextWrapping = TextWrapping.Wrap,
        .AcceptsReturn = True
    }

            Dim descriptionBorder As New Border With {
        .Margin = New Thickness(10, 10, 10, 10),
        .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush), ' Matches Search Box
        .BorderThickness = New Thickness(1.5),                                     ' Matches Search Box
        .CornerRadius = New CornerRadius(5),
        .Padding = New Thickness(10),
        .Height = 80,
        .Child = descriptionTextBox
    }

            mainStack.Children.Add(productGrid)
            mainStack.Children.Add(descriptionBorder)

            mainBorder.Child = mainStack
            targetPanel.Children.Add(mainBorder)
            UpdateGrandTotal()
        End Sub

        Public Function CreateProductSearchBox(width As Double, rowIndex As Integer) As Border
            Dim textBoxName As String = $"txtProductName_{rowIndex}"
            Dim popupKey As String = $"ProductPopup_{rowIndex}"
            Dim listBoxKey As String = $"LstProducts_{rowIndex}"
            Dim timerKey As String = $"ProductTimer_{rowIndex}"

            ' Added horizontal padding so text doesn't hug the rounded corners
            Dim textBox As New TextBox With {
        .Name = textBoxName, .FontFamily = New FontFamily("Lexend"), .FontSize = 12, .FontWeight = FontWeights.SemiBold,
        .TextWrapping = TextWrapping.NoWrap, .Padding = New Thickness(10, 0, 10, 0), .BorderThickness = New Thickness(0),
        .MinWidth = width, .MaxWidth = width, .VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
        .VerticalContentAlignment = VerticalAlignment.Center, .Background = Brushes.Transparent
    }

            Dim suggestionList As New ListBox With {.Name = listBoxKey, .MaxHeight = 150, .MinWidth = width}
            Dim factory As New FrameworkElementFactory(GetType(TextBlock))
            factory.SetBinding(TextBlock.TextProperty, New Binding("ProductName"))
            suggestionList.ItemTemplate = New DataTemplate() With {.VisualTree = factory}

            Dim popup As New Popup With {
        .Name = popupKey, .StaysOpen = False, .AllowsTransparency = True, .PlacementTarget = textBox,
        .Child = New Border With {.Background = Brushes.White, .BorderBrush = Brushes.LightGray, .BorderThickness = New Thickness(1), .Child = suggestionList}
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
                                                 If _selectedSupplier IsNot Nothing Then
                                                     ' Adjust this line to match your exact Controller method if needed
                                                     Dim results = PurchaseOrderController.SearchProductsBySupplier(_selectedSupplier.SupplierID, keyword)
                                                     suggestionList.ItemsSource = results
                                                     suggestionList.Visibility = If(results.Count > 0, Visibility.Visible, Visibility.Collapsed)
                                                     popup.IsOpen = results.Count > 0
                                                 End If
                                             Else
                                                 popup.IsOpen = False
                                             End If
                                         End Sub

            AddHandler textBox.TextChanged, Sub()
                                                typingTimer.Stop()
                                                typingTimer.Start()
                                            End Sub

            AddHandler suggestionList.SelectionChanged, Sub(sender As Object, e As SelectionChangedEventArgs)
                                                            If suggestionList.SelectedItem IsNot Nothing Then
                                                                Dim selectedProduct = CType(suggestionList.SelectedItem, ProductDataModel)
                                                                textBox.Text = selectedProduct.ProductName
                                                                popup.IsOpen = False
                                                                suggestionList.SelectedItem = Nothing

                                                                Dim rateBox = FindTextBoxByName($"txtRate_{rowIndex}")
                                                                If rateBox IsNot Nothing Then
                                                                    rateBox.Text = selectedProduct.BuyingPrice.ToString("F2")
                                                                    CalculateAmount(rowIndex)
                                                                End If
                                                            End If
                                                        End Sub

            Dim grid As New Grid()
            grid.Children.Add(textBox)
            grid.Children.Add(popup)

            ' ======= EDITED BORDER PROPERTIES =======
            Return New Border With {
        .Child = grid,
        .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush), ' Dark outline
        .BorderThickness = New Thickness(1.5),                                     ' Slightly thicker for emphasis
        .Background = Brushes.White,
        .CornerRadius = New CornerRadius(12),                                      ' High radius for the pill shape
        .Margin = New Thickness(10, 0, 5, 0),
        .Height = 35,                                                              ' Fixed height to maintain shape
        .VerticalAlignment = VerticalAlignment.Center,
        .HorizontalAlignment = HorizontalAlignment.Stretch
    }
        End Function

        Public Function CreateInputBox(text As String, width As Double, Optional isReadOnly As Boolean = False, Optional name As String = "", Optional alignment As HorizontalAlignment = HorizontalAlignment.Center) As Border
            Dim txt As New TextBox With {
        .Text = text,
        .FontFamily = New FontFamily("Lexend"),
        .FontSize = 12,
        .FontWeight = FontWeights.SemiBold,
        .Padding = New Thickness(5),
        .BorderThickness = New Thickness(0),
        .IsReadOnly = isReadOnly,
        .Width = width,
        .HorizontalContentAlignment = alignment,
        .Background = Brushes.Transparent ' Ensure text background doesn't override borders
    }

            If Not String.IsNullOrWhiteSpace(name) Then
                txt.Name = name
                _productTextBoxes(name) = txt
                If Me.FindName(name) IsNot Nothing Then Me.UnregisterName(name)
                Me.RegisterName(txt.Name, txt)
                If name.StartsWith("txtQuantity_") OrElse name.StartsWith("txtRate_") Then AddHandler txt.TextChanged, AddressOf Quantity_TextChanged
                If name.StartsWith("txtDiscountPercent_") Then AddHandler txt.TextChanged, AddressOf DiscountPercent_TextChanged
            End If

            ' Remove the background and border completely if it's read-only (makes it look like plain text)
            Return New Border With {
        .BorderBrush = If(isReadOnly, Brushes.Transparent, CType(New BrushConverter().ConvertFrom("#AEAEAE"), Brush)),
        .BorderThickness = If(isReadOnly, New Thickness(0), New Thickness(1)),
        .Background = If(isReadOnly, Brushes.Transparent, Brushes.White),
        .CornerRadius = New CornerRadius(5),
        .Margin = New Thickness(0),
        .HorizontalAlignment = HorizontalAlignment.Center,
        .Child = txt
    }
        End Function

        Private Function CreateDeleteButton(containerToRemoveFrom As UIElement, targetPanel As StackPanel) As Button
            Dim deleteButton As New Button With {
        .Background = Brushes.Transparent,
        .BorderBrush = Brushes.Transparent,
        .Width = 40,
        .Height = 40,
        .Cursor = Cursors.Hand,
        .HorizontalAlignment = HorizontalAlignment.Center,
        .Padding = New Thickness(0)
    }
            Dim icon As New MaterialDesignThemes.Wpf.PackIcon With {
        .Kind = MaterialDesignThemes.Wpf.PackIconKind.PlaylistRemove,
        .Foreground = CType(New BrushConverter().ConvertFrom("#D23636"), Brush),
        .Width = 30,
        .Height = 30
    }
            deleteButton.Content = icon

            AddHandler deleteButton.Click, Sub()
                                               targetPanel.Children.Remove(containerToRemoveFrom)
                                               Dim allTextBoxes = FindVisualChildren(Of TextBox)(containerToRemoveFrom)
                                               For Each txt In allTextBoxes
                                                   If Not String.IsNullOrEmpty(txt.Name) Then
                                                       If Me.FindName(txt.Name) IsNot Nothing Then Me.UnregisterName(txt.Name)
                                                       _productTextBoxes.Remove(txt.Name)
                                                   End If
                                               Next
                                               UpdateGrandTotal()
                                           End Sub

            Return deleteButton
        End Function

        Private Function CreateQuantityBox(rowIndex As Integer) As Border
            Return CreateInputBox("1", 45, False, $"txtQuantity_{rowIndex}", HorizontalAlignment.Center)
        End Function
        Private Function CreateRateBox(rowIndex As Integer) As Border
            Return CreateInputBox("", 80, False, $"txtRate_{rowIndex}", HorizontalAlignment.Center)
        End Function
        Private Function CreateTaxPercentBox(rowIndex As Integer) As Border
            Dim box = CreateInputBox(If(Not _TaxSelection, "", "0"), 60, Not _TaxSelection, $"txtTaxPercent_{rowIndex}", HorizontalAlignment.Center)
            Dim txt = TryCast(box.Child, TextBox)
            If txt IsNot Nothing Then AddHandler txt.TextChanged, AddressOf TaxPercent_TextChanged
            Return box
        End Function
        Private Function CreateTaxValueBox(rowIndex As Integer) As Border
            Return CreateInputBox("0.00", 60, True, $"txtTaxValue_{rowIndex}", HorizontalAlignment.Center)
        End Function
        Private Function CreateDiscountPercentBox(rowIndex As Integer) As Border
            Return CreateInputBox("", 60, False, $"txtDiscountPercent_{rowIndex}", HorizontalAlignment.Center)
        End Function
        Private Function CreateDiscountBox(rowIndex As Integer) As Border
            Return CreateInputBox("0.00", 70, True, $"txtDiscount_{rowIndex}", HorizontalAlignment.Center)
        End Function
        Private Function CreateAmountBox(text As String, rowIndex As Integer) As Border
            Return CreateInputBox(text, 90, True, $"txtAmount_{rowIndex}", HorizontalAlignment.Center)
        End Function

#End Region

#Region "Mathematics and Calculations"
        Private Sub Quantity_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim textBox = TryCast(sender, TextBox)
            If textBox Is Nothing Then Exit Sub
            Dim parts = textBox.Name.Split("_"c)
            If parts.Length >= 2 Then CalculateAmount(Integer.Parse(parts(1)))
        End Sub

        Private Sub TaxPercent_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim textBox = TryCast(sender, TextBox)
            If textBox Is Nothing Then Exit Sub
            Dim parts = textBox.Name.Split("_"c)
            If parts.Length >= 2 Then CalculateAmount(Integer.Parse(parts(1)))
        End Sub

        Private Sub DiscountPercent_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim textBox = TryCast(sender, TextBox)
            If textBox Is Nothing Then Exit Sub
            Dim parts = textBox.Name.Split("_"c)
            If parts.Length >= 2 Then CalculateAmount(Integer.Parse(parts(1)))
        End Sub

        Private Function FindTextBoxByName(name As String) As TextBox
            If _productTextBoxes.ContainsKey(name) Then Return _productTextBoxes(name)
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

            If quantityBox Is Nothing OrElse rateBox Is Nothing OrElse amountBox Is Nothing Then Exit Sub

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
            End If

            If taxValueBox IsNot Nothing Then taxValueBox.Text = taxValue.ToString("N2")

            Dim discountValue = baseAmount * (discountPercent / 100)
            Dim finalAmount = baseAmount - discountValue

            If discountBox IsNot Nothing Then discountBox.Text = discountValue.ToString("N2")
            amountBox.Text = "₱" & finalAmount.ToString("N2")

            UpdateGrandTotal()
            UpdateTotalTax()
            UpdateTotalDiscount()
        End Sub

        Public Sub UpdateTotalTax()
            Dim totalTax As Decimal = 0
            Dim taxBoxes = LogicalTreeHelper.GetChildren(MainContainer).OfType(Of UIElement)().SelectMany(Function(border) FindVisualChildren(Of TextBox)(border)).Where(Function(txt) txt.Name IsNot Nothing AndAlso txt.Name.StartsWith("txtTaxValue_")).Distinct()

            For Each txtBox In taxBoxes
                Dim tax As Decimal
                If Decimal.TryParse(txtBox.Text.Replace("₱", "").Trim(), tax) Then totalTax += tax
            Next
            txtTotalTax.Text = "₱" & totalTax.ToString("N2")
        End Sub

        Public Sub UpdateTotalDiscount()
            Dim totalDiscount As Decimal = 0
            Dim discBoxes = LogicalTreeHelper.GetChildren(MainContainer).OfType(Of UIElement)().SelectMany(Function(border) FindVisualChildren(Of TextBox)(border)).Where(Function(txt) txt.Name IsNot Nothing AndAlso txt.Name.StartsWith("txtDiscount_")).Distinct()

            For Each txtBox In discBoxes
                Dim discount As Decimal
                If Decimal.TryParse(txtBox.Text.Replace("₱", "").Trim(), discount) Then totalDiscount += discount
            Next
            txtTotalDiscount.Text = "₱" & totalDiscount.ToString("N2")
        End Sub

        Public Sub UpdateGrandTotal()
            Dim subtotalAmount As Decimal = 0
            Dim totalTaxAmount As Decimal = 0
            Dim deliveryFee As Decimal = 0

            Dim amountBoxes = LogicalTreeHelper.GetChildren(MainContainer).OfType(Of UIElement)().SelectMany(Function(border) FindVisualChildren(Of TextBox)(border)).Where(Function(txt) txt.Name IsNot Nothing AndAlso txt.Name.StartsWith("txtAmount_")).Distinct()
            For Each txtBox In amountBoxes
                Dim amount As Decimal
                If Decimal.TryParse(txtBox.Text.Replace("₱", "").Replace(",", "").Trim(), amount) Then subtotalAmount += amount
            Next

            Decimal.TryParse(txtDeliveryFee.Text.Replace("₱", "").Replace(",", "").Trim(), deliveryFee)
            UpdateTotalTax()
            Decimal.TryParse(txtTotalTax.Text.Replace("₱", "").Replace(",", "").Trim(), totalTaxAmount)

            Dim finalGrandTotal As Decimal = 0
            If _TaxSelection Then
                finalGrandTotal = subtotalAmount + deliveryFee + totalTaxAmount
            Else
                finalGrandTotal = subtotalAmount + deliveryFee
            End If

            txtGrandTotal.Text = "₱" & finalGrandTotal.ToString("N2")
        End Sub

        Private Iterator Function FindVisualChildren(Of T As DependencyObject)(depObj As DependencyObject) As IEnumerable(Of T)
            If depObj IsNot Nothing Then
                For i As Integer = 0 To VisualTreeHelper.GetChildrenCount(depObj) - 1
                    Dim child As DependencyObject = VisualTreeHelper.GetChild(depObj, i)
                    If child IsNot Nothing AndAlso TypeOf child Is T Then Yield CType(child, T)
                    For Each childOfChild In FindVisualChildren(Of T)(child)
                        Yield childOfChild
                    Next
                Next
            End If
        End Function
#End Region

#Region "Form Actions and Order Submission"
        Private Sub ClearAllRows()
            MainContainer.Children.Clear()
            _productTextBoxes.Clear()
            _productListBoxes.Clear()
            _productPopups.Clear()
            _productTypingTimers.Clear()
            rowCount = 0
            categoryCount = 0
        End Sub

        Private Sub BtnAddSupplier_Click(sender As Object, e As RoutedEventArgs)
            ' Adjust Navigation to your specific page logic
            ViewLoader.DynamicView.NavigateToView("newsuppliers", Me)
        End Sub

        Private Sub BtnReset_Click(sender As Object, e As RoutedEventArgs)
            txtSearchSupplier.Clear()
            txtInvoiceNumber.Text = PurchaseOrderController.GenerateInvoice()
            txtOrderNote.Clear()
            txtTaxSelection.SelectedIndex = 0
            txtDiscountSelection.SelectedIndex = 0
            txtDeliveryFee.Clear()
            txtTotalTax.Text = "₱0.00"
            txtTotalDiscount.Text = "₱0.00"
            txtGrandTotal.Text = "₱0.00"
            TxtSupplierDetails.Clear()
            _selectedSupplier = Nothing
            ClearAllRows()
            AddNewCategoryUI()
        End Sub

        Private Sub btnGenerateOrder_Click(sender As Object, e As RoutedEventArgs)
            If _selectedSupplier Is Nothing Then
                MessageBox.Show("Please select a supplier for this order.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            If WarehouseID <= 0 Then
                MessageBox.Show("Please select a valid warehouse.")
                Return
            End If

            Dim productItemsJson As String = SubmitAllProductInputs()
            If String.IsNullOrWhiteSpace(productItemsJson) Then Return

            ' Implement your specific Database Insertion Logic here leveraging PurchaseOrderController
            Dim isSuccess = PurchaseOrderController.InsertPurchaseOrder(txtInvoiceNumber.Text, OrderDateVM.SelectedDate.Value.ToString("yyyy-MM-dd"), OrderDueDateVM.SelectedDate.Value.ToString("yyyy-MM-dd"), txtTaxSelection.Text, txtDiscountSelection.Text, _selectedSupplier.SupplierID, _selectedSupplier.SupplierName, WarehouseID, WarehouseName, productItemsJson, txtGrandTotal.Text, txtTotalTax.Text, txtTotalDiscount.Text, txtOrderNote.Text)

            If isSuccess Then
                MessageBox.Show("Purchase order created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                ' ViewLoader.DynamicView.NavigateToView("purchaseorderstatement", Me)
            Else
                MessageBox.Show("Error generating the purchase order.")
            End If
        End Sub

        Private Function SubmitAllProductInputs() As String
            Dim flatList As New List(Of Dictionary(Of String, String))()

            For Each categoryWrapper As StackPanel In MainContainer.Children.OfType(Of StackPanel)()
                Dim headerBorder = TryCast(categoryWrapper.Children(0), Border)
                Dim headerGrid = TryCast(headerBorder?.Child, Grid)
                Dim categoryNameTxt = TryCast(headerGrid?.Children(0), TextBox)
                Dim currentCategoryName = If(categoryNameTxt IsNot Nothing, categoryNameTxt.Text.Trim(), "")

                Dim headerRow As New Dictionary(Of String, String)()
                headerRow("ProductName") = currentCategoryName
                headerRow("IsHeaderRow") = "True"
                flatList.Add(headerRow)

                Dim itemsPanel = TryCast(categoryWrapper.Children(1), StackPanel)
                For Each productBorder As Border In itemsPanel.Children.OfType(Of Border)()
                    Dim outerStack = TryCast(productBorder.Child, StackPanel)
                    If outerStack Is Nothing Then Continue For

                    Dim productRow = TryCast(outerStack.Children(0), StackPanel)
                    If productRow Is Nothing OrElse productRow.Children.Count < 8 Then Continue For

                    Dim itemData As New Dictionary(Of String, String)()
                    itemData("IsHeaderRow") = "False"
                    itemData("ProductName") = GetInputVal(productRow, 0)
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
                    itemData("Description") = If(cleanDesc.Contains("Optional"), "", cleanDesc)

                    If String.IsNullOrWhiteSpace(itemData("ProductName")) OrElse String.IsNullOrWhiteSpace(itemData("Quantity")) Then
                        MessageBox.Show("Please fill in the product name and quantity.")
                        Return Nothing
                    End If

                    flatList.Add(itemData)
                Next
            Next

            If flatList.Count = 0 Then
                MessageBox.Show("No products found in the order.")
                Return Nothing
            End If

            Return JsonConvert.SerializeObject(flatList, Formatting.None)
        End Function

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