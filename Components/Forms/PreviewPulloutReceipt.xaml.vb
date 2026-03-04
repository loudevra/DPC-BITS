Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.IO
Imports System.Windows.Markup
Imports DPC.DPC.Data.Controllers.Stocks
Imports DPC.DPC.Data.Helpers
Imports MongoDB.Bson
Imports MongoDB.Driver.GridFS
Imports Newtonsoft.Json
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Data.Controllers

Namespace DPC.Components.Forms
    Public Class PreviewPulloutReceipt
        Private _items As New ObservableCollection(Of PORModel)()
        Private _itemsSave As New List(Of Dictionary(Of String, String))

        Public Sub New()
            ' This call is required by the designer.
            InitializeComponent()
            ' Add any initialization after the InitializeComponent() call.
            LoadComponents()
        End Sub

        Private Sub LoadComponents()
            POR.Text = PORDetails.PORNumber
            PORDate.Text = Date.Now.ToString("MMMM dd, yyyy")
            PORTo.Text = PORDetails.PulloutTo
            txtPrepared.Text = CacheOnLoggedInName

            For Each item In PORDetails.ItemsList
                Dim itemModel As New PORModel With {
                    .Quantity = item("Quantity"),
                    .ProductName = item("ItemName"),
                    .Notes = item("Notes"),
                    .Stocks = item("Stocks"),
                    .ItemReturn = 0
                }
                _items.Add(itemModel)
            Next

            dataGrid.ItemsSource = _items
        End Sub

        Private Sub ValidateNumericInput(sender As Object, e As TextCompositionEventArgs)
            Dim allowedPattern As String = "^[0-9.]+$" ' Allow only digits and decimal point

            If Not System.Text.RegularExpressions.Regex.IsMatch(e.Text, allowedPattern) Then

                e.Handled = True ' Reject input if it doesn't match
            End If
        End Sub

        Private Sub SavePrint(sender As Object, e As RoutedEventArgs)
            ' Ask user: PDF or Print
            Dim result As MessageBoxResult = MessageBox.Show("Do you want to save this as a PDF?", "Choose Output", MessageBoxButton.YesNoCancel, MessageBoxImage.Question)

            Dim docName As String = PORDetails.PORNumber ' You can replace this with any string

            If result = MessageBoxResult.Yes Then
                ' ➤ Save as PDF
                SaveAsPDF(docName)
                SaveToDB()
            ElseIf result = MessageBoxResult.No Then
                ' ➤ Print physically
                PrintPhysically(docName)
                SaveToDB()
            Else
                ' ➤ Cancelled
                MessageBox.Show("Printing Cancelled")
                Exit Sub
            End If
        End Sub

        Private Sub SaveAsPDF(docName As String)
            Dim dpi As Integer = 300

            ' Keep WPF layout size as-is (in DIPs)
            Dim layoutWidth = PrintPreview.ActualWidth
            Dim layoutHeight = PrintPreview.ActualHeight

            ' Convert DIP size to pixel size for high DPI rendering
            Dim pixelWidth = CInt(layoutWidth * dpi / 96)
            Dim pixelHeight = CInt(layoutHeight * dpi / 96)

            ' Render to high-DPI bitmap
            Dim rtb As New RenderTargetBitmap(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32)

            ' Temporarily layout the control for rendering
            PrintPreview.Measure(New Size(layoutWidth, layoutHeight))
            PrintPreview.Arrange(New Rect(0, 0, layoutWidth, layoutHeight))
            PrintPreview.UpdateLayout()

            ' Render it
            rtb.Render(PrintPreview)

            ' Encode as PNG
            Dim encoder As New PngBitmapEncoder()
            encoder.Frames.Add(BitmapFrame.Create(rtb))
            Dim stream As New MemoryStream()
            encoder.Save(stream)
            stream.Position = 0

            ' Create PDF using PDFSharp
            Dim pdf As New PdfDocument()
            Dim page = pdf.AddPage()

            ' Set the page size in points (1 inch = 72 points)
            page.Width = XUnit.FromInch(layoutWidth / 96)
            page.Height = XUnit.FromInch(layoutHeight / 96)

            Dim gfx = XGraphics.FromPdfPage(page)
            Dim image = XImage.FromStream(stream)
            gfx.DrawImage(image, 0, 0, page.Width, page.Height)

            ' Save dialog
            Dim dlg As New Microsoft.Win32.SaveFileDialog()
            dlg.FileName = docName & ".pdf"
            dlg.Filter = "PDF Files (*.pdf)|*.pdf"

            If dlg.ShowDialog() = True Then
                pdf.Save(dlg.FileName)
                'MessageBox.Show("Saved to: " & dlg.FileName)
            End If
        End Sub

        Private Sub PrintPhysically(docName As String)
            Dim dlg As New PrintDialog()
            If dlg.ShowDialog() = True Then
                ' Save original layout
                Dim originalParent = VisualTreeHelper.GetParent(PrintPreview)
                Dim originalIndex As Integer = -1
                Dim originalMargin = PrintPreview.Margin
                Dim originalTransform = PrintPreview.LayoutTransform

                If TypeOf originalParent Is Panel Then
                    Dim panel = CType(originalParent, Panel)
                    originalIndex = panel.Children.IndexOf(PrintPreview)
                    panel.Children.Remove(PrintPreview)
                End If

                ' Setup layout for printing
                PrintPreview.Margin = New Thickness(0)
                PrintPreview.LayoutTransform = Transform.Identity
                PrintPreview.UpdateLayout()
                PrintPreview.Measure(New Size(Double.PositiveInfinity, Double.PositiveInfinity))
                PrintPreview.Arrange(New Rect(PrintPreview.DesiredSize))
                PrintPreview.UpdateLayout()

                ' A4 size (96 DPI)
                Dim A4Width As Double = 8.3 * 96
                Dim A4Height As Double = 11.69 * 96
                Dim scaleX = A4Width / PrintPreview.ActualWidth
                Dim scaleY = A4Height / PrintPreview.ActualHeight
                Dim scale = Math.Min(scaleX, scaleY)

                ' Container with scaling
                Dim container As New Grid()
                container.Width = A4Width
                container.Height = A4Height
                container.LayoutTransform = New ScaleTransform(scale, scale)
                container.Children.Add(PrintPreview)

                container.Measure(New Size(A4Width, A4Height))
                container.Arrange(New Rect(New Point(0, 0), New Size(A4Width, A4Height)))
                container.UpdateLayout()

                ' Create print content
                Dim fixedPage As New FixedPage()
                fixedPage.Width = A4Width
                fixedPage.Height = A4Height
                fixedPage.Children.Add(container)

                Dim pageContent As New PageContent()
                CType(pageContent, IAddChild).AddChild(fixedPage)

                Dim fixedDoc As New FixedDocument()
                fixedDoc.Pages.Add(pageContent)

                ' Print
                dlg.PrintDocument(fixedDoc.DocumentPaginator, docName)

                ' Restore layout
                container.Children.Clear()
                PrintPreview.LayoutTransform = originalTransform
                PrintPreview.Margin = originalMargin

                If TypeOf originalParent Is Panel AndAlso originalIndex >= 0 Then
                    Dim panel = CType(originalParent, Panel)
                    panel.Children.Insert(originalIndex, PrintPreview)
                End If
            End If
        End Sub

        Private Function SaveToMongo(docName As String) As String
            Dim dpi As Integer = 300
            Dim pdfFileName As String = docName & ".pdf"

            ' Prepare layout
            Dim layoutWidth = PrintPreview.ActualWidth
            Dim layoutHeight = PrintPreview.ActualHeight
            Dim pixelWidth = CInt(layoutWidth * dpi / 96)
            Dim pixelHeight = CInt(layoutHeight * dpi / 96)

            ' Render to bitmap
            Dim rtb As New RenderTargetBitmap(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32)
            PrintPreview.Measure(New Size(layoutWidth, layoutHeight))
            PrintPreview.Arrange(New Rect(0, 0, layoutWidth, layoutHeight))
            PrintPreview.UpdateLayout()
            rtb.Render(PrintPreview)

            ' Encode as PNG to memory stream
            Dim encoder As New PngBitmapEncoder()
            encoder.Frames.Add(BitmapFrame.Create(rtb))
            Dim stream As New MemoryStream()
            encoder.Save(stream)
            stream.Position = 0

            ' Paths
            Dim appDir As String = AppDomain.CurrentDomain.BaseDirectory
            Dim tempImagePath As String = System.IO.Path.Combine(appDir, Guid.NewGuid().ToString() & ".png")
            Dim tempPdfPath As String = System.IO.Path.Combine(appDir, pdfFileName)

            Try
                ' Save temp image file
                Using fs As New FileStream(tempImagePath, FileMode.Create, FileAccess.Write)
                    stream.CopyTo(fs)
                End Using

                ' Create PDF
                Dim pdf As New PdfDocument()
                Dim page = pdf.AddPage()
                page.Width = XUnit.FromInch(layoutWidth / 96)
                page.Height = XUnit.FromInch(layoutHeight / 96)

                Dim gfx = XGraphics.FromPdfPage(page)
                Using image = XImage.FromFile(tempImagePath)
                    gfx.DrawImage(image, 0, 0, page.Width, page.Height)
                End Using

                ' Save PDF to temp file
                pdf.Save(tempPdfPath)

                ' Upload PDF to MongoDB GridFS
                Dim gridFS As GridFSBucket = SplashScreen.GetGridFSConnection()

                Using FileStream As New FileStream(tempPdfPath, FileMode.Open, FileAccess.Read)
                    Dim options As New GridFSUploadOptions() With {
                        .Metadata = New BsonDocument From {
                            {"uploadedBy", CacheOnLoggedInName},
                            {"uploadedAt", BsonDateTime.Create(DateTime.UtcNow)},
                            {"source", "pullout-receipt/por"},
                            {"billiingNumber", PORDetails.PORNumber},
                            {"pdfFilePath", tempPdfPath}
                        }
                    }

                    gridFS.UploadFromStream(Path.GetFileName(tempPdfPath), FileStream, options)
                End Using

                Return pdfFileName

            Finally
                ' Delete the temp files
                If File.Exists(tempImagePath) Then
                    Try : File.Delete(tempImagePath) : Catch : End Try
                End If
                If File.Exists(tempPdfPath) Then
                    Try : File.Delete(tempPdfPath) : Catch : End Try
                End If
            End Try
        End Function

        Private Sub SaveToDB()
            _itemsSave.Clear()

            For Each item In dataGrid.ItemsSource
                Dim _itemSaveDictionary As New Dictionary(Of String, String) From {
                    {"ItemName", item.ProductName},
                    {"Quantity", item.Quantity.ToString()},
                    {"Stocks", item.Stocks.ToString()},
                    {"Notes", item.Notes},
                    {"ItemReturn", item.ItemReturn.ToString()}
                }

                _itemsSave.Add(_itemSaveDictionary)
            Next

            Dim pullouto As String = PORDetails.PulloutTo
            Dim PORNmber As String = PORDetails.PORNumber

            If PullOutFormController.SavePullOut(PORNmber, pullouto, _itemsSave) Then
                ' Save to MongoDB GridFS
                Dim pdfFileName As String = SaveToMongo(PORNmber)

                ' NEW: Save document record to documents table
                If Not String.IsNullOrEmpty(pdfFileName) Then
                    SaveDocumentRecord(PORNmber, pdfFileName)
                End If

                MessageBox.Show("Pullout saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                ViewLoader.DynamicView.NavigateToView("pulloutreceipt", Me)
            Else
                MessageBox.Show("Failed to save pullout.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End If
        End Sub

        Private Function GetCurrentEmployeeID() As Long  ' Changed from Integer to Long
            ' CacheOnLoggedInName contains the full_name from auth_users
            ' We need to get the employee_id that matches this full_name

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' Query using full_name
                    Dim query As String = "SELECT employee_id FROM auth_users WHERE full_name = @fullname LIMIT 1"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@fullname", CacheOnLoggedInName)
                        Dim result = cmd.ExecuteScalar()

                        If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                            ' Since employee_id is varchar(16), convert to string first
                            Dim employeeIdString As String = result.ToString().Trim()

                            ' Validate it's a valid long integer
                            Dim employeeId As Long
                            If Long.TryParse(employeeIdString, employeeId) Then
                                Return employeeId
                            Else
                                Throw New InvalidOperationException($"Invalid employee_id format: '{employeeIdString}' for user '{CacheOnLoggedInName}'")
                            End If
                        End If
                    End Using
                End Using
            Catch ex As MySqlException
                MessageBox.Show("Database error: " & ex.Message & vbCrLf & vbCrLf +
                               "Please contact support.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Throw
            Catch ex As Exception
                MessageBox.Show("Error retrieving employee ID: " & ex.Message & vbCrLf & vbCrLf +
                               "User: " & CacheOnLoggedInName & vbCrLf +
                               "Please contact support.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Throw
            End Try

            ' If we get here, the user wasn't found
            Throw New InvalidOperationException("Unable to determine current employee ID. User '" & CacheOnLoggedInName & "' not found in database.")
        End Function

        Private Sub SaveDocumentRecord(porNumber As String, pdfFileName As String)
            ' Get current employee ID from cache or auth_users
            Dim employeeID As Long = GetCurrentEmployeeID()  ' Changed from Integer to Long

            ' Create document object
            Dim document As New DPC.Data.Models.Document() With {
                .EmployeeID = employeeID,
                .Title = "Pull Out Receipt - " & porNumber,
                .FileName = pdfFileName,
                .FileContent = "", ' Empty since file is in MongoDB GridFS
                .FileType = "application/pdf",
                .FileSize = 0, ' You can calculate actual size if needed
                .UploadDate = DateTime.Now
            }

            ' Save to documents table using DocumentController
            Dim docController As New DPC.Data.Controllers.DocumentController()
            If Not docController.AddDocument(document) Then
                MessageBox.Show("Warning: Document was saved but failed to add to documents list.",
                               "Warning",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning)
            End If
        End Sub
    End Class

    Public Class PORModel
        Implements INotifyPropertyChanged

        Public Property Quantity As Integer
        Public Property ProductName As String
        Public Property Notes As String
        Public Property Stocks As Integer

        Private _itemReturn As Integer
        Public Property ItemReturn As Integer
            Get
                Return _itemReturn
            End Get
            Set(value As Integer)
                If _itemReturn <> value Then
                    _itemReturn = value
                    OnPropertyChanged(NameOf(ItemReturn))
                End If
            End Set
        End Property


        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        Protected Sub OnPropertyChanged(propertyName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub
    End Class
End Namespace