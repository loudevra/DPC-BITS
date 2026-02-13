Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text.Json
Imports System.Windows.Markup
Imports DocumentFormat.OpenXml.Bibliography
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Views.Sales.Quotes
Imports MaterialDesignThemes.Wpf
Imports MongoDB.Bson
Imports MongoDB.Driver
Imports MongoDB.Driver.GridFS
Imports Newtonsoft.Json
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf

Namespace DPC.Components.Forms
    Public Class PreviewPrintQuoteGovernment

#Region "1. Variables & Constants"
        ' Data Lists
        Private itemDataSource As New ObservableCollection(Of OrderItems)
        Private itemOrder As New List(Of Dictionary(Of String, String))
        Private allItems As New List(Of Dictionary(Of String, String))

        ' Pagination Data
        Private allPages As New List(Of List(Of Integer)) ' Stores lists of item indices per page
        Private currentPageIndex As Integer = 0
        Private totalPages As Integer = 1

        ' State Variables
        Private tempImagePath As String
        Private base64Image As String
        Private showProductImages As Boolean = True

        ' Grouping Class
        Private Class CategoryGroup
            Public CategoryName As String
            Public Items As New List(Of Dictionary(Of String, String))
            Public Subtotal As Decimal
        End Class
        Private _categoryGroups As New List(Of CategoryGroup)

        ' Layout Constants
        Private Const BaseItemHeight As Double = 55
        Private Const DescriptionLineHeight As Double = 15
        Private Const PaginationTriggerHeight As Double = 412
        Private Const PageMaxHeight As Double = 760
        Private Const ReservedSpaceForDescription As Double = 30
        Private Const CategoryHeaderHeight As Double = 40
        Private Const SubtotalRowHeight As Double = 40
        Private Const FooterSectionHeight As Double = 250
#End Region

#Region "2. Initialization & Loading"
        Public Sub New()
            InitializeComponent()

            showProductImages = CostEstimateDetails.CEShowProductImages

            If CostEstimateDetails.CEQuoteItemsCache Is Nothing Then
                MessageBox.Show("Quote items are not loaded.")
                Return
            End If

            ' 1. Load Data
            allItems = CostEstimateDetails.CEQuoteItemsCache

            ' 2. Calculate Layout
            SplitItemsIntoPagesByHeight()

            ' 3. Load UI Fields
            LoadDataFields()

            ' 4. Render First Page
            LoadPrintPage(0)

            UpdateNavigationButtons()
            UpdatePageInfo()
        End Sub

        Private Sub LoadDataFields()
            ' Prices
            Dim valInstall As Decimal = 0
            Decimal.TryParse(CostEstimateDetails.CEInstallation, valInstall)
            Installation.Text = "₱ " & valInstall.ToString("N2")

            Dim valDeliv As Decimal = 0
            Decimal.TryParse(CostEstimateDetails.CEDeliveryCost, valDeliv)
            Delivery.Text = "₱ " & valDeliv.ToString("N2")

            ' Text Fields
            QuoteNumber.Text = CostEstimateDetails.CEQuoteNumberCache
            QuoteDate.Text = CostEstimateDetails.CEQuoteDateCache
            QuoteValidityDate.Text = CostEstimateDetails.CEValidUntilDate
            Subtotal.Text = CostEstimateDetails.CESubTotalCache
            TotalCost.Text = CostEstimateDetails.CEGrandTotalCost
            VAT12.Text = CostEstimateDetails.CETotalTaxValueCache

            noteBox.Text = CostEstimateDetails.CEnoteTxt
            remarksBox.Text = CostEstimateDetails.CEremarksTxt

            ' Terms
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

            If CostEstimateDetails.CEisCustomTerm Then
                cmbTerms.Text = CostEstimateDetails.CEpaymentTerms
                cmbTerms.Foreground = Brushes.White
            Else
                cmbTerms.Text = CostEstimateDetails.CEpaymentTerms
            End If

            ' Subject
            Dim subjectBlock = TryCast(Me.FindName("Subject"), TextBlock)
            If subjectBlock IsNot Nothing Then
                subjectBlock.Text = If(String.IsNullOrWhiteSpace(CostEstimateDetails.CESubject), "Subject:", "Subject: " & CostEstimateDetails.CESubject)
            End If

            ' Tax Visibility
            If CEtaxSelection Then
                VatText.Visibility = Visibility.Collapsed
                VatValue.Visibility = Visibility.Collapsed
            Else
                VatText.Visibility = Visibility.Visible
                VatValue.Visibility = Visibility.Visible
            End If

            ' Images
            base64Image = CostEstimateDetails.CEImageCache
            tempImagePath = CostEstimateDetails.CEPathCache
            DisplaySignaturePreview()

            ' Header Info
            PopulateHeaderInfo()
        End Sub

        Private Sub PopulateHeaderInfo()
            Try
                Dim clientBlock = TryCast(Me.FindName("SubmittedToClient"), TextBlock)
                Dim addressBlock = TryCast(Me.FindName("SubmittedToAddress"), TextBlock)
                Dim emailBlock = TryCast(Me.FindName("SubmittedToEmail"), TextBlock)
                Dim projectBlock = TryCast(Me.FindName("SubmittedToProjectID"), TextBlock)

                If clientBlock IsNot Nothing Then
                    clientBlock.Text = If(Not String.IsNullOrWhiteSpace(CostEstimateDetails.CECompanyName), CostEstimateDetails.CECompanyName, CostEstimateDetails.CEClientName)
                End If

                If addressBlock IsNot Nothing Then
                    Dim parts As New List(Of String)
                    If Not String.IsNullOrWhiteSpace(CostEstimateDetails.CEAddress) Then parts.Add(CostEstimateDetails.CEAddress)
                    If Not String.IsNullOrWhiteSpace(CostEstimateDetails.CECity) Then parts.Add(CostEstimateDetails.CECity)
                    If Not String.IsNullOrWhiteSpace(CostEstimateDetails.CERegion) Then parts.Add(CostEstimateDetails.CERegion)
                    If Not String.IsNullOrWhiteSpace(CostEstimateDetails.CECountry) Then parts.Add(CostEstimateDetails.CECountry)
                    addressBlock.Text = String.Join(", ", parts)
                End If

                If emailBlock IsNot Nothing Then emailBlock.Text = CostEstimateDetails.CEEmail
                If projectBlock IsNot Nothing Then projectBlock.Text = CostEstimateDetails.CEProjectID
            Catch ex As Exception
                Debug.WriteLine($"Header Error: {ex.Message}")
            End Try
        End Sub
