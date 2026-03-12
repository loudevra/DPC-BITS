Imports System.Collections.ObjectModel
Imports System.Globalization
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
    Public Class UniversalPrintablePreviewDocument

#Region "1. Variables & Constants"
        Private itemDataSource As New ObservableCollection(Of OrderItems)
        Private allItems As ObservableCollection(Of OrderItems)

        ' Pagination State
        Private currentPageIndex As Integer = 0
        Private totalPages As Integer = 1
        Private _paginatedPages As New List(Of List(Of Integer))

        ' State Variables
        Private tempImagePath As String
        Private base64Image As String
        Private showProductImages As Boolean = True
        Private Shared element As FrameworkElement

        ' Layout Constants - CALIBRATED FOR LEGAL SIZE
        Private Const PageMaxHeight As Double = 950
        Private Const FooterSectionHeight As Double = 250
        Private Const BaseItemHeight As Double = 55
        Private Const DescriptionLineHeight As Double = 15
        Private Const ReservedSpaceForDescription As Double = 30
        Private categorizedItems As List(Of Dictionary(Of String, Object))
        Private Const CategoryHeaderHeight As Double = 40
        Public Property IsEditMode As Boolean = False
#End Region

#Region "2. Initialization & Loading"
        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub PrintablePreview_Loaded(sender As Object, e As RoutedEventArgs)
            DetectDocumentMode()

            txtPageInfo = TryCast(Me.FindName("txtPageInfo"), TextBlock)

            If PreviewState.CurrentPreview Is Nothing OrElse PreviewState.CurrentPreview.Items.Count = 0 Then
                MessageBox.Show("Preview data is missing.")
                Return
            End If

            LoadTextFields()

            showProductImages = PreviewState.CurrentPreview.ShowImages
            'UpdateToggleButtonState()
            RecalculatePagination()
            LoadPage(0)

            If Not String.IsNullOrWhiteSpace(PreviewState.CurrentPreview.SignatureImageBase64) Then
                base64Image = PreviewState.CurrentPreview.SignatureImageBase64
                DisplayUploadedImage()
            End If
        End Sub

        Private Sub LoadTextFields()
            Dim data = PreviewState.CurrentPreview


            Installation.Text = data.InstallationFee
            Delivery.Text = data.DeliveryFee

            ' Load Text Fields
            lblPageTitle.Text = data.DocumentTitle
            'lblBackButton.Text = data.BackButtonLabel
            DocumentNumber.Text = data.DocumentNumber
            DocumentDate.Text = data.DocumentDate
            DocumentValidityDate.Text = data.DocumentValidity
            Subtotal.Text = data.Subtotal
            lblVatValue.Text = data.VatValue
            lblVat.Text = data.VatLabel
            TotalCost.Text = data.TotalCost


            noteBox.Text = data.Notes
            remarksBox.Text = data.Remarks

            SalesRep.Text = CacheOnLoggedInName
            lblApproved.Text = data.ApprovedBy
            lblSubtotal.Text = data.SubtotalLabel
            lblDeliveryMobilization.Text = data.DeliveryMobilizationLabel
            lblTermsDisplay.Text = data.PaymentTerms

            ' Header Details
            PopulateHeaderDetails()
        End Sub

        Private Sub PopulateHeaderDetails()
            Try
                Dim data = PreviewState.CurrentPreview

                Dim clientBlock = TryCast(Me.FindName("SubmittedToClient"), TextBlock)
                Dim addressBlock = TryCast(Me.FindName("SubmittedToAddress"), TextBlock)
                Dim emailBlock = TryCast(Me.FindName("SubmittedToEmail"), TextBlock)
                Dim contactBlock = TryCast(Me.FindName("SubmittedToNumber"), TextBlock)

                If clientBlock IsNot Nothing Then
                    clientBlock.Text = data.ClientName
                End If

                If addressBlock IsNot Nothing Then
                    addressBlock.Text = data.ClientAddress
                End If

                If emailBlock IsNot Nothing Then
                    emailBlock.Text = data.ClientEmail
                End If

                If contactBlock IsNot Nothing Then
                    contactBlock.Text = data.ClientContact
                End If

            Catch ex As Exception
                Debug.WriteLine("Header Error: " & ex.Message)
            End Try
        End Sub
