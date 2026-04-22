Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.DataModules
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports Microsoft.Win32
Imports Newtonsoft.Json
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf
Imports MongoDB.Driver.GridFS
Imports MongoDB.Bson



' This MUST match the x:Class in the XAML exactly, minus the class name
Namespace DPC.Views.Stocks.PurchaseOrder.Delivery
    Public Class PreviewPrintDeliveryReceipt
        Private itemDataSource As New ObservableCollection(Of Dictionary(Of String, String))
        Private _pageMap As New List(Of List(Of Dictionary(Of String, String)))
        Private _currentPageIndex As Integer = 0

        Public Sub New()
            InitializeComponent()
            IntializeFields()
        End Sub

        Public Sub IntializeFields()
            ' Pull the current receipt from our global state
            Dim receipt As DeliveryReceiptModel = TryCast(DeliveryState.CurrentReceipt, DeliveryReceiptModel)
            If receipt Is Nothing Then Return

            txtDeliveryNumber.Text = receipt.DRNumber
            txtDocumentReference.Text = receipt.DocumentReference
            txtDeliveryDate.Text = receipt.DRDate
            txtSalesRep.Text = CacheOnLoggedInName
            txtDeliveryClientName.Text = receipt.ClientName
            txtNotes.Text = receipt.DeliveryNotes
            txtShippingMethod.Text = receipt.ShippingMethod
            txtApprovedBy.Text = receipt.ApprovedBy
            txtPaymentTerm.Text = receipt.PaymentTerm
            txtDeliveryStatus.Text = receipt.DeliveryStatus

            Dim clientDetails = receipt.ClientDetails
            If Not String.IsNullOrEmpty(clientDetails) Then
                txtDeliveryRep.Text = Regex.Match(clientDetails, "Representative Name: (.*)").Groups(1).Value.Trim()
                txtDeliveryContact.Text = Regex.Match(clientDetails, "Contact: (.*)").Groups(1).Value.Trim()
                txtDeliveryAddress.Text = Regex.Match(clientDetails, "Delivery Address: (.*)").Groups(1).Value.Trim()
            End If

            LoadPage()
        End Sub

        Private Sub LoadPage()
            _pageMap.Clear()
            Dim maxHeightPerPage As Double = 650
            Dim currentHeight As Double = 0

            Dim receipt As DeliveryReceiptModel = TryCast(DeliveryState.CurrentReceipt, DeliveryReceiptModel)
            If receipt Is Nothing OrElse String.IsNullOrEmpty(receipt.OrderItems) Then Return

            Dim rawItems = JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(receipt.OrderItems)
            Dim currentPageItems As New List(Of Dictionary(Of String, String))
            _pageMap.Add(currentPageItems)
            Dim productCounter As Integer = 1

            For Each rawItem In rawItems
                Dim isHeader = rawItem.ContainsKey("IsHeaderRow") AndAlso rawItem("IsHeaderRow").ToString().ToLower() = "true"
                Dim displayItem = New Dictionary(Of String, String)(rawItem)

                If isHeader Then
                    displayItem("Number") = ""
                Else
                    displayItem("Number") = productCounter.ToString()
                    productCounter += 1
                End If

                If displayItem.ContainsKey("SerialNumber") Then
                    Dim raw = displayItem("SerialNumber")
                    displayItem("SerialNumber") = Regex.Replace(raw, "\(\d+\)\s*", "").Replace("  ", ", ").Trim()
                End If

                Dim rowElement = CreateRowElement(displayItem)
                rowElement.Measure(New Size(726, Double.PositiveInfinity))
                Dim rowHeight = rowElement.DesiredSize.Height

                If (currentHeight + rowHeight + 5) > maxHeightPerPage AndAlso currentPageItems.Count > 0 Then
                    currentPageItems = New List(Of Dictionary(Of String, String))
                    _pageMap.Add(currentPageItems)
                    currentHeight = 0
                End If

                currentPageItems.Add(displayItem)
                currentHeight += rowHeight + 5
            Next

            RenderPages(_currentPageIndex)
        End Sub

        Private Sub RenderPages(index As Integer)
            If _pageMap.Count = 0 OrElse index < 0 OrElse index >= _pageMap.Count Then Return

            _currentPageIndex = index
            MainContainer.Children.Clear()

            Dim pageList = _pageMap(index)

            Dim dg As New DataGrid With {
                .MinWidth = 726,
                .Width = 726,
                .HorizontalAlignment = HorizontalAlignment.Stretch,
                .HorizontalContentAlignment = HorizontalAlignment.Stretch,
                .IsReadOnly = True,
                .AutoGenerateColumns = False,
                .HeadersVisibility = DataGridHeadersVisibility.Column,
                .GridLinesVisibility = DataGridGridLinesVisibility.None,
                .Background = Brushes.Transparent,
                .ItemsSource = pageList,
                .BorderThickness = New Thickness(0)
            }

            AddColumnsToGrid(dg)

            Dim container As New Border With {
                .Background = Brushes.Transparent,
                .Width = 726,
                .Child = dg,
                .HorizontalAlignment = HorizontalAlignment.Center,
                .VerticalAlignment = VerticalAlignment.Top
            }

            MainContainer.Children.Add(container)

            PageIndicatorText.Text = $"Page {index + 1} of {_pageMap.Count}"
            txtPageInfo.Text = $"Page {index + 1} of {_pageMap.Count}"
        End Sub

        Private Function CreateRowElement(item As Dictionary(Of String, String)) As FrameworkElement
            Dim rowGrid As New Grid()
            rowGrid.Width = 726

            rowGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(50)})
            rowGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(3, GridUnitType.Star)})
            rowGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})

            Dim sp As New StackPanel With {.Margin = New Thickness(5)}
            sp.Children.Add(New TextBlock With {
                .Text = If(item.ContainsKey("ProductName"), item("ProductName"), ""),
                .FontWeight = FontWeights.SemiBold,
                .TextWrapping = TextWrapping.Wrap
            })

            sp.Children.Add(New TextBlock With {
                .Text = If(item.ContainsKey("SerialNumber"), item("SerialNumber"), ""),
                .TextWrapping = TextWrapping.Wrap,
                .FontSize = 11
            })

            Grid.SetColumn(sp, 1)
            rowGrid.Children.Add(sp)
            Return rowGrid
        End Function

        Private Sub AddColumnsToGrid(dg As DataGrid)
            ' Same logic as your original code
            Dim headerStyle As New Style(GetType(System.Windows.Controls.Primitives.DataGridColumnHeader))
            headerStyle.Setters.Add(New Setter(Control.BackgroundProperty, CType(New BrushConverter().ConvertFrom("#090909"), Brush)))
            headerStyle.Setters.Add(New Setter(Control.ForegroundProperty, Brushes.White))
            headerStyle.Setters.Add(New Setter(Control.FontSizeProperty, 10.0))
            headerStyle.Setters.Add(New Setter(Control.FontWeightProperty, FontWeights.SemiBold))
            headerStyle.Setters.Add(New Setter(Control.PaddingProperty, New Thickness(5, 3, 5, 3)))
            headerStyle.Setters.Add(New Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center))
            headerStyle.Setters.Add(New Setter(Control.HeightProperty, 30.0))

            Dim rowStyle As New Style(GetType(DataGridRow))
            rowStyle.Setters.Add(New Setter(DataGridRow.BackgroundProperty, Brushes.Transparent))
            rowStyle.Setters.Add(New Setter(DataGridRow.MinHeightProperty, 55.0))
            dg.RowStyle = rowStyle

            Dim cellStyle As New Style(GetType(DataGridCell))
            cellStyle.Setters.Add(New Setter(DataGridCell.PaddingProperty, New Thickness(3, 0, 3, 0)))
            cellStyle.Setters.Add(New Setter(DataGridCell.BorderBrushProperty, Brushes.Black))
            cellStyle.Setters.Add(New Setter(DataGridCell.BorderThicknessProperty, New Thickness(0, 0, 1, 1)))
            dg.CellStyle = cellStyle

            dg.Columns.Add(New DataGridTextColumn With {.Header = "#", .Binding = New Binding("[Number]"), .Width = 40, .HeaderStyle = headerStyle})

            Dim colProduct As New DataGridTemplateColumn With {.Header = "Description", .Width = New DataGridLength(1, DataGridLengthUnitType.Star), .HeaderStyle = headerStyle}
            Dim productFactory = New FrameworkElementFactory(GetType(Border))
            Dim titleTxt = New FrameworkElementFactory(GetType(TextBlock))
            titleTxt.SetBinding(TextBlock.TextProperty, New Binding("[ProductName]"))
            titleTxt.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap)
            productFactory.AppendChild(titleTxt)
            colProduct.CellTemplate = New DataTemplate With {.VisualTree = productFactory}
            dg.Columns.Add(colProduct)

            dg.Columns.Add(New DataGridTextColumn With {.Header = "Quantity", .Binding = New Binding("[Quantity]"), .Width = 70, .HeaderStyle = headerStyle})
            dg.Columns.Add(New DataGridTextColumn With {.Header = "Serial Numbers", .Binding = New Binding("[SerialNumber]"), .Width = 250, .HeaderStyle = headerStyle})
        End Sub

        Private Sub CancelButton(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToCachedView("newdelivery", Me)
        End Sub

        Private Sub PreviousPage_Click(sender As Object, e As RoutedEventArgs)
            If _currentPageIndex > 0 Then RenderPages(_currentPageIndex - 1)
        End Sub

        Private Sub NextPage_Click(sender As Object, e As RoutedEventArgs)
            If _currentPageIndex < _pageMap.Count - 1 Then RenderPages(_currentPageIndex + 1)
        End Sub

        Private Sub RenderToPdfPage(element As FrameworkElement, page As PdfPage)
            Try
                Dim dpi As Integer = 300
                Dim layoutWidth = element.ActualWidth
                Dim layoutHeight = element.ActualHeight

                If layoutWidth = 0 OrElse layoutHeight = 0 Then
                    Throw New InvalidOperationException("Element has invalid dimensions")
                End If

                Dim pixelWidth = CInt(layoutWidth * dpi / 96)
                Dim pixelHeight = CInt(layoutHeight * dpi / 96)

                Dim rtb As New RenderTargetBitmap(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32)
                element.Measure(New Size(layoutWidth, layoutHeight))
                element.Arrange(New Rect(0, 0, layoutWidth, layoutHeight))
                element.UpdateLayout()
                rtb.Render(element)

                Dim encoder As New PngBitmapEncoder()
                encoder.Frames.Add(BitmapFrame.Create(rtb))

                Using stream As New MemoryStream()
                    encoder.Save(stream)
                    stream.Position = 0

                    Using gfx As XGraphics = XGraphics.FromPdfPage(page)
                        Dim image = XImage.FromStream(stream)
                        gfx.DrawImage(image, 0, 0, page.Width, page.Height)
                    End Using
                End Using

            Catch ex As Exception
                Throw New Exception($"Error rendering to PDF page: {ex.Message}", ex)
            End Try
        End Sub

        Private Shared Function SavePdfPathToMongoDB(filePath As String, drNumber As String, uploadedBy As String) As Boolean
            Try
                If String.IsNullOrEmpty(filePath) Then Return False

                Dim gridFS As GridFSBucket = SplashScreen.GetGridFSConnection()

                Using fileStream As New FileStream(filePath, FileMode.Open, FileAccess.Read)
                    Dim options As New GridFSUploadOptions() With {
                .Metadata = New BsonDocument From {
                    {"uploadedBy", uploadedBy},
                    {"uploadedAt", BsonDateTime.Create(DateTime.UtcNow)},
                    {"source", "delivery-receipt"},
                    {"drNumber", drNumber},
                    {"pdfFilePath", filePath}
                }
            }

                    gridFS.UploadFromStream(Path.GetFileName(filePath), fileStream, options)
                End Using
                Return True
            Catch ex As Exception
                MessageBox.Show("Error saving PDF to MongoDB: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function


        Private Function SaveAsPDF(docName As String) As String
            Dim dlg As New Microsoft.Win32.SaveFileDialog() With {
        .FileName = docName & ".pdf",
        .Filter = "PDF Files (.pdf)|.pdf"
    }

            If dlg.ShowDialog() = True Then
                Try
                    Dim pdf As New PdfDocument()
                    Dim page As PdfPage = pdf.AddPage()

                    ' Use the actual border dimensions (in device-independent units)
                    Dim layoutWidth = 816.0  ' PrintAreaBorder width
                    Dim layoutHeight = 1344.0  ' PrintAreaBorder height

                    ' Convert to inches (96 DPI)
                    page.Width = XUnit.FromInch(layoutWidth / 96)
                    page.Height = XUnit.FromInch(layoutHeight / 96)

                    ' Render to page
                    RenderToPdfPage(PrintPreview, page)

                    pdf.Save(dlg.FileName)
                    MessageBox.Show($"PDF saved to: {dlg.FileName}")

                    Return dlg.FileName

                Catch ex As Exception
                    MessageBox.Show($"Error creating PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return Nothing
                End Try
            Else
                Return Nothing
            End If
        End Function

        Private Sub SaveToDb()
            Try
                Dim receipt As DeliveryReceiptModel = TryCast(DeliveryState.CurrentReceipt, DeliveryReceiptModel)
                If receipt Is Nothing Then Return

                Dim jsonItems As String = JsonConvert.SerializeObject(receipt.OrderItems)


                Dim deliveryStatus As String = If(receipt.DeliveryStatus?.Length > 50,
                                         receipt.DeliveryStatus.Substring(0, 50),
                                         receipt.DeliveryStatus)

                Dim shippingMethod As String = If(receipt.ShippingMethod?.Length > 100,
                                          receipt.ShippingMethod.Substring(0, 100),
                                          receipt.ShippingMethod)

                Dim success As Boolean = DeliveryReceiptController.InsertDeliveryReceipt(
                    receipt.DRNumber,
                    receipt.DocumentReference,
                    receipt.DRDate,
                    receipt.ClientName,
                    receipt.ClientDetails,
                    receipt.DeliveryNotes,
                    shippingMethod,
                    deliveryStatus,
                    receipt.ApprovedBy,
                    receipt.PaymentTerm,
                    jsonItems,
                    CacheOnLoggedInName
        )

                If success Then
                    MessageBox.Show("Delivery receipt saved to database successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    ViewLoader.DynamicView.NavigateToCachedView("newdelivery", Me)
                Else
                    MessageBox.Show("Failed to save delivery receipt to database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End If

            Catch ex As Exception
                MessageBox.Show("Error saving to database: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub SaveDb_Click(sender As Object, e As RoutedEventArgs)
            Try
                Dim receipt As DeliveryReceiptModel = TryCast(DeliveryState.CurrentReceipt, DeliveryReceiptModel)
                If receipt Is Nothing Then
                    MessageBox.Show("No receipt data found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                Dim docName As String = receipt.DRNumber
                Dim savedPath As String = SaveAsPDF(docName)

                If Not SavePdfPathToMongoDB(savedPath, receipt.DRNumber, CacheOnLoggedInName) Then Exit Sub

                If Not String.IsNullOrEmpty(savedPath) Then
                    SaveToDb()
                    MessageBox.Show("Delivery receipt saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                Else
                    MessageBox.Show("PDF save cancelled. Data not saved.")
                End If
            Catch ex As Exception
                MessageBox.Show("Error saving to database: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub SavePrint(sender As Object, e As RoutedEventArgs)
            ' Printing logic
        End Sub


    End Class
End Namespace