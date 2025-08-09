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
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

        End Sub

        Private Sub CostEstimate_Loaded(sender As Object, e As RoutedEventArgs)
            ' Check if important fields are initialized
            If String.IsNullOrEmpty(CostEstimateDetails.CEQuoteNumberCache) Then
                MessageBox.Show("Quote Number is missing.")
                Return
            End If

            If CostEstimateDetails.CEQuoteItemsCache Is Nothing Then
                MessageBox.Show("Quote items are not loaded.")
                Return
            End If

            Dim installationFee As Decimal
            If Decimal.TryParse(CostEstimateDetails.CEInstallation, installationFee) Then
                Installation.Text = "₱ " & installationFee.ToString("N2")
            Else
                Installation.Text = "₱ 0.00" ' fallback value if parsing fails
                installationFee = 0D
            End If

            Dim deliveryCost As Decimal
            If Decimal.TryParse(CostEstimateDetails.CEDeliveryCost, deliveryCost) Then
                Delivery.Text = "₱ " & deliveryCost.ToString("N2")
            Else
                Delivery.Text = "₱ 0.00" ' fallback value if parsing fails
                deliveryCost = 0D
            End If

            QuoteNumber.Text = CostEstimateDetails.CEQuoteNumberCache
            QuoteDate.Text = CostEstimateDetails.CEQuoteDateCache
            QuoteValidityDate.Text = CostEstimateDetails.CEValidUntilDate
            Subtotal.Text = CostEstimateDetails.CETotalAmountCache
            TotalCost.Text = CostEstimateDetails.CETotalAmountCache
            itemOrder = CostEstimateDetails.CEQuoteItemsCache
            base64Image = CostEstimateDetails.CEImageCache
            tempImagePath = CostEstimateDetails.CEPathCache
            ClientNameBox.Text = CostEstimateDetails.CEClientName
            ' moved the vat12% to 
            CESubTotalCache = CostEstimateDetails.CETotalAmountCache
            AddressLineOne.Text = CostEstimateDetails.CEAddress & ", " & CostEstimateDetails.CECity    ' -- important when editing
            AddressLineTwo.Text = CostEstimateDetails.CERegion & ", " & CostEstimateDetails.CECountry    ' -- important when editing
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
            cmbSubtotalTax.Text = CostEstimateDetails.CESubtotalExInc ' Added in the combobox when editing if exist
            Warranty.Text = CostEstimateDetails.CEWarranty ' If there is changed the value will be rendered if not the default value will be rendered
            cmbDeliveryMobilization.Text = CostEstimateDetails.CEDeliveryMobilization
            CNIdentifier.Text = CostEstimateDetails.CECNIndetifier

            ' Check if the terms is enabled
            If CostEstimateDetails.CEisCustomTerm = True Then
                CustomTerms.Text = CostEstimateDetails.CEpaymentTerms
                cmbTerms.SelectedIndex = 6
            Else
                cmbTerms.Text = CostEstimateDetails.CEpaymentTerms
            End If

            If CEtaxSelection Then
                VatText.Visibility = Visibility.Collapsed
                VatValue.Visibility = Visibility.Collapsed
                VAT12.Text = "₱ 0.00"
                If String.IsNullOrWhiteSpace(CostEstimateDetails.CEremarksTxt) OrElse remarksBox.Text = "Tax Inclusive." OrElse remarksBox.Text = "Tax Exclusive." Then
                    remarksBox.Text = "Tax Excluded."
                End If
                ' Compute total without VAT for tax exclusive case
                Dim totalAmount As Decimal = 0
                Dim rawTotal = CostEstimateDetails.CETotalAmountCache.Trim().Replace("₱", "").Replace(",", "").Trim()
                If Decimal.TryParse(rawTotal, totalAmount) Then
                    Dim totalCostValue As Decimal = totalAmount + installationFee + deliveryCost
                    If TotalCost IsNot Nothing Then
                        TotalCost.Text = "₱ " & totalCostValue.ToString("F2")
                        CostEstimateDetails.CETotalAmountCache = "₱ " & totalAmount.ToString("F2") ' Preserve pre-additional-cost subtotal
                    Else
                        Debug.WriteLine("TotalCost is Nothing")
                    End If
                Else
                    Debug.WriteLine("Failed to parse CETotalAmountCache: '{rawTotal}'")
                    If TotalCost IsNot Nothing Then
                        TotalCost.Text = "₱ 0.00"
                    End If
                End If
            Else
                VatText.Visibility = Visibility.Visible
                VatValue.Visibility = Visibility.Visible
                If String.IsNullOrWhiteSpace(CostEstimateDetails.CEremarksTxt) OrElse remarksBox.Text = "Tax Inclusive." OrElse remarksBox.Text = "Tax Exclusive." Then
                    remarksBox.Text = "Tax Included."
                End If
                If Not String.IsNullOrWhiteSpace(CostEstimateDetails.CETaxValueCache) Then
                    Debug.WriteLine($"CETaxValueCache: '{CostEstimateDetails.CETaxValueCache}'")
                    Debug.WriteLine($"CETotalAmountCache: '{CostEstimateDetails.CETotalAmountCache}'")
                    Dim newVat12Value As Decimal
                    Dim totalAmount As Decimal
                    Dim rawTotal = CostEstimateDetails.CETotalAmountCache.Trim().Replace("₱", "").Replace(",", "").Trim()
                    If Decimal.TryParse(rawTotal, totalAmount) Then
                        ' Compute base amount including installation and delivery
                        Dim baseAmount As Decimal = totalAmount + installationFee + deliveryCost
                        newVat12Value = baseAmount * 0.12D
                        Debug.WriteLine($"NewVat12Value - {newVat12Value}")
                        If VAT12 IsNot Nothing Then
                            VAT12.Text = "₱ " & newVat12Value.ToString("F2")
                            CostEstimateDetails.CETotalTaxValueCache = VAT12.Text
                            Dim totalCostValue As Decimal = baseAmount + newVat12Value
                            If TotalCost IsNot Nothing Then
                                TotalCost.Text = "₱ " & totalCostValue.ToString("F2")
                                CostEstimateDetails.CETotalAmountCache = "₱ " & baseAmount.ToString("F2") ' Store pre-VAT total
                            Else
                                Debug.WriteLine("TotalCost is Nothing")
                            End If
                        Else
                            Debug.WriteLine("VAT12 is Nothing")
                        End If
                    Else
                        Debug.WriteLine("Failed to parse CETotalAmountCache: '{rawTotal}'")
                        newVat12Value = 0D
                        If VAT12 IsNot Nothing Then
                            VAT12.Text = "₱ 0.00"
                        End If
                    End If
                End If
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
            Dim baseAmount As Decimal = subtotalAmount + installationAmount + deliveryAmount

            ' Calculate VAT12 based on CEtaxSelection
            Dim vatAmount As Decimal = 0
            If CEtaxSelection Then
                ' Tax Exclusive
                vatAmount = baseAmount
                VAT12.Text = "₱ " & vatAmount.ToString("F2")
                CostEstimateDetails.CETotalTaxValueCache = VAT12.Text
                Debug.WriteLine($"Computed VAT (Exclusive): ")
            Else
                ' Tax Inclusive
                vatAmount = baseAmount * 0.12D
                VAT12.Text = "₱ " & vatAmount.ToString("F2")
                CostEstimateDetails.CETotalTaxValueCache = VAT12.Text
                Debug.WriteLine($"Computed VAT (Inclusive): {vatAmount}")
            End If

            ' Calculate total cost
            Dim totalCostVal As Decimal = baseAmount
            Debug.WriteLine($"Computed Total: {totalCostVal}")

            ' Update the TotalCost display (TotalCost should be a TextBox/TextBlock)
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
    End Class
End Namespace