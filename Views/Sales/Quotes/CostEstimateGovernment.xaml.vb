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
    Public Class CostEstimateGovernment

        ' Item Data
        Private tempImagePath As String
        Private base64Image As String
        Private itemDataSource As New ObservableCollection(Of OrderItems)
        Private itemOrder As New List(Of Dictionary(Of String, String))
        ' ✓ NEW: Track whether this is a NEW quote or EDITED quote
        Private _isEditingExistingQuote As Boolean = False
        ' Text Editor PopOut
        Private Shared element As FrameworkElement
        Private itemsPerPage As Integer = 14
        Private paginationTriggerThreshold As Integer = 7
        Private currentPageIndex As Integer = 0
        Private totalPages As Integer = 1
        Private allItems As List(Of Dictionary(Of String, String))
        Private showProductImages As Boolean = True
        ' Add new variables for height-based pagination
        Private Const BaseItemHeight As Double = 55 ' Base height for items without description
        Private Const DescriptionLineHeight As Double = 15 ' Additional height per line of description
        Private Const PaginationTriggerHeight As Double = 412 ' Height threshold to trigger pagination 
        Private Const PageMaxHeight As Double = 770 ' Maximum height available per page
        Private Const ReservedSpaceForDescription As Double = 30 ' Extra space when item has description


        Public Sub New()

            InitializeComponent()

        End Sub

        Private Sub CostEstimateGovernment_Loaded(sender As Object, e As RoutedEventArgs)

            Debug.WriteLine("=== DATA CHECK ===")
            Debug.WriteLine($"CESubject: '{CostEstimateDetails.CESubject}'")
            Debug.WriteLine($"CEQuoteNumberCache: '{CostEstimateDetails.CEQuoteNumberCache}'")
            Debug.WriteLine($"CEQuoteDateCache: '{CostEstimateDetails.CEQuoteDateCache}'")
            Debug.WriteLine("==================")
            ' ✓ NEW: Detect if we're editing an existing quote
            DetectQuoteMode()

            txtPageInfo = TryCast(Me.FindName("txtPageInfo"), TextBlock)

            UpdatePageInfo()
            UpdateNavigationButtons()

            ' Check if important fields are initialized
            If String.IsNullOrEmpty(CostEstimateDetails.CEQuoteNumberCache) Then
                MessageBox.Show("Quote Number is missing.")
                Return
            End If

            ' Check if CEQuoteItemsCache is Nothing before using it
            If CostEstimateDetails.CEQuoteItemsCache Is Nothing Then
                MessageBox.Show("Quote items are not loaded.")
                Return
            End If

            ' Initialize allItems FIRST
            allItems = CostEstimateDetails.CEQuoteItemsCache

            If allItems IsNot Nothing AndAlso allItems.Count > 0 Then
                ' Calculate pages dynamically based on height
                totalPages = CalculateTotalPagesByHeight()
            Else
                totalPages = 1
            End If

            Dim installationFee As Decimal
            If Decimal.TryParse(CostEstimateDetails.CEInstallation, installationFee) Then
                Installation.Text = "₱ " & installationFee.ToString("N2")
            Else
                Installation.Text = "₱ 0.00"
                installationFee = 0D
            End If

            Dim deliveryCost As Decimal
            If Decimal.TryParse(CostEstimateDetails.CEDeliveryCost, deliveryCost) Then
                Delivery.Text = "₱ " & deliveryCost.ToString("N2")
            Else
                Delivery.Text = "₱ 0.00"
                deliveryCost = 0D
            End If

            QuoteNumber.Text = CostEstimateDetails.CEQuoteNumberCache
            QuoteDate.Text = CostEstimateDetails.CEQuoteDateCache
            QuoteValidityDate.Text = CostEstimateDetails.CEValidUntilDate
            Subtotal.Text = CostEstimateDetails.CETotalAmountCache
            TotalCost.Text = CostEstimateDetails.CETotalAmountCache
            base64Image = CostEstimateDetails.CEImageCache
            tempImagePath = CostEstimateDetails.CEPathCache
            CESubTotalCache = CostEstimateDetails.CETotalAmountCache
            noteBox.Text = CostEstimateDetails.CEpaperNote

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

            ' Check if the terms is enabled
            If CostEstimateDetails.CEisCustomTerm = True Then
                CustomTerms.Text = CostEstimateDetails.CEpaymentTerms
                cmbTerms.SelectedIndex = 6
            Else
                cmbTerms.Text = CostEstimateDetails.CEpaymentTerms
            End If

            Try
                Dim subjectTextBlock = TryCast(Me.FindName("Subject"), TextBlock)
                If subjectTextBlock IsNot Nothing Then
                    If String.IsNullOrWhiteSpace(CostEstimateDetails.CESubject) Then
                        subjectTextBlock.Text = "Subject:"
                    Else
                        subjectTextBlock.Text = "Subject: " & CostEstimateDetails.CESubject
                    End If
                    Debug.WriteLine($"✓ Subject updated: {subjectTextBlock.Text}")
                Else
                    Debug.WriteLine("✗ Subject TextBlock not found!")
                End If
            Catch ex As Exception
                Debug.WriteLine($"Error in CostEstimateGovernment_Loaded: {ex.Message}")
            End Try

            ' Load items with pagination
            LoadPage(0)

            ' Initialize toggle state from cache
            showProductImages = CostEstimateDetails.CEShowProductImages

            ' Update button appearance to match current state
            If showProductImages Then
                txtToggleImage.Text = "Hide Images"
                iconToggleImage.Kind = PackIconKind.Image
                btnToggleImage.Opacity = 1.0
            Else
                txtToggleImage.Text = "Show Images"
                iconToggleImage.Kind = PackIconKind.ImageOff
                btnToggleImage.Opacity = 0.6
            End If

            ' Check if the signature is enabled
            If Not String.IsNullOrWhiteSpace(base64Image) Then
                DisplayUploadedImage()
            End If

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
            AddHandler BrowseFile.MouseLeftButtonUp, AddressOf OpenFiles

            ' Populate "Submitted to" section with client details
            Try
                Dim submittedToClient = TryCast(Me.FindName("SubmittedToClient"), TextBlock)
                Dim submittedToAddress = TryCast(Me.FindName("SubmittedToAddress"), TextBlock)
                Dim submittedToEmail = TryCast(Me.FindName("SubmittedToEmail"), TextBlock)
                Dim submittedToProjectID = TryCast(Me.FindName("SubmittedToProjectID"), TextBlock)

                If submittedToClient IsNot Nothing Then
                    ' Use company name if available, otherwise use client name
                    Dim clientDisplayName As String = ""

                    If Not String.IsNullOrWhiteSpace(CostEstimateDetails.CECompanyName) Then
                        clientDisplayName = CostEstimateDetails.CECompanyName
                        Debug.WriteLine($"✓ Client Company: {CostEstimateDetails.CECompanyName}")
                    ElseIf Not String.IsNullOrWhiteSpace(CostEstimateDetails.CEClientName) Then
                        clientDisplayName = CostEstimateDetails.CEClientName
                        Debug.WriteLine($"✓ Client Name (Fallback): {CostEstimateDetails.CEClientName}")
                    End If

                    submittedToClient.Text = clientDisplayName
                End If

                If submittedToAddress IsNot Nothing Then
                    ' Build complete address from parts
                    Dim addressParts As New List(Of String)

                    If Not String.IsNullOrWhiteSpace(CostEstimateDetails.CEAddress) Then
                        addressParts.Add(CostEstimateDetails.CEAddress)
                    End If

                    If Not String.IsNullOrWhiteSpace(CostEstimateDetails.CECity) Then
                        addressParts.Add(CostEstimateDetails.CECity)
                    End If

                    If Not String.IsNullOrWhiteSpace(CostEstimateDetails.CERegion) Then
                        addressParts.Add(CostEstimateDetails.CERegion)
                    End If

                    If Not String.IsNullOrWhiteSpace(CostEstimateDetails.CECountry) Then
                        addressParts.Add(CostEstimateDetails.CECountry)
                    End If

                    ' Join all parts with comma separator
                    Dim completeAddress As String = String.Join(", ", addressParts)
                    submittedToAddress.Text = completeAddress

                    Debug.WriteLine($"✓ Client Address: {completeAddress}")
                End If

                If submittedToEmail IsNot Nothing Then
                    submittedToEmail.Text = CostEstimateDetails.CEEmail
                    Debug.WriteLine($"✓ Client Email: {CostEstimateDetails.CEEmail}")
                End If

                If submittedToProjectID IsNot Nothing Then
                    ' Display Project ID from CEProjectID property
                    Dim projectIDText As String = CostEstimateDetails.CEProjectID

                    If String.IsNullOrWhiteSpace(projectIDText) Then
                        submittedToProjectID.Text = ""
                    Else
                        submittedToProjectID.Text = projectIDText
                    End If

                    Debug.WriteLine($"✓ Project ID: {projectIDText}")
                End If

            Catch ex As Exception
                Debug.WriteLine($"Error populating Submitted to section: {ex.Message}")
            End Try
        End Sub

        ' New method to calculate total pages based on trigger height and max heights
        Private Function CalculateTotalPagesByHeight() As Integer
            If allItems Is Nothing OrElse allItems.Count = 0 Then
                Return 1
            End If

            ' Calculate total height of all items
            Dim totalHeight As Double = 0
            For Each item In allItems
                totalHeight += CalculateItemHeight(item)
            Next

            ' Below trigger height - single page
            If totalHeight <= PaginationTriggerHeight Then
                Return 1
            End If

            ' Above trigger but below max height - 2 pages (items + visibility sections)
            If totalHeight <= PageMaxHeight Then
                Return 2
            End If

            ' Above max height - calculate pages with visibility sections at the end
            Return CalculatePagesFromHeightWithVisibility()
        End Function

        ' Calculate pages based on height accumulation - same logic for all pages
        Private Function CalculatePagesFromHeightWithVisibility() As Integer
            Dim pageCount As Integer = 1
            Dim currentPageHeight As Double = 0

            For i As Integer = 0 To allItems.Count - 1
                Dim itemHeight As Double = CalculateItemHeight(allItems(i))

                ' Check if adding this item exceeds page max height
                If currentPageHeight + itemHeight > PageMaxHeight Then
                    ' Start new page
                    pageCount += 1
                    currentPageHeight = itemHeight
                Else
                    currentPageHeight += itemHeight
                End If
            Next

            ' Check if last page is below trigger height
            If currentPageHeight <= PaginationTriggerHeight Then
                ' Don't add extra page for visibility sections
                Return pageCount
            Else
                ' Add page for visibility sections only
                Return pageCount + 1
            End If
        End Function

        ' Calculate individual item height including description space
        Private Function CalculateItemHeight(item As Dictionary(Of String, String)) As Double
            Dim baseHeight As Double = BaseItemHeight

            ' Check if item has product description
            If item.ContainsKey("Description") AndAlso Not String.IsNullOrWhiteSpace(item("Description")) Then
                Dim descriptionText As String = item("Description")
                ' Estimate lines based on character count (approximately 50 characters per line at width 400)
                Dim estimatedLines As Integer = Math.Ceiling(descriptionText.Length / 50.0)
                baseHeight += (estimatedLines * DescriptionLineHeight)

                ' Add reserved space for items with descriptions
                baseHeight += ReservedSpaceForDescription
            End If

            Return baseHeight
        End Function

        ' LoadPage method:
        Private Sub LoadPage(pageIndex As Integer)
            If allItems Is Nothing OrElse allItems.Count = 0 Then
                Return
            End If

            currentPageIndex = pageIndex
            itemDataSource.Clear()

            ' Calculate total height
            Dim totalHeight As Double = 0
            For Each item In allItems
                totalHeight += CalculateItemHeight(item)
            Next

            ' Below trigger height - single page with all items
            If totalHeight <= PaginationTriggerHeight Then
                If pageIndex = 0 Then
                    For Each item In allItems
                        itemDataSource.Add(CreateOrderItemWithDescription(item))
                    Next
                End If
                ' Above trigger but below max - items on page 1, visibility on page 2
            ElseIf totalHeight <= PageMaxHeight Then
                If pageIndex = 0 Then
                    For Each item In allItems
                        itemDataSource.Add(CreateOrderItemWithDescription(item))
                    Next
                End If
                ' Page 2 has no items, just visibility sections
            Else
                ' Multiple pages with visibility sections at the end
                Dim itemIndices = GetItemsForPageByHeightWithVisibility(pageIndex)
                For Each index In itemIndices
                    If index < allItems.Count Then
                        itemDataSource.Add(CreateOrderItemWithDescription(allItems(index)))
                    End If
                Next
            End If

            dataGrid.ItemsSource = itemDataSource

            UpdatePageInfo()
            UpdateNavigationButtons()
            UpdateNothingToFollowVisibility()
            UpdateServicesVisibility()
            UpdateTotalCostVisibility()
            UpdateWarrantyAndBottomSectionVisibility()
            UpdatePrintPageIndicator()
            UpdateImageColumnVisibility()

        End Sub

        ' Get item indices for a specific page based on height - same logic for all pages
        Private Function GetItemsForPageByHeightWithVisibility(pageIndex As Integer) As List(Of Integer)
            Dim allPagesIndices As New List(Of List(Of Integer))
            Dim currentPageIndices As New List(Of Integer)
            Dim currentPageHeight As Double = 0

            For i As Integer = 0 To allItems.Count - 1
                Dim itemHeight As Double = CalculateItemHeight(allItems(i))

                ' Check if adding this item exceeds page max height
                If currentPageHeight + itemHeight > PageMaxHeight Then
                    ' Save current page
                    If currentPageIndices.Count > 0 Then
                        allPagesIndices.Add(New List(Of Integer)(currentPageIndices))
                        currentPageIndices.Clear()
                    End If

                    ' Start new page
                    currentPageHeight = itemHeight
                    currentPageIndices.Add(i)
                Else
                    currentPageHeight += itemHeight
                    currentPageIndices.Add(i)
                End If
            Next

            ' Add remaining items
            If currentPageIndices.Count > 0 Then
                allPagesIndices.Add(currentPageIndices)

                ' Check if last page is below trigger - don't add visibility page
                If currentPageHeight > PaginationTriggerHeight Then
                    ' Add empty page for visibility sections
                    allPagesIndices.Add(New List(Of Integer))
                End If
            End If

            ' Return indices for requested page
            If pageIndex >= 0 AndAlso pageIndex < allPagesIndices.Count Then
                Return allPagesIndices(pageIndex)
            End If

            Return New List(Of Integer)
        End Function

        ' New helper method
        Private Function CreateOrderItemWithDescription(item As Dictionary(Of String, String)) As OrderItems
            Dim rate As Decimal = Decimal.Parse(item("Rate"))
            Dim rateFormatted As String = rate.ToString("N2")

            Dim linePrice As Decimal = Decimal.Parse(item("Amount"))
            Dim linePriceFormatted As String = linePrice.ToString("N2")

            Dim productImage As BitmapImage = Nothing
            If showProductImages Then
                If item.ContainsKey("ProductImageBase64") AndAlso Not String.IsNullOrEmpty(item("ProductImageBase64").ToString()) Then
                    productImage = Base64ToBitmapImage(item("ProductImageBase64").ToString())
                Else
                    productImage = GetProductImageFromDatabase(item("ProductName").ToString())
                End If
            End If

            ' Get description and set visibility
            Dim productDesc As String = ""
            Dim descVisibility As Visibility = Visibility.Collapsed

            If item.ContainsKey("Description") AndAlso Not String.IsNullOrWhiteSpace(item("Description")) Then
                productDesc = item("Description")
                descVisibility = Visibility.Visible
            End If

            Return New OrderItems With {
        .Quantity = item("Quantity"),
        .Description = item("ProductName"),
        .ProductDescription = productDesc,
        .ProductDescriptionVisibility = descVisibility,
        .UnitPrice = $"₱ {rateFormatted}",
        .LinePrice = $"₱ {linePriceFormatted}",
        .ProductImage = productImage
    }
        End Function

        Private Sub UpdateImageColumnVisibility()
            ' Find the image column in the DataGrid
            For Each column As DataGridColumn In dataGrid.Columns
                If TypeOf column Is DataGridTemplateColumn AndAlso column.Header IsNot Nothing AndAlso column.Header.ToString() = "Image" Then
                    column.Visibility = If(showProductImages, Visibility.Visible, Visibility.Collapsed)
                    Exit For
                End If
            Next
        End Sub

        Private Sub ToggleImage_Click(sender As Object, e As RoutedEventArgs)
            showProductImages = Not showProductImages
            CostEstimateDetails.CEShowProductImages = showProductImages

            ' Update button appearance
            If showProductImages Then
                txtToggleImage.Text = "Hide Images"
                iconToggleImage.Kind = PackIconKind.Image
                btnToggleImage.Opacity = 1.0
            Else
                txtToggleImage.Text = "Show Images"
                iconToggleImage.Kind = PackIconKind.ImageOff
                btnToggleImage.Opacity = 0.6
            End If

            ' Recalculate pagination based on height
            totalPages = CalculateTotalPagesByHeight()

            ' Ensure current page index is valid
            If currentPageIndex >= totalPages Then
                currentPageIndex = totalPages - 1
            End If

            LoadPage(currentPageIndex)
        End Sub

        Private Sub UpdatePageInfo()
            If txtPageInfo IsNot Nothing Then
                txtPageInfo.Text = $"Page {currentPageIndex + 1} of {totalPages}"
            End If
        End Sub

        Private Sub UpdateNavigationButtons()
            Dim btnPrev = TryCast(Me.FindName("btnPrevPage"), Button)
            Dim btnNext = TryCast(Me.FindName("btnNextPage"), Button)

            If btnPrev IsNot Nothing Then
                btnPrev.IsEnabled = currentPageIndex > 0
            End If

            If btnNext IsNot Nothing Then
                btnNext.IsEnabled = currentPageIndex < totalPages - 1
            End If
        End Sub

        Private Sub PreviousPage_Clicker(sender As Object, e As RoutedEventArgs)
            If currentPageIndex > 0 Then
                LoadPage(currentPageIndex - 1)
            End If
        End Sub

        Private Sub NextPage_Clicker(sender As Object, e As RoutedEventArgs)
            If currentPageIndex < totalPages - 1 Then
                LoadPage(currentPageIndex + 1)
            End If
        End Sub


        Private Sub UpdatePageIndicator()

        End Sub

        Private Sub NextPage_Click(sender As Object, e As RoutedEventArgs)
            If currentPageIndex < totalPages - 1 Then
                LoadPage(currentPageIndex + 1)
            End If
        End Sub

        Private Sub PreviousPage_Click(sender As Object, e As RoutedEventArgs)
            If currentPageIndex > 0 Then
                LoadPage(currentPageIndex - 1)
            End If
        End Sub


        ''' ✓ NEW FUNCTION: Detect if quote is new or existing
        ''' This determines which preview form to navigate to
        Private Sub DetectQuoteMode()
            Try
                Dim quoteNumber As String = CEQuoteNumberCache
                Debug.WriteLine("")
                Debug.WriteLine("═══════════════════════════════════════")
                Debug.WriteLine("DETECTING QUOTE MODE")
                Debug.WriteLine("═══════════════════════════════════════")
                Debug.WriteLine($"Quote Number: {quoteNumber}")

                If String.IsNullOrWhiteSpace(quoteNumber) Then
                    Debug.WriteLine("→ MODE: NEW QUOTE (No quote number)")
                    _isEditingExistingQuote = False
                    Return
                End If

                ' Check if quote exists in database
                Dim quoteExists As Boolean = QuotesController.QuoteNumberExists(quoteNumber)
                Debug.WriteLine($"Quote exists in DB: {quoteExists}")

                If quoteExists Then
                    Debug.WriteLine("→ MODE: EDITING EXISTING QUOTE")
                    _isEditingExistingQuote = True
                Else
                    Debug.WriteLine("→ MODE: NEW QUOTE")
                    _isEditingExistingQuote = False
                End If

                Debug.WriteLine("═══════════════════════════════════════")
                Debug.WriteLine("")

            Catch ex As Exception
                Debug.WriteLine($"Error in DetectQuoteMode: {ex.Message}")
                _isEditingExistingQuote = False
            End Try
        End Sub