#End Region

#Region "3. Pagination Logic"
        Private Sub SplitItemsIntoPagesByHeight()
            allPages.Clear()

            If allItems Is Nothing OrElse allItems.Count = 0 Then
                totalPages = 1
                Return
            End If

            Dim totalHeight As Double = 0

            ' Quick check for single page
            For Each item In allItems
                totalHeight += CalculateItemHeight(item)
            Next

            If totalHeight <= PaginationTriggerHeight Then
                Dim singlePage As New List(Of Integer)
                For i As Integer = 0 To allItems.Count - 1
                    singlePage.Add(i)
                Next
                allPages.Add(singlePage)
                totalPages = 1
                Return
            End If

            ' Calculation for multiple pages
            Dim currentPageIndices As New List(Of Integer)
            Dim currentPageHeight As Double = 0

            For i As Integer = 0 To allItems.Count - 1
                Dim itemHeight As Double = CalculateItemHeight(i) ' Overload to use index/dictionary

                If currentPageHeight + itemHeight > PageMaxHeight Then
                    ' Page Full
                    allPages.Add(New List(Of Integer)(currentPageIndices))
                    currentPageIndices.Clear()
                    currentPageHeight = itemHeight
                    currentPageIndices.Add(i)
                Else
                    currentPageHeight += itemHeight
                    currentPageIndices.Add(i)
                End If
            Next

            ' Add final items
            If currentPageIndices.Count > 0 Then
                allPages.Add(currentPageIndices)

                ' Check if footer fits
                If currentPageHeight > PaginationTriggerHeight Then
                    allPages.Add(New List(Of Integer)) ' Empty page for footer
                End If
            End If

            totalPages = allPages.Count
        End Sub

        ' Overload for direct Dictionary access
        Private Function CalculateItemHeight(item As Dictionary(Of String, String)) As Double
            Dim baseHeight As Double = BaseItemHeight
            If item.ContainsKey("Description") AndAlso Not String.IsNullOrWhiteSpace(item("Description")) Then
                Dim txt As String = item("Description")
                Dim estLines As Integer = Math.Ceiling(txt.Length / 50.0)
                baseHeight += (estLines * DescriptionLineHeight)
                baseHeight += ReservedSpaceForDescription
            End If
            Return baseHeight
        End Function

        ' Overload for Index access (helper)
        Private Function CalculateItemHeight(index As Integer) As Double
            Return CalculateItemHeight(allItems(index))
        End Function
