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

        ' Add pagination variables
        Private itemsPerPage As Integer = 14
        Private paginationTriggerThreshold As Integer = 7
        Private currentPageIndex As Integer = 0
        Private totalPages As Integer = 1
        Private allItems As New List(Of Dictionary(Of String, String))
        Private allPages As New List(Of List(Of OrderItems))
        Public Sub New()
            InitializeComponent()

            ' Check if CEQuoteItemsCache is Nothing
            If CostEstimateDetails.CEQuoteItemsCache Is Nothing Then
                MessageBox.Show("Quote items are not loaded.")
                Return
            End If

            ' Initialize allItems
            allItems = CostEstimateDetails.CEQuoteItemsCache
            itemOrder = CostEstimateDetails.CEQuoteItemsCache

            ' Calculate total pages
            If allItems IsNot Nothing AndAlso allItems.Count > 0 Then
                totalPages = Math.Ceiling(allItems.Count / itemsPerPage)
            Else
                totalPages = 1
            End If

            Dim installationFee As Decimal
            If Decimal.TryParse(CostEstimateDetails.CEInstallation, installationFee) Then
                Installation.Text = "₱ " & installationFee.ToString("N2")
            Else
                Installation.Text = "₱ 0.00"
            End If

            QuoteNumber.Text = CostEstimateDetails.CEQuoteNumberCache
            QuoteDate.Text = CostEstimateDetails.CEQuoteDateCache
            QuoteValidityDate.Text = CostEstimateDetails.CEValidUntilDate
            Subtotal.Text = CostEstimateDetails.CESubTotalCache
            TotalCost.Text = CostEstimateDetails.CEGrandTotalCost
            VAT12.Text = CostEstimateDetails.CETotalTaxValueCache
            Delivery.Text = "₱ " & CostEstimateDetails.CEDeliveryCost.ToString("N2")
            base64Image = CostEstimateDetails.CEImageCache
            tempImagePath = CostEstimateDetails.CEPathCache
            ClientNameBox.Text = CostEstimateDetails.CEClientName
            AddressLineOne.Text = CostEstimateDetails.CEAddress & ", " & CostEstimateDetails.CECity
            AddressLineTwo.Text = CostEstimateDetails.CERegion & ", " & CostEstimateDetails.CECountry
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

            ' Other Services
            If Not String.IsNullOrWhiteSpace(CostEstimateDetails.CEOtherServices) AndAlso
               CostEstimateDetails.CEOtherServices <> "Services:" Then
                OtherServicesText.Text = CostEstimateDetails.CEOtherServices.Replace("Services:", "").Trim()
                OtherServicesText.Visibility = Visibility.Visible
            Else
                OtherServicesText.Visibility = Visibility.Collapsed
            End If

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

            txtPageInfo = TryCast(Me.FindName("txtPageInfo"), TextBlock)

            ' Split items into pages and load first page
            SplitItemsIntoPages()
            LoadPrintPage(0)

            UpdatePageInfo()
            UpdateNavigationButtons()

        End Sub

        ' Split items into pages
        ' Update SplitItemsIntoPages method:
        Private Sub SplitItemsIntoPages()
            allPages.Clear()

            If allItems Is Nothing OrElse allItems.Count = 0 Then
                Return
            End If

            ' Special case: 8-14 items - all on first page, second page empty
            If allItems.Count > paginationTriggerThreshold AndAlso allItems.Count <= itemsPerPage Then
                Dim firstPageItems As New List(Of OrderItems)

                For Each item In allItems
                    firstPageItems.Add(CreateOrderItem(item))
                Next

                allPages.Add(firstPageItems)
                allPages.Add(New List(Of OrderItems)()) ' Empty second page for sections
                Return
            End If

            ' Normal pagination
            Dim currentPageItems As New List(Of OrderItems)

            For Each item In allItems
                currentPageItems.Add(CreateOrderItem(item))

                If currentPageItems.Count = itemsPerPage Then
                    allPages.Add(New List(Of OrderItems)(currentPageItems))
                    currentPageItems.Clear()
                End If
            Next

            If currentPageItems.Count > 0 Then
                allPages.Add(currentPageItems)
            End If
        End Sub

        ' Helper method to create OrderItem (extract repeated code)
        Private Function CreateOrderItem(item As Dictionary(Of String, String)) As OrderItems
            Dim rate As Decimal = Decimal.Parse(item("Rate"))
            Dim rateFormatted As String = rate.ToString("N2")

            Dim linePrice As Decimal = Decimal.Parse(item("Amount"))
            Dim linePriceFormatted As String = linePrice.ToString("N2")

            Dim productImage As BitmapImage = Nothing
            If item.ContainsKey("ProductImageBase64") AndAlso Not String.IsNullOrEmpty(item("ProductImageBase64").ToString()) Then
                productImage = Base64ToBitmapImage(item("ProductImageBase64").ToString())
            Else
                productImage = GetProductImageFromDatabase(item("ProductName").ToString())
            End If

            Return New OrderItems With {
        .Quantity = item("Quantity"),
        .Description = item("ProductName"),
        .UnitPrice = $"₱ {rateFormatted}",
        .LinePrice = $"₱ {linePriceFormatted}",
        .ProductImage = productImage
    }
        End Function

        ' Load specific page
        Private Sub LoadPrintPage(pageIndex As Integer)
            If pageIndex < 0 OrElse pageIndex >= allPages.Count Then Return

            currentPageIndex = pageIndex
            itemDataSource.Clear()

            For Each item In allPages(pageIndex)
                itemDataSource.Add(item)
            Next

            dataGrid.ItemsSource = itemDataSource

            ' Update page info after loading
            UpdatePageInfo()
            UpdateNavigationButtons()
            UpdateTotalCostVisibility()
            UpdateServicesVisibility()
            UpdateWarrantyAndBottomSectionVisibility()
            UpdatePrintPageIndicator()
        End Sub

        Private Sub UpdatePageInfo()
            If txtPageInfo IsNot Nothing Then
                txtPageInfo.Text = $"Page {currentPageIndex + 1} of {allPages.Count}"
            End If
        End Sub

        ' Add method to enable/disable navigation buttons
        Private Sub UpdateNavigationButtons()
            Dim btnPrev = TryCast(Me.FindName("btnPrevPage"), Button)
            Dim btnNext = TryCast(Me.FindName("btnNextPage"), Button)

            If btnPrev IsNot Nothing Then
                btnPrev.IsEnabled = currentPageIndex > 0
            End If

            If btnNext IsNot Nothing Then
                btnNext.IsEnabled = currentPageIndex < allPages.Count - 1
            End If
        End Sub

        ' Add navigation button click handlers
        Private Sub PreviousPage_Click(sender As Object, e As RoutedEventArgs)
            If currentPageIndex > 0 Then
                LoadPrintPage(currentPageIndex - 1)
            End If
        End Sub

        Private Sub NextPage_Click(sender As Object, e As RoutedEventArgs)
            If currentPageIndex < allPages.Count - 1 Then
                LoadPrintPage(currentPageIndex + 1)
            End If
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
                Dim result As MessageBoxResult = MessageBox.Show("Do you want to save this as a PDF?", "Choose Output", MessageBoxButton.YesNoCancel, MessageBoxImage.Question)

                Dim docName As String = CEQuoteNumberCache
                Dim savedPath As String = SaveAsPDF(docName)

                If result = MessageBoxResult.Yes Then
                    If Not SavePdfPathToMongoDB(savedPath, CEQuoteNumberCache, CacheOnLoggedInName) Then Exit Sub
                    SaveToDb()
                ElseIf result = MessageBoxResult.No Then
                    PrintPhysically(docName)
                    If Not SavePdfPathToMongoDB(savedPath, CEQuoteNumberCache, CacheOnLoggedInName) Then Exit Sub
                    SaveToDb()
                Else
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
                MessageBox.Show("(Tips: You can go back to the newquote without losing data) Error saving PDF path to MongoDB: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
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
            Dim dlg As New Microsoft.Win32.SaveFileDialog() With {
        .FileName = docName & ".pdf",
        .Filter = "PDF Files (.pdf)|.pdf"
    }

            If dlg.ShowDialog() = True Then
                Try
                    Dim pdf As New PdfDocument()

                    For i As Integer = 0 To allPages.Count - 1
                        LoadPrintPage(i)

                        ' Force UI update
                        Application.Current.Dispatcher.Invoke(Sub()
                                                                  PrintPreview.UpdateLayout()
                                                              End Sub, System.Windows.Threading.DispatcherPriority.Render)

                        ' Increase delay to ensure rendering is complete
                        System.Threading.Thread.Sleep(300)

                        ' Create PDF page - FIXED: Add page to document first
                        Dim page As PdfPage = pdf.AddPage()

                        ' Set page size
                        Dim layoutWidth = PrintPreview.ActualWidth
                        Dim layoutHeight = PrintPreview.ActualHeight
                        page.Width = XUnit.FromInch(layoutWidth / 96)
                        page.Height = XUnit.FromInch(layoutHeight / 96)

                        ' Render to page
                        RenderToPdfPage(PrintPreview, page)
                    Next

                    pdf.Save(dlg.FileName)
                    MessageBox.Show($"Saved {allPages.Count} page(s) to: {dlg.FileName}")

                    ' Load first page back for display
                    LoadPrintPage(0)

                    Return dlg.FileName

                Catch ex As Exception
                    MessageBox.Show($"Error creating PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return Nothing
                End Try
            Else
                Return Nothing
            End If
        End Function

        ' Helper method to render element to PDF page
        Private Sub RenderToPdfPage(element As FrameworkElement, page As PdfPage)
            Try
                Dim dpi As Integer = 300
                Dim layoutWidth = element.ActualWidth
                Dim layoutHeight = element.ActualHeight

                If layoutWidth = 0 OrElse layoutHeight = 0 Then
                    Throw New InvalidOperationException("Element has invalid dimensions")
                End If

                Dim pixelWidth = CInt(layoutWidth * dpi / 96)
                Dim pixelHeight = CInt(layoutHeight * dpi / 96)

                Dim rtb As New RenderTargetBitmap(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32)
                element.Measure(New Size(layoutWidth, layoutHeight))
                element.Arrange(New Rect(0, 0, layoutWidth, layoutHeight))
                element.UpdateLayout()
                rtb.Render(element)

                Dim encoder As New PngBitmapEncoder()
                encoder.Frames.Add(BitmapFrame.Create(rtb))

                Using stream As New MemoryStream()
                    encoder.Save(stream)
                    stream.Position = 0

                    ' Now create graphics from the page that's already in the document
                    Using gfx As XGraphics = XGraphics.FromPdfPage(page)
                        Dim image = XImage.FromStream(stream)
                        gfx.DrawImage(image, 0, 0, page.Width, page.Height)
                    End Using
                End Using

            Catch ex As Exception
                Throw New Exception($"Error rendering to PDF page: {ex.Message}", ex)
            End Try
        End Sub

        Private Sub PrintPhysically(docName As String)
            Dim dlg As New PrintDialog()
            If dlg.ShowDialog() = True Then
                ' Print each page
                For i As Integer = 0 To allPages.Count - 1
                    LoadPrintPage(i)

                    Application.Current.Dispatcher.Invoke(Sub()
                                                              PrintPreview.UpdateLayout()
                                                          End Sub, System.Windows.Threading.DispatcherPriority.Render)

                    Dim originalParent = VisualTreeHelper.GetParent(PrintPreview)
                    Dim originalIndex As Integer = -1
                    Dim originalMargin = PrintPreview.Margin
                    Dim originalTransform = PrintPreview.LayoutTransform

                    If TypeOf originalParent Is Panel Then
                        Dim panel = CType(originalParent, Panel)
                        originalIndex = panel.Children.IndexOf(PrintPreview)
                        panel.Children.Remove(PrintPreview)
                    End If

                    PrintPreview.Margin = New Thickness(0)
                    PrintPreview.LayoutTransform = Transform.Identity
                    PrintPreview.UpdateLayout()
                    PrintPreview.Measure(New Size(Double.PositiveInfinity, Double.PositiveInfinity))
                    PrintPreview.Arrange(New Rect(PrintPreview.DesiredSize))
                    PrintPreview.UpdateLayout()

                    Dim A4Width As Double = 8.3 * 96
                    Dim A4Height As Double = 11.69 * 96
                    Dim scaleX = A4Width / PrintPreview.ActualWidth
                    Dim scaleY = A4Height / PrintPreview.ActualHeight
                    Dim scale = Math.Min(scaleX, scaleY)

                    Dim container As New Grid()
                    container.Width = A4Width
                    container.Height = A4Height
                    container.LayoutTransform = New ScaleTransform(scale, scale)
                    container.Children.Add(PrintPreview)

                    container.Measure(New Size(A4Width, A4Height))
                    container.Arrange(New Rect(New Point(0, 0), New Size(A4Width, A4Height)))
                    container.UpdateLayout()

                    Dim fixedPage As New FixedPage()
                    fixedPage.Width = A4Width
                    fixedPage.Height = A4Height
                    fixedPage.Children.Add(container)

                    Dim pageContent As New PageContent()
                    CType(pageContent, IAddChild).AddChild(fixedPage)

                    Dim fixedDoc As New FixedDocument()
                    fixedDoc.Pages.Add(pageContent)

                    dlg.PrintDocument(fixedDoc.DocumentPaginator, $"{docName} - Page {i + 1}")

                    container.Children.Clear()
                    PrintPreview.LayoutTransform = originalTransform
                    PrintPreview.Margin = originalMargin

                    If TypeOf originalParent Is Panel AndAlso originalIndex >= 0 Then
                        Dim panel = CType(originalParent, Panel)
                        panel.Children.Insert(originalIndex, PrintPreview)
                    End If
                Next

                MessageBox.Show($"Printed {allPages.Count} page(s) successfully!")
                LoadPrintPage(0)
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

        Private Function GetProductImageFromDatabase(productName As String) As BitmapImage
            Try
                Dim imageBase64 As String = GetProduct.GetProductImageBase64(productName)

                If Not String.IsNullOrEmpty(imageBase64) Then
                    Return Base64ToBitmapImage(imageBase64)
                End If

                Return Nothing

            Catch ex As Exception
                Debug.WriteLine($"Error loading product image for {productName}: {ex.Message}")
                Return Nothing
            End Try
        End Function

        Private Function Base64ToBitmapImage(base64String As String) As BitmapImage
            Try
                If base64String.Contains(",") Then
                    base64String = base64String.Split(","c)(1)
                End If

                Dim imageBytes As Byte() = Convert.FromBase64String(base64String)

                Using ms As New MemoryStream(imageBytes)
                    Dim bitmap As New BitmapImage()
                    bitmap.BeginInit()
                    bitmap.CacheOption = BitmapCacheOption.OnLoad
                    bitmap.StreamSource = ms
                    bitmap.EndInit()
                    bitmap.Freeze()
                    Return bitmap
                End Using

            Catch ex As Exception
                Debug.WriteLine($"Error converting Base64 to BitmapImage: {ex.Message}")
                Return Nothing
            End Try
        End Function

        Private Sub ProductImageControl_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
            Dim img As Image = TryCast(sender, Image)
            If img IsNot Nothing AndAlso img.Source IsNot Nothing Then
                Dim enlargeWindow As New Window()
                enlargeWindow.Title = "Product Image"
                enlargeWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen
                enlargeWindow.SizeToContent = SizeToContent.WidthAndHeight
                enlargeWindow.ResizeMode = ResizeMode.NoResize
                enlargeWindow.Background = Brushes.Black

                Dim enlargedImage As New Image()
                enlargedImage.Source = img.Source
                enlargedImage.MaxWidth = 800
                enlargedImage.MaxHeight = 600
                enlargedImage.Stretch = Stretch.Uniform
                enlargedImage.Margin = New Thickness(10)
                enlargedImage.Cursor = Cursors.Hand

                AddHandler enlargedImage.MouseLeftButtonDown, Sub()
                                                                  enlargeWindow.Close()
                                                              End Sub

                enlargeWindow.Content = enlargedImage
                enlargeWindow.ShowDialog()
            End If
        End Sub

        Private Sub UpdateServicesVisibility()
            Dim nothingToFollowSection = TryCast(Me.FindName("NothingToFollowSection"), StackPanel)
            Dim otherServicesSection = TryCast(Me.FindName("OtherServicesSection"), StackPanel)

            Dim isLastPage = (currentPageIndex = allPages.Count - 1)

            If nothingToFollowSection IsNot Nothing Then
                nothingToFollowSection.Visibility = If(isLastPage, Visibility.Visible, Visibility.Collapsed)
            End If

            If otherServicesSection IsNot Nothing Then
                otherServicesSection.Visibility = If(isLastPage, Visibility.Visible, Visibility.Collapsed)
            End If
        End Sub

        Private Sub UpdateTotalCostVisibility()
            Dim totalCostSection = TryCast(Me.FindName("TotalCostSection"), Border)

            ' Only show on the last page
            Dim isLastPage = (currentPageIndex = allPages.Count - 1)

            If totalCostSection IsNot Nothing Then
                totalCostSection.Visibility = If(isLastPage, Visibility.Visible, Visibility.Collapsed)
            End If
        End Sub

        Private Sub UpdateWarrantyAndBottomSectionVisibility()
            ' Find the warranty and bottom sections
            Dim warrantySection = TryCast(Me.FindName("WarrantySection"), StackPanel)
            Dim bottomSection = TryCast(Me.FindName("BottomSection"), StackPanel)

            ' Only show on the last page
            Dim isLastPage = (currentPageIndex = allPages.Count - 1)

            If warrantySection IsNot Nothing Then
                warrantySection.Visibility = If(isLastPage, Visibility.Visible, Visibility.Collapsed)
            End If

            If bottomSection IsNot Nothing Then
                bottomSection.Visibility = If(isLastPage, Visibility.Visible, Visibility.Collapsed)
            End If
        End Sub

        Private Sub UpdatePrintPageIndicator()
            Dim pageIndicator = TryCast(Me.FindName("PageIndicatorText"), TextBlock)

            If pageIndicator IsNot Nothing Then
                pageIndicator.Text = $"Page {currentPageIndex + 1} of {allPages.Count}"
            End If
        End Sub

    End Class
End Namespace