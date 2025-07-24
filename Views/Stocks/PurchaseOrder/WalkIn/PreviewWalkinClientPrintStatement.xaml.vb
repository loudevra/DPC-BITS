Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Windows.Markup
Imports DocumentFormat.OpenXml.Bibliography
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Views.Sales.Quotes
Imports DPC.DPC.Views.Stocks.Warehouses
Imports Newtonsoft.Json
Imports System.Windows.Media.Imaging
Imports PdfSharp.Pdf
Imports PdfSharp.Drawing
Imports MongoDB.Driver
Imports MongoDB.Bson
Imports MongoDB.Driver.GridFS
Imports System.ServiceModel.Channels

Namespace DPC.Views.Stocks.PurchaseOrder.WalkIn
    Public Class PreviewWalkinClientPrintStatement

        Private tempImagePath As String
        Private base64Image As String
        Private itemDataSource As New ObservableCollection(Of OrderItems)
        Private itemOrder As New List(Of Dictionary(Of String, String))
        Private Address As String
        Private isCustom As Boolean

        Private Sub PreviewWalkInPrintStatement_Loaded(sender As Object, e As RoutedEventArgs)
            BillingNumber.Text = BLNumberCache
            BillingDate.Text = BLDateCache
            txtDRNo.Text = BLDRNo

            ClientNameBox.Text = BLClientName
            ClientAddress.Text = BLCompleteAddress
            ClientContact.Text = BLClientContact
            CompanyRep.Text = BLCompanyRep

            SalesRep.Text = BLSalesRep
            PreparedBy.Text = CacheOnLoggedInName
            cmbApproved.Text = BLApproved

            ' Check if the terms is enabled
            If BLisCustomTerm = True Then
                cmbTerms.Text = BLpaymentTerms
            Else
                cmbTerms.Text = BLpaymentTerms
            End If


            itemOrder = WalkinBillingStatementDetails.BLItemsCache
            ' Load the data in the datagrid
            For Each item In itemOrder
                Dim rate As Decimal = Decimal.Parse(item("Rate"))
                Dim rateFormatted As String = rate.ToString("N2")

                Dim linePrice As Decimal = Decimal.Parse(item("Amount"))
                Dim linePriceFormatted As String = linePrice.ToString("N2")

                itemDataSource.Add(New OrderItems With {
                    .Quantity = item("Quantity"),
                    .Description = item("ProductName"),
                    .UnitPrice = $"₱ {rateFormatted}",
                    .LinePrice = $"₱ {linePriceFormatted}"
                })
            Next

            ' Display the data in the DataGrid
            dataGrid.ItemsSource = itemDataSource


            base64Image = WalkinBillingStatementDetails.BLImageCache
            tempImagePath = WalkinBillingStatementDetails.BLPathCache
            DisplaySignaturePreview()

            Subtotal.Text = BLSubTotalCache

            Dim installationFee As Double
            If Double.TryParse(WalkinBillingStatementDetails.BLInstallation, installationFee) Then
                Installation.Text = "₱ " & installationFee.ToString("N2")
            Else
                Installation.Text = "₱ 0.00" ' fallback value if parsing fails
            End If

            Dim deliveryFee As Double
            If Double.TryParse(WalkinBillingStatementDetails.BLDeliveryCost, deliveryFee) Then
                Delivery.Text = "₱ " & deliveryFee.ToString("N2")
            Else
                Delivery.Text = "₱ 0.00" ' fallback value if parsing fails
            End If
            VAT12.Text = BLTotalTaxValueCache
            TotalCost.Text = BLTotalAmountCache

            BankDetailBox.Text = BLBankDetails
            AccNameBox.Text = BLAccountName
            AccNoBox.Text = BLAccountNumber


            remarksBox.Text = WalkinBillingStatementDetails.BLremarksTxt

            Term1.Text = WalkinBillingStatementDetails.BLTerm1
            Term2.Text = WalkinBillingStatementDetails.BLTerm2
            Term3.Text = WalkinBillingStatementDetails.BLTerm3
            Term4.Text = WalkinBillingStatementDetails.BLTerm4
            Term5.Text = WalkinBillingStatementDetails.BLTerm5
            Term6.Text = WalkinBillingStatementDetails.BLTerm6
            Term7.Text = WalkinBillingStatementDetails.BLTerm7
            Term8.Text = WalkinBillingStatementDetails.BLTerm8
            Term9.Text = WalkinBillingStatementDetails.BLTerm9
            Term10.Text = WalkinBillingStatementDetails.BLTerm10
            Term11.Text = WalkinBillingStatementDetails.BLTerm11
            Term12.Text = WalkinBillingStatementDetails.BLTerm12

            AddHandler SaveDb.Click, AddressOf SaveToDb
        End Sub
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()





            '' Add any initialization after the InitializeComponent() call.

        End Sub

        Private Function FormatPhoneWithSpaces(raw As String) As String
            If String.IsNullOrWhiteSpace(raw) OrElse raw.Length < 2 Then Return raw

            ' Remove the first character
            Dim number = raw.Substring(1)

            ' Apply formatting
            If number.Length >= 10 Then
                Return $"{number.Substring(0, 3)} {number.Substring(3, 3)} {number.Substring(6)}"
            ElseIf number.Length >= 7 Then
                Return $"{number.Substring(0, 3)} {number.Substring(3, 3)} {number.Substring(6)}"
            ElseIf number.Length >= 6 Then
                Return $"{number.Substring(0, 3)} {number.Substring(3)}"
            Else
                Return number
            End If
        End Function

        Private Sub CancelButton(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("navigatetobillingstatement", Me)
        End Sub

        Private Sub SavePrint(sender As Object, e As RoutedEventArgs)
            ' Ask user: PDF or Print
            Dim result As MessageBoxResult = MessageBox.Show("Do you want to save this as a PDF?", "Choose Output", MessageBoxButton.YesNoCancel, MessageBoxImage.Question)

            Dim docName As String = BLNumberCache ' You can replace this with any string

            If result = MessageBoxResult.Yes Then
                ' ➤ Save as PDF
                SaveAsPDF(docName)
                SaveToDb()
            ElseIf result = MessageBoxResult.No Then
                ' ➤ Print physically
                PrintPhysically(docName)
                SaveToDb()
            Else
                ' ➤ Cancelled
                MessageBox.Show("Printing Cancelled")
                Exit Sub
            End If
        End Sub

        <Obsolete>
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


        Public Sub DisplaySignaturePreview()
            Dim grid As New Grid()

            ' Define rows: Row 0 for image, Row 1 for warning text
            grid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
            grid.RowDefinitions.Add(New RowDefinition With {.Height = GridLength.Auto})

            ' If signature exists, add the image
            If WalkinBillingStatementDetails.BLsignature = True Then
                If File.Exists(tempImagePath) Then
                    GC.Collect()
                    GC.WaitForPendingFinalizers()
                    File.Delete(tempImagePath)
                End If

                Base64Utility.DecodeBase64ToFile(base64Image, tempImagePath)

                Dim imageSource As New BitmapImage()
                Using stream As New FileStream(tempImagePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                    imageSource.BeginInit()
                    imageSource.CacheOption = BitmapCacheOption.OnLoad
                    imageSource.StreamSource = stream
                    imageSource.EndInit()
                End Using
                imageSource.Freeze()

                Dim imagePreview As New Image With {
                    .Source = imageSource,
                    .MaxHeight = 70,
                    .HorizontalAlignment = HorizontalAlignment.Center,
                    .VerticalAlignment = VerticalAlignment.Center
                }

                Grid.SetRow(imagePreview, 0)
                grid.Children.Add(imagePreview)
            End If

            ' Always add the warning text at the bottom
            Dim warningText = CreateSignatureWarningText()
            Grid.SetRow(warningText, 1)
            grid.Children.Add(warningText)

            ' Set as Border child
            BrowseFile.Child = grid
        End Sub

        Public Function CreateSignatureWarningText() As TextBlock
            Return New TextBlock With {
        .Text = "By signing the document, you confirm that the billing amount is" & vbLf &
                "accurate and corresponds to your additional terms or services.",
        .FontWeight = FontWeights.Bold,
        .FontFamily = New FontFamily("Lexend"),
        .FontSize = 6.5,
        .TextAlignment = TextAlignment.Center,
        .Foreground = Brushes.Red,
        .TextWrapping = TextWrapping.Wrap,
        .HorizontalAlignment = HorizontalAlignment.Center,
        .Margin = New Thickness(0, 5, 0, 0),
        .MaxWidth = 200
    }
        End Function


        Private Sub SaveToDb()
            Dim jsonItems As String = Newtonsoft.Json.JsonConvert.SerializeObject(BLItemsCache, Newtonsoft.Json.Formatting.Indented)

            Dim docName As String = BLNumberCache
            SaveToMongo(docName)

            Dim success As Boolean = WalkInController.InsertBilling(
                BLNumberCache,
                Convert.ToDateTime(BLDateCache),
                BLDRNo,
                BLClientName,
                BLCompanyRep,
                BLSalesRep,
                CacheOnLoggedInName,
                BLApproved,
                BLpaymentTerms,
                jsonItems,
                BLWarehouseNameCache,
                base64Image,
                BLTaxProperty,
                BLDiscountProperty,
                BLTotalTaxValueCache,
                BLTotalDiscountValueCache,
                BLTotalAmountCache,
                BLnoteTxt,
                BLBankDetails,
                BLAccountName,
                BLAccountNumber,
                BLremarksTxt
)

            If success Then
                ' Unregister all textbox names before clearing UI to avoid duplicate name errors
                Dim WalkInForm As New DPC.Views.Stocks.PurchaseOrder.WalkIn.WalkInNewOrder()
                WalkInForm.ClearAllFields()
                WalkinBillingStatementDetails.ClearAllBLCache()
                ViewLoader.DynamicView.NavigateToView("walkinorder", Me)
            Else
                MessageBox.Show("Failed to submit walk-in billing.")
            End If
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
                {"source", "cost-estimate/quote"},
                {"billiingNumber", BillingNumber.Text},
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


    End Class
End Namespace