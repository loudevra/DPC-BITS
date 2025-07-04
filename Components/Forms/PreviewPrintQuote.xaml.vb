Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Windows.Markup
Imports DocumentFormat.OpenXml.Bibliography
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Views.Sales.Quotes
Imports Newtonsoft.Json

Namespace DPC.Components.Forms
    Public Class PreviewPrintQuote

        Private tempImagePath As String
        Private base64Image As String
        Private itemDataSource As New ObservableCollection(Of OrderItems)
        Private itemOrder As New List(Of Dictionary(Of String, String))
        Private Address As String
        Private isCustom As Boolean

        Public Sub New()

            ' This call is required by the designer.
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
            QuoteValidityDate.Text = CostEstimateDetails.CEQuoteValidityDateCache
            Subtotal.Text = CostEstimateDetails.CESubTotalCache
            TotalCost.Text = CostEstimateDetails.CETotalAmountCache
            Delivery.Text = "₱ " & CostEstimateDetails.CEDeliveryCost.ToString("N2")
            itemOrder = CostEstimateDetails.CEQuoteItemsCache
            base64Image = CostEstimateDetails.CEImageCache
            tempImagePath = CostEstimateDetails.CEPathCache
            ClientNameBox.Text = CostEstimateDetails.CEClientName
            AddressLineOne.Text = CostEstimateDetails.CEAddress & ", " & CostEstimateDetails.CECity    ' -- important when editing
            AddressLineTwo.Text = CostEstimateDetails.CERegion & ", " & CostEstimateDetails.CECountry    ' -- important when editing
            PhoneBox.Text = "Tel No.: +63 " & FormatPhoneWithSpaces(CostEstimateDetails.CEPhone)
            EmailBox.Text = CostEstimateDetails.CEEmail
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
            SalesRep.Text = CacheOnLoggedInName
            cmbApproved.Text = CostEstimateDetails.CEApproved

            ' Check if the terms is enabled
            If CostEstimateDetails.CEisCustomTerm = True Then
                cmbTerms.Text = CostEstimateDetails.CEpaymentTerms
                cmbTerms.Foreground = Brushes.Red
            Else
                cmbTerms.Text = CostEstimateDetails.CEpaymentTerms
            End If

            If CostEstimateDetails.CEsignature = False Then
                BrowseFile.Child = Nothing
            Else
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

            AddHandler SaveDb.Click, AddressOf SaveToDb
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

        Private Sub CancelButton(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("costestimate", Me)
        End Sub

        Private Sub SavePrint(sender As Object, e As RoutedEventArgs)
            Dim dlg As New PrintDialog()
            Dim docName As String = "CostEstimate-" & DateTime.Now.ToString("yyyyMMdd-HHmmss")

            If dlg.ShowDialog() = True Then
                ' Save original parent and layout
                Dim originalParent = VisualTreeHelper.GetParent(PrintPreview)
                Dim originalIndex As Integer = -1
                Dim originalMargin = PrintPreview.Margin
                Dim originalTransform = PrintPreview.LayoutTransform

                ' Detach from parent if it's inside a Panel
                If TypeOf originalParent Is Panel Then
                    Dim panel = CType(originalParent, Panel)
                    originalIndex = panel.Children.IndexOf(PrintPreview)
                    panel.Children.Remove(PrintPreview)
                End If

                ' Remove margin and reset transform
                PrintPreview.Margin = New Thickness(0)
                PrintPreview.LayoutTransform = Transform.Identity

                ' Ensure full layout and rendering
                PrintPreview.UpdateLayout()
                PrintPreview.Measure(New Size(Double.PositiveInfinity, Double.PositiveInfinity))
                PrintPreview.Arrange(New Rect(PrintPreview.DesiredSize))
                PrintPreview.UpdateLayout()

                ' Define A4 size in device independent pixels (96 DPI)
                Dim A4Width As Double = 8.3 * 96   ' ~794 pixels
                Dim A4Height As Double = 11.69 * 96 ' ~1123 pixels

                Dim borderWidth = PrintPreview.ActualWidth
                Dim borderHeight = PrintPreview.ActualHeight

                ' Calculate scale factor to fit PrintPreview inside A4
                Dim scaleX = A4Width / borderWidth
                Dim scaleY = A4Height / borderHeight
                Dim scale = Math.Min(scaleX, scaleY)

                ' Create container with fixed A4 size
                Dim container As New Grid()
                container.Width = A4Width
                container.Height = A4Height
                container.LayoutTransform = New ScaleTransform(scale, scale)
                container.Children.Add(PrintPreview)

                container.Measure(New Size(A4Width, A4Height))
                container.Arrange(New Rect(New Point(0, 0), New Size(A4Width, A4Height)))
                container.UpdateLayout()

                ' Prepare fixed page with A4 dimensions
                Dim fixedPage As New FixedPage()
                fixedPage.Width = A4Width
                fixedPage.Height = A4Height
                fixedPage.Children.Add(container)

                Dim pageContent As New PageContent()
                CType(pageContent, IAddChild).AddChild(fixedPage)

                Dim fixedDoc As New FixedDocument()
                fixedDoc.Pages.Add(pageContent)

                ' Print the document
                dlg.PrintDocument(fixedDoc.DocumentPaginator, docName)

                ' Restore original layout
                container.Children.Clear()
                PrintPreview.LayoutTransform = originalTransform
                PrintPreview.Margin = originalMargin

                If TypeOf originalParent Is Panel AndAlso originalIndex >= 0 Then
                    Dim panel = CType(originalParent, Panel)
                    panel.Children.Insert(originalIndex, PrintPreview)
                End If
            End If

            SaveToDb()

        End Sub

        Public Sub DisplayUploadedImage()
            Try
                'Dim tempImagePath As String = Path.Combine(Path.GetTempPath(), "decoded_image.png")

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
                ' Unregister all textbox names before clearing UI to avoid duplicate name errors
                Dim quoteForm As New DPC.Views.Sales.Quotes.NewQuote()
                quoteForm.ClearAllFields()
                CostEstimateDetails.ClearAllCECache()
                ViewLoader.DynamicView.NavigateToView("newquote", Me)
            Else
                MessageBox.Show("Failed to submit quote.")
            End If
        End Sub
    End Class
End Namespace