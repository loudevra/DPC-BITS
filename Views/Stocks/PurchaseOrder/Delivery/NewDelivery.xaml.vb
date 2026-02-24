Imports System.Collections.ObjectModel
Imports System.IO
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

        Public Sub New()
            InitializeComponent()
            InitializeFields()
        End Sub

        Public Sub InitializeFields()

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