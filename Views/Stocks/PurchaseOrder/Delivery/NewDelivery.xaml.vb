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

Namespace DPC.Views.Stocks.PurchaseOrder.Delivery

    Public Class NewDelivery
        Private _selectedClient As Client
        Private _client As New ObservableCollection(Of Client)
        Private deliveryDate As New CalendarController.SingleCalendar()
        Private itemDataSource As New System.Collections.ObjectModel.ObservableCollection(Of Dictionary(Of String, String))
        ' Add this line with your other private variables
        Private _productTextBoxes As New Dictionary(Of String, TextBox)

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
                itemDataSource.Add(displayItem)

                ' Outer row container
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

                rowGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(0, GridUnitType.Auto)})
                rowGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(0, GridUnitType.Auto)})

                Dim nameBorder As New Border With {
                    .Style = CType(FindResource("RoundedBorderStyle"), Style),
                    .Background = CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush),
                    .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                    .BorderThickness = New Thickness(2),
                    .CornerRadius = New CornerRadius(10),
                    .Height = 50,
                    .Margin = New Thickness(5, 0, 5, 0)
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

                ' 2. Quantity Container (Column 1)
                Dim qtyBorder As New Border With {
                    .Style = CType(FindResource("RoundedBorderStyle"), Style),
                    .Background = CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush),
                    .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                    .BorderThickness = New Thickness(2),
                    .CornerRadius = New CornerRadius(10),
                    .Height = 50,
                    .Margin = New Thickness(5, 0, 5, 0)
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

                ' 3. Serial Number Container (Column 2)
                Dim serialBorder As New Border With {
                    .Style = CType(FindResource("RoundedBorderStyle"), Style),
                    .Background = Brushes.White,
                    .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                    .BorderThickness = New Thickness(2),
                    .CornerRadius = New CornerRadius(10),
                    .Height = 50,
                    .Margin = New Thickness(5, 0, 5, 0)
                }
                Dim txtSerial As New TextBox With {
                    .Name = $"txtSerialInput_{i}",
                    .IsReadOnly = False,
                    .BorderThickness = New Thickness(0),
                    .Background = Brushes.Transparent,
                    .VerticalContentAlignment = VerticalAlignment.Center,
                    .Padding = New Thickness(10, 0, 10, 0),
                    .FontFamily = New FontFamily("Lexend"),
                    .Tag = i - 1
                }

                AddHandler txtSerial.TextChanged, Sub(sender, e)
                                                      Dim tb = DirectCast(sender, TextBox)
                                                      itemDataSource(CInt(tb.Tag))("SerialNumber") = tb.Text
                                                  End Sub

                _productTextBoxes(txtSerial.Name) = txtSerial
                serialBorder.Child = txtSerial
                Grid.SetRow(serialBorder, 0)
                Grid.SetColumn(serialBorder, 2)
                rowGrid.Children.Add(serialBorder)

                Dim serialListBorder As New Border With {
                    .Style = CType(FindResource("RoundedBorderStyle"), Style),
                    .Background = CType(New BrushConverter().ConvertFrom("#F5F5F5"), Brush),
                    .BorderBrush = CType(New BrushConverter().ConvertFrom("#1D3242"), Brush),
                    .BorderThickness = New Thickness(2),
                    .CornerRadius = New CornerRadius(5),
                    .MinHeight = 80,
                    .Margin = New Thickness(5, 5, 5, 5)
                }
                Dim txtSerialList As New TextBox With {
                    .IsReadOnly = True,
                    .TextWrapping = TextWrapping.Wrap,
                    .BorderThickness = New Thickness(0),
                    .Background = Brushes.Transparent,
                    .Padding = New Thickness(10, 5, 10, 5),
                    .FontFamily = New FontFamily("Lexend"),
                    .FontSize = 11
                }

                AddHandler txtSerial.KeyDown, Sub(sender, e)
                                                  If e.Key = Key.Enter Then
                                                      Dim input = DirectCast(sender, TextBox)
                                                      Dim currentVal = input.Text.Trim()

                                                      Dim index As Integer = CInt(input.Tag)

                                                      If Not String.IsNullOrEmpty(currentVal) Then
                                                          If String.IsNullOrEmpty(txtSerialList.Text) Then
                                                              txtSerialList.Text = currentVal
                                                          Else
                                                              txtSerialList.Text &= $", {currentVal}"
                                                          End If

                                                          itemDataSource(index)("SerialNumber") = txtSerialList.Text

                                                          input.Clear()
                                                          input.Focus()
                                                      End If
                                                      e.Handled = True
                                                  End If
                                              End Sub

                serialListBorder.Child = txtSerialList
                Grid.SetRow(serialListBorder, 1)
                Grid.SetColumnSpan(serialListBorder, 3)
                rowGrid.Children.Add(serialListBorder)

                rowBorder.Child = rowGrid
                MainContainer.Children.Add(rowBorder)
                i += 1
            Next
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