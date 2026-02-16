Imports System.Collections.ObjectModel
Imports System.Globalization
Imports System.IO
Imports System.Windows
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports MaterialDesignThemes.Wpf
Imports Microsoft.Win32
Imports SkiaSharp.Views.WPF

Namespace DPC.Views.Sales.Quotes
    Public Class CostEstimate

#Region "1. Variables & Constants"
        Private itemDataSource As New ObservableCollection(Of OrderItems)
        Private allItems As List(Of Dictionary(Of String, String))

        ' Pagination State
        Private currentPageIndex As Integer = 0
        Private totalPages As Integer = 1
        Private _paginatedPages As New List(Of List(Of Integer))

        ' State Variables
        Private tempImagePath As String
        Private base64Image As String
        Private showProductImages As Boolean = True
        Private _isEditingExistingQuote As Boolean = False
        Private Shared element As FrameworkElement

        ' Layout Constants - CALIBRATED FOR LEGAL SIZE
        Private Const PageMaxHeight As Double = 800
        Private Const FooterSectionHeight As Double = 250
        Private Const BaseItemHeight As Double = 55
        Private Const DescriptionLineHeight As Double = 15
        Private Const ReservedSpaceForDescription As Double = 30
#End Region

#Region "2. Initialization & Loading"
        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub CostEstimate_Loaded(sender As Object, e As RoutedEventArgs)
            DetectQuoteMode()
            txtPageInfo = TryCast(Me.FindName("txtPageInfo"), TextBlock)

            If String.IsNullOrEmpty(CostEstimateDetails.CEQuoteNumberCache) OrElse CostEstimateDetails.CEQuoteItemsCache Is Nothing Then
                MessageBox.Show("Quote data is missing.")
                Return
            End If

            allItems = CostEstimateDetails.CEQuoteItemsCache
            LoadTextFields()

            showProductImages = CostEstimateDetails.CEShowProductImages
            UpdateToggleButtonState()

            RecalculatePagination()

            LoadPage(0)

            If Not String.IsNullOrWhiteSpace(CostEstimateDetails.CEImageCache) Then
                base64Image = CostEstimateDetails.CEImageCache
                DisplayUploadedImage()
            End If
        End Sub

        Private Sub LoadTextFields()
            ' Load Prices
            Dim _install As Decimal = 0
            Dim _deliv As Decimal = 0
            Decimal.TryParse(CostEstimateDetails.CEInstallation, _install)
            Decimal.TryParse(CostEstimateDetails.CEDeliveryCost, _deliv)

            Installation.Text = "₱ " & _install.ToString("N2")
            Delivery.Text = "₱ " & _deliv.ToString("N2")

            ' Load Text Fields
            QuoteNumber.Text = CostEstimateDetails.CEQuoteNumberCache
            QuoteDate.Text = CostEstimateDetails.CEQuoteDateCache
            QuoteValidityDate.Text = CostEstimateDetails.CEValidUntilDate
            Subtotal.Text = CostEstimateDetails.CETotalAmountCache
            TotalCost.Text = CostEstimateDetails.CETotalAmountCache

            noteBox.Text = CostEstimateDetails.CEpaperNote
            remarksBox.Text = CostEstimateDetails.CEremarksTxt

            ' Load Terms
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
            cmbSubtotalTax.Text = CostEstimateDetails.CESubtotalExInc
            Warranty.Text = CostEstimateDetails.CEWarranty
            cmbDeliveryMobilization.Text = CostEstimateDetails.CEDeliveryMobilization
            CNIdentifier.Text = CostEstimateDetails.CECNIndetifier

            ' Terms Selection
            If CostEstimateDetails.CEisCustomTerm Then
                CustomTerms.Text = CostEstimateDetails.CEpaymentTerms
                cmbTerms.SelectedIndex = 6
            Else
                cmbTerms.Text = CostEstimateDetails.CEpaymentTerms
            End If

            ' Subject Line
            Dim subjectBlock = TryCast(Me.FindName("Subject"), TextBlock)
            If subjectBlock IsNot Nothing Then
                subjectBlock.Text = If(String.IsNullOrWhiteSpace(CostEstimateDetails.CESubject), "Subject:", "Subject: " & CostEstimateDetails.CESubject)
            End If

            ' Header Details
            PopulateHeaderDetails()
        End Sub

        Private Sub PopulateHeaderDetails()
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
                Debug.WriteLine("Header Error: " & ex.Message)
            End Try
        End Sub
#End Region