#Region "Computation Part"
        Private Sub Delivery_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim tb As TextBox = TryCast(sender, TextBox)
            If tb Is Nothing Then Exit Sub

            RemoveHandler tb.TextChanged, AddressOf Delivery_TextChanged

            Dim raw As String = tb.Text.Replace("₱", "").TrimStart()
            If raw = "" Then
                tb.Text = "₱ "
            ElseIf Not tb.Text.StartsWith("₱ ") Then
                tb.Text = "₱ " & raw
            End If
            tb.CaretIndex = tb.Text.Length

            AddHandler tb.TextChanged, AddressOf Delivery_TextChanged
            ComputeCost(sender, e)
        End Sub

        Private Sub Installation_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim tb As TextBox = TryCast(sender, TextBox)
            If tb Is Nothing Then Exit Sub

            RemoveHandler tb.TextChanged, AddressOf Installation_TextChanged

            Dim raw As String = tb.Text.Replace("₱", "").TrimStart()
            If raw = "" Then
                tb.Text = "₱ "
            ElseIf Not tb.Text.StartsWith("₱ ") Then
                tb.Text = "₱ " & raw
            End If
            tb.CaretIndex = tb.Text.Length

            AddHandler tb.TextChanged, AddressOf Installation_TextChanged
            ComputeCost(sender, e)
        End Sub

        Private Sub ComputeCost(s As Object, e As TextChangedEventArgs)
            ' Parse Delivery
            Dim deliveryAmount As Decimal = 0
            Dim rawDelivery = Delivery.Text.Replace("₱", "").Trim().Replace(",", "").Trim()
            Debug.WriteLine($"Raw Delivery: '{rawDelivery}'")
            If Not Decimal.TryParse(rawDelivery, NumberStyles.Any, CultureInfo.InvariantCulture, deliveryAmount) Then
                Debug.WriteLine($"Failed to parse Delivery.Text, input: '{rawDelivery}'")
                deliveryAmount = 0D
            End If

            ' Parse Installation
            Dim installationAmount As Decimal = 0
            Dim rawInstallation = Installation.Text.Replace("₱", "").Trim().Replace(",", "").Trim()
            Debug.WriteLine($"Raw Installation: '{rawInstallation}'")
            If Not Decimal.TryParse(rawInstallation, NumberStyles.Any, CultureInfo.InvariantCulture, installationAmount) Then
                Debug.WriteLine($"Failed to parse Installation.Text, input: '{rawInstallation}'")
                installationAmount = 0D
            End If

            ' Parse Subtotal
            Dim subtotalAmount As Decimal = 0
            Dim rawSubtotal = Subtotal.Text.Replace("₱", "").Trim().Replace(",", "").Trim()
            Debug.WriteLine($"Raw Subtotal: '{rawSubtotal}'")
            If Not Decimal.TryParse(rawSubtotal, NumberStyles.Any, CultureInfo.InvariantCulture, subtotalAmount) Then
                Debug.WriteLine($"Failed to parse Subtotal.Text, input: '{rawSubtotal}'")
                subtotalAmount = 0D
            End If

            ' ✓ FIXED: Calculate baseAmount first
            Dim baseAmount As Decimal = subtotalAmount + installationAmount + deliveryAmount
            Debug.WriteLine($"Base Amount: {baseAmount}")

            ' Calculate VAT12 for display
            Dim vatAmount As Decimal = 0
            ' Calculate total cost
            Dim totalCostVal As Decimal

            Debug.WriteLine($"Computed Total: {totalCostVal}")

            ' TotalCost display
            TotalCost.Text = "₱ " & totalCostVal.ToString("N2")
        End Sub