#End Region

#Region "3. The Pagination Engine (Core Logic)"
        Private Sub RecalculatePagination()
            Dim data = PreviewState.CurrentPreview
            _paginatedPages.Clear()

            allItems = data.Items

            If allItems Is Nothing OrElse allItems.Count = 0 Then
                _paginatedPages.Add(New List(Of Integer))
                totalPages = 1
                Return
            End If

            Dim pageIndices As New List(Of Integer)
            Dim currentHeight As Double = 0

            For i As Integer = 0 To allItems.Count - 1
                Dim h As Double = CalculateItemHeight(allItems(i))

                If currentHeight + h > PageMaxHeight Then
                    _paginatedPages.Add(New List(Of Integer)(pageIndices))
                    pageIndices.Clear()
                    currentHeight = 0
                End If

                currentHeight += h
                pageIndices.Add(i)
            Next

            If pageIndices.Count > 0 Then
                _paginatedPages.Add(pageIndices)
            End If

            If currentHeight > (PageMaxHeight - FooterSectionHeight) Then
                _paginatedPages.Add(New List(Of Integer))
            End If

            totalPages = _paginatedPages.Count
        End Sub

        Private Function CalculateItemHeight(item As OrderItems) As Double
            If item.IsHeaderRow Then
                Return CategoryHeaderHeight
            End If

            Dim h As Double = BaseItemHeight

            If Not String.IsNullOrWhiteSpace(item.ProductDescription) Then
                Dim text As String = item.ProductDescription.Trim()
                Dim ft As New FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, New Typeface("Lexend"), 12, Brushes.Black, 1.0)
                ft.MaxTextWidth = 400
                h += ft.Height + ReservedSpaceForDescription
            End If
            Return h
        End Function

        Private Sub LoadPage(index As Integer)
            If index < 0 OrElse index >= _paginatedPages.Count Then Return

            currentPageIndex = index
            itemDataSource.Clear()

            Dim indices = _paginatedPages(index)

            For Each idx In indices
                itemDataSource.Add(allItems(idx))
            Next

            dataGrid.ItemsSource = itemDataSource

            Dim isLastPage As Boolean = (currentPageIndex = totalPages - 1)
            SetFooterVisibility(isLastPage)

            UpdatePageInfo()
            UpdateNavigationButtons()
            UpdateImageColumnVisibility()
            UpdatePrintPageIndicator()
        End Sub

        Private Sub SetFooterVisibility(isVisible As Boolean)
            Dim vis = If(isVisible, Visibility.Visible, Visibility.Collapsed)

            If NothingToFollowSection IsNot Nothing Then NothingToFollowSection.Visibility = vis
            If OtherServicesSection IsNot Nothing Then OtherServicesSection.Visibility = vis
            If TotalCostSection IsNot Nothing Then TotalCostSection.Visibility = vis
            If WarrantySection IsNot Nothing Then WarrantySection.Visibility = vis
            If BottomSection IsNot Nothing Then BottomSection.Visibility = vis
        End Sub
#End Region

#Region "4. Item & Group Processing"
        Private Function CreateVisualItem(item As Dictionary(Of String, String)) As OrderItems
            Dim isHeader As Boolean = item.ContainsKey("IsCategoryHeader") AndAlso item("IsCategoryHeader") = "True"

            Dim rate As Decimal = 0
            Dim linePrice As Decimal = 0
            Dim qty As String = ""
            Dim prodName As String = If(item.ContainsKey("ProductName"), item("ProductName"), "")
            Dim desc As String = ""
            Dim vis As Visibility = Visibility.Collapsed
            Dim img As BitmapImage = Nothing

            If Not isHeader Then
                If item.ContainsKey("Rate") Then
                    Decimal.TryParse(item("Rate").Replace("₱", "").Replace(",", "").Trim(), rate)
                End If

                If item.ContainsKey("Amount") Then
                    Decimal.TryParse(item("Amount").Replace("₱", "").Replace(",", "").Trim(), linePrice)
                End If

                qty = If(item.ContainsKey("Quantity"), item("Quantity"), "0")

                If item.ContainsKey("Description") AndAlso Not String.IsNullOrWhiteSpace(item("Description")) Then
                    desc = item("Description").Trim()
                    vis = Visibility.Visible
                End If

                If showProductImages Then
                    If item.ContainsKey("ProductImageBase64") AndAlso Not String.IsNullOrEmpty(item("ProductImageBase64")) Then
                        img = Base64ToBitmapImage(item("ProductImageBase64"))
                    Else
                        img = GetProductImageFromDatabase(prodName)
                    End If
                End If
            End If

            Return New OrderItems With {
            .Quantity = qty,
            .Description = prodName,
            .ProductDescription = desc,
            .ProductDescriptionVisibility = vis,
            .UnitPrice = If(isHeader, "", $"₱ {rate:N2}"),
            .LinePrice = If(isHeader, "", $"₱ {linePrice:N2}"),
            .ProductImage = img,
            .IsHeaderRow = isHeader
        }
        End Function
