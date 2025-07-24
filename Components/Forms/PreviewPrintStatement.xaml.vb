Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Windows.Markup
Imports DocumentFormat.OpenXml.Bibliography
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports MongoDB.Bson
Imports MongoDB.Driver.GridFS
Imports Newtonsoft.Json
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf

Namespace DPC.Components.Forms
    Public Class PreviewPrintStatement

        Private tempImagePath As String
        Private base64Image As String
        Private itemDataSource As New ObservableCollection(Of OrderItems)
        Private itemOrder As New List(Of Dictionary(Of String, String))
        Private Address As String
        Private isCustom As Boolean

        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.


            InvoiceNumber.Text = StatementDetails.InvoiceNumberCache
            InvoiceDate.Text = StatementDetails.InvoiceDateCache
            DueDate.Text = StatementDetails.DueDateCache
            Tax.Text = "₱ " & StatementDetails.TaxCache.ToString("N2")
            TotalCost.Text = "₱ " & (StatementDetails.TotalCostCache + DeliveryCost).ToString("N2")
            Delivery.Text = "₱ " & StatementDetails.DeliveryCost.ToString("N2")
            itemOrder = StatementDetails.OrderItemsCache
            base64Image = StatementDetails.ImageCache
            tempImagePath = StatementDetails.PathCache
            SupplierNameBox.Text = StatementDetails.SupplierName
            AddressLineOne.Text = StatementDetails.City & ", " & StatementDetails.Region
            AddressLineTwo.Text = StatementDetails.Country
            PhoneBox.Text = "Tel No: " & StatementDetails.Phone
            EmailBox.Text = StatementDetails.Email
            noteBox.Text = StatementDetails.noteTxt
            remarksBox.Text = StatementDetails.remarksTxt
            Term1.Text = StatementDetails.Term1
            Term2.Text = StatementDetails.Term2
            Term3.Text = StatementDetails.Term3
            Term4.Text = StatementDetails.Term4
            Term5.Text = StatementDetails.Term5
            Term6.Text = StatementDetails.Term6
            Term7.Text = StatementDetails.Term7
            Term8.Text = StatementDetails.Term8
            Term9.Text = StatementDetails.Term9
            Term10.Text = StatementDetails.Term10
            Term11.Text = StatementDetails.Term11
            Term12.Text = StatementDetails.Term12
            SalesRep.Text = CacheOnLoggedInName
            PaymentTerms.Text = StatementDetails.paymentTerms
            isCustom = StatementDetails.isCustomTerm
            Approved.Text = StatementDetails.Approved



            If StatementDetails.signature = False Then
                BrowseFile.Child = Nothing
            Else
                DisplayUploadedImage()
            End If

            For Each item In StatementDetails.OrderItemsCache
                Dim rate As Decimal = item("Rate")
                Dim rateFormatted As String = rate.ToString("N2")

                Dim linePrice As Decimal = item("Price")
                Dim linePriceFormatted As String = linePrice.ToString("N2")

                itemDataSource.Add(New OrderItems With {
                        .Quantity = item("Quantity"),
                        .Description = item("ItemName"),
                        .UnitPrice = rateFormatted,
                        .LinePrice = linePriceFormatted
                    })
            Next


            dataGrid.ItemsSource = itemDataSource

            AddHandler SaveDb.Click, AddressOf SaveToDB
        End Sub

        Private Sub CancelButton(sender As Object, e As RoutedEventArgs)
            Dim _city, _region, _phone As String

            _city = StatementDetails.City
            _region = StatementDetails.Region
            _phone = StatementDetails.Phone

            Dim _deliveryCost, _tax, _totalCost As Decimal

            _deliveryCost = StatementDetails.DeliveryCost
            _tax = StatementDetails.TaxCache
            _totalCost = StatementDetails.TotalCostCache

            StatementDetails.paymentTerms = PaymentTerms.Text

            StatementDetails.signature = If(String.IsNullOrWhiteSpace(base64Image), False, True)
            StatementDetails.InvoiceNumberCache = InvoiceNumber.Text
            StatementDetails.InvoiceDateCache = InvoiceDate.Text
            StatementDetails.DueDateCache = DueDate.Text
            StatementDetails.TaxCache = _tax
            StatementDetails.TotalCostCache = _totalCost
            StatementDetails.OrderItemsCache = itemOrder
            StatementDetails.ImageCache = base64Image
            StatementDetails.PathCache = tempImagePath
            StatementDetails.SupplierName = SupplierNameBox.Text
            StatementDetails.City = _city
            StatementDetails.Region = _region
            StatementDetails.Country = AddressLineTwo.Text
            StatementDetails.Phone = _phone
            StatementDetails.Email = EmailBox.Text
            StatementDetails.noteTxt = noteBox.Text
            StatementDetails.remarksTxt = remarksBox.Text
            StatementDetails.Term1 = Term1.Text
            StatementDetails.Term2 = Term2.Text
            StatementDetails.Term3 = Term3.Text
            StatementDetails.Term4 = Term4.Text
            StatementDetails.Term5 = Term5.Text
            StatementDetails.Term6 = Term6.Text
            StatementDetails.Term7 = Term7.Text
            StatementDetails.Term8 = Term8.Text
            StatementDetails.Term9 = Term9.Text
            StatementDetails.Term10 = Term10.Text
            StatementDetails.Term11 = Term11.Text
            StatementDetails.Term12 = Term12.Text
            StatementDetails.DeliveryCost = _deliveryCost
            StatementDetails.isCustomTerm = isCustom
            StatementDetails.Approved = Approved.Text

            ViewLoader.DynamicView.NavigateToView("purchaseorderstatement", Me)
        End Sub

        Private Sub SavePrint(sender As Object, e As RoutedEventArgs)
            ' Ask user: PDF or Print
            Dim result As MessageBoxResult = MessageBox.Show("Do you want to save this as a PDF?", "Choose Output", MessageBoxButton.YesNoCancel, MessageBoxImage.Question)

            Dim docName As String = InvoiceNumberCache ' You can replace this with any string

            If result = MessageBoxResult.Yes Then
                ' ➤ Save as PDF
                SaveAsPDF(docName)
                SaveToDB()
            ElseIf result = MessageBoxResult.No Then
                ' ➤ Print physically
                PrintPhysically(docName)
                SaveToDB()
            Else
                ' ➤ Cancelled
                MessageBox.Show("Printing Cancelled")
                Exit Sub
            End If
        End Sub

        Private Sub SaveAsPDF(docName As String)
            Dim dpi As Integer = 300

            ' Keep WPF layout size as-is (in DIPs)
            Dim layoutWidth = PrintPreview.ActualWidth
            Dim layoutHeight = PrintPreview.ActualHeight

            ' Convert DIP size to pixel size for high DPI rendering
            Dim pixelWidth = CInt(layoutWidth * dpi / 96)
            Dim pixelHeight = CInt(layoutHeight * dpi / 96)

            ' Render to high-DPI bitmap
            Dim rtb As New RenderTargetBitmap(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32)

            ' Temporarily layout the control for rendering
            PrintPreview.Measure(New Size(layoutWidth, layoutHeight))
            PrintPreview.Arrange(New Rect(0, 0, layoutWidth, layoutHeight))
            PrintPreview.UpdateLayout()

            ' Render it
            rtb.Render(PrintPreview)

            ' Encode as PNG
            Dim encoder As New PngBitmapEncoder()
            encoder.Frames.Add(BitmapFrame.Create(rtb))
            Dim stream As New MemoryStream()
            encoder.Save(stream)
            stream.Position = 0

            ' Create PDF using PDFSharp
            Dim pdf As New PdfDocument()
            Dim page = pdf.AddPage()

            ' Set the page size in points (1 inch = 72 points)
            page.Width = XUnit.FromInch(layoutWidth / 96)
            page.Height = XUnit.FromInch(layoutHeight / 96)

            Dim gfx = XGraphics.FromPdfPage(page)
            Dim image = XImage.FromStream(stream)
            gfx.DrawImage(image, 0, 0, page.Width, page.Height)

            ' Save dialog
            Dim dlg As New Microsoft.Win32.SaveFileDialog()
            dlg.FileName = docName & ".pdf"
            dlg.Filter = "PDF Files (*.pdf)|*.pdf"

            If dlg.ShowDialog() = True Then
                pdf.Save(dlg.FileName)
                'MessageBox.Show("Saved to: " & dlg.FileName)
            End If
        End Sub

        Private Sub PrintPhysically(docName As String)
            Dim dlg As New PrintDialog()
            If dlg.ShowDialog() = True Then
                ' Save original layout
                Dim originalParent = VisualTreeHelper.GetParent(PrintPreview)
                Dim originalIndex As Integer = -1
                Dim originalMargin = PrintPreview.Margin
                Dim originalTransform = PrintPreview.LayoutTransform

                If TypeOf originalParent Is Panel Then
                    Dim panel = CType(originalParent, Panel)
                    originalIndex = panel.Children.IndexOf(PrintPreview)
                    panel.Children.Remove(PrintPreview)
                End If

                ' Setup layout for printing
                PrintPreview.Margin = New Thickness(0)
                PrintPreview.LayoutTransform = Transform.Identity
                PrintPreview.UpdateLayout()
                PrintPreview.Measure(New Size(Double.PositiveInfinity, Double.PositiveInfinity))
                PrintPreview.Arrange(New Rect(PrintPreview.DesiredSize))
                PrintPreview.UpdateLayout()

                ' A4 size (96 DPI)
                Dim A4Width As Double = 8.3 * 96
                Dim A4Height As Double = 11.69 * 96
                Dim scaleX = A4Width / PrintPreview.ActualWidth
                Dim scaleY = A4Height / PrintPreview.ActualHeight
                Dim scale = Math.Min(scaleX, scaleY)

                ' Container with scaling
                Dim container As New Grid()
                container.Width = A4Width
                container.Height = A4Height
                container.LayoutTransform = New ScaleTransform(scale, scale)
                container.Children.Add(PrintPreview)

                container.Measure(New Size(A4Width, A4Height))
                container.Arrange(New Rect(New Point(0, 0), New Size(A4Width, A4Height)))
                container.UpdateLayout()

                ' Create print content
                Dim fixedPage As New FixedPage()
                fixedPage.Width = A4Width
                fixedPage.Height = A4Height
                fixedPage.Children.Add(container)

                Dim pageContent As New PageContent()
                CType(pageContent, IAddChild).AddChild(fixedPage)

                Dim fixedDoc As New FixedDocument()
                fixedDoc.Pages.Add(pageContent)

                ' Print
                dlg.PrintDocument(fixedDoc.DocumentPaginator, docName)

                ' Restore layout
                container.Children.Clear()
                PrintPreview.LayoutTransform = originalTransform
                PrintPreview.Margin = originalMargin

                If TypeOf originalParent Is Panel AndAlso originalIndex >= 0 Then
                    Dim panel = CType(originalParent, Panel)
                    panel.Children.Insert(originalIndex, PrintPreview)
                End If
            End If
        End Sub

        Private Sub ClearCache()
            StatementDetails.InvoiceNumberCache = Nothing
            StatementDetails.InvoiceDateCache = Nothing
            StatementDetails.DueDateCache = Nothing
            StatementDetails.TaxCache = Nothing
            StatementDetails.TotalCostCache = Nothing
            StatementDetails.OrderItemsCache = Nothing
            StatementDetails.signature = False
            StatementDetails.ImageCache = Nothing
            StatementDetails.PathCache = Nothing
            StatementDetails.SupplierName = Nothing
            StatementDetails.City = Nothing
            StatementDetails.Region = Nothing
            StatementDetails.Country = Nothing
            StatementDetails.Phone = Nothing
            StatementDetails.Email = Nothing
            StatementDetails.noteTxt = Nothing
            StatementDetails.remarksTxt = Nothing
            StatementDetails.paymentTerms = Nothing
            StatementDetails.Term1 = Nothing
            StatementDetails.Term2 = Nothing
            StatementDetails.Term3 = Nothing
            StatementDetails.Term4 = Nothing
            StatementDetails.Term5 = Nothing
            StatementDetails.Term6 = Nothing
            StatementDetails.Term7 = Nothing
            StatementDetails.Term8 = Nothing
            StatementDetails.Term9 = Nothing
            StatementDetails.Term10 = Nothing
            StatementDetails.Term11 = Nothing
            StatementDetails.Term12 = Nothing
            StatementDetails.DeliveryCost = Nothing
            StatementDetails.paymentTerms = Nothing
            StatementDetails.isCustomTerm = Nothing
            StatementDetails.Approved = Nothing
        End Sub

        Private Sub SaveToDB()
            Dim itemsJSON As String = JsonConvert.SerializeObject(itemOrder, Formatting.Indented)
            Dim InvoiceDate As String = Date.Now.ToString("yyyy-MM-dd")
            Dim DeliveryString As String = Delivery.Text.Trim("₱"c, " "c)
            Dim TaxString As String = Tax.Text.Trim("₱"c, " "c)
            Dim TotalCostString As String = TotalCost.Text.Trim("₱"c, " "c)

            Dim DateParsed As Date = Date.Parse(StatementDetails.DueDateCache)
            Dim DueDateFormatted As String = DateParsed.ToString("yyyy-MM-dd")

            For Each child In BillAddress.Children
                Address += child.Text & Environment.NewLine
            Next

            Dim success As Boolean = PurchaseOrderController.InsertInvoicePurchaseOrder(InvoiceNumber.Text, InvoiceDate, DueDateFormatted, Address, itemsJSON, DeliveryString, TaxString, TotalCostString, base64Image, Approved.Text, PaymentTerms.Text, noteBox.Text)

            If success Then
                SaveToMongo(InvoiceNumberCache)
                MessageBox.Show("Added to database")
            End If

            ClearCache()
            ViewLoader.DynamicView.NavigateToView("neworder", Me)
        End Sub

        Private Async Sub SaveToMongo(docName As String)
            Dim dpi As Integer = 300

            ' Prepare layout
            Dim layoutWidth = PrintPreview.ActualWidth
            Dim layoutHeight = PrintPreview.ActualHeight
            Dim pixelWidth = CInt(layoutWidth * dpi / 96)
            Dim pixelHeight = CInt(layoutHeight * dpi / 96)

            ' Render to bitmap
            Dim rtb As New RenderTargetBitmap(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32)
            PrintPreview.Measure(New Size(layoutWidth, layoutHeight))
            PrintPreview.Arrange(New Rect(0, 0, layoutWidth, layoutHeight))
            PrintPreview.UpdateLayout()
            rtb.Render(PrintPreview)

            ' Encode as PNG to memory stream
            Dim encoder As New PngBitmapEncoder()
            encoder.Frames.Add(BitmapFrame.Create(rtb))
            Dim stream As New MemoryStream()
            encoder.Save(stream)
            stream.Position = 0

            ' Paths
            Dim appDir As String = AppDomain.CurrentDomain.BaseDirectory
            Dim tempImagePath As String = System.IO.Path.Combine(appDir, Guid.NewGuid().ToString() & ".png")
            Dim tempPdfPath As String = System.IO.Path.Combine(appDir, docName & ".pdf")

            Try
                ' Save temp image file
                Using fs As New FileStream(tempImagePath, FileMode.Create, FileAccess.Write)
                    stream.CopyTo(fs)
                End Using

                ' Create PDF
                Dim pdf As New PdfDocument()
                Dim page = pdf.AddPage()
                page.Width = XUnit.FromInch(layoutWidth / 96)
                page.Height = XUnit.FromInch(layoutHeight / 96)

                Dim gfx = XGraphics.FromPdfPage(page)
                Using image = XImage.FromFile(tempImagePath)
                    gfx.DrawImage(image, 0, 0, page.Width, page.Height)
                End Using

                ' Save PDF to temp file
                pdf.Save(tempPdfPath)

                ' Upload PDF to MongoDB GridFS
                Dim gridFS As GridFSBucket = SplashScreen.GetGridFSConnection()

                Using FileStream As New FileStream(tempPdfPath, FileMode.Open, FileAccess.Read)
                    Dim options As New GridFSUploadOptions() With {
            .Metadata = New BsonDocument From {
                {"uploadedBy", CacheOnLoggedInName},
                {"uploadedAt", BsonDateTime.Create(DateTime.UtcNow)},
                {"source", "purchase-order/neworder"},
                {"billiingNumber", InvoiceNumberCache},
                {"pdfFilePath", tempPdfPath}
            }
        }

                    gridFS.UploadFromStream(Path.GetFileName(tempPdfPath), FileStream, options)
                End Using

                'MessageBox.Show("PDF uploaded to MongoDB.")

            Finally
                ' Delete the temp files
                If File.Exists(tempImagePath) Then
                    Try : File.Delete(tempImagePath) : Catch : End Try
                End If
                If File.Exists(tempPdfPath) Then
                    Try : File.Delete(tempPdfPath) : Catch : End Try
                End If
            End Try
        End Sub

        Public Sub DisplayUploadedImage()
            Try
                'Dim tempImagePath As String = Path.Combine(Path.GetTempPath(), "decoded_image.png")

                ' Clean up previous image file
                If File.Exists(tempImagePath) Then
                    GC.Collect()
                    GC.WaitForPendingFinalizers()
                    File.Delete(tempImagePath)
                End If

                ' Decode and save new image
                Base64Utility.DecodeBase64ToFile(base64Image, tempImagePath)

                ' Load image safely
                Dim imageSource As New BitmapImage()
                Using stream As New FileStream(tempImagePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                    imageSource.BeginInit()
                    imageSource.CacheOption = BitmapCacheOption.OnLoad
                    imageSource.StreamSource = stream
                    imageSource.EndInit()
                End Using
                imageSource.Freeze() ' Allow image to be accessed in different threads

                BrowseFile.Child = Nothing

                Dim imagePreview As New Image()
                imagePreview.Source = imageSource
                imagePreview.MaxHeight = 70

                BrowseFile.Child = imagePreview

                '' Set the image source
                'UploadedImage.Source = imageSource
            Catch ex As Exception
                MessageBox.Show("Error decoding image: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
    End Class

End Namespace