#End Region

#Region "Signature Upload"
        Private Sub OpenFiles()

            Dim openFileDialog As New OpenFileDialog With {
                .Filter = "Image Files|*.jpg;*.jpeg;*.png",
                .Title = "Select an Image"
            }

            If openFileDialog.ShowDialog() = True Then
                Dim filePath As String = openFileDialog.FileName

                If LogicProduct.ValidateImageFile(filePath) Then
                    StartFileUpload(filePath)
                End If
            End If
        End Sub

        Private Sub StartFileUpload(filePath As String)

            Dim fileInfo As New FileInfo(filePath)
            Dim fileSizeText As String = Base64Utility.GetReadableFileSize(fileInfo.Length)

            Try
                base64Image = Base64Utility.EncodeFileToBase64(filePath)
                ImageCache = base64Image
            Catch ex As Exception
                MessageBox.Show("Error encoding image: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Exit Sub
            End Try

            CostEstimateDetails.CEsignature = True
            DisplayUploadedImage()
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

        Public Sub DisplayUploadedImage()
            Try
                tempImagePath = Path.Combine(Path.GetTempPath(), "decoded_image.png")

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
                imageSource.Freeze()

                BrowseFile.Child = Nothing

                Dim imagePreview As New Image()
                imagePreview.Source = imageSource
                imagePreview.MaxHeight = 70

                BrowseFile.Child = imagePreview

            Catch ex As Exception
                MessageBox.Show("Error decoding image: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
#End Region

#Region "Navigation"
        ' Going back to the NewQuote View
        Private Sub BackToUI_Click(sender As Object, e As MouseButtonEventArgs)
            Try
                ' Helper function to clean and parse currency text

                ' Safely parse Installation and Delivery
                CEInstallation = ParseCurrency(If(Installation?.Text, "0"))
                CEDeliveryCost = ParseCurrency(If(Delivery?.Text, "0"))

                ' Assign other fields (null-safe)
                CostEstimateDetails.CEpaperNote = If(noteBox?.Text, "")
                CostEstimateDetails.CEApproved = If(cmbApproved?.Text, "")
                CostEstimateDetails.CEpaymentTerms = If(CostEstimateDetails.CEisCustomTerm, If(CustomTerms?.Text, ""), If(cmbTerms?.Text, ""))
                CostEstimateDetails.CESubtotalExInc = If(cmbSubtotalTax?.Text, "")
                CostEstimateDetails.CEWarranty = If(Warranty?.Text, "")
                CostEstimateDetails.CEDeliveryMobilization = If(cmbDeliveryMobilization?.Text, "")
                CostEstimateDetails.CETotalTaxValueCache = ""
                CostEstimateDetails.CETotalAmountCache = If(TotalCost?.Text, "₱ 0.00")

                Debug.WriteLine($"Approved - {CostEstimateDetails.CEApproved}")


                ' ✓ EDIT: Add smart routing (REPLACE old ViewLoader line)
                If _isEditingExistingQuote Then
                    Debug.WriteLine("→ Back to: EditQuote")
                    ViewLoader.DynamicView.NavigateToView("editquote", Me)
                Else
                    Debug.WriteLine("→ Back to: NewQuote")
                    ViewLoader.DynamicView.NavigateToView("salesquotegovernment", Me)
                End If

            Catch ex As Exception
                MessageBox.Show("Occurred when the installation or delivery fields were empty or invalid. Treated as 0.")

                ' ✓ EDIT: Add smart routing here too
                If _isEditingExistingQuote Then
                    ViewLoader.DynamicView.NavigateToView("editquote", Me)
                Else
                    ViewLoader.DynamicView.NavigateToView("salesquotegovernment", Me)
                End If
            End Try
        End Sub

        Private Function ParseCurrency(txt As String) As Decimal
            If String.IsNullOrWhiteSpace(txt) Then Return 0D
            Dim cleaned As String = txt.Replace("₱", "").Replace(",", "").Trim()
            If cleaned = "" Then Return 0D
            Dim value As Decimal
            If Decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, value) Then
                Return value
            Else
                Return 0D
            End If
        End Function

        Private Sub PrintPreview(sender As Object, e As RoutedEventArgs)

            ' Delivery Cost Conversion to Decimal
            Dim _deliveryCost As Decimal = 0
            Dim _installationCost As Decimal = 0

            Decimal.TryParse(Delivery.Text.Replace("₱", "").Trim(), _deliveryCost)
            Decimal.TryParse(Installation.Text.Replace("₱", "").Trim(), _installationCost)

            CostEstimateDetails.CEQuoteNumberCache = QuoteNumber.Text
            CostEstimateDetails.CEQuoteDateCache = QuoteDate.Text
            CostEstimateDetails.CEValidUntilDate = QuoteValidityDate.Text
            CostEstimateDetails.CEGrandTotalCost = TotalCost.Text
            CostEstimateDetails.CEDeliveryCost = _deliveryCost
            CostEstimateDetails.CEInstallation = _installationCost
            CostEstimateDetails.CETaxValueCache = VAT12.Text
            CostEstimateDetails.CEQuoteItemsCache = itemOrder
            CostEstimateDetails.CEImageCache = base64Image
            CostEstimateDetails.CEPathCache = tempImagePath
            CostEstimateDetails.CEnoteTxt = noteBox.Text
            CostEstimateDetails.CEremarksTxt = remarksBox.Text
            CostEstimateDetails.CETerm1 = Term1.Text
            CostEstimateDetails.CETerm2 = Term2.Text
            CostEstimateDetails.CETerm3 = Term3.Text
            CostEstimateDetails.CETerm4 = Term4.Text
            CostEstimateDetails.CETerm5 = Term5.Text
            CostEstimateDetails.CETerm6 = Term6.Text
            CostEstimateDetails.CETerm7 = Term7.Text
            CostEstimateDetails.CETerm8 = Term8.Text
            CostEstimateDetails.CETerm9 = Term9.Text
            CostEstimateDetails.CETerm10 = Term10.Text
            CostEstimateDetails.CETerm11 = Term11.Text
            CostEstimateDetails.CETerm12 = Term12.Text
            CostEstimateDetails.CETerm13 = Term13.Text
            CostEstimateDetails.CETerm14 = Term14.Text
            CostEstimateDetails.CETerm15 = Term15.Text
            CostEstimateDetails.CEApproved = cmbApproved.Text
            CostEstimateDetails.CESubtotalExInc = cmbSubtotalTax.Text
            CostEstimateDetails.CEWarranty = Warranty.Text
            CostEstimateDetails.CEDeliveryMobilization = cmbDeliveryMobilization.Text
            CostEstimateDetails.CECNIndetifier = CNIdentifier.Text
            CostEstimateDetails.CEOtherServices = OtherServices.Text ' Add this line
            CostEstimateDetails.CEpaymentTerms = cmbTerms.Text
            If CostEstimateDetails.CEisCustomTerm = True Then
                CostEstimateDetails.CEpaymentTerms = CustomTerms.Text
            Else
                CostEstimateDetails.CEpaymentTerms = cmbTerms.Text
            End If

            ' ✓ EDIT: Add smart routing here (REPLACE old ViewLoader line)
            If _isEditingExistingQuote Then
                Debug.WriteLine("→ Routing to: PreviewPrintEditedQuote ")
                ViewLoader.DynamicView.NavigateToView("previewprintquoteeditedquote", Me)
            Else
                Debug.WriteLine("→ Routing to: PreviewPrintQuote")
                ViewLoader.DynamicView.NavigateToView("printpreviewquotes", Me)
            End If
        End Sub
#End Region

#Region "Text Editor Function"
        Private Sub TextEditorPopOut(sender As Object, e As MouseButtonEventArgs)
            element = TryCast(sender, FrameworkElement)
            Dim elementText = DirectCast(element, TextBlock).Text

            Dim txtEditor As New PopOutQuoteTextEditor(elementText)
            txtEditor.ShowDialog()
        End Sub

        Public Shared Sub ModifyText(textEdited As String)
            DirectCast(element, TextBlock).Text = textEdited
        End Sub

        Private Sub cmbTerms_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If cmbTerms.SelectedIndex = 6 Then
                CostEstimateDetails.CEisCustomTerm = True
            Else
                CostEstimateDetails.CEisCustomTerm = False
            End If
        End Sub

        Private Sub VAT12_TextChanged(sender As Object, e As TextChangedEventArgs)

        End Sub


        ''' ✓ NEW PROPERTY: Public property to check if currently editing an existing quote
        Public ReadOnly Property IsEditingExistingQuote As Boolean
            Get
                Return _isEditingExistingQuote
            End Get
        End Property
#End Region

        ' CHANGES START HERE
        Private Function GetProductImageFromDatabase(productName As String) As BitmapImage
            Try
                ' Get Base64 image from database using GetProduct controller
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
                ' Remove data URI prefix if present (e.g., "data:image/png;base64,")
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

        ' Image Enlarge 
        Private Sub ProductImageControl_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
            Dim img As Image = TryCast(sender, Image)
            If img IsNot Nothing AndAlso img.Source IsNot Nothing Then
                ' Create a new window to display the enlarged image
                Dim enlargeWindow As New Window()
                enlargeWindow.Title = "Product Image"
                enlargeWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen
                enlargeWindow.SizeToContent = SizeToContent.WidthAndHeight
                enlargeWindow.ResizeMode = ResizeMode.NoResize
                enlargeWindow.Background = Brushes.Black

                ' Create an image control for the enlarged view
                Dim enlargedImage As New Image()
                enlargedImage.Source = img.Source
                enlargedImage.MaxWidth = 800
                enlargedImage.MaxHeight = 600
                enlargedImage.Stretch = Stretch.Uniform
                enlargedImage.Margin = New Thickness(10)
                enlargedImage.Cursor = Cursors.Hand

                ' Close window when clicking the image
                AddHandler enlargedImage.MouseLeftButtonDown,
                Sub()
                    enlargeWindow.Close()
                End Sub

                ' Set the image as window content
                enlargeWindow.Content = enlargedImage

                ' Show the window
                enlargeWindow.ShowDialog()
            End If
        End Sub

        ' Nothing To follow Visisbility
        Private Sub UpdateNothingToFollowVisibility()
            ' Find the "Nothing to follow" StackPanel
            Dim nothingToFollowPanel As StackPanel = Nothing
            Dim otherServicesPanel As StackPanel = Nothing

            ' Search through the visual tree to find these elements
            For Each child As UIElement In FindVisualChildren(Of StackPanel)(Me)
                Dim stackPanel = TryCast(child, StackPanel)
                If stackPanel IsNot Nothing Then
                    ' Find the panel containing "Nothing to follow" text
                    For Each element In stackPanel.Children
                        If TypeOf element Is Border Then
                            Dim border = TryCast(element, Border)
                            If border.Child IsNot Nothing AndAlso TypeOf border.Child Is TextBlock Then
                                Dim textBlock = TryCast(border.Child, TextBlock)
                                If textBlock.Text.Contains("Nothing to follow") Then
                                    nothingToFollowPanel = stackPanel
                                    Exit For
                                End If
                            End If
                        End If
                    Next

                    ' Find the OtherServices panel
                    For Each element In stackPanel.Children
                        If TypeOf element Is Border Then
                            Dim border = TryCast(element, Border)
                            If border.Child IsNot Nothing AndAlso TypeOf border.Child Is StackPanel Then
                                Dim innerStack = TryCast(border.Child, StackPanel)
                                For Each innerElement In innerStack.Children
                                    If TypeOf innerElement Is TextBlock Then
                                        Dim tb = TryCast(innerElement, TextBlock)
                                        If tb.Name = "OtherServicesText" Then
                                            otherServicesPanel = stackPanel
                                            Exit For
                                        End If
                                    End If
                                Next
                            End If
                        End If
                    Next
                End If
            Next

            ' Show on last page only, hide on other pages
            If nothingToFollowPanel IsNot Nothing Then
                nothingToFollowPanel.Visibility = If(currentPageIndex = totalPages - 1, Visibility.Visible, Visibility.Collapsed)
            End If

            If otherServicesPanel IsNot Nothing Then
                otherServicesPanel.Visibility = If(currentPageIndex = totalPages - 1, Visibility.Visible, Visibility.Collapsed)
            End If
        End Sub

        Private Iterator Function FindVisualChildren(Of T As DependencyObject)(depObj As DependencyObject) As IEnumerable(Of T)
            If depObj IsNot Nothing Then
                For i As Integer = 0 To VisualTreeHelper.GetChildrenCount(depObj) - 1
                    Dim child As DependencyObject = VisualTreeHelper.GetChild(depObj, i)
                    If child IsNot Nothing AndAlso TypeOf child Is T Then
                        Yield CType(child, T)
                    End If

                    For Each childOfChild In FindVisualChildren(Of T)(child)
                        Yield childOfChild
                    Next
                Next
            End If
        End Function

        Private Sub UpdateServicesVisibility()
            ' Using FindName to locate the named elements
            Dim nothingToFollowSection = TryCast(Me.FindName("NothingToFollowSection"), StackPanel)
            Dim otherServicesSection = TryCast(Me.FindName("OtherServicesSection"), StackPanel)

            ' Only show on the last page
            Dim isLastPage = (currentPageIndex = totalPages - 1)

            If nothingToFollowSection IsNot Nothing Then
                nothingToFollowSection.Visibility = If(isLastPage, Visibility.Visible, Visibility.Collapsed)
            End If

            If otherServicesSection IsNot Nothing Then
                otherServicesSection.Visibility = If(isLastPage, Visibility.Visible, Visibility.Collapsed)
            End If
        End Sub

        'Total Cost Section 
        Private Sub UpdateTotalCostVisibility()
            Dim totalCostSection = TryCast(Me.FindName("TotalCostSection"), Border)

            ' Only show on the last page
            Dim isLastPage = (currentPageIndex = totalPages - 1)

            If totalCostSection IsNot Nothing Then
                totalCostSection.Visibility = If(isLastPage, Visibility.Visible, Visibility.Collapsed)
            End If
        End Sub

        Private Sub UpdateWarrantyAndBottomSectionVisibility()
            ' Find the warranty and bottom sections
            Dim warrantySection = TryCast(Me.FindName("WarrantySection"), StackPanel)
            Dim bottomSection = TryCast(Me.FindName("BottomSection"), StackPanel)

            ' Only show on the last page
            Dim isLastPage = (currentPageIndex = totalPages - 1)

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
                pageIndicator.Text = $"Page {currentPageIndex + 1} of {totalPages}"
            End If
        End Sub

        Private Sub cmbApproved_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbApproved.SelectionChanged

        End Sub
    End Class
End Namespace