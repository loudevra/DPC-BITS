Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Windows.Markup
Imports DocumentFormat.OpenXml.Bibliography
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports Newtonsoft.Json

Namespace DPC.Components.Forms
    Public Class PreviewPrintStatement

        Private tempImagePath As String
        Private base64Image As String
        Private itemDataSource As New ObservableCollection(Of OrderItems)
        Private itemOrder As New List(Of Dictionary(Of String, String))
        Private Address As String
        Private isCustom As Boolean

        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.


            InvoiceNumber.Text = StatementDetails.InvoiceNumberCache
            InvoiceDate.Text = StatementDetails.InvoiceDateCache
            DueDate.Text = StatementDetails.DueDateCache
            Tax.Text = "₱ " & StatementDetails.TaxCache.ToString("N2")
            TotalCost.Text = "₱ " & (StatementDetails.TotalCostCache + DeliveryCost).ToString("N2")
            Delivery.Text = "₱ " & StatementDetails.DeliveryCost.ToString("N2")
            itemOrder = StatementDetails.OrderItemsCache
            base64Image = StatementDetails.ImageCache
            tempImagePath = StatementDetails.PathCache
            SupplierNameBox.Text = StatementDetails.SupplierName
            AddressLineOne.Text = StatementDetails.City & ", " & StatementDetails.Region
            AddressLineTwo.Text = StatementDetails.Country
            PhoneBox.Text = "Tel No: " & StatementDetails.Phone
            EmailBox.Text = StatementDetails.Email
            noteBox.Text = StatementDetails.noteTxt
            remarksBox.Text = StatementDetails.remarksTxt
            Term1.Text = StatementDetails.Term1
            Term2.Text = StatementDetails.Term2
            Term3.Text = StatementDetails.Term3
            Term4.Text = StatementDetails.Term4
            Term5.Text = StatementDetails.Term5
            Term6.Text = StatementDetails.Term6
            Term7.Text = StatementDetails.Term7
            Term8.Text = StatementDetails.Term8
            Term9.Text = StatementDetails.Term9
            Term10.Text = StatementDetails.Term10
            Term11.Text = StatementDetails.Term11
            Term12.Text = StatementDetails.Term12
            SalesRep.Text = CacheOnLoggedInName
            PaymentTerms.Text = StatementDetails.paymentTerms
            isCustom = StatementDetails.isCustomTerm
            Approved.Text = StatementDetails.Approved

            If StatementDetails.signature = False Then
                BrowseFile.Child = Nothing
            Else
                DisplayUploadedImage()
            End If

            For Each item In StatementDetails.OrderItemsCache
                Dim rate As Decimal = item("Rate")
                Dim rateFormatted As String = rate.ToString("N2")

                Dim linePrice As Decimal = item("Price")
                Dim linePriceFormatted As String = linePrice.ToString("N2")

                itemDataSource.Add(New OrderItems With {
                        .Quantity = item("Quantity"),
                        .Description = item("ItemName"),
                        .UnitPrice = rateFormatted,
                        .LinePrice = linePriceFormatted
                    })
            Next


            dataGrid.ItemsSource = itemDataSource

            AddHandler SaveDb.Click, AddressOf SaveToDB
        End Sub

        Private Sub CancelButton(sender As Object, e As RoutedEventArgs)
            Dim _city, _region, _phone As String

            _city = StatementDetails.City
            _region = StatementDetails.Region
            _phone = StatementDetails.Phone

            Dim _deliveryCost, _tax, _totalCost As Decimal

            _deliveryCost = StatementDetails.DeliveryCost
            _tax = StatementDetails.TaxCache
            _totalCost = StatementDetails.TotalCostCache

            StatementDetails.paymentTerms = PaymentTerms.Text

            StatementDetails.signature = If(String.IsNullOrWhiteSpace(base64Image), False, True)
            StatementDetails.InvoiceNumberCache = InvoiceNumber.Text
            StatementDetails.InvoiceDateCache = InvoiceDate.Text
            StatementDetails.DueDateCache = DueDate.Text
            StatementDetails.TaxCache = _tax
            StatementDetails.TotalCostCache = _totalCost
            StatementDetails.OrderItemsCache = itemOrder
            StatementDetails.ImageCache = base64Image
            StatementDetails.PathCache = tempImagePath
            StatementDetails.SupplierName = SupplierNameBox.Text
            StatementDetails.City = _city
            StatementDetails.Region = _region
            StatementDetails.Country = AddressLineTwo.Text
            StatementDetails.Phone = _phone
            StatementDetails.Email = EmailBox.Text
            StatementDetails.noteTxt = noteBox.Text
            StatementDetails.remarksTxt = remarksBox.Text
            StatementDetails.Term1 = Term1.Text
            StatementDetails.Term2 = Term2.Text
            StatementDetails.Term3 = Term3.Text
            StatementDetails.Term4 = Term4.Text
            StatementDetails.Term5 = Term5.Text
            StatementDetails.Term6 = Term6.Text
            StatementDetails.Term7 = Term7.Text
            StatementDetails.Term8 = Term8.Text
            StatementDetails.Term9 = Term9.Text
            StatementDetails.Term10 = Term10.Text
            StatementDetails.Term11 = Term11.Text
            StatementDetails.Term12 = Term12.Text
            StatementDetails.DeliveryCost = _deliveryCost
            StatementDetails.isCustomTerm = isCustom
            StatementDetails.Approved = Approved.Text

            ViewLoader.DynamicView.NavigateToView("purchaseorderstatement", Me)
        End Sub

        Private Sub SavePrint(sender As Object, e As RoutedEventArgs)
            Dim dlg As New PrintDialog()
            Dim docName As String = "PurchaseOrder-" & DateTime.Now.ToString("yyyyMMdd-HHmmss")

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

                Dim borderWidth = PrintPreview.ActualWidth
                Dim borderHeight = PrintPreview.ActualHeight

                ' Get printable area
                Dim printableWidth = dlg.PrintableAreaWidth
                Dim printableHeight = dlg.PrintableAreaHeight

                ' Calculate scale factor
                Dim scaleX = printableWidth / borderWidth
                Dim scaleY = printableHeight / borderHeight
                Dim scale = Math.Min(scaleX, scaleY)

                ' Use your existing "container" Grid name
                Dim container As New Grid()
                container.LayoutTransform = New ScaleTransform(scale, scale)
                container.Children.Add(PrintPreview)

                container.Measure(New Size(printableWidth, printableHeight))
                container.Arrange(New Rect(New Point(0, 0), New Size(printableWidth, printableHeight)))
                container.UpdateLayout()

                ' Wrap in a FixedDocument to ensure full fidelity
                Dim fixedPage As New FixedPage()
                fixedPage.Width = printableWidth
                fixedPage.Height = printableHeight
                fixedPage.Children.Add(container)

                Dim pageContent As New PageContent()
                CType(pageContent, IAddChild).AddChild(fixedPage)

                Dim fixedDoc As New FixedDocument()
                fixedDoc.Pages.Add(pageContent)

                ' Print using DocumentPaginator (works for printer and PDF)
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

            SaveToDB()
        End Sub

        Private Sub ClearCache()
            StatementDetails.InvoiceNumberCache = Nothing
            StatementDetails.InvoiceDateCache = Nothing
            StatementDetails.DueDateCache = Nothing
            StatementDetails.TaxCache = Nothing
            StatementDetails.TotalCostCache = Nothing
            StatementDetails.OrderItemsCache = Nothing
            StatementDetails.signature = False
            StatementDetails.ImageCache = Nothing
            StatementDetails.PathCache = Nothing
            StatementDetails.SupplierName = Nothing
            StatementDetails.City = Nothing
            StatementDetails.Region = Nothing
            StatementDetails.Country = Nothing
            StatementDetails.Phone = Nothing
            StatementDetails.Email = Nothing
            StatementDetails.noteTxt = Nothing
            StatementDetails.remarksTxt = Nothing
            StatementDetails.paymentTerms = Nothing
            StatementDetails.Term1 = Nothing
            StatementDetails.Term2 = Nothing
            StatementDetails.Term3 = Nothing
            StatementDetails.Term4 = Nothing
            StatementDetails.Term5 = Nothing
            StatementDetails.Term6 = Nothing
            StatementDetails.Term7 = Nothing
            StatementDetails.Term8 = Nothing
            StatementDetails.Term9 = Nothing
            StatementDetails.Term10 = Nothing
            StatementDetails.Term11 = Nothing
            StatementDetails.Term12 = Nothing
            StatementDetails.paymentTerms = Nothing
            StatementDetails.isCustomTerm = Nothing
            StatementDetails.Approved = Nothing
        End Sub

        Private Sub SaveToDB()
            Dim itemsJSON As String = JsonConvert.SerializeObject(itemOrder, Formatting.Indented)
            Dim InvoiceDate As String = Date.Now.ToString("yyyy-MM-dd")
            Dim DeliveryString As String = Delivery.Text.Trim("₱"c, " "c)
            Dim TaxString As String = Tax.Text.Trim("₱"c, " "c)
            Dim TotalCostString As String = TotalCost.Text.Trim("₱"c, " "c)

            Dim DateParsed As Date = Date.Parse(StatementDetails.DueDateCache)
            Dim DueDateFormatted As String = DateParsed.ToString("yyyy-MM-dd")

            For Each child In BillAddress.Children
                Address += child.Text & Environment.NewLine
            Next

            Dim success As Boolean = PurchaseOrderController.InsertInvoicePurchaseOrder(InvoiceNumber.Text, InvoiceDate, DueDateFormatted, Address, itemsJSON, DeliveryString, TaxString, TotalCostString, base64Image, Approved.Text, PaymentTerms.Text, noteBox.Text)

            If success Then
                MessageBox.Show("Added to database")
            End If

            ClearCache()
            ViewLoader.DynamicView.NavigateToView("neworder", Me)
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
    End Class

End Namespace


