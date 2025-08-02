Imports System.Collections.ObjectModel
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
            End If

            QuoteNumber.Text = CostEstimateDetails.CEQuoteNumberCache
            QuoteDate.Text = CostEstimateDetails.CEQuoteDateCache
            QuoteValidityDate.Text = CostEstimateDetails.CEValidUntilDate
            Subtotal.Text = CostEstimateDetails.CETotalAmountCache
            TotalCost.Text = CostEstimateDetails.CETotalAmountCache
            Delivery.Text = "₱ " & CostEstimateDetails.CEDeliveryCost.ToString("N2")
            itemOrder = CostEstimateDetails.CEQuoteItemsCache
            base64Image = CostEstimateDetails.CEImageCache
            tempImagePath = CostEstimateDetails.CEPathCache
            ClientNameBox.Text = CostEstimateDetails.CEClientName
            VAT12.Text = CostEstimateDetails.CETaxValueCache
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

                If String.IsNullOrWhiteSpace(CostEstimateDetails.CEremarksTxt) OrElse
       remarksBox.Text = "Tax Inclusive." OrElse remarksBox.Text = "Tax Exclusive." Then
                    remarksBox.Text = "Tax Exclusive."
                End If
            Else
                VatText.Visibility = Visibility.Visible
                VatValue.Visibility = Visibility.Visible

                If String.IsNullOrWhiteSpace(CostEstimateDetails.CEremarksTxt) OrElse
       remarksBox.Text = "Tax Inclusive." OrElse remarksBox.Text = "Tax Exclusive." Then
                    remarksBox.Text = "Tax Inclusive."
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

            If Not tb.Text.StartsWith("₱ ") Then
                tb.Text = "₱ " & tb.Text.Replace("₱", "").TrimStart()
                tb.CaretIndex = tb.Text.Length
            End If

            AddHandler tb.TextChanged, AddressOf Delivery_TextChanged
            ComputeCost(sender, e)
        End Sub

        Private Sub Installation_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim tb As TextBox = TryCast(sender, TextBox)
            If tb Is Nothing Then Exit Sub

            RemoveHandler tb.TextChanged, AddressOf Installation_TextChanged

            If Not tb.Text.StartsWith("₱ ") Then
                tb.Text = "₱ " & tb.Text.Replace("₱", "").TrimStart()
                tb.CaretIndex = tb.Text.Length
            End If

            AddHandler tb.TextChanged, AddressOf Installation_TextChanged
            ComputeCost(sender, e)
        End Sub

        Private Sub ComputeCost(s As Object, e As TextChangedEventArgs)
            ' Only clean and compute — don't reset textbox text
            Dim deliveryAmount As Decimal = 0
            Decimal.TryParse(Delivery.Text.Replace("₱", "").Trim(), deliveryAmount)

            Dim installationAmount As Decimal = 0
            Decimal.TryParse(Installation.Text.Replace("₱", "").Trim(), installationAmount)

            Dim subtotalAmount As Decimal = 0
            Decimal.TryParse(CostEstimateDetails.CETotalAmountCache.Replace("₱", "").Trim(), subtotalAmount)


            ' Update the Subtotal text display
            Subtotal.Text = "₱ " & subtotalAmount.ToString("N2")

            ' Total = subtotal + delivery + installation
            Dim total = subtotalAmount + deliveryAmount + installationAmount

            ' Update the TotalCost display
            TotalCost.Text = "₱ " & total.ToString("N2")
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
            If Decimal.TryParse(Installation.Text.Replace("₱", "").Trim(), CEInstallation) = False Then
                CEInstallation = 0D ' fallback value
            End If
            If Decimal.TryParse(Delivery.Text.Replace("₱", "").Replace(",", "").Trim(), CEDeliveryCost) = False Then
                CEDeliveryCost = 0D ' fallback value if conversion fails
            End If
            CostEstimateDetails.CEpaperNote = noteBox.Text ' Changed 07/09/2025 to noteBox
            CostEstimateDetails.CEApproved = cmbApproved.Text
            CostEstimateDetails.CEpaymentTerms = cmbTerms.Text
            CostEstimateDetails.CESubtotalExInc = cmbSubtotalTax.Text
            CostEstimateDetails.CEWarranty = Warranty.Text
            CostEstimateDetails.CEDeliveryMobilization = cmbDeliveryMobilization.Text
            If CostEstimateDetails.CEisCustomTerm = True Then
                CostEstimateDetails.CEpaymentTerms = CustomTerms.Text
            Else
                CostEstimateDetails.CEpaymentTerms = cmbTerms.Text
            End If
            Debug.WriteLine($"Approved - {CostEstimateDetails.CEApproved}")
            ViewLoader.DynamicView.NavigateToView("salesnewquote", Me)
        End Sub

        Private Sub PrintPreview(sender As Object, e As RoutedEventArgs)

            ' Delivery Cost Conversion to Decimal
            Dim _deliveryCost As Decimal = 0
            Dim _installationCost As Decimal = 0

            Decimal.TryParse(Delivery.Text.Replace("₱", "").Trim(), _deliveryCost)
            Decimal.TryParse(Installation.Text.Replace("₱", "").Trim(), _installationCost)

            CostEstimateDetails.CEQuoteNumberCache = QuoteNumber.Text
            CostEstimateDetails.CEQuoteDateCache = QuoteDate.Text
            CostEstimateDetails.CEValidUntilDate = QuoteValidityDate.Text
            CostEstimateDetails.CETotalAmountCache = TotalCost.Text
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