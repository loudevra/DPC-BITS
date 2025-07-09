Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports Microsoft.Win32
Imports SkiaSharp.Views.WPF

Namespace DPC.Views.Stocks.PurchaseOrder.WalkIn
    Public Class WalkInBillingStatement

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

        Private Sub WalkInBillilingStatement_Loaded(sender As Object, e As RoutedEventArgs)
            Dim priceString As String = WalkinBillingStatementDetails.BLTotalAmountCache
            Dim cleaned As String = priceString.Replace("₱", "").Replace(",", "").Trim()
            Dim numericValue As Double = Double.Parse(cleaned)
            Dim vat As Double = (numericValue - (numericValue / 1.12))

            Dim subTot As Double = (numericValue / 1.12)
            Dim totCost As Double = vat + subTot


            ' Check if important fields are initialized
            If String.IsNullOrEmpty(WalkinBillingStatementDetails.BLNumberCache) Then
                MessageBox.Show("Quote Number is missing.")
                Return
            End If

            If WalkinBillingStatementDetails.BLItemsCache Is Nothing Then
                MessageBox.Show("Quote items are not loaded.")
                Return
            End If

            Dim installationFee As Decimal
            If Decimal.TryParse(WalkinBillingStatementDetails.BLInstallation, installationFee) Then
                Installation.Text = "₱ " & installationFee.ToString("N2")
            Else
                Installation.Text = "₱ 0.00" ' fallback value if parsing fails
            End If

            billingNumber.Text = BLNumberCache
            billingDate.Text = BLDateCache
            txtDRNo.Text = BLNumberCache.Replace("BL", "DR")

            ClientNameBox.Text = BLClientName
            ClientAddress.Text = BLAddress & ", " & BLCity & ", " & BLRegion & ", " & BLCountry
            ClientContact.Text = "+63 " & FormatPhoneWithSpaces(BLPhone)
            CompanyRep.Text = BLCompanyRep

            SalesRep.Text = BLSalesRep
            PreparedBy.Text = CacheOnLoggedInName
            cmbApproved.Text = WalkinBillingStatementDetails.BLApproved
            ' Check if the terms is enabled
            If WalkinBillingStatementDetails.BLisCustomTerm = True Then
                CustomTerms.Text = WalkinBillingStatementDetails.BLpaymentTerms
                cmbTerms.SelectedIndex = 6
            Else
                cmbTerms.Text = WalkinBillingStatementDetails.BLpaymentTerms
            End If

            itemOrder = WalkinBillingStatementDetails.BLItemsCache
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

            base64Image = WalkinBillingStatementDetails.BLImageCache
            tempImagePath = WalkinBillingStatementDetails.BLPathCache

            ' Check if the signature is enabled
            If Not String.IsNullOrWhiteSpace(base64Image) Then
                DisplayUploadedImage()
            End If

            If BLTaxProperty = "Exclusive" Then
                Subtotal.Text = "₱ " & totCost.ToString("N2")
            Else
                Subtotal.Text = "₱ " & subTot.ToString("N2")
            End If
            Delivery.Text = "₱ " & WalkinBillingStatementDetails.BLDeliveryCost.ToString("N2")
            VAT12.Text = BLTotalTaxValueCache
            TotalCost.Text = "₱ " & totCost.ToString("N2")

            remarksBox.Text = WalkinBillingStatementDetails.BLremarksTxt
            Term1.Text = WalkinBillingStatementDetails.BLTerm1
            Term2.Text = WalkinBillingStatementDetails.BLTerm2
            Term3.Text = WalkinBillingStatementDetails.BLTerm3
            Term4.Text = WalkinBillingStatementDetails.BLTerm4
            Term5.Text = WalkinBillingStatementDetails.BLTerm5
            Term6.Text = WalkinBillingStatementDetails.BLTerm6
            Term7.Text = WalkinBillingStatementDetails.BLTerm7
            Term8.Text = WalkinBillingStatementDetails.BLTerm8
            Term9.Text = WalkinBillingStatementDetails.BLTerm9
            Term10.Text = WalkinBillingStatementDetails.BLTerm10
            Term11.Text = WalkinBillingStatementDetails.BLTerm11
            Term12.Text = WalkinBillingStatementDetails.BLTerm12

            AddHandler BrowseFile.MouseLeftButtonUp, AddressOf OpenFiles
        End Sub

