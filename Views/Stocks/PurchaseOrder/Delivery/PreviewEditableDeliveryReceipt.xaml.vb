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
        Private itemDataSource As New System.Collections.ObjectModel.ObservableCollection(Of Dictionary(Of String, String))

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

            LoadTestPlaceholderData()
            ' LoadPage()
        End Sub

        Private Sub LoadPage()
            If DeliveryDetails.DRDeliveryItems Is Nothing Then Return

            itemDataSource.Clear()

            For Each item As Dictionary(Of String, String) In DeliveryDetails.DRDeliveryItems
                Dim displayItem As New Dictionary(Of String, String)(item)

                If displayItem.ContainsKey("Description") Then
                    Dim currentDesc As String = displayItem("Description").Trim()

                    If currentDesc = "Enter product description (Optional)" OrElse String.IsNullOrWhiteSpace(currentDesc) Then
                        displayItem("Description") = "No additional details provided."
                    End If
                End If

                itemDataSource.Add(displayItem)
            Next

            DeliveryDataGrid.ItemsSource = itemDataSource
        End Sub

        ' FOR TESTING ONLY
        Public Sub LoadTestPlaceholderData()
            DeliveryDetails.DRDeliveryItems.Clear()

            DeliveryDetails.DRDeliveryItems = New List(Of Dictionary(Of String, String))()

            Dim item1 As New Dictionary(Of String, String) From {
                {"Quantity", "1"},
                {"ProductName", "HIKVISION - 2MP WEATHERPROOF IR IP CAMERA"},
                {"Description", "High-definition outdoor security camera with night vision."},
                {"Amount", "1881.60"},
                {"SerialNumber", "SN-HK-992831"}
            }

            Dim item2 As New Dictionary(Of String, String) From {
                {"Quantity", "5"},
                {"ProductName", "AEROCOOL UNITED POWER 500W (80+ WHITE)"},
                {"Description", "Enter product description (Optional)"},
                {"Amount", "2035.04"},
                {"SerialNumber", "N/A"}
            }

            Dim item3 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item4 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item5 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item6 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item7 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item8 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item9 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item10 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item11 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item12 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item13 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item14 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item15 As New Dictionary(Of String, String) From {
                {"Quantity", "1"},
                {"ProductName", "HIKVISION - 2MP WEATHERPROOF IR IP CAMERA"},
                {"Description", "High-definition outdoor security camera with night vision."},
                {"Amount", "1881.60"},
                {"SerialNumber", "SN-HK-992831"}
            }

            DeliveryDetails.DRDeliveryItems.Add(item1)
            DeliveryDetails.DRDeliveryItems.Add(item2)
            DeliveryDetails.DRDeliveryItems.Add(item3)
            DeliveryDetails.DRDeliveryItems.Add(item4)
            DeliveryDetails.DRDeliveryItems.Add(item5)
            DeliveryDetails.DRDeliveryItems.Add(item6)
            DeliveryDetails.DRDeliveryItems.Add(item7)
            DeliveryDetails.DRDeliveryItems.Add(item8)
            DeliveryDetails.DRDeliveryItems.Add(item9)
            DeliveryDetails.DRDeliveryItems.Add(item10)
            DeliveryDetails.DRDeliveryItems.Add(item11)
            DeliveryDetails.DRDeliveryItems.Add(item12)
            DeliveryDetails.DRDeliveryItems.Add(item13)
            DeliveryDetails.DRDeliveryItems.Add(item14)
            DeliveryDetails.DRDeliveryItems.Add(item15)

            LoadPage()
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

        Private Sub PrintPreview(sender As Object, e As RoutedEventArgs)
            DeliveryDetails.DRApprovedBy = cmbApproved.Text
            DeliveryDetails.DRPaymentTerm = cmbTerms.Text
            ViewLoader.DynamicView.NavigateToView("previewprintdeliveryreceipt", Me)
        End Sub
    End Class
End Namespace