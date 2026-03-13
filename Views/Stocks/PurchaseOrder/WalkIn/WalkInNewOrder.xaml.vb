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

            InitializeProductUI()
            rowCount += 1

            ' Set a default date today and tomorrow
            OrderDateVM.SelectedDate = DateTime.Today

            ' Set Date to bind
            billingDate.DataContext = OrderDateVM
            BillingDateButton.DataContext = OrderDateVM



            ' For Tax Selection
            If _TaxSelection Then
                txtTaxSelection.SelectedItem = txtTaxSelection.Items.Cast(Of ComboBoxItem)().FirstOrDefault(Function(i) i.Content.ToString() = "Exclusive")
            Else
                txtTaxSelection.SelectedItem = txtTaxSelection.Items.Cast(Of ComboBoxItem)().FirstOrDefault(Function(i) i.Content.ToString() = "Inclusive")
            End If

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
            Dim model = PreviewState.CurrentPreview

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
            PreviewState.ResetPreview()
            lblPageTitle.Text = "Billing Statement"
            lblButton.Text = "Generate Billing Statement"
            ViewLoader.DynamicView.NavigateToView("walkinorder", Me)
        End Sub

        Private Sub LoadFromUniversalPreview(model As UniversalPreviewModel)
            lblPageTitle.Text = model.EditLabel
            lblButton.Text = model.EditButtonLabel
            txtBillingNumber.Text = model.DocumentNumber
            txtBillingNote.Text = model.Notes


            If model.WarehouseID > 0 Then
                ComboBoxWarehouse.SelectedValue = model.WarehouseID
                WarehouseID = model.WarehouseID
            End If

            FillClientsFieldFromModel(model)

            MainContainer.Children.Clear()
            rowCount = 0

            Dim currentTargetPanel As StackPanel = Nothing

            For Each item As OrderItems In model.Items
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

                    PopulateDynamicRow(rowCount, item)
                End If
            Next

            UpdateGrandTotal()
        End Sub

        Private Sub FillClientsFieldFromModel(model As UniversalPreviewModel)
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
            ' 1. DEFINE KEYS
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

            ' 1. THE MAIN WRAPPER (The Container for this specific group)
            Dim categoryWrapper As New StackPanel With {
        .Margin = New Thickness(0, 10, 0, 20)
    }

            ' 2. THE HEADER BORDER (Visual Background)
            Dim headerBorder As New Border With {
        .Background = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
        .CornerRadius = New CornerRadius(10, 10, 0, 0),
        .Padding = New Thickness(15, 8, 15, 8),
        .Margin = New Thickness(0, 5, 0, 0)
    }

            ' 3. THE HEADER CONTENT (Grid with Text and Delete Button)
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

            ' Unique instance of the Close Icon to avoid "Already a child" error
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
        .Tag = categoryWrapper ' Links button to this specific section for deletion
    }
            AddHandler removeGroupBtn.Click, AddressOf DeleteCategory_Click

            ' Assemble Header
            headerGrid.Children.Add(categoryHeader)
            Grid.SetColumn(categoryHeader, 0)
            headerGrid.Children.Add(removeGroupBtn)
            Grid.SetColumn(removeGroupBtn, 1)
            headerBorder.Child = headerGrid

            ' 4. THE ITEMS PANEL (Where product rows go)
            Dim categoryItemsPanel As New StackPanel()

            ' 5. THE ADD ROW BUTTON (Specific to this group)
            ' Note: We call CreateAddButtonContent() here to get a NEW Icon instance
            Dim addRowBtn As New Button With {
        .Content = CreateAddButtonContent(),
        .HorizontalAlignment = HorizontalAlignment.Center,
        .Margin = New Thickness(0, 10, 0, 0),
        .Style = DirectCast(Me.FindResource("MaterialDesignOutlinedButton"), System.Windows.Style),
        .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
        .Foreground = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
        .Height = 35,
        .Tag = categoryItemsPanel ' Crucial: Links button to its own row panel
    }
            AddHandler addRowBtn.Click, AddressOf CategoryAddRow_Click

            ' 6. ASSEMBLE EVERYTHING INTO THE WRAPPER
            categoryWrapper.Children.Add(headerBorder)
            categoryWrapper.Children.Add(categoryItemsPanel)
            categoryWrapper.Children.Add(addRowBtn)

            ' 7. ADD TO MAIN UI
            MainContainer.Children.Add(categoryWrapper)

            ' 8. ADD INITIAL ROW
            'AddProductInputUI(categoryItemsPanel)
        End Sub

        ' The UI will Add ProductUI to the Interface
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

            productPanel.Children.Add(CreateDeleteButton(mainBorder, targetPanel))
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
            productPanel.Children.Add(CreateDeleteButton(mainBorder, targetPanel))
            targetPanel.Children.Add(mainBorder)
            UpdateGrandTotal()
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
            .MaxWidth = width,
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
        Private Function CreateDeleteButton(containerToRemoveFrom As UIElement, targetPanel As StackPanel) As Button
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
                                               targetPanel.Children.Remove(containerToRemoveFrom)

                                               ' CLEANUP: Unregister names so they can be reused
                                               Dim allTextBoxes = FindVisualChildren(Of TextBox)(containerToRemoveFrom)
                                               For Each txt In allTextBoxes
                                                   If Not String.IsNullOrEmpty(txt.Name) Then
                                                       If Me.FindName(txt.Name) IsNot Nothing Then Me.UnregisterName(txt.Name)
                                                       _productTextBoxes.Remove(txt.Name)
                                                   End If
                                               Next

                                               ' Recalculate totals
                                               UpdateGrandTotal()
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
        Private Sub GenerateBilling_Click(sender As Object, e As RoutedEventArgs)
            Dim productItemsJson As String = SubmitAllProductInputs()

            If productItemsJson Is Nothing Then
                Exit Sub
            End If

            Dim client As Client = _selectedClient

            If client Is Nothing Then
                MessageBox.Show("Please select a client.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            GetAllDataInBillingProperties(client, productItemsJson)
        End Sub

        ' Function for converting all of the product inputs to JSON Format before saving it and print it
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
                headerRow("IsCategoryHeader") = "True"
                headerRow("ProductDescription") = ""
                flatList.Add(headerRow)

                Dim itemsPanel = TryCast(categoryWrapper.Children(1), StackPanel)

                For Each productBorder As Border In itemsPanel.Children.OfType(Of Border)()
                    Dim outerStack = TryCast(productBorder.Child, StackPanel)
                    If outerStack Is Nothing Then Continue For

                    Dim productRow = TryCast(outerStack.Children(0), StackPanel)
                    If productRow Is Nothing OrElse productRow.Children.Count < 8 Then Continue For

                    Dim itemData As New Dictionary(Of String, String)()
                    itemData("IsHeaderRow") = "False"
                    itemData("IsCategoryHeader") = "False"

                    ' --- PRODUCT DATA ---
                    itemData("ProductName") = GetInputVal(productRow, 0)
                    itemData("Quantity") = GetInputVal(productRow, 1)
                    itemData("UnitPrice") = GetInputVal(productRow, 2)
                    itemData("TaxPercent") = GetInputVal(productRow, 3)
                    itemData("TaxValue") = GetInputVal(productRow, 4)
                    itemData("DiscountPercent") = GetInputVal(productRow, 5)
                    itemData("Discount") = GetInputVal(productRow, 6)
                    itemData("LinePrice") = GetInputVal(productRow, 7).Replace("₱", "").Trim()

                    ' --- DESCRIPTION DATA ---
                    Dim descStack = TryCast(outerStack.Children(1), StackPanel)
                    Dim descBorder = TryCast(descStack?.Children(0), Border)
                    Dim descTxt = TryCast(descBorder?.Child, TextBox)
                    Dim cleanDesc = If(descTxt IsNot Nothing, descTxt.Text.Trim(), "")

                    itemData("ProductDescription") = If(cleanDesc.Contains("Optional"), "", cleanDesc)

                    itemData("Description") = itemData("ProductDescription")

                    ' Validation
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
            Try
                PreviewState.ResetPreview()
                Dim data = PreviewState.CurrentPreview

                data.DocumentTitle = "BILLING STATEMENT"
                data.BackButtonLabel = "Back to Billing Statement"
                data.CreatePath = "walkinorder"

                data.DocumentNumber = txtBillingNumber.Text
                data.DocumentDate = OrderDateVM.SelectedDate.Value.ToString("MMMM dd, yyyy")
                data.IsEditMode = QuotesController.QuoteNumberExists(txtBillingNumber.Text)

                data.ClientId = client.ClientID
                data.ClientName = If(Not String.IsNullOrWhiteSpace(client.Company), client.Company, client.Name)
                data.ClientAddress = client.BillingAddress
                data.ClientContact = client.Phone
                data.ClientEmail = client.Email
                data.PreparedBy = CacheOnLoggedInName

                Dim rawItems = JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(productItemsJson)

                For Each dict In rawItems
                    Dim isHeader As Boolean = (dict("IsCategoryHeader") = "True")

                    Dim newItem As New OrderItems With {
                        .IsHeaderRow = isHeader,
                        .ProductName = If(dict.ContainsKey("ProductName"), dict("ProductName"), ""),
                        .ProductDescription = If(isHeader, "", If(dict.ContainsKey("Description"), dict("Description"), "")),
                        .Quantity = If(isHeader, "", If(dict.ContainsKey("Quantity"), dict("Quantity"), "0")),
                        .UnitPrice = If(isHeader, "", "₱ " & If(dict.ContainsKey("UnitPrice"), dict("UnitPrice"), "0.00")),
                        .LinePrice = If(isHeader, "", "₱ " & If(dict.ContainsKey("LinePrice"), dict("LinePrice"), "0.00")),
                        .ProductDescriptionVisibility = If(isHeader OrElse Not dict.ContainsKey("Description") OrElse String.IsNullOrEmpty(dict("Description")), Visibility.Collapsed, Visibility.Visible)
                    }

                    If Not isHeader AndAlso dict.ContainsKey("ProductImageBase64") Then
                        newItem.ProductImage = Base64ToBitmapImage(dict("ProductImageBase64"))
                    End If

                    data.Items.Add(newItem)
                Next

                Dim selectedTaxType As String = CType(txtTaxSelection.SelectedItem, ComboBoxItem).Content.ToString()

                data.Subtotal = "₱" & WalkinBillingStatementDetails.BLSubtotalAmountCache.Replace("₱", "").Trim()
                data.VatValue = txtTotalTax.Text
                data.TotalCost = txtGrandTotal.Text
                data.DiscountValue = txtTotalDiscount.Text
                data.DiscountSelection = txtDiscountSelection.Text

                If selectedTaxType = "Exclusive" Then
                    data.VatLabel = "VAT EXCLUSIVE"
                    data.SubtotalLabel = "SUBTOTAL VAT EX."
                Else
                    data.VatLabel = "VAT 12%"
                    data.SubtotalLabel = "SUBTOTAL VAT IN."
                End If

                data.VatType = selectedTaxType
                data.Notes = txtBillingNote.Text
                data.WarrantyText = "Dream PC Build and IT Solutions Inc. offers 1 year warranty for this cost estimate..."
                data.WarehouseName = ComboBoxWarehouse.Text

                'Currently Hardcoded (Subject to Change)
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
#End Region

        Private Function GetInputVal(parent As StackPanel, index As Integer) As String
            Dim borderInput = TryCast(parent.Children(index), Border)
            If borderInput Is Nothing Then Return ""

            Dim txt = TryCast(borderInput.Child, TextBox)
            If txt IsNot Nothing Then Return txt.Text.Trim()

            ' Handle ProductName Grid
            Dim grid = TryCast(borderInput.Child, Grid)
            If grid IsNot Nothing Then
                Dim gridTxt = TryCast(grid.Children(0), TextBox)
                Return If(gridTxt IsNot Nothing, gridTxt.Text.Trim(), "")
            End If
            Return ""
        End Function
    End Class
End Namespace