#Region "3. The Pagination Engine (Core Logic)"

        Private Sub RecalculatePagination()
            _paginatedPages.Clear()

            If allItems Is Nothing OrElse allItems.Count = 0 Then
                _paginatedPages.Add(New List(Of Integer))
                totalPages = 1
                Return
            End If

            Dim pageIndices As New List(Of Integer)
            Dim currentHeight As Double = 0

            ' LOOP DIRECTLY THROUGH ALL ITEMS
            For i As Integer = 0 To allItems.Count - 1
                Dim h As Double = CalculateItemHeight(allItems(i))

                If currentHeight + h > PageMaxHeight Then
                    _paginatedPages.Add(New List(Of Integer)(pageIndices))
                    pageIndices.Clear()
                    currentHeight = h
                Else
                    currentHeight += h
                End If
                pageIndices.Add(i)
            Next

            If pageIndices.Count > 0 Then
                _paginatedPages.Add(pageIndices)
            End If

            ' Check if Footer (250px) clips
            If currentHeight > (PageMaxHeight - FooterSectionHeight) Then
                _paginatedPages.Add(New List(Of Integer))
            End If

            totalPages = _paginatedPages.Count
        End Sub

        Private Function CalculateItemHeight(item As Dictionary(Of String, String)) As Double
            Dim h As Double = BaseItemHeight
            If item.ContainsKey("Description") AndAlso Not String.IsNullOrWhiteSpace(item("Description")) Then
                Dim text As String = item("Description").Trim()
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

            ' Just add the items for this page index
            Dim indices = _paginatedPages(index)
            For Each idx In indices
                itemDataSource.Add(CreateVisualItem(allItems(idx)))
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
            Dim rate As Decimal = 0
            Dim linePrice As Decimal = 0
            Decimal.TryParse(item("Rate").Replace("₱", "").Replace(",", "").Trim(), rate)
            Decimal.TryParse(item("Amount").Replace("₱", "").Replace(",", "").Trim(), linePrice)

            Dim img As BitmapImage = Nothing
            If showProductImages Then
                If item.ContainsKey("ProductImageBase64") AndAlso Not String.IsNullOrEmpty(item("ProductImageBase64")) Then
                    img = Base64ToBitmapImage(item("ProductImageBase64"))
                Else
                    img = GetProductImageFromDatabase(item("ProductName"))
                End If
            End If

            Dim desc As String = ""
            Dim vis As Visibility = Visibility.Collapsed
            If item.ContainsKey("Description") AndAlso Not String.IsNullOrWhiteSpace(item("Description")) Then
                desc = item("Description").Trim()
                vis = Visibility.Visible
            End If

            Return New OrderItems With {
                .Quantity = item("Quantity"),
                .Description = item("ProductName"),
                .ProductDescription = desc,
                .ProductDescriptionVisibility = vis,
                .UnitPrice = $"₱ {rate:N2}",
                .LinePrice = $"₱ {linePrice:N2}",
                .ProductImage = img
            }
        End Function
#End Region