#End Region

#Region "5. Navigation & UI Interaction"
        Private Sub PreviousPage_Click(sender As Object, e As RoutedEventArgs)
            If currentPageIndex > 0 Then LoadPage(currentPageIndex - 1)
        End Sub

        Private Sub NextPage_Click(sender As Object, e As RoutedEventArgs)
            If currentPageIndex < totalPages - 1 Then LoadPage(currentPageIndex + 1)
        End Sub

        Private Sub CancelButton(sender As Object, e As RoutedEventArgs)
            Dim data = PreviewState.CurrentPreview
            PreviewState.ResetPreview()

            itemDataSource.Clear()
            If allItems IsNot Nothing Then allItems.Clear()
            _paginatedPages.Clear()

            currentPageIndex = 0
            totalPages = 1
            base64Image = ""

            ViewLoader.DynamicView.NavigateToCachedView($"{data.CreatePath}", Me)
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
            For Each col In DataGrid.Columns
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
                Dim data = PreviewState.CurrentPreview
                Dim docName As String = data.DocumentNumber

                Dim res As MessageBoxResult = MessageBox.Show("Do you want to save this as a PDF?", "Output", MessageBoxButton.YesNoCancel)
                Dim path As String = SaveAsPDF(docName)

                If res = MessageBoxResult.Yes Then
                    If Not SavePdfPathToMongoDB(path, data.DocumentNumber, CacheOnLoggedInName) Then Exit Sub
                    SaveToDb()
                ElseIf res = MessageBoxResult.No Then
                    PrintPhysically(docName)
                    If Not SavePdfPathToMongoDB(path, data.DocumentNumber, CacheOnLoggedInName) Then Exit Sub
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
                        LoadPage(i)
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
                    LoadPage(0) ' Reset
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
                    LoadPage(i)
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
                LoadPage(0)
            End If
        End Sub

        Private Sub SaveToDb()
            Dim data = PreviewState.CurrentPreview

            Dim json As String = JsonConvert.SerializeObject(data.Items)

            If QuotesController.InsertQuote(data.DocumentNumber, "", data.DocumentDate,
                                    data.DocumentValidity, "", "",
                                    0, data.ClientName, 0, "",
                                    json, data.Notes, data.VatValue, "",
                                    data.TotalCost, data.PreparedBy, data.ApprovedBy, data.PaymentTerms) Then

                PreviewState.ResetPreview()
                ViewLoader.DynamicView.NavigateToView("salesnewquote", Me)
            Else
                MessageBox.Show("Failed to submit document.")
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
        Private Sub DetectDocumentMode()
            IsEditMode = PreviewState.CurrentPreview.IsEditMode
        End Sub

        Public Sub DisplaySignaturePreview()
            Dim data = PreviewState.CurrentPreview
            Dim grid As New Grid()
            grid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
            grid.RowDefinitions.Add(New RowDefinition With {.Height = GridLength.Auto})

            If Not String.IsNullOrEmpty(data.SignatureImageBase64) Then
                Try
                    Dim bmp = Base64ToBitmapImage(data.SignatureImageBase64)
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

        Private Sub DisplayUploadedImage()
            Try
                tempImagePath = Path.Combine(Path.GetTempPath(), "decoded_image.png")
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

                BrowseFile.Child = New Image With {.Source = bmp, .MaxHeight = 70}
            Catch
            End Try
        End Sub
#End Region

    End Class
End Namespace