#Region "Computation Part"

        Private Sub Delivery_TextChanged(sender As Object, e As TextChangedEventArgs) Handles Delivery.TextChanged
            '' Clean the text input to remove currency symbol and commas and compute the total costs
            ComputeCost(sender, e)
        End Sub


        Private Sub Delivery_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles Delivery.PreviewTextInput
            Dim tb = TryCast(sender, TextBox)
            If tb IsNot Nothing Then
                ' Simulate what the text will look like if the character is accepted
                Dim futureText = tb.Text.Insert(tb.CaretIndex, e.Text)

                ' Only allow digits and at most one decimal point
                Dim isValid = futureText.All(Function(c) Char.IsDigit(c) OrElse c = "."c) AndAlso
                      futureText.Count(Function(c) c = "."c) <= 1

                ' Disallow starting with a decimal point
                If futureText.StartsWith("."c) Then
                    isValid = False
                End If

                e.Handled = Not isValid
            End If
        End Sub

        Private Sub Delivery_LostFocus(sender As Object, e As RoutedEventArgs) Handles Delivery.LostFocus
            ' Format the text as currency when the textbox loses focus
            Dim tb = TryCast(sender, TextBox)
            If tb IsNot Nothing Then
                Dim value As Double
                If Double.TryParse(tb.Text, value) Then
                    tb.Text = "₱ " & value.ToString("N2") ' ₱ format
                End If
            End If
        End Sub

        Private Sub Installation_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles Installation.PreviewTextInput
            '' Validate the input to allow only digits and at most one decimal point
            Dim tb = TryCast(sender, TextBox)
            If tb IsNot Nothing Then
                ' Simulate what the text will look like if the character is accepted
                Dim futureText = tb.Text.Insert(tb.CaretIndex, e.Text)

                ' Only allow digits and at most one decimal point
                Dim isValid = futureText.All(Function(c) Char.IsDigit(c) OrElse c = "."c) AndAlso
                      futureText.Count(Function(c) c = "."c) <= 1

                ' Disallow starting with a decimal point
                If futureText.StartsWith("."c) Then
                    isValid = False
                End If

                e.Handled = Not isValid
            End If
        End Sub

        Private Sub Installation_LostFocus(sender As Object, e As RoutedEventArgs) Handles Installation.LostFocus
            ' Format the text as currency when the textbox loses focus
            Dim tb = TryCast(sender, TextBox)
            If tb IsNot Nothing Then
                Dim value As Double
                If Double.TryParse(tb.Text, value) Then
                    tb.Text = "₱ " & value.ToString("N2") ' ₱ format
                End If
            End If
        End Sub


        Private Sub Installation_TextChanged(sender As Object, e As TextChangedEventArgs)
            '' Clean the text input to remove currency symbol and commas and compute the total costs
            ComputeCost(sender, e)
        End Sub

        Private Sub ComputeCost(s As Object, e As TextChangedEventArgs)
            ' Only clean and compute — don't reset textbox text
            Dim deliveryAmount As Double = 0
            Double.TryParse(Delivery.Text.Replace("₱", "").Trim(), deliveryAmount)

            Dim installationAmount As Double = 0
            Double.TryParse(Installation.Text.Replace("₱", "").Trim(), installationAmount)

            Dim subtotalAmount As Double = Convert.ToDouble(Subtotal.Text.Replace("₱", "").Replace(",", "").Trim())
            Dim vat As Double = Convert.ToDouble(VAT12.Text.Replace("₱", "").Replace(",", "").Trim())

            ' Total = subtotal + delivery + installation
            Dim total = subtotalAmount + vat + deliveryAmount + installationAmount

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
            WalkinBillingStatementDetails.BLsignature = True
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
        ' Going back to the WalkinOrder View
        Private Sub BackToUI_Click(sender As Object, e As MouseButtonEventArgs)
            If Decimal.TryParse(Installation.Text.Replace("₱", "").Trim(), BLInstallation) = False Then
                BLInstallation = 0D ' fallback value
            End If
            If Decimal.TryParse(Delivery.Text.Replace("₱", "").Replace(",", "").Trim(), BLDeliveryCost) = False Then
                BLDeliveryCost = 0D ' fallback value if conversion fails
            End If
            WalkinBillingStatementDetails.BLApproved = cmbApproved.Text
            WalkinBillingStatementDetails.BLpaymentTerms = cmbTerms.Text
            If WalkinBillingStatementDetails.BLisCustomTerm = True Then
                WalkinBillingStatementDetails.BLpaymentTerms = CustomTerms.Text
            Else
                WalkinBillingStatementDetails.BLpaymentTerms = cmbTerms.Text
            End If
            Debug.WriteLine($"Approved - {WalkinBillingStatementDetails.BLApproved}")
            ViewLoader.DynamicView.NavigateToView("walkinorder", Me)
        End Sub

        Private Sub PrintPreview(sender As Object, e As RoutedEventArgs)

            If String.IsNullOrWhiteSpace(PreparedBy.Text) OrElse
               String.IsNullOrWhiteSpace(SalesRep.Text) OrElse
               cmbApproved.SelectedItem Is Nothing OrElse
               cmbTerms.SelectedItem Is Nothing Then

                MessageBox.Show("Please fill in all required fields before proceeding to print.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            Else
                ' Delivery Cost Conversion to Decimal
                Dim _deliveryCost As Decimal = 0
                Dim _installationCost As Decimal = 0

                Decimal.TryParse(Delivery.Text.Replace("₱", "").Trim(), _deliveryCost)
                Decimal.TryParse(Installation.Text.Replace("₱", "").Trim(), _installationCost)

                WalkinBillingStatementDetails.BLNumberCache = billingNumber.Text
                WalkinBillingStatementDetails.BLDateCache = billingDate.Text
                WalkinBillingStatementDetails.BLDRNo = txtDRNo.Text

                WalkinBillingStatementDetails.BLClientName = ClientNameBox.Text
                WalkinBillingStatementDetails.BLCompleteAddress = ClientAddress.Text
                WalkinBillingStatementDetails.BLClientContact = ClientContact.Text
                WalkinBillingStatementDetails.BLCompanyRep = CompanyRep.Text

                WalkinBillingStatementDetails.BLSalesRep = SalesRep.Text
                WalkinBillingStatementDetails.BLApproved = cmbApproved.Text
                WalkinBillingStatementDetails.BLpaymentTerms = cmbTerms.Text
                If WalkinBillingStatementDetails.BLisCustomTerm = True Then
                    WalkinBillingStatementDetails.BLpaymentTerms = CustomTerms.Text
                Else
                    WalkinBillingStatementDetails.BLpaymentTerms = cmbTerms.Text
                End If

                WalkinBillingStatementDetails.BLItemsCache = itemOrder

                WalkinBillingStatementDetails.BLImageCache = base64Image
                WalkinBillingStatementDetails.BLPathCache = tempImagePath

                WalkinBillingStatementDetails.BLSubTotalCache = Subtotal.Text
                WalkinBillingStatementDetails.BLInstallation = _installationCost
                WalkinBillingStatementDetails.BLDeliveryCost = _deliveryCost
                WalkinBillingStatementDetails.BLTotalTaxValueCache = VAT12.Text
                WalkinBillingStatementDetails.BLTotalAmountCache = TotalCost.Text

                WalkinBillingStatementDetails.BLBankDetails = BankDetailBox.Text
                WalkinBillingStatementDetails.BLAccountName = AccNameBox.Text
                WalkinBillingStatementDetails.BLAccountNumber = AccNoBox.Text

                WalkinBillingStatementDetails.BLremarksTxt = remarksBox.Text
                WalkinBillingStatementDetails.BLTerm1 = Term1.Text
                WalkinBillingStatementDetails.BLTerm2 = Term2.Text
                WalkinBillingStatementDetails.BLTerm3 = Term3.Text
                WalkinBillingStatementDetails.BLTerm4 = Term4.Text
                WalkinBillingStatementDetails.BLTerm5 = Term5.Text
                WalkinBillingStatementDetails.BLTerm6 = Term6.Text
                WalkinBillingStatementDetails.BLTerm7 = Term7.Text
                WalkinBillingStatementDetails.BLTerm8 = Term8.Text
                WalkinBillingStatementDetails.BLTerm9 = Term9.Text
                WalkinBillingStatementDetails.BLTerm10 = Term10.Text
                WalkinBillingStatementDetails.BLTerm11 = Term11.Text
                WalkinBillingStatementDetails.BLTerm12 = Term12.Text

                ViewLoader.DynamicView.NavigateToView("previewwalkinclientprintstatement", Me)
            End If
        End Sub
#End Region

#Region "Text Editor Function"
        Private Sub TextEditorPopOut(sender As Object, e As MouseButtonEventArgs)
            element = TryCast(sender, FrameworkElement)
            Dim elementText = DirectCast(element, TextBlock).Text

            Dim txtEditor As New PopoutWalkInTextEditor(elementText)
            txtEditor.ShowDialog()
        End Sub

        Public Shared Sub ModifyText(textEdited As String)
            DirectCast(element, TextBlock).Text = textEdited
        End Sub

        Private Sub cmbTerms_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If cmbTerms.SelectedIndex = 6 Then
                WalkinBillingStatementDetails.BLisCustomTerm = True
            Else
                WalkinBillingStatementDetails.BLisCustomTerm = False
            End If
        End Sub
#End Region
    End Class
End Namespace