#End Region

#Region "4. Rendering & Display"
        Private Sub LoadPrintPage(pageIndex As Integer)
            If pageIndex < 0 OrElse pageIndex >= allPages.Count Then Return

            currentPageIndex = pageIndex
            itemDataSource.Clear()

            ' 1. Group data (Raw)
            GroupItemsByCategory()

            ' 2. Filter data for this page and Add to View
            LoadAllCategoriesWithSubtotals(allPages(pageIndex))

            dataGrid.ItemsSource = itemDataSource

            ' 3. UI Updates
            UpdatePageInfo()
            UpdateNavigationButtons()
            UpdatePrintPageIndicator()

            ' 4. Visibility Logic
            Dim isLastPage As Boolean = (currentPageIndex = totalPages - 1)
            SetFooterVisibility(isLastPage)
            UpdateImageColumnVisibility()
        End Sub

        Private Sub SetFooterVisibility(isVisible As Boolean)
            Dim vis = If(isVisible, Visibility.Visible, Visibility.Collapsed)
            If NothingToFollowSection IsNot Nothing Then NothingToFollowSection.Visibility = vis
            If OtherServicesSection IsNot Nothing Then OtherServicesSection.Visibility = vis
            If WarrantySection IsNot Nothing Then WarrantySection.Visibility = vis
            If BottomSection IsNot Nothing Then BottomSection.Visibility = vis
        End Sub

        Private Sub GroupItemsByCategory()
            _categoryGroups.Clear()
            If allItems Is Nothing Then Return

            Dim currentGroup As CategoryGroup = Nothing
            Dim lastCat As String = ""

            For Each item In allItems
                Dim catName As String = ""
                If item.ContainsKey("Category") Then catName = item("Category").ToString().Trim()

                If catName <> lastCat OrElse currentGroup Is Nothing Then
                    currentGroup = New CategoryGroup With {.CategoryName = catName}
                    _categoryGroups.Add(currentGroup)
                    lastCat = catName
                End If

                currentGroup.Items.Add(item)

                Dim amt As Decimal = 0
                If item.ContainsKey("Amount") Then
                    Decimal.TryParse(item("Amount").Replace("₱", "").Replace(",", "").Trim(), amt)
                End If
                currentGroup.Subtotal += amt
            Next
        End Sub

        Private Sub LoadAllCategoriesWithSubtotals(pageIndices As List(Of Integer))
            Dim validIndices As New HashSet(Of Integer)(pageIndices)
            Dim globalIdx As Integer = 0

            For Each group In _categoryGroups
                Dim itemsOnThisPage As New List(Of Dictionary(Of String, String))

                For Each item In group.Items
                    If validIndices.Contains(globalIdx) Then
                        itemsOnThisPage.Add(item)
                    End If
                    globalIdx += 1
                Next

                If itemsOnThisPage.Count > 0 Then
                    If Not String.IsNullOrWhiteSpace(group.CategoryName) Then
                        AddCategoryHeaderRow(group.CategoryName)
                    End If

                    For Each item In itemsOnThisPage
                        itemDataSource.Add(CreateOrderItem(item))
                    Next

                    Dim lastItemOfGroup = group.Items.Last()
                    If itemsOnThisPage.Contains(lastItemOfGroup) Then
                        AddCategorySubtotalRow(group.Subtotal)
                    End If
                End If
            Next
        End Sub

        Private Sub AddCategoryHeaderRow(name As String)
            itemDataSource.Add(New OrderItems With {
                .Description = name.ToUpper(),
                .IsCategoryHeader = True,
                .ProductDescriptionVisibility = Visibility.Collapsed
            })
        End Sub

        Private Sub AddCategorySubtotalRow(val As Decimal)
            itemDataSource.Add(New OrderItems With {
                .UnitPrice = "Subtotal:",
                .LinePrice = $"₱ {val:N2}",
                .IsSubtotalRow = True,
                .ProductDescriptionVisibility = Visibility.Collapsed
            })
        End Sub

        Private Function CreateOrderItem(item As Dictionary(Of String, String)) As OrderItems
            Dim rate As Decimal = 0
            Dim line As Decimal = 0
            Decimal.TryParse(item("Rate").Replace("₱", "").Replace(",", "").Trim(), rate)
            Decimal.TryParse(item("Amount").Replace("₱", "").Replace(",", "").Trim(), line)

            Dim img As BitmapImage = Nothing
            If showProductImages Then
                If item.ContainsKey("ProductImageBase64") AndAlso Not String.IsNullOrEmpty(item("ProductImageBase64").ToString()) Then
                    img = Base64ToBitmapImage(item("ProductImageBase64").ToString())
                Else
                    img = GetProductImageFromDatabase(item("ProductName").ToString())
                End If
            End If

            Dim desc As String = ""
            Dim vis As Visibility = Visibility.Collapsed
            If item.ContainsKey("Description") AndAlso Not String.IsNullOrWhiteSpace(item("Description")) Then
                desc = item("Description")
                vis = Visibility.Visible
            End If

            Return New OrderItems With {
                .Quantity = item("Quantity"),
                .Description = item("ProductName"),
                .ProductDescription = desc,
                .ProductDescriptionVisibility = vis,
                .UnitPrice = $"₱ {rate:N2}",
                .LinePrice = $"₱ {line:N2}",
                .ProductImage = img
            }
        End Function
