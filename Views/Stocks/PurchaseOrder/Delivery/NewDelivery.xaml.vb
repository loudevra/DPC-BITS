Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Linq
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports MaterialDesignThemes.Wpf
Imports Microsoft.Win32
Imports Newtonsoft.Json
Imports NuGet.Protocol.Plugins

Namespace DPC.Views.Stocks.PurchaseOrder.Delivery

    Public Class NewDelivery
        Private _selectedClient As Client
        Private _client As New ObservableCollection(Of Client)
        Private deliveryDate As New CalendarController.SingleCalendar()
        Private itemDataSource As New System.Collections.ObjectModel.ObservableCollection(Of Dictionary(Of String, String))
        Private itemOriginalData As New System.Collections.ObjectModel.ObservableCollection(Of Dictionary(Of String, String))
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
                .Interval = TimeSpan.FromMilliseconds(500)
            }
            AddHandler _billingTypingTimer.Tick, AddressOf OnBillingTypingTimerTick

            InitializeFields()
            _isInitialized = True
        End Sub

        Public Sub InitializeFields()
            Dim receipt = TransactionState.ActiveRecord
            If receipt Is Nothing Then Exit Sub

            txtClientName.Text = receipt.ClientName
            txtInvoiceNumber.Text = receipt.DocumentReference

            GetClientInfo()
            LoadItems()

            If String.IsNullOrWhiteSpace(receipt.DocumentNumber) Then
                txtDeliveryNumber.Text = GenerateDeliveryId(txtInvoiceNumber.Text)
                rbFullDelivery.IsEnabled = True
            Else
                If (Not receipt.DocumentNumber.StartsWith("DR-")) Then
                    Dim referenceNumber As String = receipt.DocumentNumber
                    Dim newReferenceNumber As String = ""
                    Dim firstHyphenIndex As Integer = referenceNumber.IndexOf("-"c)

                    If firstHyphenIndex <> -1 Then
                        Dim remainder As String = referenceNumber.Substring(firstHyphenIndex + 1)
                        newReferenceNumber = "DR-" & remainder
                    End If

                    txtDeliveryNumber.Text = newReferenceNumber
                    txtInvoiceNumber.Text = receipt.DocumentNumber
                    txtInvoiceNumber.IsReadOnly = True
                    txtInvoiceNumber.Foreground = Brushes.Gray
                Else
                    txtDeliveryNumber.Text = receipt.DocumentNumber
                    txtInvoiceNumber.IsReadOnly = True
                    txtInvoiceNumber.Foreground = Brushes.Gray
                    rbPartialDelivery.IsChecked = True
                    rbFullDelivery.IsChecked = False
                    rbFullDelivery.IsEnabled = False
                End If
            End If

            Dim drDate As DateTime
            If DateTime.TryParse(receipt.DocumentDate, drDate) Then
                dtDate.SelectedDate = drDate
            Else
                dtDate.SelectedDate = DateTime.Today
            End If
            txtSelectedDate.Text = dtDate.SelectedDate.Value.ToString("MMM dd, yyyy")

            If Not String.IsNullOrWhiteSpace(receipt.ShippingMethod) Then
                cmbShippingMethod.Text = receipt.ShippingMethod
            End If

            If Not String.IsNullOrWhiteSpace(receipt.Notes) Then
                txtDeliveryNote.Text = receipt.Notes
            End If

            If Not String.IsNullOrWhiteSpace(receipt.ApprovedBy) Then
                cmbApprovedBy.Text = receipt.ApprovedBy
            End If

            If Not String.IsNullOrWhiteSpace(receipt.PaymentTerm) Then
                cmbPaymentTerm.Text = receipt.PaymentTerm
            End If
        End Sub

        Private Sub GetClientInfo()
            If _client Is Nothing OrElse _client.Count = 0 Then
                _client = ClientController.SearchClient(txtClientName.Text)
            End If

            If _client IsNot Nothing AndAlso _client.Count > 0 Then
                Dim match = _client.FirstOrDefault(Function(c) c.Name = txtClientName.Text)
                If match IsNot Nothing Then
                    _selectedClient = match
                    UpdateClientDetails(_selectedClient)
                End If
            End If
        End Sub

        Private Sub GenerateDeliveryReceipt_Click(sender As Object, e As RoutedEventArgs)
            If Not String.IsNullOrWhiteSpace(txtDeliveryNumber.Text) Then

                If DeliveryState.CurrentReceipt Is Nothing Then
                    DeliveryState.CurrentReceipt = New DeliveryReceiptModel()
                End If

                DeliveryDetails.DRDocumentReference = txtInvoiceNumber.Text
                DeliveryDetails.DRNumber = txtDeliveryNumber.Text
                DeliveryDetails.DRDate = DateTime.Today.ToString("MMM dd, yyyy")
                DeliveryDetails.DRClientName = txtClientName.Text
                DeliveryDetails.DRClientDetails = txtClientDetails.Text
                DeliveryDetails.DRDeliveryNotes = txtDeliveryNote.Text

                Dim receipt = DeliveryState.CurrentReceipt
                receipt.DocumentReference = txtInvoiceNumber.Text
                receipt.DRNumber = txtDeliveryNumber.Text
                receipt.DRDate = DateTime.Today.ToString("MMM dd, yyyy")
                receipt.ClientName = txtClientName.Text
                receipt.ClientDetails = txtClientDetails.Text
                receipt.DeliveryNotes = txtDeliveryNote.Text

                Dim status = If(rbFullDelivery.IsChecked = True, "FULL DELIVERY",
                     If(rbPartialDelivery.IsChecked = True, "PARTIAL DELIVERY", "Not Specified"))
                DeliveryDetails.DRDeliveryStatus = status
                receipt.DeliveryStatus = status

                Dim selectedMethod As ComboBoxItem = TryCast(cmbShippingMethod.SelectedItem, ComboBoxItem)
                If selectedMethod IsNot Nothing Then
                    Dim methodStr = selectedMethod.Content.ToString()
                    DeliveryDetails.DRShippingMethod = methodStr
                    receipt.ShippingMethod = methodStr
                Else
                    MessageBox.Show("Please select a Shipping Method before proceeding.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If

                DeliveryDetails.DRApprovedBy = cmbApprovedBy.Text
                receipt.ApprovedBy = cmbApprovedBy.Text

                DeliveryDetails.DRPaymentTerm = cmbPaymentTerm.Text
                receipt.PaymentTerm = cmbPaymentTerm.Text

                DeliveryDetails.DRDeliveryItems = itemDataSource.ToList()
                receipt.OrderItems = JsonConvert.SerializeObject(itemDataSource.ToList())

                ViewLoader.DynamicView.NavigateToView("previewprintdeliveryreceipt", Me)
            Else
                MessageBox.Show("Please ensure that you have selected a valid Reference Number to proceed.", "Field Required", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If
        End Sub

        Private Sub LoadItems()
            Dim model = TransactionState.ActiveRecord

            MainContainer.Children.Clear()
            itemDataSource.Clear()
            _productTextBoxes.Clear()
            _serialCounters.Clear()
            categoryCount = 0

            If model?.OrderItems Is Nothing OrElse model.OrderItems.Count = 0 Then Return

            Dim currentTargetPanel As StackPanel = Nothing
            Dim masterIdx As Integer = 0

            For Each item In model.OrderItems
                Dim displayItem As New Dictionary(Of String, String) From {
                    {"ProductName", If(item.ProductName, "Unknown Item")},
                    {"Quantity", If(item.Quantity, "0")},
                    {"SerialNumber", If(item.SerialNumber, "")},
                    {"IsHeaderRow", item.IsHeaderRow.ToString().ToLower()},
                    {"MaxAllowed", If(Not String.IsNullOrEmpty(item.MaxAllowed), item.MaxAllowed, item.Quantity)},
                    {"ProductID", If(Not String.IsNullOrEmpty(item.ProductID), item.ProductID, "")}
                }

                If displayItem("IsHeaderRow").ToLower() = "true" Then
                    itemDataSource.Add(displayItem)
                    Dim catName = If(Not String.IsNullOrEmpty(item.ProductName), item.ProductName, "New Category")
                    AddNewCategoryWithSpecificName(catName)
                    currentTargetPanel = GetLatestItemsPanel()
                    masterIdx += 1
                    Continue For
                End If

                If currentTargetPanel Is Nothing Then
                    AddNewCategoryWithSpecificName("General Items")
                    currentTargetPanel = GetLatestItemsPanel()
                End If

                If String.IsNullOrEmpty(displayItem("SerialNumber")) Then
                    Dim fetchedSerials As List(Of String)

                    If Not String.IsNullOrEmpty(displayItem("ProductID")) Then
                        fetchedSerials = DeliveryReceiptController.GetAvailableSerialsForProduct(displayItem("ProductID"))
                    Else
                        fetchedSerials = DeliveryReceiptController.GetAvailableSerialsForProductByName(displayItem("ProductName"))
                    End If

                    Dim deliveryQty As Integer = 0
                    Integer.TryParse(displayItem("Quantity"), deliveryQty)

                    If fetchedSerials IsNot Nothing AndAlso fetchedSerials.Count > 0 Then
                        Dim capped = fetchedSerials.Take(deliveryQty).ToList()
                        displayItem("SerialNumber") = DeliveryReceiptController.FormatSerialsForDisplay(capped)
                    End If
                End If

                itemDataSource.Add(displayItem)

                Dim currentSerials = displayItem("SerialNumber")
                Dim serialCount As Integer = 0
                If Not String.IsNullOrEmpty(currentSerials) Then
                    serialCount = currentSerials.Split(New String() {"  "}, StringSplitOptions.RemoveEmptyEntries).Length
                End If
                _serialCounters(masterIdx) = serialCount

                Dim productUI = AddNewProductUI(displayItem, masterIdx, currentSerials)
                If productUI IsNot Nothing Then
                    currentTargetPanel.Children.Add(productUI)
                End If

                masterIdx += 1
            Next
        End Sub

        Private Function AddNewProductUI(item As Dictionary(Of String, String), idx As Integer, existingSerials As String) As Border
            If item.ContainsKey("IsHeaderRow") AndAlso item("IsHeaderRow").ToString().ToLower() = "true" Then
                Return Nothing
            End If

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
            Dim txtName As New TextBox With {
                .Text = item("ProductName"),
                .IsReadOnly = True,
                .BorderThickness = New Thickness(0),
                .Background = Brushes.Transparent,
                .VerticalContentAlignment = VerticalAlignment.Center,
                .Padding = New Thickness(10, 0, 10, 0),
                .FontFamily = New FontFamily("Lexend")
            }
            Dim nameBorder As New Border With {
                .Child = txtName,
                .Height = 50,
                .Background = CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush),
                .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                .BorderThickness = New Thickness(2),
                .CornerRadius = New CornerRadius(10),
                .Margin = New Thickness(5, 0, 5, 5)
            }
            Grid.SetColumn(nameBorder, 0) : rowGrid.Children.Add(nameBorder)

            Dim maxRemaining = If(item.ContainsKey("MaxAllowed"), item("MaxAllowed"), item("Quantity"))
            Dim txtQty As New TextBox With {
                .Text = item("Quantity"),
                .IsReadOnly = True,
                .BorderThickness = New Thickness(0),
                .Background = Brushes.Transparent,
                .VerticalContentAlignment = VerticalAlignment.Center,
                .HorizontalContentAlignment = HorizontalAlignment.Center,
                .FontFamily = New FontFamily("Lexend"),
                .Tag = maxRemaining,
                .Name = $"txtQuantity_{idx}"
            }
            RegisterControlName(txtQty.Name, txtQty)
            _productTextBoxes.Add(txtQty.Name, txtQty)
            AddHandler txtQty.TextChanged, AddressOf Quantity_TextChanged

            Dim qtyBorder As New Border With {
                .Child = txtQty,
                .Height = 50,
                .Background = Brushes.White,
                .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                .BorderThickness = New Thickness(2),
                .CornerRadius = New CornerRadius(10),
                .Margin = New Thickness(5, 0, 5, 5),
                .Name = $"qtyBorder_{idx}"
            }
            RegisterControlName(qtyBorder.Name, qtyBorder)
            Grid.SetColumn(qtyBorder, 1) : rowGrid.Children.Add(qtyBorder)

            ' 3. Serial Inline Edit Field
            '    - Displays "DONE" when complete and not focused
            '    - Shows actual serial string for direct editing when focused
            Dim txtSerial As New TextBox With {
                .Name = $"txtSerialInput_{idx}",
                .BorderThickness = New Thickness(0),
                .Background = Brushes.Transparent,
                .VerticalContentAlignment = VerticalAlignment.Center,
                .Padding = New Thickness(10, 0, 10, 0),
                .FontFamily = New FontFamily("Lexend"),
                .Tag = idx,
                .TextWrapping = TextWrapping.Wrap,
                .AcceptsReturn = False
            }
            RegisterControlName(txtSerial.Name, txtSerial)

            Dim serialBorder As New Border With {
                .Child = txtSerial,
                .Height = 50,
                .Background = Brushes.White,
                .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                .BorderThickness = New Thickness(2),
                .CornerRadius = New CornerRadius(10),
                .Margin = New Thickness(5, 0, 5, 5),
                .Name = $"serialBorder_{idx}"
            }
            RegisterControlName(serialBorder.Name, serialBorder)
            Grid.SetColumn(serialBorder, 2) : rowGrid.Children.Add(serialBorder)

            ' 4. Header Row (Labels & Edit Button)
            Dim headerGrid As New Grid With {.Margin = New Thickness(5, 5, 5, 2)}
            headerGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            headerGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            headerGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

            Dim lblRemaining As New TextBlock With {
                .Name = $"lblRemaining_{idx}",
                .VerticalAlignment = VerticalAlignment.Center,
                .FontStyle = FontStyles.Italic,
                .Foreground = Brushes.Gray,
                .Margin = New Thickness(5, 0, 0, 0)
            }
            RegisterControlName(lblRemaining.Name, lblRemaining)

            ' Determine initial state
            Dim currentQty As Integer = 0
            Integer.TryParse(item("Quantity"), currentQty)
            Dim remCount = currentQty - _serialCounters(idx)
            lblRemaining.Text = If(remCount <= 0, "COMPLETE", $"Remaining: {remCount}")

            If remCount <= 0 Then
                ' Complete: show DONE in the field, green label
                lblRemaining.Foreground = Brushes.Green
                lblRemaining.FontWeight = FontWeights.Bold
                txtSerial.Text = "DONE"
                txtSerial.Foreground = Brushes.DimGray
                serialBorder.Background = CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush)
            Else
                ' Incomplete: show empty field ready for typing
                txtSerial.Text = ""
                txtSerial.Foreground = Brushes.Black
            End If

            ' --- GotFocus: switch from "DONE" display to actual serial content for editing ---
            AddHandler txtSerial.GotFocus, Sub(s, ev)
                                               If txtSerial.Text = "DONE" Then
                                                   txtSerial.Text = itemDataSource(idx)("SerialNumber")
                                                   txtSerial.Foreground = Brushes.Black
                                               End If
                                               serialBorder.Background = Brushes.White
                                               txtSerial.SelectAll()
                                           End Sub

            ' --- LostFocus: parse content, update counters, refresh display, restore DONE if complete ---
            AddHandler txtSerial.LostFocus, Sub(s, ev)
                                                Dim rawText = txtSerial.Text.Trim()

                                                ' Ignore if user left it as DONE without changing
                                                If rawText = "DONE" Then Return

                                                ' Persist edited serial string
                                                itemDataSource(idx)("SerialNumber") = rawText

                                                ' Re-count serials by splitting on double-space separator
                                                Dim serialCount As Integer = 0
                                                If Not String.IsNullOrEmpty(rawText) Then
                                                    serialCount = rawText.Split(
                                                        New String() {"  "},
                                                        StringSplitOptions.RemoveEmptyEntries).Length
                                                End If
                                                _serialCounters(idx) = serialCount

                                                ' Update the read-only bottom display
                                                Dim targetList As TextBlock = TryCast(Me.FindName($"txtSerialList_{idx}"), TextBlock)
                                                If targetList IsNot Nothing Then
                                                    targetList.Text = rawText
                                                End If

                                                ' Recalculate remaining
                                                Dim totalQty As Integer = 0
                                                Integer.TryParse(itemDataSource(idx)("Quantity"), totalQty)
                                                Dim newRem = totalQty - serialCount

                                                lblRemaining.Text = If(newRem <= 0, "COMPLETE", $"Remaining: {newRem}")

                                                If newRem <= 0 Then
                                                    lblRemaining.Foreground = Brushes.Green
                                                    lblRemaining.FontWeight = FontWeights.Bold
                                                    txtSerial.Text = "DONE"
                                                    txtSerial.Foreground = Brushes.DimGray
                                                    serialBorder.Background = CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush)
                                                Else
                                                    lblRemaining.Foreground = New SolidColorBrush(
                                                        ColorConverter.ConvertFromString("#555555"))
                                                    lblRemaining.FontWeight = FontWeights.Normal
                                                    txtSerial.Foreground = Brushes.Black
                                                    serialBorder.Background = Brushes.White
                                                End If
                                            End Sub

            Dim btnEditSerial As New Button With {
                .Style = CType(FindResource("MaterialDesignIconButton"), Style),
                .Width = 30,
                .Height = 30,
                .Content = New PackIcon With {.Kind = PackIconKind.EditOutline, .Width = 18, .Height = 18},
                .Tag = idx
            }
            AddHandler btnEditSerial.Click, AddressOf OpenEditSerialPopup

            Dim lblTitle As New TextBlock With {
                .Text = "Serial Numbers: ",
                .FontWeight = FontWeights.SemiBold,
                .FontFamily = New FontFamily("Lexend"),
                .VerticalAlignment = VerticalAlignment.Center
            }
            Grid.SetColumn(lblTitle, 0)
            headerGrid.Children.Add(lblTitle)

            lblRemaining.VerticalAlignment = VerticalAlignment.Center
            Grid.SetColumn(lblRemaining, 1)
            headerGrid.Children.Add(lblRemaining)

            Grid.SetColumn(btnEditSerial, 2)
            headerGrid.Children.Add(btnEditSerial)

            Grid.SetRow(headerGrid, 1)
            Grid.SetColumnSpan(headerGrid, 3)
            rowGrid.Children.Add(headerGrid)

            ' 5. Read-Only Serial List Display (TextBlock — never editable)
            Dim txtSerialList As New TextBlock With {
                .Name = $"txtSerialList_{idx}",
                .TextWrapping = TextWrapping.Wrap,
                .Padding = New Thickness(10),
                .FontFamily = New FontFamily("Lexend"),
                .FontSize = 11,
                .Text = existingSerials,
                .Foreground = Brushes.DimGray
            }
            RegisterControlName(txtSerialList.Name, txtSerialList)

            Dim listBorder As New Border With {
                .Child = txtSerialList,
                .Background = CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush),
                .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                .BorderThickness = New Thickness(2),
                .CornerRadius = New CornerRadius(5),
                .MinHeight = 80,
                .Margin = New Thickness(5)
            }
            Grid.SetRow(listBorder, 2) : Grid.SetColumnSpan(listBorder, 3) : rowGrid.Children.Add(listBorder)

            rowBorder.Child = rowGrid
            Return rowBorder
        End Function

        Private Sub AddNewCategoryUI()
            categoryCount += 1

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

            headerGrid.Children.Add(categoryHeader)
            Grid.SetColumn(categoryHeader, 0)
            headerBorder.Child = headerGrid

            Dim categoryItemsPanel As New StackPanel()

            categoryWrapper.Children.Add(headerBorder)
            categoryWrapper.Children.Add(categoryItemsPanel)

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

                                                       ' Update bottom display (read-only TextBlock)
                                                       Dim targetTxt As TextBlock = TryCast(Me.FindName($"txtSerialList_{index}"), TextBlock)
                                                       If targetTxt IsNot Nothing Then
                                                           targetTxt.Text = newList
                                                       End If

                                                       ' Update top inline field
                                                       Dim serialInput As TextBox = TryCast(Me.FindName($"txtSerialInput_{index}"), TextBox)
                                                       Dim lblRemaining As TextBlock = TryCast(Me.FindName($"lblRemaining_{index}"), TextBlock)
                                                       Dim serialBorder As Border = TryCast(Me.FindName($"serialBorder_{index}"), Border)

                                                       Dim totalQty As Integer = CInt(itemDataSource(index)("Quantity"))
                                                       Dim remCount As Integer = totalQty - newLength

                                                       If lblRemaining IsNot Nothing Then
                                                           lblRemaining.Text = If(remCount <= 0, "COMPLETE", $"Remaining: {remCount}")

                                                           If remCount <= 0 Then
                                                               lblRemaining.Foreground = Brushes.Green
                                                               lblRemaining.FontWeight = FontWeights.Bold

                                                               If serialInput IsNot Nothing Then
                                                                   serialInput.Text = "DONE"
                                                                   serialInput.Foreground = Brushes.DimGray
                                                               End If
                                                               If serialBorder IsNot Nothing Then
                                                                   serialBorder.Background = CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush)
                                                               End If
                                                           Else
                                                               lblRemaining.Foreground = New SolidColorBrush(
                                                                   ColorConverter.ConvertFromString("#555555"))
                                                               lblRemaining.FontWeight = FontWeights.Normal

                                                               If serialInput IsNot Nothing Then
                                                                   serialInput.Text = newList
                                                                   serialInput.Foreground = Brushes.Black
                                                               End If
                                                               If serialBorder IsNot Nothing Then
                                                                   serialBorder.Background = Brushes.White
                                                               End If
                                                           End If
                                                       End If
                                                   End If

                                                   Task.Delay(100).ContinueWith(
                                                       Sub() recentlyClosedSerial = False,
                                                       TaskScheduler.FromCurrentSynchronizationContext())
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

            ' Reset serial state when quantity changes
            itemDataSource(index)("Quantity") = newQty.ToString()
            _serialCounters(index) = 0
            itemDataSource(index)("SerialNumber") = ""

            Dim lblRemaining As TextBlock = TryCast(Me.FindName($"lblRemaining_{index}"), TextBlock)
            Dim txtSerialList As TextBlock = TryCast(Me.FindName($"txtSerialList_{index}"), TextBlock)
            Dim txtSerialInput As TextBox = TryCast(Me.FindName($"txtSerialInput_{index}"), TextBox)
            Dim serialBorder As Border = TryCast(Me.FindName($"serialBorder_{index}"), Border)

            If txtSerialList IsNot Nothing Then txtSerialList.Text = ""

            If txtSerialInput IsNot Nothing Then
                txtSerialInput.Text = ""
                txtSerialInput.Foreground = Brushes.Black

                If serialBorder IsNot Nothing Then
                    serialBorder.Background = If(newQty <= 0,
                        CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush),
                        Brushes.White)
                End If
            End If

            If lblRemaining IsNot Nothing Then
                If newQty <= 0 Then
                    lblRemaining.Text = "COMPLETE"
                    lblRemaining.Foreground = Brushes.Green
                    lblRemaining.FontWeight = FontWeights.Bold
                    If txtSerialInput IsNot Nothing Then
                        txtSerialInput.Text = "DONE"
                        txtSerialInput.Foreground = Brushes.DimGray
                    End If
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

            Dim userInput As String = txtInvoiceNumber.Text.Trim().ToUpper()
            Dim searchID As String = userInput.Replace("DR-", "BL-")

            If searchID.Length < 5 Then
                ClearDeliveryForm()
                Return
            End If

            Dim results = BillingController.SearchBillingStatements(searchID, 1, "Private")
            Dim billing = results.FirstOrDefault(Function(b) b.BillingNumber.Equals(searchID, StringComparison.OrdinalIgnoreCase))

            If billing IsNot Nothing Then
                If TransactionState.ActiveRecord Is Nothing Then
                    TransactionState.ActiveRecord = New UniversalTransactionModel()
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

                TransactionState.ActiveRecord.ClientName = txtClientName.Text
                TransactionState.ActiveRecord.DocumentReference = billing.BillingNumber

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

                Dim jsonString = JsonConvert.SerializeObject(remainingList)
                TransactionState.ActiveRecord.RawItemsJson = jsonString
                TransactionState.ActiveRecord.OrderItems = New ObservableCollection(Of OrderItems)(
                    JsonConvert.DeserializeObject(Of List(Of OrderItems))(jsonString))

                LoadItems()
                txtDeliveryNumber.Text = GenerateDeliveryId(billing.BillingNumber)
            Else
                ClearDeliveryForm()
            End If
        End Sub

        Private Sub ClearDeliveryForm()
            txtClientName.Clear()
            txtClientDetails.Clear()
            cmbPaymentTerm.SelectedIndex = -1
            cmbShippingMethod.SelectedIndex = -1
            cmbApprovedBy.SelectedIndex = -1
            txtDeliveryNote.Clear()

            txtDeliveryNumber.Text = "-"

            itemDataSource.Clear()
            MainContainer.Children.Clear()
            _productTextBoxes.Clear()
            _serialCounters.Clear()

            DeliveryDetails.DRClientName = ""
            DeliveryDetails.DRDocumentReference = ""
            DeliveryDetails.DRDeliveryItems = New List(Of Dictionary(Of String, String))()
        End Sub

        Private Sub BtnReset_Click(sender As Object, e As RoutedEventArgs) Handles BtnReset.Click
            TransactionState.ResetRecord()
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
            Dim latestDR As String = DeliveryReceiptController.GetLatestDRFromDatabase(invoiceNumber)

            Dim baseId As String
            Dim hasExistingPartial As Boolean = False

            If Not String.IsNullOrEmpty(latestDR) Then
                baseId = latestDR
                If baseId.Contains("(P") Then hasExistingPartial = True
            Else
                baseId = invoiceNumber.Trim().Replace("BL", "DR").Replace(" ", "")
            End If

            If hasExistingPartial Then
                rbPartialDelivery.IsChecked = True
                rbFullDelivery.IsEnabled = False
            Else
                rbPartialDelivery.IsEnabled = True
                rbFullDelivery.IsEnabled = True
            End If

            If rbPartialDelivery?.IsChecked = True Then
                Dim pattern As String = "\(P(\d+)\)$"
                Dim match = System.Text.RegularExpressions.Regex.Match(baseId, pattern)

                If match.Success Then
                    Dim currentNum As Integer = Integer.Parse(match.Groups(1).Value)
                    Return System.Text.RegularExpressions.Regex.Replace(baseId, pattern, $"(P{currentNum + 1})")
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
