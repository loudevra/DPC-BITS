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

        Public Sub New()
            InitializeComponent()
            InitializeFields()
        End Sub

        Public Sub InitializeFields()

            If WalkinBillingStatementDetails.BLItemsCache IsNot Nothing Then
                DeliveryDetails.DRDeliveryItems = New List(Of Dictionary(Of String, String))(WalkinBillingStatementDetails.BLItemsCache)
            End If

            txtClientName.Text = WalkinBillingStatementDetails.BLClientName
            txtInvoiceNumber.Text = WalkinBillingStatementDetails.BLNumberCache
            txtDeliveryNumber.Text = GenerateDeliveryId(txtInvoiceNumber.Text)

            dtDate.SelectedDate = DateTime.Today
            txtSelectedDate.Text = dtDate.SelectedDate.Value.ToString("MMM dd, yyyy")

            If Not String.IsNullOrEmpty(DeliveryDetails.DRShippingMethod) Then
                cmbShippingMethod.Text = DeliveryDetails.DRShippingMethod
            End If

            If Not String.IsNullOrEmpty(DeliveryDetails.DRDeliveryNotes) Then
                txtDeliveryNote.Text = DeliveryDetails.DRDeliveryNotes
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
            DeliveryDetails.DRReferenceInvoice = txtInvoiceNumber.Text
            DeliveryDetails.DRNumber = txtDeliveryNumber.Text
            DeliveryDetails.DRDate = DateTime.Today.ToString("MMM dd, yyyy")

            DeliveryDetails.DRClientName = txtClientName.Text
            DeliveryDetails.DRClientDetails = txtClientDetails.Text
            DeliveryDetails.DRDeliveryNotes = txtDeliveryNote.Text

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

            DeliveryDetails.DRDeliveryItems = WalkinBillingStatementDetails.BLItemsCache

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
                Dim txtQty As New TextBox With {
                    .Text = item("Quantity"),
                    .IsReadOnly = True,
                    .BorderThickness = New Thickness(0),
                    .Background = Brushes.Transparent,
                    .VerticalContentAlignment = VerticalAlignment.Center,
                    .HorizontalContentAlignment = HorizontalAlignment.Center,
                    .Padding = New Thickness(10, 0, 10, 0),
                    .FontFamily = New FontFamily("Lexend")
                }
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
                    .Tag = i - 1 ' Crucial for identifying the row
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
                                                      Dim total As Integer = 0
                                                      Integer.TryParse(item("Quantity"), total)

                                                      If Not String.IsNullOrEmpty(val) AndAlso _serialCounters(idx) < total Then
                                                          _serialCounters(idx) += 1
                                                          Dim entry = $"({_serialCounters(idx)}) {val}"
                                                          txtSerialList.Text &= If(String.IsNullOrEmpty(txtSerialList.Text), entry, $"  {entry}")

                                                          Dim remCount = total - _serialCounters(idx)
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

        Private Function GenerateDeliveryId(invoiceNumber As String) As String
            Return invoiceNumber.Trim().Replace("BL", "DR").Replace(" ", "")
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
#End Region
    End Class
End Namespace