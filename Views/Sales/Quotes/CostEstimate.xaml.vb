Imports System.Collections.ObjectModel
Imports System.Globalization
Imports System.IO
Imports System.Windows
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports Microsoft.Win32
Imports SkiaSharp.Views.WPF

Namespace DPC.Views.Sales.Quotes
    Public Class CostEstimate

        ' Item Data
        Private tempImagePath As String
        Private base64Image As String
        Private itemDataSource As New ObservableCollection(Of OrderItems)
        Private itemOrder As New List(Of Dictionary(Of String, String))
        ' Text Editor PopOut
        Private Shared element As FrameworkElement
        Private itemsPerPage As Integer = 14
        Private paginationTriggerThreshold As Integer = 7
        Private currentPageIndex As Integer = 0
        Private totalPages As Integer = 1
        Private allItems As List(Of Dictionary(Of String, String))

        Public Sub New()

            InitializeComponent()

        End Sub

        Private Sub CostEstimate_Loaded(sender As Object, e As RoutedEventArgs)

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
            itemOrder = CostEstimateDetails.CEQuoteItemsCache

            If allItems IsNot Nothing AndAlso allItems.Count > 0 Then
                ' Trigger pagination when items exceed 7, but first page can hold up to 14
                If allItems.Count > paginationTriggerThreshold Then
                    ' Calculate pages based on 14 items per page
                    If allItems.Count <= itemsPerPage Then
                        totalPages = 2 ' First page has all items (up to 14), second page has sections only
                    Else
                        ' Items exceed 14, so calculate normally
                        totalPages = Math.Ceiling(allItems.Count / itemsPerPage)
                    End If
                Else
                    totalPages = 1 ' 7 or fewer items, single page
                End If
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
            ClientNameBox.Text = CostEstimateDetails.CEClientName
            CESubTotalCache = CostEstimateDetails.CETotalAmountCache
            AddressLineOne.Text = CostEstimateDetails.CEAddress & ", " & CostEstimateDetails.CECity
            AddressLineTwo.Text = CostEstimateDetails.CERegion & ", " & CostEstimateDetails.CECountry
            PhoneBox.Text = "+63" & FormatPhoneWithSpaces(CostEstimateDetails.CEPhone)
            RepresentativeBox.Text = CostEstimateDetails.CERepresentative
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

            ' Load items with pagination
            LoadPage(0)

            ' Tax calculation logic
            If Not CEtaxSelection Then
                ' Tax Inclusive
                VatText.Visibility = Visibility.Visible
                VatValue.Visibility = Visibility.Visible
                If String.IsNullOrWhiteSpace(CostEstimateDetails.CEremarksTxt) OrElse remarksBox.Text = "Tax Inclusive." OrElse remarksBox.Text = "Tax Exclusive." Then
                    remarksBox.Text = "Tax Inclusive."
                End If

                Dim totalAmountBeforeVAT As Decimal = 0
                Dim rawSubtotal = Subtotal.Text.Replace("₱", "").Trim().Replace(",", "").Trim()
                If Decimal.TryParse(rawSubtotal, totalAmountBeforeVAT) Then
                    Dim vatAmount As Decimal = totalAmountBeforeVAT * 0.12D
                    VAT12.Text = "₱ " & vatAmount.ToString("N2")
                    CostEstimateDetails.CETotalTaxValueCache = VAT12.Text

                    Dim totalCostValue As Decimal = totalAmountBeforeVAT + installationFee + deliveryCost
                    TotalCost.Text = "₱ " & totalCostValue.ToString("N2")
                    CostEstimateDetails.CETotalAmountCache = "₱ " & totalCostValue.ToString("N2")
                Else
                    Debug.WriteLine($"Failed to parse Subtotal.Text: '{rawSubtotal}'")
                    TotalCost.Text = "₱ 0.00"
                    VAT12.Text = "₱ 0.00"
                End If
            Else
                ' Tax Exclusive
                VatText.Visibility = Visibility.Visible
                VatValue.Visibility = Visibility.Visible
                If String.IsNullOrWhiteSpace(CostEstimateDetails.CEremarksTxt) OrElse remarksBox.Text = "Tax Inclusive." OrElse remarksBox.Text = "Tax Exclusive." Then
                    remarksBox.Text = "Tax Exclusive."
                End If

                Dim totalCostValue As Decimal = 0
                Dim rawSubtotal = Subtotal.Text.Replace("₱", "").Trim().Replace(",", "").Trim()
                If Decimal.TryParse(rawSubtotal, totalCostValue) Then
                    Dim vatAmount As Decimal
                    If CEisVatExInclude Then
                        vatAmount = totalCostValue * 0.12D
                    Else
                        vatAmount = 0D
                        VatText.Visibility = Visibility.Collapsed
                        VatValue.Visibility = Visibility.Collapsed
                    End If
                    VAT12.Text = "₱ " & vatAmount.ToString("N2")
                    CostEstimateDetails.CETotalTaxValueCache = VAT12.Text

                    totalCostValue += installationFee + deliveryCost + vatAmount
                    TotalCost.Text = "₱ " & totalCostValue.ToString("N2")
                    CostEstimateDetails.CETotalAmountCache = "₱ " & totalCostValue.ToString("N2")
                Else
                    Debug.WriteLine($"Failed to parse Subtotal.Text: '{rawSubtotal}'")
                    TotalCost.Text = "₱ 0.00"
                    VAT12.Text = "₱ 0.00"
                End If
            End If

            ' Check if the signature is enabled
            If Not String.IsNullOrWhiteSpace(base64Image) Then
                DisplayUploadedImage()
            End If

            AddHandler BrowseFile.MouseLeftButtonUp, AddressOf OpenFiles
        End Sub

        ' Update LoadPage method:
        Private Sub LoadPage(pageIndex As Integer)
            If allItems Is Nothing OrElse allItems.Count = 0 Then
                Return
            End If

            currentPageIndex = pageIndex
            itemDataSource.Clear()

            Dim startIndex As Integer
            Dim endIndex As Integer

            ' Special handling for items between 8-14
            If allItems.Count > paginationTriggerThreshold AndAlso allItems.Count <= itemsPerPage Then
                ' All items fit on first page, second page is sections only
                If pageIndex = 0 Then
                    startIndex = 0
                    endIndex = allItems.Count
                Else
                    ' Second page - no items, just sections
                    dataGrid.ItemsSource = itemDataSource
                    UpdatePageInfo()
                    UpdateNavigationButtons()
                    UpdateNothingToFollowVisibility()
                    UpdateServicesVisibility()
                    UpdateTotalCostVisibility()
                    UpdateWarrantyAndBottomSectionVisibility()
                    UpdatePrintPageIndicator()
                    Return
                End If
            Else
                ' Normal pagination for items > 14 or <= 7
                startIndex = pageIndex * itemsPerPage
                endIndex = Math.Min(startIndex + itemsPerPage, allItems.Count)
            End If

            For i As Integer = startIndex To endIndex - 1
                Dim item = allItems(i)

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

                itemDataSource.Add(New OrderItems With {
            .Quantity = item("Quantity"),
            .Description = item("ProductName"),
            .UnitPrice = $"₱ {rateFormatted}",
            .LinePrice = $"₱ {linePriceFormatted}",
            .ProductImage = productImage
        })
            Next

            dataGrid.ItemsSource = itemDataSource

            ' Update UI
            UpdatePageInfo()
            UpdateNavigationButtons()
            UpdateNothingToFollowVisibility()
            UpdateServicesVisibility()
            UpdateTotalCostVisibility()
            UpdateWarrantyAndBottomSectionVisibility()
            UpdatePrintPageIndicator()
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
            ' You can add a TextBlock to show "Page 1 of 3" etc.
            ' This is optional but helpful
        End Sub

        ' Add navigation methods (if you want manual navigation)
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

            ' Calculate base amount
            Dim baseAmount As Decimal

            ' Calculate VAT12 for display only
            Dim vatAmount As Decimal = baseAmount * 0.12D
            VAT12.Text = "₱ " & vatAmount.ToString("N2")
            CostEstimateDetails.CETotalTaxValueCache = VAT12.Text
            Debug.WriteLine($"Computed VAT: {vatAmount}")

            ' Calculate total cost
            Dim totalCostVal As Decimal
            If CEtaxSelection Then
                ' Tax Exclusive: Add VAT to total cost

                ' Checks if the user choose to show hide the vat
                If CEisVatExInclude Then
                    vatAmount = subtotalAmount * 0.12D
                Else
                    vatAmount = 0D
                End If

                ' Display the value result
                VAT12.Text = "₱ " & vatAmount.ToString("N2")
                CostEstimateDetails.CETotalTaxValueCache = VAT12.Text
                Debug.WriteLine($"Computed VAT: {vatAmount}")

                totalCostVal = subtotalAmount + installationAmount + deliveryAmount + vatAmount
            Else
                ' Tax Inclusive: Do NOT add VAT to total cost
                vatAmount = subtotalAmount * 0.12D
                VAT12.Text = "₱ " & vatAmount.ToString("N2")
                CostEstimateDetails.CETotalTaxValueCache = VAT12.Text

                baseAmount = subtotalAmount + installationAmount + deliveryAmount
                totalCostVal = baseAmount
            End If
            Debug.WriteLine($"Computed Total: {totalCostVal}")

            ' Update the TotalCost display
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
            '' Reset upload progress
            'UploadProgressBar.Value = 0
            'UploadStatus.Text = "Uploading..."

            ' Update file info
            Dim fileInfo As New FileInfo(filePath)
            Dim fileSizeText As String = Base64Utility.GetReadableFileSize(fileInfo.Length)

            'ImgName.Text = Path.GetFileName(filePath)
            'ImgSize.Text = fileSizeText

            ' Convert image to Base64 using Base64Utility
            Try
                base64Image = Base64Utility.EncodeFileToBase64(filePath)
                ImageCache = base64Image
            Catch ex As Exception
                MessageBox.Show("Error encoding image: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Exit Sub
            End Try

            '' Show the panel with image info
            'ImageInfoPanel.Visibility = Visibility.Visible

            '' Disable browse button and drag-drop functionality
            'BtnBrowse.IsEnabled = False
            'DropBorder.AllowDrop = False
            'isUploadLocked = True

            '' Configure and start the timer
            'ConfigureUploadTimer()
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

                ViewLoader.DynamicView.NavigateToView("salesnewquote", Me)

            Catch ex As Exception
                MessageBox.Show("Occurred when the installation or delivery fields were empty or invalid. Treated as 0.")
                ViewLoader.DynamicView.NavigateToView("salesnewquote", Me)
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
            CostEstimateDetails.CEClientName = ClientNameBox.Text
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

            ViewLoader.DynamicView.NavigateToView("printpreviewquotes", Me)
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

    End Class
End Namespace