Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports Microsoft.Win32
Imports SkiaSharp.Views.WPF

Namespace DPC.Views.Stocks.PurchaseOrder.Delivery
    Public Class PreviewEditableDeliveryReceipt
        Public Sub New()
            InitializeComponent()

            IntializeFields()
        End Sub

        Public Sub IntializeFields()
            txtDeliveryNumber.Text = DeliveryDetails.DRNumber
            txtReferenceInvoice.Text = DeliveryDetails.DRReferenceInvoice
            txtDeliveryDate.Text = DeliveryDetails.DRDate
            'DRClientDetails.Text = DeliveryDetails.DRClientDetails
            txtSalesRep.Text = CacheOnLoggedInName
            txtDeliveryClientName.Text = DeliveryDetails.DRClientName
            txtNotes.Text = DeliveryDetails.DRDeliveryNotes
            txtShippingMethod.Text = DeliveryDetails.DRShippingMethod

            ' Populate the DataGrid with the delivery items
            Dim deliveryItems As New ObservableCollection(Of Dictionary(Of String, String))(DeliveryDetails.DRDeliveryItems)
            DeliveryDataGrid.ItemsSource = deliveryItems

            Dim clientDetails As String
            clientDetails = DeliveryDetails.DRClientDetails

            If Not String.IsNullOrEmpty(clientDetails) Then
                txtDeliveryRep.Text = Regex.Match(clientDetails, "Representative Name: (.*)").Groups(1).Value.Trim()
                txtDeliveryContact.Text = Regex.Match(clientDetails, "Contact: (.*)").Groups(1).Value.Trim()
                txtDeliveryAddress.Text = Regex.Match(clientDetails, "Delivery Address: (.*)").Groups(1).Value.Trim()
            End If
        End Sub
        Private Sub BackToUI_Click(sender As Object, e As MouseButtonEventArgs)
            ViewLoader.DynamicView.NavigateToView("newdelivery", Me)
        End Sub

        Private Sub PreviousPage_Click(sender As Object, e As RoutedEventArgs)
            'If currentPageIndex > 0 Then LoadPage(currentPageIndex - 1)
        End Sub

        Private Sub NextPage_Click(sender As Object, e As RoutedEventArgs)
            'If currentPageIndex < totalPages - 1 Then LoadPage(currentPageIndex + 1)
        End Sub
    End Class
End Namespace