#Region "5. Navigation & UI Interaction"
        Private Sub BackToUI_Click(sender As Object, e As MouseButtonEventArgs)
            Try
                CEInstallation = ParseCurrency(Installation.Text)
                CEDeliveryCost = ParseCurrency(Delivery.Text)
                CostEstimateDetails.CEpaperNote = noteBox.Text
                CostEstimateDetails.CEApproved = cmbApproved.Text
                CostEstimateDetails.CEWarranty = Warranty.Text
                CostEstimateDetails.CETotalAmountCache = TotalCost.Text

                If _isEditingExistingQuote Then
                    ViewLoader.DynamicView.NavigateToView("editquote", Me)
                Else
                    ViewLoader.DynamicView.NavigateToView("salesquotegovernment", Me)
                End If
            Catch ex As Exception
                MessageBox.Show("Error navigating: " & ex.Message)
            End Try
        End Sub

        Private Sub PrintPreview(sender As Object, e As RoutedEventArgs)
            CostEstimateDetails.CEQuoteNumberCache = QuoteNumber.Text
            CostEstimateDetails.CEnoteTxt = noteBox.Text
            CostEstimateDetails.CEremarksTxt = remarksBox.Text
            CostEstimateDetails.CEGrandTotalCost = TotalCost.Text
            CostEstimateDetails.CEApproved = cmbApproved.Text
            CostEstimateDetails.CEWarranty = Warranty.Text

            Dim deliv As Decimal = 0
            Dim install As Decimal = 0
            Decimal.TryParse(Delivery.Text.Replace("₱", "").Trim(), deliv)
            Decimal.TryParse(Installation.Text.Replace("₱", "").Trim(), install)
            CostEstimateDetails.CEDeliveryCost = deliv
            CostEstimateDetails.CEInstallation = install

            If _isEditingExistingQuote Then
                ViewLoader.DynamicView.NavigateToView("previewprintquoteeditedquote", Me)
            Else
                ViewLoader.DynamicView.NavigateToView("printpreviewquotes", Me)
            End If
        End Sub

        Private Sub ToggleImage_Click(sender As Object, e As RoutedEventArgs)
            showProductImages = Not showProductImages
            CostEstimateDetails.CEShowProductImages = showProductImages
            UpdateToggleButtonState()
            RecalculatePagination()
            LoadPage(currentPageIndex)
        End Sub

        Private Sub UpdateToggleButtonState()
            If showProductImages Then
                txtToggleImage.Text = "Hide Images"
                iconToggleImage.Kind = PackIconKind.Image
                btnToggleImage.Opacity = 1.0
            Else
                txtToggleImage.Text = "Show Images"
                iconToggleImage.Kind = PackIconKind.ImageOff
                btnToggleImage.Opacity = 0.6
            End If
        End Sub

        Private Sub PreviousPage_Click(sender As Object, e As RoutedEventArgs)
            If currentPageIndex > 0 Then LoadPage(currentPageIndex - 1)
        End Sub

        Private Sub NextPage_Click(sender As Object, e As RoutedEventArgs)
            If currentPageIndex < totalPages - 1 Then LoadPage(currentPageIndex + 1)
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
            Dim ind = TryCast(Me.FindName("PageIndicatorText"), TextBlock)
            If ind IsNot Nothing Then ind.Text = $"Page {currentPageIndex + 1} of {totalPages}"
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

#Region "6. Calculation Helpers"
        ' Corrected: Renamed local vars to avoid collision with XAML controls
        Private Sub ComputeCost(s As Object, e As TextChangedEventArgs)
            Dim valSubTotal As Decimal = 0
            Dim valInstall As Decimal = 0
            Dim valDeliv As Decimal = 0
            Dim valVat As Decimal = 0

            Decimal.TryParse(Subtotal.Text.Replace("₱", "").Replace(",", "").Trim(), valSubTotal)
            Decimal.TryParse(Installation.Text.Replace("₱", "").Replace(",", "").Trim(), valInstall)
            Decimal.TryParse(Delivery.Text.Replace("₱", "").Replace(",", "").Trim(), valDeliv)
            Decimal.TryParse(VAT12.Text.Replace("₱", "").Replace(",", "").Trim(), valVat)

            Dim total As Decimal = valSubTotal + valInstall + valDeliv + valVat
            TotalCost.Text = "₱ " & total.ToString("N2")
        End Sub

        Private Sub Delivery_TextChanged(sender As Object, e As TextChangedEventArgs)
            HandleCurrencyInput(sender, e)
        End Sub

        Private Sub Installation_TextChanged(sender As Object, e As TextChangedEventArgs)
            HandleCurrencyInput(sender, e)
        End Sub

        Private Sub VAT12_TextChanged(sender As Object, e As TextChangedEventArgs)
        End Sub

        Private Sub HandleCurrencyInput(sender As Object, e As TextChangedEventArgs)
            Dim tb As TextBox = TryCast(sender, TextBox)
            If tb Is Nothing Then Return

            RemoveHandler tb.TextChanged, AddressOf Delivery_TextChanged
            RemoveHandler tb.TextChanged, AddressOf Installation_TextChanged

            Dim raw As String = tb.Text.Replace("₱", "").TrimStart()
            If raw = "" Then
                tb.Text = "₱ "
            ElseIf Not tb.Text.StartsWith("₱ ") Then
                tb.Text = "₱ " & raw
            End If
            tb.CaretIndex = tb.Text.Length

            ComputeCost(Nothing, Nothing)

            AddHandler tb.TextChanged, AddressOf Delivery_TextChanged
            AddHandler tb.TextChanged, AddressOf Installation_TextChanged
        End Sub

        Private Function ParseCurrency(txt As String) As Decimal
            If String.IsNullOrWhiteSpace(txt) Then Return 0
            Dim clean As String = txt.Replace("₱", "").Replace(",", "").Trim()
            Dim val As Decimal
            If Decimal.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, val) Then
                Return val
            End If
            Return 0
        End Function
