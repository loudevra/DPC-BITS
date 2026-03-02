Imports System.Collections.ObjectModel
Imports System.IO
Imports DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Models
Imports MaterialDesignThemes.Wpf
Imports Microsoft.Win32
Imports NuGet.Protocol.Plugins
Imports System.Windows.Controls.Primitives

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

        Public Sub New()
            InitializeComponent()
            InitializeFields()
            _isInitialized = True
        End Sub

        Public Sub InitializeFields()

            txtClientName.Text = If(Not String.IsNullOrEmpty(WalkinBillingStatementDetails.BLClientName),
                            WalkinBillingStatementDetails.BLClientName,
                            DeliveryDetails.DRClientName)

            txtInvoiceNumber.Text = If(Not String.IsNullOrEmpty(WalkinBillingStatementDetails.BLNumberCache),
                               WalkinBillingStatementDetails.BLNumberCache,
                               DeliveryDetails.DRReferenceInvoice)

            If WalkinBillingStatementDetails.BLItemsCache IsNot Nothing AndAlso WalkinBillingStatementDetails.BLItemsCache.Count > 0 Then
                DeliveryDetails.DRDeliveryItems = New List(Of Dictionary(Of String, String))(WalkinBillingStatementDetails.BLItemsCache)
            ElseIf DeliveryDetails.DRDeliveryItems IsNot Nothing AndAlso DeliveryDetails.DRDeliveryItems.Count > 0 Then
                ' Already has items (perhaps from an Edit session), keep them
            Else
                FetchItemsFromInvoice(txtInvoiceNumber.Text)
            End If

            txtDeliveryNumber.Text = GenerateDeliveryId(txtInvoiceNumber.Text)

            dtDate.SelectedDate = DateTime.Today
            txtSelectedDate.Text = dtDate.SelectedDate.Value.ToString("MMM dd, yyyy")

            If Not String.IsNullOrEmpty(DeliveryDetails.DRShippingMethod) Then
                cmbShippingMethod.Text = DeliveryDetails.DRShippingMethod
            End If

            If Not String.IsNullOrEmpty(DeliveryDetails.DRDeliveryNotes) Then
                txtDeliveryNote.Text = DeliveryDetails.DRDeliveryNotes
            End If

            If Not String.IsNullOrEmpty(DeliveryDetails.DRApprovedBy) Then
                cmbApprovedBy.Text = DeliveryDetails.DRApprovedBy
            End If

            If Not String.IsNullOrEmpty(DeliveryDetails.DRPaymentTerm) Then
                cmbPaymentTerm.Text = DeliveryDetails.DRPaymentTerm
            End If

            GetClientInfo()
            LoadItems()
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

            If Not AreAllSerialsCompleted() Then
                MessageBox.Show("Some items have missing serial numbers. Please ensure all items are 'COMPLETE' before generating the receipt.",
                        "Incomplete Serials",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning)
                Return
            End If

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
        End Sub

        Private Sub LoadItems()
            If DeliveryDetails.DRDeliveryItems Is Nothing Then Return

            itemDataSource.Clear()
            MainContainer.Children.Clear()
            _productTextBoxes.Clear()

            Dim i As Integer = 1

            For Each item As Dictionary(Of String, String) In DeliveryDetails.DRDeliveryItems
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

                Dim txtQty As New TextBox With {
                    .Text = item("Quantity"),
                    .IsReadOnly = True,
                    .BorderThickness = New Thickness(0),
                    .Background = Brushes.Transparent,
                    .VerticalContentAlignment = VerticalAlignment.Center,
                    .HorizontalContentAlignment = HorizontalAlignment.Center,
                    .FontFamily = New FontFamily("Lexend"),
                    .Tag = item("Quantity")
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
            If rbPartialDelivery Is Nothing OrElse rbFullDelivery Is Nothing Then Return
            Dim isPartial As Boolean = rbPartialDelivery.IsChecked = True

            txtDeliveryNumber.Text = GenerateDeliveryId(txtInvoiceNumber.Text)

            For Each kvp In _productTextBoxes
                If kvp.Key.StartsWith("txtQuantity_") Then
                    Dim qtyBox = kvp.Value
                    Dim index = kvp.Key.Split("_"c).Last()
                    Dim parentBorder As Border = TryCast(Me.FindName($"qtyBorder_{index}"), Border)

                    If isPartial Then
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
            Dim indexString = tb.Name.Split("_"c).Last()
            Dim index As Integer = CInt(indexString)

            Dim newQty As Integer = 0
            If Not Integer.TryParse(tb.Text, newQty) Then Exit Sub
            Dim originalQty As Integer = 0
            Integer.TryParse(tb.Tag?.ToString(), originalQty)

            If newQty > originalQty Then
                MessageBox.Show($"Quantity cannot exceed the original invoiced amount ({originalQty}).",
                        "Invalid Quantity", MessageBoxButton.OK, MessageBoxImage.Warning)

                tb.Text = originalQty.ToString()
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
                txtSerialInput.IsReadOnly = False
                txtSerialInput.Clear()
                If serialBorder IsNot Nothing Then serialBorder.Background = Brushes.White
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