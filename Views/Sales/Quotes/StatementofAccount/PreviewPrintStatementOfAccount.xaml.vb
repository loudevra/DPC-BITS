Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.IO
Imports System.Linq
Imports Microsoft.Win32
Imports PdfSharp.Pdf
Imports PdfSharp.Drawing
Imports DPC.DPC.Data.Models ' <== Ensure this matches your actual StatementModel location

Namespace DPC.Views.SOA
    Public Class PreviewPrintStatementOfAccount
        Inherits UserControl

        Private _statementData As StatementModel
        Private _imagesVisible As Boolean = True

        ' --- PAGINATION VARIABLES ---
        Private _currentPage As Integer = 1
        Private _itemsPerPage As Integer = 12 ' Adjust this to perfectly fit your printed page
        Private _totalPages As Integer = 1

        ' Default Constructor
        Public Sub New()
            InitializeComponent()
        End Sub

        ' Overloaded constructor receiving data
        Public Sub New(data As StatementModel)
            InitializeComponent()
            _statementData = data
            Me.DataContext = _statementData

            ' Calculate total pages based on how many items exist
            If _statementData.LineItems IsNot Nothing AndAlso _statementData.LineItems.Count > 0 Then
                _totalPages = Math.Ceiling(_statementData.LineItems.Count / _itemsPerPage)
            Else
                _totalPages = 1
            End If

            ' Load the first page
            LoadPage()

            ' Ensure images are visible by default
            SetImageColumnVisibility(Visibility.Visible)
        End Sub

        ' ==========================================
        ' PAGINATION LOGIC
        ' ==========================================
        Private Sub LoadPage()
            If _statementData Is Nothing OrElse _statementData.LineItems Is Nothing Then Return

            ' 1. Grab only the items for the current page
            Dim pagedItems = _statementData.LineItems.Skip((_currentPage - 1) * _itemsPerPage).Take(_itemsPerPage).ToList()

            ' 2. Re-bind the DataGrid to only show this page's chunk
            dgSOAItems.ItemsSource = pagedItems

            ' 3. Update the UI Text Indicators
            Dim pageText As String = $"Page {_currentPage} of {_totalPages}"
            txtPageInfo.Text = pageText

            If PageIndicatorText IsNot Nothing Then
                PageIndicatorText.Text = pageText
            End If

            ' 4. Disable/Enable buttons based on the current page
            btnPrevPage.IsEnabled = (_currentPage > 1)
            btnNextPage.IsEnabled = (_currentPage < _totalPages)
        End Sub

        Private Sub PreviousPage_Click(sender As Object, e As RoutedEventArgs)
            If _currentPage > 1 Then
                _currentPage -= 1
                LoadPage()
            End If
        End Sub

        Private Sub NextPage_Click(sender As Object, e As RoutedEventArgs)
            If _currentPage < _totalPages Then
                _currentPage += 1
                LoadPage()
            End If
        End Sub

        ' ==========================================
        ' IMAGE TOGGLE
        ' ==========================================
        Private Sub BtnHideImages_Click(sender As Object, e As RoutedEventArgs)
            SetImageColumnVisibility(Visibility.Collapsed)
        End Sub

        Private Sub BtnViewImages_Click(sender As Object, e As RoutedEventArgs)
            SetImageColumnVisibility(Visibility.Visible)
        End Sub

        Private Sub SetImageColumnVisibility(state As Visibility)
            If dgSOAItems IsNot Nothing AndAlso ColImage IsNot Nothing Then
                ColImage.Visibility = state
            End If
        End Sub

        ' ==========================================
        ' CANCEL BUTTON
        ' ==========================================
        Private Sub CancelButton(sender As Object, e As RoutedEventArgs)
            ' Close overlay safely
            Dim parentContent = TryCast(Me.Parent, ContentControl)
            If parentContent IsNot Nothing Then
                Dim overlayGrid = TryCast(parentContent.Parent, Grid)
                If overlayGrid IsNot Nothing Then
                    overlayGrid.Visibility = Visibility.Collapsed
                    parentContent.Content = Nothing
                End If
            End If
        End Sub

        ' ==========================================
        ' SAVE AS PDF LOGIC
        ' ==========================================
        Private Sub SaveDb_Click(sender As Object, e As RoutedEventArgs)
            ' 1. Open Save File Dialog so user can choose where to download the PDF
            Dim saveFileDialog As New SaveFileDialog()
            saveFileDialog.Filter = "PDF Document (*.pdf)|*.pdf"

            ' Give it a smart default file name based on the SOA Number
            Dim defaultName As String = "StatementOfAccount"
            If _statementData IsNot Nothing AndAlso Not String.IsNullOrEmpty(_statementData.SOANo) Then
                defaultName &= "_" & _statementData.SOANo
            End If
            saveFileDialog.FileName = defaultName

            If saveFileDialog.ShowDialog() = True Then
                Try
                    ' 2. Prepare the WPF Border to be rendered as an image
                    Dim width As Integer = CInt(PrintAreaBorder.ActualWidth)
                    Dim height As Integer = CInt(PrintAreaBorder.ActualHeight)
                    Dim dpi As Double = 96 ' Standard screen DPI

                    ' Ensure the layout is updated before capturing
                    PrintAreaBorder.Measure(New Size(width, height))
                    PrintAreaBorder.Arrange(New Rect(New Size(width, height)))
                    PrintAreaBorder.UpdateLayout()

                    ' 3. Take a visual "Snapshot" of the PrintAreaBorder
                    Dim rtb As New RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Pbgra32)
                    rtb.Render(PrintAreaBorder)

                    ' Encode the snapshot into a Memory Stream as a PNG
                    Dim encoder As New PngBitmapEncoder()
                    encoder.Frames.Add(BitmapFrame.Create(rtb))
                    Dim ms As New MemoryStream()
                    encoder.Save(ms)
                    ms.Position = 0

                    ' 4. Create the PDF using PDFsharp
                    Dim pdf As New PdfDocument()
                    Dim page As PdfPage = pdf.AddPage()

                    ' Match the PDF page size to the WPF Border size
                    page.Width = width
                    page.Height = height

                    ' Draw the captured image onto the PDF page
                    Dim gfx As XGraphics = XGraphics.FromPdfPage(page)
                    Dim xImg As XImage = XImage.FromStream(ms)
                    gfx.DrawImage(xImg, 0, 0, width, height)

                    ' 5. Save the PDF to the chosen location
                    pdf.Save(saveFileDialog.FileName)

                    MessageBox.Show("Statement of Account successfully saved to PDF!", "Download Complete", MessageBoxButton.OK, MessageBoxImage.Information)

                Catch ex As Exception
                    MessageBox.Show("Error generating PDF: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        End Sub

        Private Sub SavePrint(sender As Object, e As RoutedEventArgs)
            Dim printDialog As New PrintDialog()
            If printDialog.ShowDialog() = True Then
                ' Print only the document border
                Dim docName As String = "Statement of Account"
                If _statementData IsNot Nothing Then docName &= " - " & _statementData.SOANo

                printDialog.PrintVisual(PrintAreaBorder, docName)
            End If
        End Sub

    End Class
End Namespace