#End Region

#Region "7. Utilities (Images, Files, Popups)"
        Private Sub DetectQuoteMode()
            Try
                Dim qNum As String = CEQuoteNumberCache
                If String.IsNullOrWhiteSpace(qNum) Then
                    _isEditingExistingQuote = False
                Else
                    _isEditingExistingQuote = QuotesController.QuoteNumberExists(qNum)
                End If
            Catch
                _isEditingExistingQuote = False
            End Try
        End Sub

        Private Sub TextEditorPopOut(sender As Object, e As MouseButtonEventArgs)
            element = TryCast(sender, FrameworkElement)
            Dim txt = DirectCast(element, TextBlock).Text
            Dim editor As New PopOutQuoteTextEditor(txt)
            editor.ShowDialog()
        End Sub

        Public Shared Sub ModifyText(newText As String)
            DirectCast(element, TextBlock).Text = newText
        End Sub

        Private Sub OpenFiles()
            Dim dlg As New OpenFileDialog With {.Filter = "Image Files|*.jpg;*.jpeg;*.png"}
            If dlg.ShowDialog() = True Then
                If LogicProduct.ValidateImageFile(dlg.FileName) Then
                    StartFileUpload(dlg.FileName)
                End If
            End If
        End Sub

        Private Sub StartFileUpload(path As String)
            Try
                base64Image = Base64Utility.EncodeFileToBase64(path)
                ImageCache = base64Image
                CostEstimateDetails.CEsignature = True
                DisplayUploadedImage()
            Catch ex As Exception
                MessageBox.Show("Image Error: " & ex.Message)
            End Try
        End Sub

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

        ' Corrected: Event handler now attached properly to Image object
        Private Sub ProductImageControl_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
            Dim img As Image = TryCast(sender, Image)
            If img Is Nothing OrElse img.Source Is Nothing Then Return

            Dim enlargedImage As New Image With {
                .Source = img.Source,
                .MaxWidth = 800,
                .MaxHeight = 600,
                .Stretch = Stretch.Uniform,
                .Margin = New Thickness(10),
                .Cursor = Cursors.Hand
            }

            Dim w As New Window With {
                .Title = "Product Image",
                .WindowStartupLocation = WindowStartupLocation.CenterScreen,
                .SizeToContent = SizeToContent.WidthAndHeight,
                .ResizeMode = ResizeMode.NoResize,
                .Background = Brushes.Black,
                .Content = enlargedImage
            }

            ' Attach handler to the image, not the generic window content
            AddHandler enlargedImage.MouseLeftButtonDown, Sub() w.Close()
            w.ShowDialog()
        End Sub

        Private Function GetProductImageFromDatabase(name As String) As BitmapImage
            Try
                Dim b64 As String = GetProduct.GetProductImageBase64(name)
                Return If(String.IsNullOrEmpty(b64), Nothing, Base64ToBitmapImage(b64))
            Catch
                Return Nothing
            End Try
        End Function

        Private Function Base64ToBitmapImage(b64 As String) As BitmapImage
            Try
                If b64.Contains(",") Then b64 = b64.Split(","c)(1)
                Dim bytes As Byte() = Convert.FromBase64String(b64)
                Using ms As New MemoryStream(bytes)
                    Dim bmp As New BitmapImage()
                    bmp.BeginInit()
                    bmp.CacheOption = BitmapCacheOption.OnLoad
                    bmp.StreamSource = ms
                    bmp.EndInit()
                    bmp.Freeze()
                    Return bmp
                End Using
            Catch
                Return Nothing
            End Try
        End Function

        Private Iterator Function FindVisualChildren(Of T As DependencyObject)(depObj As DependencyObject) As IEnumerable(Of T)
            If depObj IsNot Nothing Then
                For i As Integer = 0 To VisualTreeHelper.GetChildrenCount(depObj) - 1
                    Dim child As DependencyObject = VisualTreeHelper.GetChild(depObj, i)
                    If TypeOf child Is T Then Yield CType(child, T)
                    For Each childOfChild In FindVisualChildren(Of T)(child)
                        Yield childOfChild
                    Next
                Next
            End If
        End Function

        Private Sub cmbTerms_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If cmbTerms.SelectedIndex = 6 Then
                CostEstimateDetails.CEisCustomTerm = True
            Else
                CostEstimateDetails.CEisCustomTerm = False
            End If
        End Sub

        Private Sub cmbApproved_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbApproved.SelectionChanged
        End Sub
#End Region

    End Class
End Namespace