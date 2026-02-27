Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Views.Sales.Quotes
Imports Microsoft.Win32
Imports MongoDB.Bson
Imports MongoDB.Driver
Imports MongoDB.Driver.GridFS
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf
Imports SkiaSharp.Views.WPF

Namespace DPC.Views.Stocks.PurchaseOrder.Delivery
    Public Class PreviewPrintDeliveryReceipt
        Private itemDataSource As New System.Collections.ObjectModel.ObservableCollection(Of Dictionary(Of String, String))
        Private _pageMap As New List(Of List(Of Dictionary(Of String, String)))
        Private _currentPageIndex As Integer = 0

        Public Sub New()
            InitializeComponent()

            IntializeFields()
        End Sub

        Public Sub IntializeFields()
            txtDeliveryNumber.Text = DeliveryDetails.DRNumber
            txtReferenceInvoice.Text = DeliveryDetails.DRReferenceInvoice
            txtDeliveryDate.Text = DeliveryDetails.DRDate
            txtSalesRep.Text = CacheOnLoggedInName
            txtDeliveryClientName.Text = DeliveryDetails.DRClientName
            txtNotes.Text = DeliveryDetails.DRDeliveryNotes
            txtShippingMethod.Text = DeliveryDetails.DRShippingMethod
            txtApprovedBy.Text = DeliveryDetails.DRApprovedBy
            txtPaymentTerm.Text = DeliveryDetails.DRPaymentTerm

            LoadTestPlaceholderData()
            'LoadPage()

            Dim clientDetails As String
            clientDetails = DeliveryDetails.DRClientDetails

            If Not String.IsNullOrEmpty(clientDetails) Then
                txtDeliveryRep.Text = Regex.Match(clientDetails, "Representative Name: (.*)").Groups(1).Value.Trim()
                txtDeliveryContact.Text = Regex.Match(clientDetails, "Contact: (.*)").Groups(1).Value.Trim()
                txtDeliveryAddress.Text = Regex.Match(clientDetails, "Delivery Address: (.*)").Groups(1).Value.Trim()
            End If
        End Sub

        Private Sub LoadPage()
            _pageMap.Clear()

            Dim maxHeightPerPage As Double = 650
            Dim currentHeight As Double = 0
            Dim currentPageItems As New List(Of Dictionary(Of String, String))

            _pageMap.Add(currentPageItems)

            For i As Integer = 0 To DeliveryDetails.DRDeliveryItems.Count - 1
                Dim rawItem = DeliveryDetails.DRDeliveryItems(i)
                Dim displayItem = CleanItemForDisplay(rawItem, i)

                Dim rowElement = CreateRowElement(displayItem)
                rowElement.Measure(New Size(726, Double.PositiveInfinity))
                Dim rowHeight = rowElement.DesiredSize.Height

                If (currentHeight + rowHeight + 10) > maxHeightPerPage AndAlso currentPageItems.Count > 0 Then
                    currentPageItems = New List(Of Dictionary(Of String, String))
                    _pageMap.Add(currentPageItems)
                    currentHeight = 0
                End If

                currentPageItems.Add(displayItem)
                currentHeight += rowHeight + 10
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
                .GridLinesVisibility = DataGridGridLinesVisibility.All,
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

        Private Function CleanItemForDisplay(item As Dictionary(Of String, String), index As Integer) As Dictionary(Of String, String)
            Dim displayItem As New Dictionary(Of String, String)(item)
            displayItem("Number") = (index + 1).ToString()

            If displayItem.ContainsKey("SerialNumber") Then
                Dim raw = displayItem("SerialNumber")
                Dim clean = Regex.Replace(raw, "\(\d+\)\s*", "").Replace("-", ChrW(&H2011)).Replace("  ", ", ")
                displayItem("SerialNumber") = clean.Trim()
            End If

            Return displayItem
        End Function

        Private Function CreateRowElement(item As Dictionary(Of String, String)) As FrameworkElement
            Dim rowGrid As New Grid()
            rowGrid.Width = 726

            rowGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(50)})
            rowGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(3, GridUnitType.Star)})
            rowGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})

            Dim sp As New StackPanel With {.Margin = New Thickness(5)}
            sp.Children.Add(New TextBlock With {
                .Text = item("ProductName"),
                .FontWeight = FontWeights.SemiBold,
                .TextWrapping = TextWrapping.Wrap
            })

            sp.Children.Add(New TextBlock With {
                .Text = item("SerialNumber"),
                .TextWrapping = TextWrapping.Wrap,
                .FontSize = 11
            })

            Grid.SetColumn(sp, 1)
            rowGrid.Children.Add(sp)

            Return rowGrid
        End Function

        Private Sub AddColumnsToGrid(dg As DataGrid)
            Dim headerStyle As New Style(GetType(System.Windows.Controls.Primitives.DataGridColumnHeader))
            headerStyle.Setters.Add(New Setter(Control.BackgroundProperty, Brushes.Black))
            headerStyle.Setters.Add(New Setter(Control.ForegroundProperty, Brushes.White))
            headerStyle.Setters.Add(New Setter(Control.FontSizeProperty, 9.0))
            headerStyle.Setters.Add(New Setter(Control.PaddingProperty, New Thickness(5)))
            headerStyle.Setters.Add(New Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center))
            headerStyle.Setters.Add(New Setter(Control.BorderBrushProperty, Brushes.Black))
            headerStyle.Setters.Add(New Setter(Control.BorderThicknessProperty, New Thickness(0, 0, 1, 0)))

            Dim rowStyle As New Style(GetType(DataGridRow))
            rowStyle.Setters.Add(New Setter(DataGridRow.BackgroundProperty, Brushes.Transparent))
            rowStyle.Setters.Add(New Setter(DataGridRow.BorderThicknessProperty, New Thickness(0)))
            dg.RowStyle = rowStyle

            Dim cellStyle As New Style(GetType(DataGridCell))
            cellStyle.Setters.Add(New Setter(DataGridCell.BackgroundProperty, Brushes.Transparent))
            cellStyle.Setters.Add(New Setter(DataGridCell.BorderThicknessProperty, New Thickness(0)))
            cellStyle.Setters.Add(New Setter(DataGridCell.FocusVisualStyleProperty, Nothing))
            dg.CellStyle = cellStyle

            Dim centeredStyle As New Style(GetType(TextBlock))
            centeredStyle.Setters.Add(New Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center))
            centeredStyle.Setters.Add(New Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center))
            centeredStyle.Setters.Add(New Setter(TextBlock.FontSizeProperty, 11.0))

            dg.Columns.Add(New DataGridTextColumn With {
                .Header = "#",
                .Binding = New Binding("[Number]"),
                .MinWidth = 60,
                .Width = 60,
                .HeaderStyle = headerStyle,
                .ElementStyle = centeredStyle
            })

            Dim colProduct As New DataGridTemplateColumn With {
                .Header = "Product / Description",
                .Width = New DataGridLength(2, DataGridLengthUnitType.Star),
                .MinWidth = 303,
                .HeaderStyle = headerStyle
            }

            Dim factory = New FrameworkElementFactory(GetType(StackPanel))
            factory.SetValue(StackPanel.MarginProperty, New Thickness(5))
            factory.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center)

            Dim txtName = New FrameworkElementFactory(GetType(TextBlock))
            txtName.SetBinding(TextBlock.TextProperty, New Binding("[ProductName]"))
            txtName.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold)
            txtName.SetValue(TextBlock.FontSizeProperty, 11.0)
            txtName.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap)

            Dim txtDesc = New FrameworkElementFactory(GetType(TextBlock))
            txtDesc.SetBinding(TextBlock.TextProperty, New Binding("[Description]"))
            txtDesc.SetValue(TextBlock.FontSizeProperty, 11.0)
            txtDesc.SetValue(TextBlock.ForegroundProperty, Brushes.DimGray)
            txtDesc.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap)

            factory.AppendChild(txtName)
            factory.AppendChild(txtDesc)
            colProduct.CellTemplate = New DataTemplate With {.VisualTree = factory}
            dg.Columns.Add(colProduct)

            dg.Columns.Add(New DataGridTextColumn With {
                .Header = "Qty",
                .Binding = New Binding("[Quantity]"),
                .MinWidth = 60,
                .Width = 60,
                .HeaderStyle = headerStyle,
                .ElementStyle = centeredStyle
            })

            Dim serialStyle As New Style(GetType(TextBlock))
            serialStyle.Setters.Add(New Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap))
            serialStyle.Setters.Add(New Setter(TextBlock.FontSizeProperty, 11.0))
            serialStyle.Setters.Add(New Setter(TextBlock.PaddingProperty, New Thickness(5)))
            serialStyle.Setters.Add(New Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center))
            serialStyle.Setters.Add(New Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center))
            serialStyle.Setters.Add(New Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center))

            dg.Columns.Add(New DataGridTextColumn With {
                .Header = "Serial Numbers",
                .Binding = New Binding("[SerialNumber]"),
                .Width = New DataGridLength(2, DataGridLengthUnitType.Star),
                .MinWidth = 303,
                .HeaderStyle = headerStyle,
                .ElementStyle = serialStyle
            })
        End Sub

        Private Sub CancelButton(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("newdelivery", Me)
        End Sub
        Private Sub PreviousPage_Click(sender As Object, e As RoutedEventArgs)
            If _currentPageIndex > 0 Then
                RenderPages(_currentPageIndex - 1)
            End If
        End Sub

        Private Sub NextPage_Click(sender As Object, e As RoutedEventArgs)
            If _currentPageIndex < _pageMap.Count - 1 Then
                RenderPages(_currentPageIndex + 1)
            End If
        End Sub

        Private Sub SavePrint(sender As Object, e As RoutedEventArgs)
            Try
                Dim res As MessageBoxResult = MessageBox.Show("Do you want to save this as a PDF?", "Output", MessageBoxButton.YesNoCancel)
                Dim docName As String = DeliveryDetails.DRNumber

                If res = MessageBoxResult.Cancel Then Return


                If res = MessageBoxResult.Yes Then
                    Dim path As String = SaveAsPDF(docName)
                    If Not SavePdfPathToMongoDB(path, docName, CacheOnLoggedInName) Then Exit Sub
                    SaveToDb()
                ElseIf res = MessageBoxResult.No Then
                    PrintPhysically(docName)
                    'If Not SavePdfPathToMongoDB(path, docName, CacheOnLoggedInName) Then Exit Sub
                    'SaveToDb()
                End If
            Catch ex As Exception
                MessageBox.Show("Print Error: " & ex.Message)
            End Try
        End Sub

        Private Sub SaveDb_Click(sender As Object, e As RoutedEventArgs)
            Try
                Dim path As String = SaveAsPDF(DeliveryDetails.DRNumber)
                If Not String.IsNullOrEmpty(path) Then
                    If Not SavePdfPathToMongoDB(path, DeliveryDetails.DRNumber, CacheOnLoggedInName) Then Exit Sub
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

                    For i As Integer = 0 To _pageMap.Count - 1
                        RenderPages(i)

                        Application.Current.Dispatcher.Invoke(Sub() Me.UpdateLayout(), System.Windows.Threading.DispatcherPriority.Render)
                        System.Threading.Thread.Sleep(100)

                        Dim page As PdfPage = pdf.AddPage()
                        page.Width = XUnit.FromInch(8.5)
                        page.Height = XUnit.FromInch(14)

                        RenderToPdfPage(PrintAreaBorder, page)
                    Next

                    pdf.Save(dlg.FileName)
                    RenderPages(0)
                    Return dlg.FileName
                Catch ex As Exception
                    MessageBox.Show("PDF Error: " & ex.Message)
                    Return Nothing
                End Try
            End If
            Return Nothing
        End Function

        Private Sub RenderToPdfPage(elem As FrameworkElement, page As PdfPage)

            Dim pageWidth As Double = 8.5 * 96
            Dim pageHeight As Double = 14 * 96

            elem.Measure(New Size(pageWidth, pageHeight))
            elem.Arrange(New Rect(0, 0, pageWidth, pageHeight))
            elem.UpdateLayout()

            Dim dpi As Integer = 300
            Dim pxW As Integer = CInt(8.5 * dpi)
            Dim pxH As Integer = CInt(14 * dpi)

            Dim rtb As New RenderTargetBitmap(pxW, pxH, dpi, dpi, PixelFormats.Pbgra32)
            rtb.Render(elem)

            Dim encoder As New PngBitmapEncoder()
            encoder.Frames.Add(BitmapFrame.Create(rtb))

            Using ms As New MemoryStream()
                encoder.Save(ms)
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
                Try
                    Dim fixedDoc As New FixedDocument()
                    Dim legalWidth As Double = 8.5 * 96
                    Dim legalHeight As Double = 14 * 96

                    For i As Integer = 0 To _pageMap.Count - 1
                        RenderPages(i)
                        Application.Current.Dispatcher.Invoke(Sub() Me.UpdateLayout(), System.Windows.Threading.DispatcherPriority.Render)

                        Dim rtb As New RenderTargetBitmap(CInt(legalWidth), CInt(legalHeight), 96, 96, PixelFormats.Pbgra32)

                        Dim dv As New DrawingVisual()
                        Using ctx = dv.RenderOpen()
                            Dim vb As New VisualBrush(PrintAreaBorder)
                            ctx.DrawRectangle(vb, Nothing, New Rect(0, 0, legalWidth, legalHeight))
                        End Using

                        rtb.Render(dv)
                        rtb.Freeze()

                        Dim imageBrush As New ImageBrush(rtb)
                        Dim visualRect As New System.Windows.Shapes.Rectangle() With {
                            .Width = legalWidth,
                            .Height = legalHeight,
                            .Fill = imageBrush
                        }

                        Dim fp As New FixedPage() With {
                            .Width = legalWidth,
                            .Height = legalHeight
                        }
                        fp.Children.Add(visualRect)

                        fp.Measure(New Size(legalWidth, legalHeight))
                        fp.Arrange(New Rect(0, 0, legalWidth, legalHeight))
                        fp.UpdateLayout()

                        ' 6. Add to document
                        Dim pc As New PageContent()
                        CType(pc, System.Windows.Markup.IAddChild).AddChild(fp)
                        fixedDoc.Pages.Add(pc)
                    Next

                    dlg.PrintDocument(fixedDoc.DocumentPaginator, docName)

                    RenderPages(0)

                Catch ex As Exception
                    MessageBox.Show("Printing Error: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        End Sub

        Private Sub SaveToDb()
            Dim json As String = Newtonsoft.Json.JsonConvert.SerializeObject(DeliveryDetails.DRDeliveryItems)

            If DeliveryReceiptController.InsertDeliveryReceipt(
                DeliveryDetails.DRNumber,
                DeliveryDetails.DRReferenceInvoice,
                DeliveryDetails.DRDate,
                DeliveryDetails.DRClientName,
                DeliveryDetails.DRClientDetails,
                DeliveryDetails.DRDeliveryNotes,
                DeliveryDetails.DRShippingMethod,
                DeliveryDetails.DRApprovedBy,
                DeliveryDetails.DRPaymentTerm,
                json,
                CacheOnLoggedInName) Then

                MessageBox.Show("Delivery Receipt successfully saved to database.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

                DeliveryDetails.ClearDeliveryDetails()

                ViewLoader.DynamicView.NavigateToView("walkinorder", Me)
            Else
                MessageBox.Show("Failed to submit Delivery Receipt to the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End If
        End Sub

        Private Shared Function SavePdfPathToMongoDB(path As String, drNum As String, user As String) As Boolean
            Try
                Dim fs As MongoDB.Driver.GridFS.GridFSBucket = SplashScreen.GetGridFSConnection()

                Dim filter = MongoDB.Driver.Builders(Of MongoDB.Driver.GridFS.GridFSFileInfo).Filter.Eq(Of String)("metadata.deliveryNumber", drNum)
                Dim existingFiles = fs.Find(filter).ToList()

                For Each file In existingFiles
                    fs.Delete(file.Id)
                Next

                Using s As New FileStream(path, FileMode.Open, FileAccess.Read)
                    Dim opts As New MongoDB.Driver.GridFS.GridFSUploadOptions() With {
                .Metadata = New MongoDB.Bson.BsonDocument From {
                    {"uploadedBy", user},
                    {"uploadedAt", MongoDB.Bson.BsonDateTime.Create(DateTime.UtcNow)},
                    {"source", "stocks/delivery-receipt"},
                    {"deliveryNumber", drNum},
                    {"pdfFilePath", path}
                }
            }
                    fs.UploadFromStream(System.IO.Path.GetFileName(path), s, opts)
                End Using

                Return True
            Catch ex As Exception
                MessageBox.Show("MongoDB File Error: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return False
            End Try
        End Function

        ' FOR TESTING ONLY
        Public Sub LoadTestPlaceholderData()
            DeliveryDetails.DRDeliveryItems.Clear()

            DeliveryDetails.DRDeliveryItems = New List(Of Dictionary(Of String, String))()

            Dim item1 As New Dictionary(Of String, String) From {
                {"Quantity", "1"},
                {"ProductName", "HIKVISION - 2MP WEATHERPROOF IR IP CAMERA"},
                {"Description", "High-definition outdoor security camera with night vision."},
                {"Amount", "1881.60"},
                {"SerialNumber", "SN-HK-992831, SN-HK-992831, SN-HK-992831, SN-HK-992831, SN-HK-992831, SN-HK-992831, SN-HK-992831, SN-HK-992831, SN-HK-992831, SN-HK-992831, SN-HK-992831, SN-HK-992831"}
            }

            Dim item2 As New Dictionary(Of String, String) From {
                {"Quantity", "5"},
                {"ProductName", "AEROCOOL UNITED POWER 500W (80+ WHITE)"},
                {"Description", "Enter product description (Optional)"},
                {"Amount", "2035.04"},
                {"SerialNumber", "N/A"}
            }

            Dim item3 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item4 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item5 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item6 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item7 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item8 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item9 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item10 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item11 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item12 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item13 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item14 As New Dictionary(Of String, String) From {
                {"Quantity", "12"},
                {"ProductName", "LOGITECH G-PRO WIRELESS LIGHTSPEED GAMING MOUSE - BLACK EDITION"},
                {"Description", "Ultra-lightweight gaming mouse used by esports professionals."},
                {"Amount", "4500.00"},
                {"SerialNumber", "SN-LOGI-7721"}
            }

            Dim item15 As New Dictionary(Of String, String) From {
                {"Quantity", "1"},
                {"ProductName", "HIKVISION - 2MP WEATHERPROOF IR IP CAMERA"},
                {"Description", "High-definition outdoor security camera with night vision."},
                {"Amount", "1881.60"},
                {"SerialNumber", "SN-HK-992831"}
            }

            DeliveryDetails.DRDeliveryItems.Add(item1)
            DeliveryDetails.DRDeliveryItems.Add(item2)
            DeliveryDetails.DRDeliveryItems.Add(item3)
            DeliveryDetails.DRDeliveryItems.Add(item4)
            DeliveryDetails.DRDeliveryItems.Add(item5)
            DeliveryDetails.DRDeliveryItems.Add(item6)
            DeliveryDetails.DRDeliveryItems.Add(item7)
            DeliveryDetails.DRDeliveryItems.Add(item8)
            DeliveryDetails.DRDeliveryItems.Add(item9)
            DeliveryDetails.DRDeliveryItems.Add(item10)
            DeliveryDetails.DRDeliveryItems.Add(item11)
            DeliveryDetails.DRDeliveryItems.Add(item12)
            DeliveryDetails.DRDeliveryItems.Add(item13)
            DeliveryDetails.DRDeliveryItems.Add(item14)
            DeliveryDetails.DRDeliveryItems.Add(item15)

            LoadPage()
        End Sub
    End Class
End Namespace