Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text.Json
Imports System.Windows.Markup
Imports DocumentFormat.OpenXml.Bibliography
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Views.Sales.Quotes
Imports Newtonsoft.Json
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf
Imports MongoDB.Driver
Imports MongoDB.Bson
Imports MongoDB.Driver.GridFS

Namespace DPC.Components.Forms
    Public Class PreviewPrintQuote

        Private tempImagePath As String
        Private base64Image As String
        Private itemDataSource As New ObservableCollection(Of OrderItems)
        Private itemOrder As New List(Of Dictionary(Of String, String))
        Private Address As String
        Private isCustom As Boolean

        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            Dim installationFee As Decimal
            If Decimal.TryParse(CostEstimateDetails.CEInstallation, installationFee) Then
                Installation.Text = "₱ " & installationFee.ToString("N2")
            Else
                Installation.Text = "₱ 0.00" ' fallback value if parsing fails
            End If

            ' Add any initialization after the InitializeComponent() call.
            QuoteNumber.Text = CostEstimateDetails.CEQuoteNumberCache
            QuoteDate.Text = CostEstimateDetails.CEQuoteDateCache
            QuoteValidityDate.Text = CostEstimateDetails.CEQuoteValidityDateCache
            Subtotal.Text = CostEstimateDetails.CESubTotalCache
            TotalCost.Text = CostEstimateDetails.CETotalAmountCache
            VAT12.Text = CostEstimateDetails.CETotalTaxValueCache
            Delivery.Text = "₱ " & CostEstimateDetails.CEDeliveryCost.ToString("N2")
            itemOrder = CostEstimateDetails.CEQuoteItemsCache
            base64Image = CostEstimateDetails.CEImageCache
            tempImagePath = CostEstimateDetails.CEPathCache
            ClientNameBox.Text = CostEstimateDetails.CEClientName
            AddressLineOne.Text = CostEstimateDetails.CEAddress & ", " & CostEstimateDetails.CECity    ' -- important when editing
            AddressLineTwo.Text = CostEstimateDetails.CERegion & ", " & CostEstimateDetails.CECountry    ' -- important when editing
            PhoneBox.Text = "Tel No.: +63 " & FormatPhoneWithSpaces(CostEstimateDetails.CEPhone)
            EmailBox.Text = CostEstimateDetails.CEEmail
            noteBox.Text = CostEstimateDetails.CEnoteTxt
            remarksBox.Text = CostEstimateDetails.CEremarksTxt
            Term1.Text = CostEstimateDetails.CETerm1
            Term2.Text = CostEstimateDetails.CETerm2
            Term3.Text = CostEstimateDetails.CETerm3
            Term4.Text = CostEstimateDetails.CETerm4
            Term5.Text = CostEstimateDetails.CETerm5
            Term6.Text = CostEstimateDetails.CETerm6
            Term7.Text = CostEstimateDetails.CETerm7
            Term8.Text = CostEstimateDetails.CETerm8
            Term9.Text = CostEstimateDetails.CETerm9
            Term10.Text = CostEstimateDetails.CETerm10
            Term11.Text = CostEstimateDetails.CETerm11
            Term12.Text = CostEstimateDetails.CETerm12
            SalesRep.Text = CacheOnLoggedInName
            cmbApproved.Text = CostEstimateDetails.CEApproved

            ' Check if the terms is enabled
            If CostEstimateDetails.CEisCustomTerm = True Then
                cmbTerms.Text = CostEstimateDetails.CEpaymentTerms
                cmbTerms.Foreground = Brushes.White

            Else
                cmbTerms.Text = CostEstimateDetails.CEpaymentTerms
            End If

            If CEtaxSelection Then
                VatText.Visibility = Visibility.Collapsed
                VatValue.Visibility = Visibility.Collapsed
            Else
                VatText.Visibility = Visibility.Visible
                VatValue.Visibility = Visibility.Visible
            End If

            DisplaySignaturePreview()

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
            ViewLoader.DynamicView.NavigateToView("costestimate", Me)
        End Sub

        Private Sub SavePrint(sender As Object, e As RoutedEventArgs)
            Try
                ' Ask user: PDF or Print
                Dim result As MessageBoxResult = MessageBox.Show("Do you want to save this as a PDF?", "Choose Output", MessageBoxButton.YesNoCancel, MessageBoxImage.Question)

                Dim docName As String = CEQuoteNumberCache
                Dim savedPath As String = SaveAsPDF(docName) ' Always generate PDF

                If result = MessageBoxResult.Yes Then
                    ' Save as PDF (already done above)
                    If Not SavePdfPathToMongoDB(savedPath, CEQuoteNumberCache, CacheOnLoggedInName) Then Exit Sub
                    SaveToDb()
                ElseIf result = MessageBoxResult.No Then
                    ' Print physically
                    PrintPhysically(docName)
                    If Not SavePdfPathToMongoDB(savedPath, CEQuoteNumberCache, CacheOnLoggedInName) Then Exit Sub
                    SaveToDb()
                Else
                    ' Cancelled
                    MessageBox.Show("Printing Cancelled")
                    Exit Sub
                End If
            Catch ex As Exception
                MessageBox.Show("Error during save/print: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Public Sub DisplaySignaturePreview()
            Dim grid As New Grid()

            ' Define rows: Row 0 for image, Row 1 for warning text
            grid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
            grid.RowDefinitions.Add(New RowDefinition With {.Height = GridLength.Auto})

            ' If signature exists, add the image
            If CostEstimateDetails.CEsignature = True Then
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

        Private Shared Function SavePdfPathToMongoDB(filePath As String, quoteNumber As String, uploadedBy As String) As Boolean
            Try
                ' Get the MongoDB GridFS connection from SplashScreen
                Dim gridFS As GridFSBucket = SplashScreen.GetGridFSConnection()

                Using fileStream As New FileStream(filePath, FileMode.Open, FileAccess.Read)
                    Dim options As New GridFSUploadOptions() With {
                        .Metadata = New BsonDocument From {
                            {"uploadedBy", uploadedBy},
                            {"uploadedAt", BsonDateTime.Create(DateTime.UtcNow)},
                            {"source", "cost-estimate/quote"},
                            {"quoteNumber", quoteNumber},
                            {"pdfFilePath", filePath}
                        }
                    }

                    gridFS.UploadFromStream(Path.GetFileName(filePath), fileStream, options)
                End Using
                Return True
            Catch ex As Exception
                MessageBox.Show("(Tips: You can go back to the newquote without lossing data) Error saving PDF path to MongoDB: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function

        Private Sub SaveToDb()
            Dim jsonItems As String = Newtonsoft.Json.JsonConvert.SerializeObject(CEQuoteItemsCache)

            Dim success As Boolean = QuotesController.InsertQuote(CEQuoteNumberCache,
                                                                  CEReferenceNumber,
            CEQuoteDateCache,
            CEQuoteValidityDateCache,
            CETaxProperty,
            CEDiscountProperty,
            CEClientIDCache,
            CEClientName,
            CEWarehouseIDCache,
            CEWarehouseNameCache,
            jsonItems,
            CEQuoteNumberCache,
            CETotalTaxValueCache,
            CETotalDiscountValueCache,
            CETotalAmountCache,
            CacheOnLoggedInName,
            CEApproved,
            CEpaymentTerms)
            If success Then
                ' Unregister all textbox names before clearing UI to avoid duplicate name errors
                Dim quoteForm As New DPC.Views.Sales.Quotes.NewQuote()
                quoteForm.ClearAllFields()
                CostEstimateDetails.ClearAllCECache()
                ViewLoader.DynamicView.NavigateToView("newquote", Me)
            Else
                MessageBox.Show("Failed to submit quote.")
            End If
        End Sub

        <Obsolete>
        Private Function SaveAsPDF(docName As String) As String
            Dim dpi As Integer = 300
            Dim layoutWidth = PrintPreview.ActualWidth
            Dim layoutHeight = PrintPreview.ActualHeight
            Dim pixelWidth = CInt(layoutWidth * dpi / 96)
            Dim pixelHeight = CInt(layoutHeight * dpi / 96)

            Dim rtb As New RenderTargetBitmap(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32)
            PrintPreview.Measure(New Size(layoutWidth, layoutHeight))
            PrintPreview.Arrange(New Rect(0, 0, layoutWidth, layoutHeight))
            PrintPreview.UpdateLayout()
            rtb.Render(PrintPreview)

            Dim encoder As New PngBitmapEncoder()
            encoder.Frames.Add(BitmapFrame.Create(rtb))
            Dim stream As New MemoryStream()
            encoder.Save(stream)
            stream.Position = 0

            Dim pdf As New PdfDocument()
            Dim page = pdf.AddPage()
            page.Width = XUnit.FromInch(layoutWidth / 96)
            page.Height = XUnit.FromInch(layoutHeight / 96)

            Dim gfx = XGraphics.FromPdfPage(page)
            Dim image = XImage.FromStream(stream)
            gfx.DrawImage(image, 0, 0, page.Width, page.Height)

            Dim dlg As New Microsoft.Win32.SaveFileDialog() With {
        .FileName = docName & ".pdf",
        .Filter = "PDF Files (.pdf)|.pdf"
    }

            If dlg.ShowDialog() = True Then
                pdf.Save(dlg.FileName)
                MessageBox.Show("Saved to: " & dlg.FileName)
                Return dlg.FileName
            Else
                Return Nothing
            End If
        End Function

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

        Private Sub SaveDb_Click(sender As Object, e As RoutedEventArgs)
            Try
                Dim docName As String = CEQuoteNumberCache
                Dim savedPath As String = SaveAsPDF(docName)
                If Not SavePdfPathToMongoDB(savedPath, CEQuoteNumberCache, CacheOnLoggedInName) Then Exit Sub
                If Not String.IsNullOrEmpty(savedPath) Then
                    SaveToDb()
                Else
                    MessageBox.Show("PDF save cancelled. Data not saved.")
                End If
            Catch ex As Exception
                MessageBox.Show("Error saving to database: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

    End Class
End Namespace