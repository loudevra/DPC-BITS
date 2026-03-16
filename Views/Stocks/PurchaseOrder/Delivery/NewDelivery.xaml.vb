Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Models
Imports MaterialDesignThemes.Wpf
Imports Microsoft.Win32
Imports NuGet.Protocol.Plugins
Imports Newtonsoft.Json
Imports System.Linq

Namespace DPC.Views.Stocks.PurchaseOrder.Delivery

    Public Class NewDelivery
        Private _selectedClient As Client
        Private _client As New ObservableCollection(Of Client)
        Private deliveryDate As New CalendarController.SingleCalendar()
        Private itemDataSource As New System.Collections.ObjectModel.ObservableCollection(Of Dictionary(Of String, String))
        Private _productTextBoxes As New Dictionary(Of String, TextBox)
        Private _serialCounters As New Dictionary(Of Integer, Integer)
        Private popupEditSerial As Popup
        Private recentlyClosedSerial As Boolean = False
        Private _isInitialized As Boolean = False
        Private _billingTypingTimer As DispatcherTimer
        Private categoryCount As Integer = 0

        Public Sub New()
            InitializeComponent()

            _billingTypingTimer = New DispatcherTimer With {
                .Interval = TimeSpan.FromMilliseconds(500) ' Wait for 500ms of no typing
            }
            AddHandler _billingTypingTimer.Tick, AddressOf OnBillingTypingTimerTick

            InitializeFields()
            _isInitialized = True
        End Sub

        Public Sub InitializeFields()
            Dim receipt = DeliveryState.CurrentReceipt
            If receipt Is Nothing Then Exit Sub

            txtClientName.Text = receipt.ClientName
            txtInvoiceNumber.Text = receipt.ReferenceInvoice

            GetClientInfo()
            LoadItems()

            If String.IsNullOrWhiteSpace(receipt.DRNumber) Then
                txtDeliveryNumber.Text = GenerateDeliveryId(txtInvoiceNumber.Text)
                rbFullDelivery.IsEnabled = True
            Else
                txtDeliveryNumber.Text = receipt.DRNumber

                txtInvoiceNumber.IsReadOnly = True
                txtInvoiceNumber.Foreground = Brushes.Gray
                rbPartialDelivery.IsChecked = True
                rbFullDelivery.IsChecked = False
                rbFullDelivery.IsEnabled = False
            End If

            Dim drDate As DateTime
            If DateTime.TryParse(receipt.DRDate, drDate) Then
                dtDate.SelectedDate = drDate
            Else
                dtDate.SelectedDate = DateTime.Today
            End If
            txtSelectedDate.Text = dtDate.SelectedDate.Value.ToString("MMM dd, yyyy")

            If Not String.IsNullOrWhiteSpace(receipt.ShippingMethod) Then
                cmbShippingMethod.Text = receipt.ShippingMethod
            End If

            If Not String.IsNullOrWhiteSpace(receipt.DeliveryNotes) Then
                txtDeliveryNote.Text = receipt.DeliveryNotes
            End If

            If Not String.IsNullOrWhiteSpace(receipt.ApprovedBy) Then
                cmbApprovedBy.Text = receipt.ApprovedBy
            End If

            If Not String.IsNullOrWhiteSpace(receipt.PaymentTerm) Then
                cmbPaymentTerm.Text = receipt.PaymentTerm
            End If
        End Sub

        Private Sub GetClientInfo()

            ' Load clients manually before trying to match
            If _client Is Nothing OrElse _client.Count = 0 Then
                _client = ClientController.SearchClient(txtClientName.Text)
            End If

            ' Now we can match safely
            If _client IsNot Nothing AndAlso _client.Count > 0 Then
                Dim match = _client.FirstOrDefault(Function(c) c.Name = txtClientName.Text)
                If match IsNot Nothing Then
                    _selectedClient = match
                    UpdateClientDetails(_selectedClient)
                End If
            End If
        End Sub

        Private Sub GenerateDeliveryReceipt_Click(sender As Object, e As RoutedEventArgs)

            'If Not AreAllSerialsCompleted() Then
            '    MessageBox.Show("Some items have missing serial numbers. Please ensure all items are 'COMPLETE' before generating the receipt.",
            '            "Incomplete Serials",
            '            MessageBoxButton.OK,
            '            MessageBoxImage.Warning)
            '    Return
            'End If

            If Not String.IsNullOrWhiteSpace(txtDeliveryNumber.Text) Then

                DeliveryDetails.DRReferenceInvoice = txtInvoiceNumber.Text
                DeliveryDetails.DRNumber = txtDeliveryNumber.Text
                DeliveryDetails.DRDate = DateTime.Today.ToString("MMM dd, yyyy")

                DeliveryDetails.DRClientName = txtClientName.Text
                DeliveryDetails.DRClientDetails = txtClientDetails.Text
                DeliveryDetails.DRDeliveryNotes = txtDeliveryNote.Text
                DeliveryDetails.DRDeliveryStatus = If(rbFullDelivery.IsChecked = True, "FULL DELIVERY", If(rbPartialDelivery.IsChecked = True, "PARTIAL DELIVERY", "Not Specified"))

                Dim selectedMethod As ComboBoxItem = TryCast(cmbShippingMethod.SelectedItem, ComboBoxItem)
                If selectedMethod IsNot Nothing Then
                    DeliveryDetails.DRShippingMethod = selectedMethod.Content.ToString()
                Else
                    MessageBox.Show("Please select a Shipping Method (e.g., Pick-up or Delivery) before proceeding.",
                        "Selection Required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning)
                    Return
                End If

                DeliveryDetails.DRDeliveryItems = itemDataSource.ToList()

                DeliveryDetails.DRApprovedBy = cmbApprovedBy.Text
                DeliveryDetails.DRPaymentTerm = cmbPaymentTerm.Text

                ViewLoader.DynamicView.NavigateToView("previewprintdeliveryreceipt", Me)
            Else
                MessageBox.Show("Please ensure that you have selected a valid Reference Number to proceed..",
                    "Field Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning)
                Return
            End If
        End Sub

        Private Sub LoadItems()
            Dim model = DeliveryState.CurrentReceipt
            If model Is Nothing OrElse String.IsNullOrWhiteSpace(model.OrderItems) Then Return

            ' 1. RESET UI & TRACKING
            MainContainer.Children.Clear()
            itemDataSource.Clear()
            _productTextBoxes.Clear()
            _serialCounters.Clear()

            ' Parse the flat JSON list
            Dim itemsList = JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(model.OrderItems)

            Dim currentTargetPanel As StackPanel = Nothing
            Dim dataIdx As Integer = 0

            ' 2. ITERATE AND BUILD UI
            For Each item In itemsList
                ' A. Handle Header Rows
                If item.ContainsKey("IsHeaderRow") AndAlso item("IsHeaderRow").ToString().ToLower() = "true" Then
                    ' Get the name from ProductName if CategoryName is null
                    Dim catName = If(item.ContainsKey("CategoryName") AndAlso Not String.IsNullOrEmpty(item("CategoryName")),
                            item("CategoryName"), item("ProductName"))

                    AddNewCategoryWithSpecificName(catName)
                    currentTargetPanel = GetLatestItemsPanel()
                    Continue For
                End If

                ' B. Handle Product Rows
                If currentTargetPanel Is Nothing Then
                    AddNewCategoryWithSpecificName("")
                    currentTargetPanel = GetLatestItemsPanel()
                End If

                ' Prepare Product Data
                Dim displayItem As New Dictionary(Of String, String)(item)
                Dim currentSerials = If(displayItem.ContainsKey("SerialNumber"), displayItem("SerialNumber"), "")

                ' Initialize Serial Counter (For Edit/Continue Mode)
                _serialCounters(dataIdx) = If(String.IsNullOrEmpty(currentSerials), 0,
                                     currentSerials.Split(New String() {"  "}, StringSplitOptions.RemoveEmptyEntries).Length)

                itemDataSource.Add(displayItem)

                ' Create the Product Row UI and add it to the Category's StackPanel
                Dim productUI = AddNewProductUI(item, dataIdx, currentSerials)
                currentTargetPanel.Children.Add(productUI)

                dataIdx += 1
            Next
        End Sub

        Private Function AddNewProductUI(item As Dictionary(Of String, String), idx As Integer, existingSerials As String) As Border
            ' 1. Row Container
            Dim rowBorder As New Border With {
                .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                .BorderThickness = New Thickness(2),
                .CornerRadius = New CornerRadius(15),
                .Padding = New Thickness(10),
                .Margin = New Thickness(0, 5, 0, 5),
                .Background = Brushes.White,
                .HorizontalAlignment = HorizontalAlignment.Stretch
            }

            Dim rowGrid As New Grid()
            rowGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(4, GridUnitType.Star)})
            rowGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(70, GridUnitType.Pixel)})
            rowGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(2, GridUnitType.Star)})
            rowGrid.RowDefinitions.Add(New RowDefinition With {.Height = GridLength.Auto})
            rowGrid.RowDefinitions.Add(New RowDefinition With {.Height = GridLength.Auto})
            rowGrid.RowDefinitions.Add(New RowDefinition With {.Height = GridLength.Auto})

            ' 2. Name & Quantity Row
            Dim txtName As New TextBox With {.Text = item("ProductName"), .IsReadOnly = True, .BorderThickness = New Thickness(0), .Background = Brushes.Transparent, .VerticalContentAlignment = VerticalAlignment.Center, .Padding = New Thickness(10, 0, 10, 0), .FontFamily = New FontFamily("Lexend")}
            Dim nameBorder As New Border With {.Child = txtName, .Height = 50, .Background = CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush), .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush), .BorderThickness = New Thickness(2), .CornerRadius = New CornerRadius(10), .Margin = New Thickness(5, 0, 5, 5)}
            Grid.SetColumn(nameBorder, 0) : rowGrid.Children.Add(nameBorder)

            Dim maxRemaining = If(item.ContainsKey("MaxAllowed"), item("MaxAllowed"), item("Quantity"))
            Dim txtQty As New TextBox With {.Text = item("Quantity"), .IsReadOnly = True, .BorderThickness = New Thickness(0), .Background = Brushes.Transparent, .VerticalContentAlignment = VerticalAlignment.Center, .HorizontalContentAlignment = HorizontalAlignment.Center, .FontFamily = New FontFamily("Lexend"), .Tag = maxRemaining, .Name = $"txtQuantity_{idx}"}
            RegisterControlName(txtQty.Name, txtQty)
            _productTextBoxes.Add(txtQty.Name, txtQty)
            AddHandler txtQty.TextChanged, AddressOf Quantity_TextChanged

            Dim qtyBorder As New Border With {.Child = txtQty, .Height = 50, .Background = Brushes.White, .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush), .BorderThickness = New Thickness(2), .CornerRadius = New CornerRadius(10), .Margin = New Thickness(5, 0, 5, 5), .Name = $"qtyBorder_{idx}"}
            RegisterControlName(qtyBorder.Name, qtyBorder)
            Grid.SetColumn(qtyBorder, 1) : rowGrid.Children.Add(qtyBorder)

            ' 3. Serial Input
            Dim txtSerial As New TextBox With {.Name = $"txtSerialInput_{idx}", .BorderThickness = New Thickness(0), .Background = Brushes.Transparent, .VerticalContentAlignment = VerticalAlignment.Center, .Padding = New Thickness(10, 0, 10, 0), .FontFamily = New FontFamily("Lexend"), .Tag = idx}
            RegisterControlName(txtSerial.Name, txtSerial)
            Dim serialBorder As New Border With {.Child = txtSerial, .Height = 50, .Background = Brushes.White, .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush), .BorderThickness = New Thickness(2), .CornerRadius = New CornerRadius(10), .Margin = New Thickness(5, 0, 5, 5), .Name = $"serialBorder_{idx}"}
            RegisterControlName(serialBorder.Name, serialBorder)
            Grid.SetColumn(serialBorder, 2) : rowGrid.Children.Add(serialBorder)

            ' 4. Header (Labels & Edit Button)
            Dim headerGrid As New Grid With {.Margin = New Thickness(5, 5, 5, 2)}
            headerGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            headerGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            headerGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

            Dim lblRemaining As New TextBlock With {.Name = $"lblRemaining_{idx}", .VerticalAlignment = VerticalAlignment.Center, .FontStyle = FontStyles.Italic, .Foreground = Brushes.Gray, .Margin = New Thickness(5, 0, 0, 0)}
            RegisterControlName(lblRemaining.Name, lblRemaining)

            ' Initial logic for existing data
            Dim currentQty As Integer = 0
            Integer.TryParse(item("Quantity"), currentQty)
            Dim remCount = currentQty - _serialCounters(idx)
            lblRemaining.Text = If(remCount <= 0, "COMPLETE", $"Remaining: {remCount}")

            ' Styling for completed items
            If remCount <= 0 Then
                lblRemaining.Foreground = Brushes.Green : lblRemaining.FontWeight = FontWeights.Bold
                txtSerial.IsReadOnly = True : txtSerial.Text = "DONE" : serialBorder.Background = CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush)
            End If

            Dim btnEditSerial As New Button With {.Style = CType(FindResource("MaterialDesignIconButton"), Style), .Width = 30, .Height = 30, .Content = New PackIcon With {.Kind = PackIconKind.EditOutline, .Width = 18, .Height = 18}, .Tag = idx}
            AddHandler btnEditSerial.Click, AddressOf OpenEditSerialPopup

            Dim lblTitle As New TextBlock With {
                .Text = "Serial Numbers: ",
                .FontWeight = FontWeights.SemiBold,
                .FontFamily = New FontFamily("Lexend"),
                .VerticalAlignment = VerticalAlignment.Center ' Keep text vertically centered
            }
            Grid.SetColumn(lblTitle, 0)
            headerGrid.Children.Add(lblTitle)

            ' lblRemaining (Already created earlier in your code)
            lblRemaining.VerticalAlignment = VerticalAlignment.Center
            Grid.SetColumn(lblRemaining, 1)
            headerGrid.Children.Add(lblRemaining)

            ' btnEditSerial (Already created earlier in your code)
            Grid.SetColumn(btnEditSerial, 2)
            headerGrid.Children.Add(btnEditSerial)

            ' Finally add the headerGrid to the main rowGrid
            Grid.SetRow(headerGrid, 1)
            Grid.SetColumnSpan(headerGrid, 3)
            rowGrid.Children.Add(headerGrid)

            ' 5. Serial List View
            Dim txtSerialList As New TextBlock With {.Name = $"txtSerialList_{idx}", .TextWrapping = TextWrapping.Wrap, .Padding = New Thickness(10), .FontFamily = New FontFamily("Lexend"), .FontSize = 11, .Text = existingSerials}
            RegisterControlName(txtSerialList.Name, txtSerialList)
            Dim listBorder As New Border With {.Child = txtSerialList, .Background = CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush), .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush), .BorderThickness = New Thickness(2), .CornerRadius = New CornerRadius(5), .MinHeight = 80, .Margin = New Thickness(5)}
            Grid.SetRow(listBorder, 2) : Grid.SetColumnSpan(listBorder, 3) : rowGrid.Children.Add(listBorder)

            ' 6. Input Logic
            AddHandler txtSerial.KeyDown, Sub(sender, e)
                                              If e.Key = Key.Enter Then
                                                  Dim input = DirectCast(sender, TextBox)
                                                  Dim val = input.Text.Trim()
                                                  If Not String.IsNullOrEmpty(val) AndAlso _serialCounters(idx) < currentQty Then
                                                      _serialCounters(idx) += 1
                                                      Dim entry = $"({_serialCounters(idx)}) {val}"
                                                      txtSerialList.Text &= If(String.IsNullOrEmpty(txtSerialList.Text), entry, $"  {entry}")

                                                      Dim newRem = currentQty - _serialCounters(idx)
                                                      lblRemaining.Text = If(newRem <= 0, "COMPLETE", $"Remaining: {newRem}")
                                                      If newRem <= 0 Then
                                                          lblRemaining.Foreground = Brushes.Green : lblRemaining.FontWeight = FontWeights.Bold
                                                          input.IsReadOnly = True : input.Text = "DONE" : serialBorder.Background = CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush)
                                                      End If
                                                      itemDataSource(idx)("SerialNumber") = txtSerialList.Text
                                                      input.Clear()
                                                  End If
                                                  e.Handled = True
                                              End If
                                          End Sub

            rowBorder.Child = rowGrid
            Return rowBorder
        End Function

        Private Sub AddProductUI()
            Dim receipt = DeliveryState.CurrentReceipt
            If receipt Is Nothing OrElse String.IsNullOrWhiteSpace(receipt.OrderItems) Then Return

            itemDataSource.Clear()
            MainContainer.Children.Clear()
            _productTextBoxes.Clear()
            _serialCounters.Clear()

            Dim masterItems As List(Of Dictionary(Of String, String)) = JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(receipt.OrderItems)

            Dim i As Integer = 1

            For Each item As Dictionary(Of String, String) In masterItems
                Dim displayItem As New Dictionary(Of String, String)(item)
                displayItem("SerialNumber") = ""
                _serialCounters(i - 1) = 0 ' Initialize counter
                itemDataSource.Add(displayItem)

                ' 1. Outer row container
                Dim rowBorder As New Border With {
                    .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                    .BorderThickness = New Thickness(2),
                    .CornerRadius = New CornerRadius(15),
                    .Padding = New Thickness(10),
                    .Margin = New Thickness(0, 5, 0, 5),
                    .Background = Brushes.White,
                    .HorizontalAlignment = HorizontalAlignment.Stretch
                }

                Dim rowGrid As New Grid()
                rowGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(4, GridUnitType.Star)})
                rowGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(70, GridUnitType.Pixel)})
                rowGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(2, GridUnitType.Star)})

                ' Three rows: Inputs, Header (Labels/Button), and List
                rowGrid.RowDefinitions.Add(New RowDefinition With {.Height = GridLength.Auto})
                rowGrid.RowDefinitions.Add(New RowDefinition With {.Height = GridLength.Auto})
                rowGrid.RowDefinitions.Add(New RowDefinition With {.Height = GridLength.Auto})

                ' 2. Item Name & Qty (Row 0)
                Dim nameBorder As New Border With {
                    .Style = CType(FindResource("RoundedBorderStyle"), Style),
                    .Background = CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush),
                    .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                    .BorderThickness = New Thickness(2),
                    .CornerRadius = New CornerRadius(10),
                    .Height = 50,
                    .Margin = New Thickness(5, 0, 5, 5)
                }
                Dim txtName As New TextBox With {
                    .Text = item("ProductName"),
                    .IsReadOnly = True,
                    .BorderThickness = New Thickness(0),
                    .Background = Brushes.Transparent,
                    .VerticalContentAlignment = VerticalAlignment.Center,
                    .Padding = New Thickness(10, 0, 10, 0),
                    .FontFamily = New FontFamily("Lexend")
                }
                nameBorder.Child = txtName
                Grid.SetRow(nameBorder, 0)
                Grid.SetColumn(nameBorder, 0)
                rowGrid.Children.Add(nameBorder)

                Dim qtyBorder As New Border With {
                    .Style = CType(FindResource("RoundedBorderStyle"), Style),
                    .Background = CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush),
                    .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                    .BorderThickness = New Thickness(2),
                    .CornerRadius = New CornerRadius(10),
                    .Height = 50,
                    .Margin = New Thickness(5, 0, 5, 5)
                }

                qtyBorder.Name = $"qtyBorder_{i - 1}"
                If Me.FindName(qtyBorder.Name) IsNot Nothing Then Me.UnregisterName(qtyBorder.Name)
                Me.RegisterName(qtyBorder.Name, qtyBorder)

                Dim maxRemaining = If(item.ContainsKey("MaxAllowed"), item("MaxAllowed"), item("Quantity"))

                Dim txtQty As New TextBox With {
                    .Text = item("Quantity"),
                    .IsReadOnly = True,
                    .BorderThickness = New Thickness(0),
                    .Background = Brushes.Transparent,
                    .VerticalContentAlignment = VerticalAlignment.Center,
                    .HorizontalContentAlignment = HorizontalAlignment.Center,
                    .FontFamily = New FontFamily("Lexend"),
                    .Tag = maxRemaining
                }

                AddHandler txtQty.TextChanged, AddressOf Quantity_TextChanged

                txtQty.Name = $"txtQuantity_{i - 1}"
                If Me.FindName(txtQty.Name) IsNot Nothing Then Me.UnregisterName(txtQty.Name)
                Me.RegisterName(txtQty.Name, txtQty)
                _productTextBoxes.Add(txtQty.Name, txtQty)

                qtyBorder.Child = txtQty
                Grid.SetRow(qtyBorder, 0)
                Grid.SetColumn(qtyBorder, 1)
                rowGrid.Children.Add(qtyBorder)

                ' 3. Serial Input (Row 0, Col 2)
                Dim serialBorder As New Border With {
                    .Style = CType(FindResource("RoundedBorderStyle"), Style),
                    .Background = Brushes.White,
                    .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                    .BorderThickness = New Thickness(2),
                    .CornerRadius = New CornerRadius(10),
                    .Height = 50,
                    .Margin = New Thickness(5, 0, 5, 5)
                }
                Dim txtSerial As New TextBox With {
                    .Name = $"txtSerialInput_{i}",
                    .BorderThickness = New Thickness(0),
                    .Background = Brushes.Transparent,
                    .VerticalContentAlignment = VerticalAlignment.Center,
                    .Padding = New Thickness(10, 0, 10, 0),
                    .FontFamily = New FontFamily("Lexend"),
                    .Tag = i - 1
                }

                txtSerial.Name = $"txtSerialInput_{i - 1}"
                If Me.FindName(txtSerial.Name) IsNot Nothing Then Me.UnregisterName(txtSerial.Name)
                Me.RegisterName(txtSerial.Name, txtSerial)

                serialBorder.Child = txtSerial
                Grid.SetRow(serialBorder, 0)
                Grid.SetColumn(serialBorder, 2)
                rowGrid.Children.Add(serialBorder)

                ' 4. Header Grid (Row 1): Labels and Edit Button
                Dim headerGrid As New Grid With {
                    .VerticalAlignment = VerticalAlignment.Center,
                    .Margin = New Thickness(5, 5, 5, 2)
                }
                headerGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
                headerGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
                headerGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

                Dim lblSerial As New TextBlock With {
                    .Text = "Serial Numbers: ",
                    .VerticalAlignment = VerticalAlignment.Center,
                    .FontFamily = New FontFamily("Lexend"),
                    .FontWeight = FontWeights.SemiBold,
                    .Foreground = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush)
                }
                Grid.SetColumn(lblSerial, 0)

                Dim lblRemaining As New TextBlock With {
                    .Name = $"lblRemaining_{i}",
                    .Text = $"Remaining: {item("Quantity")}",
                    .VerticalAlignment = VerticalAlignment.Center,
                    .FontStyle = FontStyles.Italic,
                    .Foreground = Brushes.Gray,
                    .Margin = New Thickness(5, 0, 0, 0)
                }

                lblRemaining.Name = $"lblRemaining_{i - 1}"
                If Me.FindName(lblRemaining.Name) IsNot Nothing Then Me.UnregisterName(lblRemaining.Name)
                Me.RegisterName(lblRemaining.Name, lblRemaining)

                Grid.SetColumn(lblRemaining, 1)

                Dim btnEditSerial As New Button With {
                    .Style = CType(FindResource("MaterialDesignIconButton"), Style),
                    .Width = 30, .Height = 30,
                    .Content = New PackIcon With {.Kind = PackIconKind.EditOutline, .Width = 18, .Height = 18},
                    .Cursor = Cursors.Hand,
                    .Tag = i - 1,
                    .VerticalAlignment = VerticalAlignment.Center
                }
                Grid.SetColumn(btnEditSerial, 2)

                headerGrid.Children.Add(lblSerial)
                headerGrid.Children.Add(lblRemaining)
                headerGrid.Children.Add(btnEditSerial)

                Grid.SetRow(headerGrid, 1)
                Grid.SetColumnSpan(headerGrid, 3)
                rowGrid.Children.Add(headerGrid)

                ' 5. Serial List (Row 2)
                Dim serialListBorder As New Border With {
                    .Style = CType(FindResource("RoundedBorderStyle"), Style),
                    .Background = CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush),
                    .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                    .BorderThickness = New Thickness(2),
                    .CornerRadius = New CornerRadius(5),
                    .MinHeight = 80,
                    .Margin = New Thickness(5)
                }

                serialBorder.Name = $"serialBorder_{i - 1}"
                If Me.FindName(serialBorder.Name) IsNot Nothing Then Me.UnregisterName(serialBorder.Name)
                Me.RegisterName(serialBorder.Name, serialBorder)

                Dim txtSerialList As New TextBlock With {
                    .Name = $"txtSerialList_{i}",
                    .TextWrapping = TextWrapping.Wrap,
                    .Padding = New Thickness(10),
                    .FontFamily = New FontFamily("Lexend"),
                    .FontSize = 11
                }

                txtSerialList.Name = $"txtSerialList_{i - 1}"
                If Me.FindName(txtSerialList.Name) IsNot Nothing Then Me.UnregisterName(txtSerialList.Name)
                Me.RegisterName(txtSerialList.Name, txtSerialList)

                serialListBorder.Child = txtSerialList
                Grid.SetRow(serialListBorder, 2)
                Grid.SetColumnSpan(serialListBorder, 3)
                rowGrid.Children.Add(serialListBorder)

                ' Logic
                AddHandler txtSerial.KeyDown, Sub(sender, e)
                                                  If e.Key = Key.Enter Then
                                                      Dim input = DirectCast(sender, TextBox)
                                                      Dim idx As Integer = CInt(input.Tag)
                                                      Dim val = input.Text.Trim()

                                                      Dim currentQty As Integer = 0
                                                      Integer.TryParse(itemDataSource(idx)("Quantity"), currentQty)

                                                      If Not String.IsNullOrEmpty(val) AndAlso _serialCounters(idx) < currentQty Then
                                                          _serialCounters(idx) += 1
                                                          Dim entry = $"({_serialCounters(idx)}) {val}"
                                                          txtSerialList.Text &= If(String.IsNullOrEmpty(txtSerialList.Text), entry, $"  {entry}")

                                                          Dim remCount = currentQty - _serialCounters(idx)
                                                          lblRemaining.Text = If(remCount <= 0, "COMPLETE", $"Remaining: {remCount}")

                                                          If remCount <= 0 Then
                                                              lblRemaining.Foreground = Brushes.Green
                                                              lblRemaining.FontWeight = FontWeights.Bold
                                                              input.IsReadOnly = True
                                                              serialBorder.Background = CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush)
                                                              input.Text = "DONE"
                                                          End If

                                                          itemDataSource(idx)("SerialNumber") = txtSerialList.Text
                                                          input.Clear()
                                                      End If
                                                      e.Handled = True
                                                  End If
                                              End Sub

                AddHandler btnEditSerial.Click, AddressOf OpenEditSerialPopup

                rowBorder.Child = rowGrid
                MainContainer.Children.Add(rowBorder)
                i += 1
            Next
        End Sub

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
                .Margin = New Thickness(0, 5, 0, 0),
                .Height = 45
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
                .VerticalAlignment = VerticalAlignment.Center,
                .IsReadOnly = True
            }

            ' Assemble Header
            headerGrid.Children.Add(categoryHeader)
            Grid.SetColumn(categoryHeader, 0)
            headerBorder.Child = headerGrid

            ' THE ITEMS PANEL (Where product rows go)
            Dim categoryItemsPanel As New StackPanel()

            ' ASSEMBLE
            categoryWrapper.Children.Add(headerBorder)
            categoryWrapper.Children.Add(categoryItemsPanel)

            ' ADD TO MAIN UI
            MainContainer.Children.Add(categoryWrapper)
        End Sub

        Private Sub AddNewCategoryWithSpecificName(catName As String)
            AddNewCategoryUI()

            Dim lastWrapper = TryCast(MainContainer.Children(MainContainer.Children.Count - 1), StackPanel)
            If lastWrapper IsNot Nothing Then
                Dim headerBorder = TryCast(lastWrapper.Children(0), Border)
                Dim headerGrid = TryCast(headerBorder?.Child, Grid)
                Dim nameTxt = TryCast(headerGrid?.Children(0), TextBox)

                If nameTxt IsNot Nothing Then
                    nameTxt.Text = If(String.IsNullOrWhiteSpace(catName), "", catName)
                End If
            End If
        End Sub

        Private Function GetLatestItemsPanel() As StackPanel
            If MainContainer.Children.Count = 0 Then Return Nothing

            Dim lastWrapper = TryCast(MainContainer.Children(MainContainer.Children.Count - 1), StackPanel)
            If lastWrapper Is Nothing OrElse lastWrapper.Children.Count < 2 Then Return Nothing

            Return TryCast(lastWrapper.Children(1), StackPanel)
        End Function

        Private Sub RegisterControlName(name As String, control As FrameworkElement)
            If Me.FindName(name) IsNot Nothing Then
                Me.UnregisterName(name)
            End If
            Me.RegisterName(name, control)
        End Sub

        Private Sub OpenEditSerialPopup(sender As Object, e As RoutedEventArgs)
            Dim clickedButton As Button = TryCast(sender, Button)
            If clickedButton Is Nothing OrElse recentlyClosedSerial Then
                recentlyClosedSerial = False
                Return
            End If

            If popupEditSerial IsNot Nothing AndAlso popupEditSerial.IsOpen Then
                popupEditSerial.IsOpen = False
                Return
            End If

            Dim index As Integer = CInt(clickedButton.Tag)
            Dim item = itemDataSource(index)
            Dim qty As Integer = 0
            Integer.TryParse(item("Quantity"), qty)

            Dim editControl As New DPC.Components.Forms.EditSerialNumberList(item("ProductName"), item("SerialNumber"), qty)

            popupEditSerial = New Popup With {
                .Placement = PlacementMode.AbsolutePoint,
                .StaysOpen = False,
                .AllowsTransparency = True,
                .Child = editControl
            }

            AddHandler popupEditSerial.Opened, Sub()
                                                   Dim screenWidth As Double = SystemParameters.PrimaryScreenWidth
                                                   Dim screenHeight As Double = SystemParameters.PrimaryScreenHeight
                                                   popupEditSerial.HorizontalOffset = (screenWidth / 2) - (editControl.ActualWidth / 2)
                                                   popupEditSerial.VerticalOffset = (screenHeight / 2) - (editControl.ActualHeight / 2)
                                               End Sub

            AddHandler popupEditSerial.Closed, Sub()
                                                   recentlyClosedSerial = True

                                                   If editControl.IsSaved Then
                                                       Dim newList As String = editControl.SerialResult
                                                       Dim newLength As Integer = CInt(editControl.ListLength)

                                                       _serialCounters(index) = newLength

                                                       itemDataSource(index)("SerialNumber") = newList

                                                       Dim targetTxt As TextBlock = TryCast(Me.FindName($"txtSerialList_{index}"), TextBlock)
                                                       Dim lblRemaining As TextBlock = TryCast(Me.FindName($"lblRemaining_{index}"), TextBlock)
                                                       Dim serialBorder As Border = TryCast(Me.FindName($"serialBorder_{index}"), Border)
                                                       Dim input As TextBox = TryCast(Me.FindName($"txtSerialInput_{index}"), TextBox)

                                                       Dim totalQty As Integer = CInt(itemDataSource(index)("Quantity"))
                                                       Dim remCount As Integer = totalQty - newLength

                                                       If lblRemaining IsNot Nothing Then
                                                           lblRemaining.Text = If(remCount <= 0, "COMPLETE", $"Remaining: {remCount}")

                                                           If remCount <= 0 Then
                                                               lblRemaining.Foreground = Brushes.Green
                                                               lblRemaining.FontWeight = FontWeights.Bold

                                                               If input IsNot Nothing Then
                                                                   input.IsReadOnly = True
                                                                   input.Text = "DONE"
                                                               End If

                                                               If serialBorder IsNot Nothing Then
                                                                   serialBorder.Background = CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush)
                                                               End If
                                                           Else
                                                               lblRemaining.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#555555"))
                                                               lblRemaining.FontWeight = FontWeights.Normal
                                                               If input IsNot Nothing Then
                                                                   input.IsReadOnly = False
                                                                   input.Clear()
                                                               End If
                                                               If serialBorder IsNot Nothing Then serialBorder.Background = Brushes.Transparent
                                                           End If
                                                       End If

                                                       If targetTxt IsNot Nothing Then
                                                           targetTxt.Text = newList
                                                       End If
                                                   End If

                                                   Task.Delay(100).ContinueWith(Sub() recentlyClosedSerial = False, TaskScheduler.FromCurrentSynchronizationContext())
                                               End Sub

            popupEditSerial.IsOpen = True
        End Sub

        Private Sub DeliveryMode_Checked(sender As Object, e As RoutedEventArgs)
            Dim isEditMode = DeliveryState.IsEditMode
            If rbPartialDelivery Is Nothing OrElse rbFullDelivery Is Nothing Then Return
            Dim isPartial As Boolean = rbPartialDelivery.IsChecked = True

            txtDeliveryNumber.Text = If(String.IsNullOrEmpty(DeliveryDetails.DRNumber),
                            GenerateDeliveryId(txtInvoiceNumber.Text),
                            DeliveryDetails.DRNumber)

            For Each kvp In _productTextBoxes
                If kvp.Key.StartsWith("txtQuantity_") Then
                    Dim qtyBox = kvp.Value
                    Dim index = kvp.Key.Split("_"c).Last()
                    Dim parentBorder As Border = TryCast(Me.FindName($"qtyBorder_{index}"), Border)

                    If isPartial And Not isEditMode Then
                        If qtyBox.Tag Is Nothing Then qtyBox.Tag = qtyBox.Text
                        qtyBox.IsReadOnly = False

                        If parentBorder IsNot Nothing Then
                            parentBorder.Background = Brushes.White
                        End If
                    Else
                        If qtyBox.Tag IsNot Nothing Then qtyBox.Text = qtyBox.Tag.ToString()
                        qtyBox.IsReadOnly = True

                        If parentBorder IsNot Nothing Then
                            parentBorder.Background = CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush)
                        End If
                    End If
                End If
            Next
        End Sub

        Private Sub Quantity_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim tb = DirectCast(sender, TextBox)
            If tb Is Nothing OrElse String.IsNullOrWhiteSpace(tb.Text) Then Exit Sub

            Dim indexString = tb.Name.Split("_"c).Last()
            Dim index As Integer
            If Not Integer.TryParse(indexString, index) Then Exit Sub

            Dim newQty As Integer = 0
            If Not Integer.TryParse(tb.Text, newQty) Then Exit Sub

            Dim maxAllowed As Integer = 0
            Integer.TryParse(tb.Tag?.ToString(), maxAllowed)

            If newQty > maxAllowed Then
                MessageBox.Show($"Quantity cannot exceed the remaining balance ({maxAllowed}).",
                        "Invalid Quantity", MessageBoxButton.OK, MessageBoxImage.Warning)

                tb.Text = maxAllowed.ToString()
                tb.SelectionStart = tb.Text.Length
                Exit Sub
            End If

            itemDataSource(index)("Quantity") = newQty.ToString()
            _serialCounters(index) = 0
            itemDataSource(index)("SerialNumber") = ""

            Dim lblRemaining As TextBlock = TryCast(Me.FindName($"lblRemaining_{index}"), TextBlock)
            Dim txtSerialList As TextBlock = TryCast(Me.FindName($"txtSerialList_{index}"), TextBlock)
            Dim txtSerialInput As TextBox = TryCast(Me.FindName($"txtSerialInput_{index}"), TextBox)
            Dim serialBorder As Border = TryCast(Me.FindName($"serialBorder_{index}"), Border)

            If txtSerialList IsNot Nothing Then txtSerialList.Text = ""

            If txtSerialInput IsNot Nothing Then
                txtSerialInput.IsReadOnly = (newQty <= 0)
                txtSerialInput.Clear()

                If serialBorder IsNot Nothing Then
                    serialBorder.Background = If(newQty <= 0, CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush), Brushes.White)
                End If
            End If

            If lblRemaining IsNot Nothing Then
                If newQty <= 0 Then
                    lblRemaining.Text = "COMPLETE"
                    lblRemaining.Foreground = Brushes.Green
                    lblRemaining.FontWeight = FontWeights.Bold
                Else
                    lblRemaining.Text = $"Remaining: {newQty}"
                    lblRemaining.Foreground = Brushes.Gray
                    lblRemaining.FontWeight = FontWeights.Normal
                End If
            End If
        End Sub

        Private Sub txtInvoiceNumber_TextChanged(sender As Object, e As TextChangedEventArgs)
            If Not _isInitialized Then Return

            _billingTypingTimer.Stop()
            _billingTypingTimer.Start()
        End Sub

        Private Sub OnBillingTypingTimerTick(sender As Object, e As EventArgs)
            _billingTypingTimer.Stop()

            Dim searchText = txtInvoiceNumber.Text.Trim()

            If searchText.Length < 3 Then
                ClearDeliveryForm()
                Return
            End If

            Dim results = BillingController.SearchBillingStatements(searchText, 1, "Private")

            If results.Count > 0 Then
                Dim billing = results(0)

                If DeliveryState.CurrentReceipt Is Nothing Then
                    DeliveryState.CurrentReceipt = New DeliveryReceiptModel()
                End If

                Dim clientList = ClientController.SearchClients(billing.ClientID)
                Dim clientMatch = clientList.FirstOrDefault(Function(c) c.ClientID = billing.ClientID)

                If clientMatch IsNot Nothing Then
                    _selectedClient = clientMatch
                    txtClientName.Text = _selectedClient.Name
                    UpdateClientDetails(_selectedClient)
                Else
                    txtClientName.Text = billing.ClientID
                End If

                DeliveryState.CurrentReceipt.ClientName = txtClientName.Text
                DeliveryState.CurrentReceipt.ReferenceInvoice = billing.BillingNumber

                Dim historyTotals = DeliveryReceiptController.GetAccumulatedDeliveryTotals(billing.BillingNumber)
                Dim masterItems = JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(billing.OrderItems)

                Dim remainingList As New List(Of Dictionary(Of String, String))

                If masterItems IsNot Nothing Then
                    For Each item In masterItems
                        If (item.ContainsKey("IsHeaderRow") AndAlso item("IsHeaderRow").ToString().ToLower() = "true") OrElse
                   (item.ContainsKey("IsSubotalRow") AndAlso item("IsSubotalRow").ToString().ToLower() = "true") Then
                            remainingList.Add(New Dictionary(Of String, String)(item))
                            Continue For
                        End If

                        Dim pName = item("ProductName")
                        Dim originalQty = 0
                        Integer.TryParse(item("Quantity"), originalQty)

                        Dim deliveredSoFar = If(historyTotals.ContainsKey(pName), historyTotals(pName), 0)
                        Dim balance = originalQty - deliveredSoFar

                        If balance > 0 Then
                            Dim newItem As New Dictionary(Of String, String)(item)
                            newItem("Quantity") = balance.ToString()
                            newItem("MaxAllowed") = balance.ToString()
                            remainingList.Add(newItem)
                        End If
                    Next
                End If

                Dim actualProductCount = remainingList.Where(Function(x)
                                                                 Return Not x.ContainsKey("IsHeaderRow") OrElse
                                                        x("IsHeaderRow").ToString().ToLower() <> "true"
                                                             End Function).Count()

                If actualProductCount = 0 Then
                    MessageBox.Show($"All items in Billing Statement {billing.BillingNumber} are already fully delivered.",
                            "Delivery Complete", MessageBoxButton.OK, MessageBoxImage.Information)
                    txtInvoiceNumber.Clear()
                    ClearDeliveryForm()
                    Return
                End If

                DeliveryState.CurrentReceipt.OrderItems = JsonConvert.SerializeObject(remainingList)

                LoadItems()

                txtDeliveryNumber.Text = GenerateDeliveryId(billing.BillingNumber)
            Else
                ClearDeliveryForm()
            End If
        End Sub

        Private Sub ClearDeliveryForm()
            ' 1. Clear UI Header Fields
            txtClientName.Clear()
            txtClientDetails.Clear()
            cmbPaymentTerm.SelectedIndex = -1
            cmbShippingMethod.SelectedIndex = -1
            cmbApprovedBy.SelectedIndex = -1
            txtDeliveryNote.Clear()

            ' 2. Reset the Delivery ID to default (base invoice ref or empty)
            txtDeliveryNumber.Text = "-"

            ' 3. Clear the Item List and Data Source
            itemDataSource.Clear()
            MainContainer.Children.Clear()
            _productTextBoxes.Clear()
            _serialCounters.Clear()

            ' 4. Clear Global Delivery Cache
            DeliveryDetails.DRClientName = ""
            DeliveryDetails.DRReferenceInvoice = ""
            DeliveryDetails.DRDeliveryItems = New List(Of Dictionary(Of String, String))()
        End Sub

        Private Sub BtnReset_Click(sender As Object, e As RoutedEventArgs) Handles BtnReset.Click
            DeliveryState.ClearDeliveryState()
            lblPageTitle.Text = "Delivery Form"
            lblButton.Text = "GENERATE DELIVERY RECEIPT"
            ViewLoader.DynamicView.NavigateToView("newdelivery", Me)
        End Sub