#End Region

#Region "5. Navigation & UI Interaction"
        Private Sub PreviousPage_Click(sender As Object, e As RoutedEventArgs)
            If currentPageIndex > 0 Then LoadPrintPage(currentPageIndex - 1)
        End Sub

        Private Sub NextPage_Click(sender As Object, e As RoutedEventArgs)
            If currentPageIndex < totalPages - 1 Then LoadPrintPage(currentPageIndex + 1)
        End Sub

        Private Sub CancelButton(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("costestimategovernment", Me)
        End Sub

        Private Sub UpdatePageInfo()
            If txtPageInfo IsNot Nothing Then
                txtPageInfo.Text = $"Page {currentPageIndex + 1} of {totalPages}"
            End If
        End Sub

        Private Sub UpdateNavigationButtons()
            If btnPrevPage IsNot Nothing Then btnPrevPage.IsEnabled = (currentPageIndex > 0)
            If btnNextPage IsNot Nothing Then btnNextPage.IsEnabled = (currentPageIndex < totalPages - 1)
        End Sub

        Private Sub UpdatePrintPageIndicator()
            If PageIndicatorText IsNot Nothing Then
                PageIndicatorText.Text = $"Page {currentPageIndex + 1} of {totalPages}"
            End If
        End Sub

        Private Sub UpdateImageColumnVisibility()
            For Each col In dataGrid.Columns
                If TypeOf col Is DataGridTemplateColumn AndAlso col.Header?.ToString() = "Image" Then
                    col.Visibility = If(showProductImages, Visibility.Visible, Visibility.Collapsed)
                    Exit For
                End If
            Next
        End Sub
#End Region

#Region "6. Printing & PDF"
        Private Sub SavePrint(sender As Object, e As RoutedEventArgs)
            Try
                Dim res As MessageBoxResult = MessageBox.Show("Do you want to save this as a PDF?", "Output", MessageBoxButton.YesNoCancel)
                Dim docName As String = CEQuoteNumberCache
                Dim path As String = SaveAsPDF(docName)

                If res = MessageBoxResult.Yes Then
                    If Not SavePdfPathToMongoDB(path, CEQuoteNumberCache, CacheOnLoggedInName) Then Exit Sub
                    SaveToDb()
                ElseIf res = MessageBoxResult.No Then
                    PrintPhysically(docName)
                    If Not SavePdfPathToMongoDB(path, CEQuoteNumberCache, CacheOnLoggedInName) Then Exit Sub
                    SaveToDb()
                End If
            Catch ex As Exception
                MessageBox.Show("Print Error: " & ex.Message)
            End Try
        End Sub

        Private Sub SaveDb_Click(sender As Object, e As RoutedEventArgs)
            Try
                Dim path As String = SaveAsPDF(CEQuoteNumberCache)
                If Not String.IsNullOrEmpty(path) Then
                    If Not SavePdfPathToMongoDB(path, CEQuoteNumberCache, CacheOnLoggedInName) Then Exit Sub
                    SaveToDb()
                End If
            Catch ex As Exception
                MessageBox.Show("Save Error: " & ex.Message)
            End Try
        End Sub

        Private Function SaveAsPDF(docName As String) As String
            Dim dlg As New Microsoft.Win32.SaveFileDialog() With {.FileName = docName & ".pdf", .Filter = "PDF Files|*.pdf"}
            If dlg.ShowDialog() = True Then
                Try
                    Dim pdf As New PdfDocument()
                    For i As Integer = 0 To totalPages - 1
                        LoadPrintPage(i)
                        Application.Current.Dispatcher.Invoke(Sub() PrintPreview.UpdateLayout(), System.Windows.Threading.DispatcherPriority.Render)
                        System.Threading.Thread.Sleep(300)

                        Dim page As PdfPage = pdf.AddPage()
                        Dim layoutWidth = PrintPreview.ActualWidth
                        Dim layoutHeight = PrintPreview.ActualHeight

                        page.Width = XUnit.FromInch(layoutWidth / 96)
                        page.Height = XUnit.FromInch(layoutHeight / 96)
                        RenderToPdfPage(PrintPreview, page)
                    Next
                    pdf.Save(dlg.FileName)
                    LoadPrintPage(0) ' Reset
                    Return dlg.FileName
                Catch ex As Exception
                    MessageBox.Show("PDF Error: " & ex.Message)
                    Return Nothing
                End Try
            End If
            Return Nothing
        End Function

        Private Sub RenderToPdfPage(elem As FrameworkElement, page As PdfPage)
            Dim dpi As Integer = 300
            Dim w = elem.ActualWidth
            Dim h = elem.ActualHeight
            Dim pxW = CInt(w * dpi / 96)
            Dim pxH = CInt(h * dpi / 96)

            Dim rtb As New RenderTargetBitmap(pxW, pxH, dpi, dpi, PixelFormats.Pbgra32)

            elem.Measure(New Size(w, h))
            elem.Arrange(New Rect(0, 0, w, h))
            elem.UpdateLayout()
            rtb.Render(elem)

            Dim enc As New PngBitmapEncoder()
            enc.Frames.Add(BitmapFrame.Create(rtb))

            Using ms As New MemoryStream()
                enc.Save(ms)
                ms.Position = 0
                Using gfx As XGraphics = XGraphics.FromPdfPage(page)
                    Dim img = XImage.FromStream(ms)
                    gfx.DrawImage(img, 0, 0, page.Width.Point, page.Height.Point)
                End Using
            End Using
        End Sub

        Private Sub PrintPhysically(docName As String)
            Dim dlg As New PrintDialog()
            If dlg.ShowDialog() = True Then
                For i As Integer = 0 To totalPages - 1
                    LoadPrintPage(i)
                    Application.Current.Dispatcher.Invoke(Sub() PrintPreview.UpdateLayout(), System.Windows.Threading.DispatcherPriority.Render)

                    ' Create Fixed Document Logic
                    Dim width As Double = 8.3 * 96
                    Dim height As Double = 11.69 * 96

                    ' Create a temporary grid for printing to avoid detaching the visual parent
                    Dim container As New Grid With {.Width = width, .Height = height}

                    ' Transform visual
                    Dim scale = Math.Min(width / PrintPreview.ActualWidth, height / PrintPreview.ActualHeight)
                    Dim brush As New VisualBrush(PrintPreview)
                    Dim rect As New Rectangle With {
                        .Width = PrintPreview.ActualWidth,
                        .Height = PrintPreview.ActualHeight,
                        .Fill = brush,
                        .LayoutTransform = New ScaleTransform(scale, scale)
                    }
                    container.Children.Add(rect)

                    container.Measure(New Size(width, height))
                    container.Arrange(New Rect(0, 0, width, height))

                    dlg.PrintVisual(container, $"{docName} - Page {i + 1}")
                Next
                LoadPrintPage(0)
            End If
        End Sub

        Private Sub SaveToDb()
            Dim json As String = Newtonsoft.Json.JsonConvert.SerializeObject(CEQuoteItemsCache)
            If QuotesController.InsertQuote(CEQuoteNumberCache, CEReferenceNumber, CEQuoteDateCache,
                                            CEQuoteValidityDateCache, CETaxProperty, CEDiscountProperty,
                                            CEClientIDCache, CEClientName, CEWarehouseIDCache, CEWarehouseNameCache,
                                            json, CEQuoteNumberCache, CETotalTaxValueCache, CETotalDiscountValueCache,
                                            CETotalAmountCache, CacheOnLoggedInName, CEApproved, CEpaymentTerms) Then

                Dim f As New NewQuoteGovernment()
                f.ClearAllFields()
                CostEstimateDetails.ClearAllCECache()
                ViewLoader.DynamicView.NavigateToView("salesquotegovernment", Me)
            Else
                MessageBox.Show("Failed to submit quote.")
            End If
        End Sub

        Private Shared Function SavePdfPathToMongoDB(path As String, qNum As String, user As String) As Boolean
            Try
                Dim fs As GridFSBucket = SplashScreen.GetGridFSConnection()
                Using s As New FileStream(path, FileMode.Open, FileAccess.Read)
                    Dim opts As New GridFSUploadOptions() With {
                        .Metadata = New BsonDocument From {
                            {"uploadedBy", user}, {"uploadedAt", BsonDateTime.Create(DateTime.UtcNow)},
                            {"source", "cost-estimate/quote"}, {"quoteNumber", qNum}, {"pdfFilePath", path}
                        }
                    }
                    fs.UploadFromStream(System.IO.Path.GetFileName(path), s, opts)
                End Using
                Return True
            Catch ex As Exception
                MessageBox.Show("Database Error: " & ex.Message)
                Return False
            End Try
        End Function
#End Region

#Region "7. Utilities"
        Public Sub DisplaySignaturePreview()
            Dim grid As New Grid()
            grid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
            grid.RowDefinitions.Add(New RowDefinition With {.Height = GridLength.Auto})

            If CostEstimateDetails.CEsignature Then
                Try
                    If File.Exists(tempImagePath) Then File.Delete(tempImagePath)
                    Base64Utility.DecodeBase64ToFile(base64Image, tempImagePath)

                    Dim bmp As New BitmapImage()
                    Using ms As New FileStream(tempImagePath, FileMode.Open, FileAccess.Read)
                        bmp.BeginInit()
                        bmp.CacheOption = BitmapCacheOption.OnLoad
                        bmp.StreamSource = ms
                        bmp.EndInit()
                    End Using
                    bmp.Freeze()

                    Dim img As New Image With {.Source = bmp, .MaxHeight = 70, .HorizontalAlignment = HorizontalAlignment.Center}
                    Grid.SetRow(img, 0)
                    grid.Children.Add(img)
                Catch
                End Try
            End If

            Dim warn = CreateSignatureWarningText()
            Grid.SetRow(warn, 1)
            grid.Children.Add(warn)
            BrowseFile.Child = grid
        End Sub

        Public Function CreateSignatureWarningText() As TextBlock
            Return New TextBlock With {
                .Text = "By signing the document, you confirm that the billing amount is" & vbLf &
                        "accurate and corresponds to your additional terms or services.",
                .FontWeight = FontWeights.Bold,
                .FontFamily = New FontFamily("Lexend"),
                .FontSize = 6.5,
                .Foreground = Brushes.Red,
                .TextWrapping = TextWrapping.Wrap,
                .HorizontalAlignment = HorizontalAlignment.Center,
                .MaxWidth = 200
            }
        End Function

        Private Function GetProductImageFromDatabase(name As String) As BitmapImage
            Try
                Dim b64 = GetProduct.GetProductImageBase64(name)
                Return If(String.IsNullOrEmpty(b64), Nothing, Base64ToBitmapImage(b64))
            Catch
                Return Nothing
            End Try
        End Function

        Private Function Base64ToBitmapImage(b64 As String) As BitmapImage
            Try
                If b64.Contains(",") Then b64 = b64.Split(","c)(1)
                Dim bytes = Convert.FromBase64String(b64)
                Using ms As New MemoryStream(bytes)
                    Dim bmp As New BitmapImage()
                    bmp.BeginInit()
                    bmp.StreamSource = ms
                    bmp.CacheOption = BitmapCacheOption.OnLoad
                    bmp.EndInit()
                    bmp.Freeze()
                    Return bmp
                End Using
            Catch
                Return Nothing
            End Try
        End Function
#End Region

    End Class
End Namespace