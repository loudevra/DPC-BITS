Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text.Json
Imports System.Windows.Markup
Imports DocumentFormat.OpenXml.Bibliography
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Views.Sales.Quotes
Imports MongoDB.Bson
Imports MongoDB.Driver
Imports MongoDB.Driver.GridFS
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf

Namespace DPC.Components.Forms

    Public Class PreviewPrintEditedQuote

        Private tempImagePath As String
        Private base64Image As String
        Private itemDataSource As New ObservableCollection(Of OrderItems)
        Private itemOrder As New List(Of Dictionary(Of String, String))
        Private Address As String
        Private isCustom As Boolean

        Public Sub New()
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
            QuoteValidityDate.Text = CostEstimateDetails.CEValidUntilDate ' Changed the Cache name while the previous cache variable would be stay
            Subtotal.Text = CostEstimateDetails.CESubTotalCache
            TotalCost.Text = CostEstimateDetails.CEGrandTotalCost
            VAT12.Text = CostEstimateDetails.CETotalTaxValueCache
            Delivery.Text = "₱ " & CostEstimateDetails.CEDeliveryCost.ToString("N2")
            itemOrder = CostEstimateDetails.CEQuoteItemsCache
            base64Image = CostEstimateDetails.CEImageCache
            tempImagePath = CostEstimateDetails.CEPathCache
            ClientNameBox.Text = CostEstimateDetails.CEClientName
            AddressLineOne.Text = CostEstimateDetails.CEAddress & ", " & CostEstimateDetails.CECity    ' -- important when editing
            AddressLineTwo.Text = CostEstimateDetails.CERegion & ", " & CostEstimateDetails.CECountry    ' -- important when editing
            PhoneBox.Text = "+63 " & FormatPhoneWithSpaces(CostEstimateDetails.CEPhone)
            RepresentativeBox.Text = CostEstimateDetails.CERepresentative
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
            Term13.Text = CostEstimateDetails.CETerm13
            Term14.Text = CostEstimateDetails.CETerm14
            Term15.Text = CostEstimateDetails.CETerm15
            SalesRep.Text = CacheOnLoggedInName
            cmbApproved.Text = CostEstimateDetails.CEApproved
            SubtotalTax.Text = CostEstimateDetails.CESubtotalExInc
            Warranty.Text = CostEstimateDetails.CEWarranty
            DeliveryMobilization.Text = CostEstimateDetails.CEDeliveryMobilization
            CNIdentifier.Text = CostEstimateDetails.CECNIndetifier

            ' Check if the terms is enabled
            If CostEstimateDetails.CEisCustomTerm = True Then
                cmbTerms.Text = CostEstimateDetails.CEpaymentTerms
                cmbTerms.Foreground = Brushes.White

            Else
                cmbTerms.Text = CostEstimateDetails.CEpaymentTerms
            End If

            If CEtaxSelection Then
                ' Visibility of the Vat Text
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
            Dim number = raw.Substring(1)
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
                Dim savedPath As String = SaveAsPDFForExistingQuote(docName) ' Always generate PDF

                If result = MessageBoxResult.Yes Then
                    ' Save as PDF (already done above)
                    If Not UpdatePdfPathInMongoDB(savedPath, CEQuoteNumberCache, CacheOnLoggedInName) Then Exit Sub
                    UpdateToDb()
                ElseIf result = MessageBoxResult.No Then
                    ' Print physically
                    PrintPhysically(docName)
                    If Not UpdatePdfPathInMongoDB(savedPath, CEQuoteNumberCache, CacheOnLoggedInName) Then Exit Sub
                    UpdateToDb()
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
            grid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
            grid.RowDefinitions.Add(New RowDefinition With {.Height = GridLength.Auto})

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

            Dim warningText = CreateSignatureWarningText()
            Grid.SetRow(warningText, 1)
            grid.Children.Add(warningText)

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

        ''' <summary>
        ''' Saves PDF with automatic filename (quote number) - no dialog
        ''' </summary>
        Private Function SaveAsPDFForExistingQuote(docName As String) As String
            Try
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
                gfx.DrawImage(image, 0, 0, page.Width.Point, page.Height.Point)

                ' Auto-generate filename with timestamp for version control
                Dim fileName As String = docName & "_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".pdf"
                Dim filePath As String = Path.Combine(My.Computer.FileSystem.SpecialDirectories.Temp, fileName)

                ' Temp directory is automatically created by system, no need to create it

                pdf.Save(filePath)
                MessageBox.Show("Saved to: " & filePath)
                Return filePath

                ViewLoader.DynamicView.NavigateToView("salesquote", Me)

            Catch ex As Exception
                MessageBox.Show("Error generating PDF: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Updates existing PDF entry in MongoDB GridFS
        ''' </summary>
        Private Shared Function UpdatePdfPathInMongoDB(filePath As String, quoteNumber As String, uploadedBy As String) As Boolean
            Try
                Dim gridFS As GridFSBucket = SplashScreen.GetGridFSConnection()
                Dim db = gridFS.Database

                ' Delete old file with same quote number
                Dim filter = Builders(Of GridFSFileInfo).Filter.Eq(Function(f) f.Metadata("quoteNumber"), quoteNumber)
                Dim oldFiles = gridFS.Find(filter).ToList()

                For Each oldFile In oldFiles
                    gridFS.Delete(oldFile.Id)
                Next

                ' Upload new file
                Using fileStream As New FileStream(filePath, FileMode.Open, FileAccess.Read)
                    Dim options As New GridFSUploadOptions() With {
                        .Metadata = New BsonDocument From {
                            {"uploadedBy", uploadedBy},
                            {"uploadedAt", BsonDateTime.Create(DateTime.UtcNow)},
                            {"source", "cost-estimate/quote"},
                            {"quoteNumber", quoteNumber},
                            {"pdfFilePath", filePath},
                            {"version", "updated"}
                        }
                    }

                    gridFS.UploadFromStream(Path.GetFileName(filePath), fileStream, options)
                End Using

                MessageBox.Show("Quote PDF updated successfully in database.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                Return True

            Catch ex As Exception
                MessageBox.Show("Error updating PDF in MongoDB: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
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

        ''' <summary>
        ''' Saves the edited quote - UPDATE instead of INSERT
        ''' </summary>
        Private Sub SaveUpdateDb_Click(sender As Object, e As RoutedEventArgs)
            Try
                Dim docName As String = CEQuoteNumberCache
                Dim savedPath As String = SaveAsPDFForExistingQuote(docName)

                If String.IsNullOrEmpty(savedPath) Then
                    MessageBox.Show("PDF save cancelled. Data not saved.")
                    Exit Sub
                End If

                ' Update MongoDB with PDF path
                If Not UpdatePdfPathInMongoDB(savedPath, CEQuoteNumberCache, CacheOnLoggedInName) Then
                    Exit Sub
                End If

                ' Update database - this should call UpdateToDb which calls UpdateQuote
                UpdateToDb()

            Catch ex As Exception
                MessageBox.Show("Error saving to database: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Debug.WriteLine("Error in SaveUpdateDb_Click: " & ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' Updates the quote in database
        ''' </summary>
        Private Sub UpdateToDb()
            Try
                Debug.WriteLine("")
                Debug.WriteLine("===============================================")
                Debug.WriteLine("UpdateToDb METHOD CALLED - STARTING UPDATE PROCESS")
                Debug.WriteLine("===============================================")

                Dim quoteNumber As String = CEQuoteNumberCache
                Debug.WriteLine($"1. Quote Number (from cache): {quoteNumber}")

                ' VALIDATION: Check if quote number exists
                If String.IsNullOrWhiteSpace(quoteNumber) Then
                    Debug.WriteLine("ERROR: Quote number is empty or null!")
                    MessageBox.Show("Error: Quote number not found. Cannot update.", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' VALIDATION: Verify quote exists in database before attempting update
                If Not QuotesController.QuoteNumberExists(quoteNumber) Then
                    Debug.WriteLine($"ERROR: Quote number '{quoteNumber}' does not exist in database!")
                    MessageBox.Show($"Error: Quote '{quoteNumber}' not found in database. Cannot update.", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                Debug.WriteLine($"   ✓ Quote number validated and exists in database")

                ' Serialize items
                Dim jsonItems As String = ""
                If CEQuoteItemsCache IsNot Nothing AndAlso CEQuoteItemsCache.Count > 0 Then
                    jsonItems = Newtonsoft.Json.JsonConvert.SerializeObject(CEQuoteItemsCache)
                End If
                Debug.WriteLine($"2. JSON Items serialized: {jsonItems.Length} characters")

                ' Parse and clean numeric values
                Dim totalTaxString As String = CETotalTaxValueCache.Replace("₱", "").Replace(" ", "").Trim()
                Dim totalDiscountString As String = CETotalDiscountValueCache.Replace("₱", "").Replace(" ", "").Trim()
                Dim totalAmountString As String = CETotalAmountCache.Replace("₱", "").Replace(" ", "").Trim()

                Debug.WriteLine($"3. Raw Values Before Cleaning:")
                Debug.WriteLine($"   Tax Value: {CETotalTaxValueCache}")
                Debug.WriteLine($"   Discount Value: {CETotalDiscountValueCache}")
                Debug.WriteLine($"   Amount Value: {CETotalAmountCache}")

                ' Handle empty values
                If String.IsNullOrWhiteSpace(totalTaxString) Then totalTaxString = "0"
                If String.IsNullOrWhiteSpace(totalDiscountString) Then totalDiscountString = "0"
                If String.IsNullOrWhiteSpace(totalAmountString) Then totalAmountString = "0"

                Debug.WriteLine($"   Cleaned Tax: {totalTaxString}")
                Debug.WriteLine($"   Cleaned Discount: {totalDiscountString}")
                Debug.WriteLine($"   Cleaned Amount: {totalAmountString}")

                ' Parse with error handling
                Dim totalTax As String = "0.00"
                Dim totalDiscount As String = "0.00"
                Dim totalAmount As String = "0.00"

                Try
                    totalTax = Decimal.Parse(totalTaxString).ToString("F2")
                Catch
                    Debug.WriteLine($"WARNING: Could not parse tax value '{totalTaxString}', defaulting to 0.00")
                    totalTax = "0.00"
                End Try

                Try
                    totalDiscount = Decimal.Parse(totalDiscountString).ToString("F2")
                Catch
                    Debug.WriteLine($"WARNING: Could not parse discount value '{totalDiscountString}', defaulting to 0.00")
                    totalDiscount = "0.00"
                End Try

                Try
                    totalAmount = Decimal.Parse(totalAmountString).ToString("F2")
                Catch
                    Debug.WriteLine($"WARNING: Could not parse amount value '{totalAmountString}', defaulting to 0.00")
                    totalAmount = "0.00"
                End Try

                Debug.WriteLine($"4. Parsed & Formatted Values:")
                Debug.WriteLine($"   Total Tax: {totalTax}")
                Debug.WriteLine($"   Total Discount: {totalDiscount}")
                Debug.WriteLine($"   Total Amount: {totalAmount}")

                Debug.WriteLine($"5. Other Details:")
                Debug.WriteLine($"   ClientID: {CEClientIDCache}")
                Debug.WriteLine($"   ClientName: {CEClientName}")
                Debug.WriteLine($"   WarehouseID: {CEWarehouseIDCache}")
                Debug.WriteLine($"   WarehouseName: {CEWarehouseNameCache}")
                Debug.WriteLine($"   ReferenceNo: {CEReferenceNumber}")
                Debug.WriteLine($"   QuoteDate: {CEQuoteDateCache}")
                Debug.WriteLine($"   Validity: {CEQuoteValidityDateCache}")
                Debug.WriteLine($"   Tax Property: {CETaxProperty}")
                Debug.WriteLine($"   Discount Property: {CEDiscountProperty}")
                Debug.WriteLine($"   Username: {CacheOnLoggedInName}")
                Debug.WriteLine($"   ApprovedBy: {CEApproved}")
                Debug.WriteLine($"   PaymentTerms: {CEpaymentTerms}")
                Debug.WriteLine($"   Note: {CEnoteTxt}")

                Debug.WriteLine("6. CALLING QuotesController.UpdateQuote()...")
                Debug.WriteLine("===============================================")

                ' Validate date before parsing
                Dim quoteDate As DateTime = DateTime.Now
                Try
                    quoteDate = DateTime.Parse(CEQuoteDateCache)
                Catch
                    Debug.WriteLine($"WARNING: Could not parse quote date '{CEQuoteDateCache}', using current date")
                    quoteDate = DateTime.Now
                End Try

                ' Call UpdateQuote
                Dim success As Boolean = QuotesController.UpdateQuote(
            quoteNumber,
            If(String.IsNullOrEmpty(CEReferenceNumber), "", CEReferenceNumber),
            quoteDate,
            If(String.IsNullOrEmpty(CEQuoteValidityDateCache), DateTime.Now.AddDays(30).ToString("yyyy-MM-dd"), CEQuoteValidityDateCache),
            If(String.IsNullOrEmpty(CETaxProperty), "Inclusive", CETaxProperty),
            If(String.IsNullOrEmpty(CEDiscountProperty), "None", CEDiscountProperty),
            If(String.IsNullOrEmpty(CEClientIDCache), "", CEClientIDCache),
            If(String.IsNullOrEmpty(CEClientName), "", CEClientName),
            If(String.IsNullOrEmpty(CEWarehouseIDCache), "0", CEWarehouseIDCache),
            If(String.IsNullOrEmpty(CEWarehouseNameCache), "", CEWarehouseNameCache),
            jsonItems,
            If(String.IsNullOrEmpty(CEnoteTxt), "", CEnoteTxt),
            totalTax,
            totalDiscount,
            totalAmount,
            If(String.IsNullOrEmpty(CacheOnLoggedInName), "", CacheOnLoggedInName),
            If(String.IsNullOrEmpty(CEApproved), "", CEApproved),
            If(String.IsNullOrEmpty(CEpaymentTerms), "", CEpaymentTerms))

                Debug.WriteLine("===============================================")
                Debug.WriteLine($"7. UpdateQuote returned: {success}")

                If success Then
                    Debug.WriteLine("8. SUCCESS - Quote updated, clearing cache and navigating...")
                    CostEstimateDetails.ClearAllCECache()
                    ViewLoader.DynamicView.NavigateToView("salesquote", Me)
                    Debug.WriteLine("UpdateToDb COMPLETED SUCCESSFULLY")
                Else
                    Debug.WriteLine("8. FAILED - UpdateQuote returned False")
                    Debug.WriteLine("UpdateToDb FAILED - Quote not updated")
                End If

                Debug.WriteLine("===============================================")
                Debug.WriteLine("")

            Catch ex As Exception
                Debug.WriteLine("===============================================")
                Debug.WriteLine("EXCEPTION IN UpdateToDb:")
                Debug.WriteLine($"Message: {ex.Message}")
                Debug.WriteLine($"StackTrace: {ex.StackTrace}")
                Debug.WriteLine("===============================================")
                Debug.WriteLine("")
                MessageBox.Show("Error updating quote: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

    End Class

End Namespace