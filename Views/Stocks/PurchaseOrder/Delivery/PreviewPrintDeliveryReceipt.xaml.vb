Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Views.Sales.Quotes
Imports Microsoft.Win32
Imports MongoDB.Bson
Imports MongoDB.Driver
Imports MongoDB.Driver.GridFS
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf
Imports SkiaSharp.Views.WPF

Namespace DPC.Views.Stocks.PurchaseOrder.Delivery
    Public Class PreviewPrintDeliveryReceipt
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
            txtApprovedBy.Text = DeliveryDetails.DRApprovedBy
            txtPaymentTerm.Text = DeliveryDetails.DRPaymentTerm

            ' Populate the DataGrid with the delivery items
            Dim deliveryItems As New ObservableCollection(Of Dictionary(Of String, String))(DeliveryDetails.DRDeliveryItems)
            DeliveryDataGrid.ItemsSource = deliveryItems

            LoadPage()

            Dim clientDetails As String
            clientDetails = DeliveryDetails.DRClientDetails

            If Not String.IsNullOrEmpty(clientDetails) Then
                txtDeliveryRep.Text = Regex.Match(clientDetails, "Representative Name: (.*)").Groups(1).Value.Trim()
                txtDeliveryContact.Text = Regex.Match(clientDetails, "Contact: (.*)").Groups(1).Value.Trim()
                txtDeliveryAddress.Text = Regex.Match(clientDetails, "Delivery Address: (.*)").Groups(1).Value.Trim()
            End If
        End Sub

        Private Sub LoadPage()
            If DeliveryDetails.DRDeliveryItems Is Nothing Then Return

            itemDataSource.Clear()

            Dim i As Integer = 1

            For Each item As Dictionary(Of String, String) In DeliveryDetails.DRDeliveryItems
                Dim displayItem As New Dictionary(Of String, String)(item)

                displayItem("Number") = i.ToString()
                i += 1

                If displayItem.ContainsKey("SerialNumber") Then
                    Dim rawSerials As String = displayItem("SerialNumber").Trim()

                    If Not String.IsNullOrEmpty(rawSerials) Then
                        Dim cleanSerials = Regex.Replace(rawSerials, "\(\d+\)\s*", "")

                        cleanSerials = cleanSerials.Replace("-", ChrW(&H2011))

                        displayItem("SerialNumber") = cleanSerials.Replace("  ", ", ").Trim()
                    Else
                        displayItem("SerialNumber") = "N/A"
                    End If
                End If

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
                {"SerialNumber", "SN-HK-992831, SN-HK-992831, SN-HK-992831, SN-HK-992831, SN-HK-992831, SN-HK-992831, SN-HK-992831, SN-HK-992831, SN-HK-992831, SN-HK-992831, SN-HK-992831, SN-HK-992831"}
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

        Private Sub CancelButton(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("newdelivery", Me)
        End Sub
        Private Sub PreviousPage_Click(sender As Object, e As RoutedEventArgs)
            'If currentPageIndex > 0 Then LoadPage(currentPageIndex - 1)
        End Sub

        Private Sub NextPage_Click(sender As Object, e As RoutedEventArgs)
            'If currentPageIndex < totalPages - 1 Then LoadPage(currentPageIndex + 1)
        End Sub

        Private Sub SavePrint(sender As Object, e As RoutedEventArgs)
            'Try
            '    Dim res As MessageBoxResult = MessageBox.Show("Do you want to save this as a PDF?", "Output", MessageBoxButton.YesNoCancel)
            '    Dim docName As String = CEQuoteNumberCache
            '    Dim path As String = SaveAsPDF(docName)

            '    If res = MessageBoxResult.Yes Then
            '        If Not SavePdfPathToMongoDB(path, CEQuoteNumberCache, CacheOnLoggedInName) Then Exit Sub
            '        SaveToDb()
            '    ElseIf res = MessageBoxResult.No Then
            '        PrintPhysically(docName)
            '        If Not SavePdfPathToMongoDB(path, CEQuoteNumberCache, CacheOnLoggedInName) Then Exit Sub
            '        SaveToDb()
            '    End If
            'Catch ex As Exception
            '    MessageBox.Show("Print Error: " & ex.Message)
            'End Try
        End Sub

        Private Sub SaveDb_Click(sender As Object, e As RoutedEventArgs)
            'Try
            '    Dim path As String = SaveAsPDF(CEQuoteNumberCache)
            '    If Not String.IsNullOrEmpty(path) Then
            '        If Not SavePdfPathToMongoDB(path, CEQuoteNumberCache, CacheOnLoggedInName) Then Exit Sub
            '        SaveToDb()
            '    End If
            'Catch ex As Exception
            '    MessageBox.Show("Save Error: " & ex.Message)
            'End Try
        End Sub

        'Private Function SaveAsPDF(docName As String) As String
        '    Dim dlg As New Microsoft.Win32.SaveFileDialog() With {.FileName = docName & ".pdf", .Filter = "PDF Files|*.pdf"}
        '    If dlg.ShowDialog() = True Then
        '        Try
        '            Dim pdf As New PdfDocument()
        '            For i As Integer = 0 To totalPages - 1
        '                LoadPrintPage(i)
        '                Application.Current.Dispatcher.Invoke(Sub() PrintPreview.UpdateLayout(), System.Windows.Threading.DispatcherPriority.Render)
        '                System.Threading.Thread.Sleep(300)

        '                Dim page As PdfPage = pdf.AddPage()
        '                Dim layoutWidth = PrintPreview.ActualWidth
        '                Dim layoutHeight = PrintPreview.ActualHeight

        '                page.Width = XUnit.FromInch(layoutWidth / 96)
        '                page.Height = XUnit.FromInch(layoutHeight / 96)
        '                RenderToPdfPage(PrintPreview, page)
        '            Next
        '            pdf.Save(dlg.FileName)
        '            LoadPrintPage(0) ' Reset
        '            Return dlg.FileName
        '        Catch ex As Exception
        '            MessageBox.Show("PDF Error: " & ex.Message)
        '            Return Nothing
        '        End Try
        '    End If
        '    Return Nothing
        'End Function

        'Private Sub RenderToPdfPage(elem As FrameworkElement, page As PdfPage)
        '    Dim dpi As Integer = 300
        '    Dim w = elem.ActualWidth
        '    Dim h = elem.ActualHeight
        '    Dim pxW = CInt(w * dpi / 96)
        '    Dim pxH = CInt(h * dpi / 96)

        '    Dim rtb As New RenderTargetBitmap(pxW, pxH, dpi, dpi, PixelFormats.Pbgra32)

        '    elem.Measure(New Size(w, h))
        '    elem.Arrange(New Rect(0, 0, w, h))
        '    elem.UpdateLayout()
        '    rtb.Render(elem)

        '    Dim enc As New PngBitmapEncoder()
        '    enc.Frames.Add(BitmapFrame.Create(rtb))

        '    Using ms As New MemoryStream()
        '        enc.Save(ms)
        '        ms.Position = 0
        '        Using gfx As XGraphics = XGraphics.FromPdfPage(page)
        '            Dim img = XImage.FromStream(ms)
        '            gfx.DrawImage(img, 0, 0, page.Width.Point, page.Height.Point)
        '        End Using
        '    End Using
        'End Sub

        'Private Sub PrintPhysically(docName As String)
        '    Dim dlg As New PrintDialog()
        '    If dlg.ShowDialog() = True Then
        '        For i As Integer = 0 To totalPages - 1
        '            LoadPrintPage(i)
        '            Application.Current.Dispatcher.Invoke(Sub() PrintPreview.UpdateLayout(), System.Windows.Threading.DispatcherPriority.Render)

        '            ' Create Fixed Document Logic
        '            Dim width As Double = 8.3 * 96
        '            Dim height As Double = 11.69 * 96

        '            ' Create a temporary grid for printing to avoid detaching the visual parent
        '            Dim container As New Grid With {.Width = width, .Height = height}

        '            ' Transform visual
        '            Dim scale = Math.Min(width / PrintPreview.ActualWidth, height / PrintPreview.ActualHeight)
        '            Dim brush As New VisualBrush(PrintPreview)
        '            Dim rect As New Rectangle With {
        '                .Width = PrintPreview.ActualWidth,
        '                .Height = PrintPreview.ActualHeight,
        '                .Fill = brush,
        '                .LayoutTransform = New ScaleTransform(scale, scale)
        '            }
        '            container.Children.Add(rect)

        '            container.Measure(New Size(width, height))
        '            container.Arrange(New Rect(0, 0, width, height))

        '            dlg.PrintVisual(container, $"{docName} - Page {i + 1}")
        '        Next
        '        LoadPrintPage(0)
        '    End If
        'End Sub

        'Private Sub SaveToDb()
        '    Dim json As String = Newtonsoft.Json.JsonConvert.SerializeObject(CEQuoteItemsCache)
        '    If QuotesController.InsertQuote(CEQuoteNumberCache, CEReferenceNumber, CEQuoteDateCache,
        '                                    CEQuoteValidityDateCache, CETaxProperty, CEDiscountProperty,
        '                                    CEClientIDCache, CEClientName, CEWarehouseIDCache, CEWarehouseNameCache,
        '                                    json, CEQuoteNumberCache, CETotalTaxValueCache, CETotalDiscountValueCache,
        '                                    CETotalAmountCache, CacheOnLoggedInName, CEApproved, CEpaymentTerms) Then

        '        Dim f As New NewQuoteGovernment()
        '        f.ClearAllFields()
        '        CostEstimateDetails.ClearAllCECache()

        '        If Application.Current.Properties.Contains("QuoteCache") Then
        '            Application.Current.Properties.Remove("QuoteCache")
        '        End If

        '        ViewLoader.DynamicView.NavigateToView("salesquotegovernment", Me)
        '    Else
        '        MessageBox.Show("Failed to submit quote.")
        '    End If
        'End Sub

        'Private Shared Function SavePdfPathToMongoDB(path As String, qNum As String, user As String) As Boolean
        '    Try
        '        Dim fs As GridFSBucket = SplashScreen.GetGridFSConnection()
        '        Dim filter = Builders(Of GridFSFileInfo).Filter.Eq(Of String)("metadata.quoteNumber", qNum)
        '        Dim existingFiles = fs.Find(filter).ToList()

        '        For Each file In existingFiles
        '            fs.Delete(file.Id)
        '        Next

        '        Using s As New FileStream(path, FileMode.Open, FileAccess.Read)
        '            Dim opts As New GridFSUploadOptions() With {
        '        .Metadata = New BsonDocument From {
        '            {"uploadedBy", user},
        '            {"uploadedAt", BsonDateTime.Create(DateTime.UtcNow)},
        '            {"source", "cost-estimate/quote"},
        '            {"quoteNumber", qNum},
        '            {"pdfFilePath", path}
        '        }
        '    }
        '            fs.UploadFromStream(System.IO.Path.GetFileName(path), s, opts)
        '        End Using
        '        Return True
        '    Catch ex As Exception
        '        MessageBox.Show("Database Error during file replace: " & ex.Message)
        '        Return False
        '    End Try
        'End Function
    End Class
End Namespace