#Region "Helpers"
        Private Sub UpdateClientDetails(client As Client)
            Dim txtClientDetails As TextBox = TryCast(FindName("txtClientDetails"), TextBox)
            If txtClientDetails Is Nothing OrElse client Is Nothing Then Return

            Dim details As String =
                    $"Representative Name: {client.Representative}{Environment.NewLine}" &
                    $"Company: {client.Company}{Environment.NewLine}" &
                    $"Contact: {client.Phone}{Environment.NewLine}" &
                    $"Email: {client.Email}{Environment.NewLine}"

            If client.BillingAddress Is Nothing Then
                details &= $"{Environment.NewLine}{Environment.NewLine}Delivery Address: (No data)"
            Else
                details &= String.Join(Environment.NewLine, $"Delivery Address: {client.BillingAddress}")
            End If

            txtClientDetails.Text = details
        End Sub

        Private Function AreAllSerialsCompleted() As Boolean
            For Each item In itemDataSource
                Dim requiredQty As Integer = 0
                Integer.TryParse(item("Quantity"), requiredQty)

                Dim index As Integer = itemDataSource.IndexOf(item)
                Dim currentCount As Integer = If(_serialCounters.ContainsKey(index), _serialCounters(index), 0)

                If currentCount < requiredQty Then
                    Return False
                End If
            Next
            Return True
        End Function

        Private Function GenerateDeliveryId(invoiceNumber As String) As String
            Dim baseId As String = invoiceNumber.Trim().Replace("BL", "DR").Replace(" ", "")

            If rbPartialDelivery IsNot Nothing AndAlso rbPartialDelivery.IsChecked = True Then
                If baseId.Contains("(P") Then
                    Try
                        Dim parts = baseId.Split(New String() {"(P"}, StringSplitOptions.None)
                        Dim prefix = parts(0)
                        Dim currentNum As Integer = 0

                        If Integer.TryParse(parts(1), currentNum) Then
                            Return $"{prefix}(P{currentNum + 1})"
                        End If
                    Catch
                        Return baseId & "(P1)"
                    End Try
                Else
                    Return baseId & "(P1)"
                End If
            End If

            Return baseId
        End Function

        Private Sub btnOpenCalendar_Click(sender As Object, e As RoutedEventArgs)
            dtDate.IsDropDownOpen = True
        End Sub

        Private Sub dtDate_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
            If dtDate.SelectedDate.HasValue Then
                ' Update the TextBlock/TextBox display
                txtSelectedDate.Text = dtDate.SelectedDate.Value.ToString("MMM dd, yyyy")
            End If
        End Sub

        Private Sub FetchItemsFromInvoice(invoiceNo As String)
            If String.IsNullOrEmpty(invoiceNo) OrElse invoiceNo = "-" Then Return

            Try
                Dim results = BillingController.SearchBillingStatements(invoiceNo, 1, "Private")

                If results.Count > 0 Then
                    Dim originalStatement = results(0)
                    Dim items = Newtonsoft.Json.JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(originalStatement.OrderItems)

                    If items IsNot Nothing Then
                        DeliveryDetails.DRDeliveryItems = items
                    End If
                End If
            Catch ex As Exception
                Debug.WriteLine("Direct Fetch Error: " & ex.Message)
            End Try
        End Sub

#End Region
    End Class
End Namespace