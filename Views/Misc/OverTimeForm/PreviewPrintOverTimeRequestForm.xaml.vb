Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Markup
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports DPC.Views.Misc.OverTime
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf

Namespace DPC.Views.Misc.OverTime

    Public Class PreviewPrintOverTimeRequestForm
        Inherits UserControl

        ' This holds the row data we pass from the DataGrid
        Public Shared TargetPrintRecord As OvertimeRequestModel

        Public Sub New()
            InitializeComponent()
            ' Ensure data loads EVERY TIME the page is opened, not just once!
            AddHandler Me.Loaded, AddressOf PreviewPrintOverTimeRequestForm_Loaded
        End Sub

        Private Sub PreviewPrintOverTimeRequestForm_Loaded(sender As Object, e As RoutedEventArgs)
            LoadData()
        End Sub

        Private Sub LoadData()
            If TargetPrintRecord IsNot Nothing Then
                ' Map the fields to the XAML layout
                EmployeeName.Text = TargetPrintRecord.EmployeeName
                Jobtitle.Text = TargetPrintRecord.JobTitle
                EmployeeID.Text = TargetPrintRecord.EmployeeID
                OvertimeDate.Text = TargetPrintRecord.RequestDate
                SupervisorName.Text = TargetPrintRecord.Supervisor
                Department.Text = TargetPrintRecord.Department
                StartTime.Text = TargetPrintRecord.StartTime
                EndTime.Text = TargetPrintRecord.EndTime
                Reason.Text = TargetPrintRecord.Reason
                TotalHours.Text = TargetPrintRecord.TotalHours
                Remarks.Text = TargetPrintRecord.Remarks
                RequestedBy.Text = TargetPrintRecord.RequestedBy
                RequestedDate.Text = TargetPrintRecord.RequestDate

                ' Approver mappings
                ApprovedBy.Text = TargetPrintRecord.ApprovedBy
                ApprovalDate.Text = TargetPrintRecord.ApprovalDate
            End If
        End Sub

        Private Sub SavePrint(sender As Object, e As RoutedEventArgs)
            Dim result As MessageBoxResult = MessageBox.Show("Do you also want to save this file as PDF?", "Choose Output", MessageBoxButton.YesNoCancel, MessageBoxImage.Question)

            Dim docName As String = TargetPrintRecord?.OvertimeID

            If result = MessageBoxResult.Yes Then
                SaveAsPDF(docName)
                PrintPhysically(docName)
            ElseIf result = MessageBoxResult.No Then
                PrintPhysically(docName)
            End If
        End Sub

        <Obsolete>
        Private Sub SaveAsPDF(docName As String)
            Dim dpi As Integer = 300
            Dim layoutWidth = PrintPreview.ActualWidth
            Dim layoutHeight = PrintPreview.ActualHeight

            Dim pixelWidth = CInt(layoutWidth * dpi / 96)
            Dim pixelHeight = CInt(layoutHeight * dpi / 96)

            Dim rtb As New RenderTargetBitmap(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32)

            PrintPreview.Measure(New Size(layoutWidth, layoutHeight))
            PrintPreview.Arrange(New Rect(0, 0, layoutWidth, layoutHeight))
            PrintPreview.UpdateLayout()

            rtb.Render(PrintPreview)

            Dim encoder As New PngBitmapEncoder()
            encoder.Frames.Add(BitmapFrame.Create(rtb))
            Dim stream As New MemoryStream()
            encoder.Save(stream)
            stream.Position = 0

            Dim pdf As New PdfDocument()
            Dim page = pdf.AddPage()

            page.Width = XUnit.FromInch(layoutWidth / 96)
            page.Height = XUnit.FromInch(layoutHeight / 96)

            Dim gfx = XGraphics.FromPdfPage(page)
            Dim image = XImage.FromStream(stream)
            gfx.DrawImage(image, 0, 0, page.Width, page.Height)

            Dim dlg As New Microsoft.Win32.SaveFileDialog()
            dlg.FileName = If(docName, "OvertimeRequest") & ".pdf"
            dlg.Filter = "PDF Files (*.pdf)|*.pdf"

            If dlg.ShowDialog() = True Then
                pdf.Save(dlg.FileName)
            End If
        End Sub

        Private Sub PrintPhysically(docName As String)
            Dim dlg As New PrintDialog()
            If dlg.ShowDialog() = True Then
                Dim originalParent As DependencyObject = VisualTreeHelper.GetParent(PrintPreview)
                Dim panelParent As System.Windows.Controls.Panel = Nothing

                While originalParent IsNot Nothing AndAlso panelParent Is Nothing
                    panelParent = TryCast(originalParent, System.Windows.Controls.Panel)
                    If panelParent Is Nothing Then
                        originalParent = VisualTreeHelper.GetParent(originalParent)
                    End If
                End While

                Dim originalIndex As Integer = -1
                Dim originalMargin = PrintPreview.Margin
                Dim originalTransform = PrintPreview.LayoutTransform

                If panelParent IsNot Nothing Then
                    originalIndex = panelParent.Children.IndexOf(PrintPreview)
                    panelParent.Children.Remove(PrintPreview)
                End If

                PrintPreview.Margin = New Thickness(0)
                PrintPreview.LayoutTransform = Transform.Identity
                PrintPreview.UpdateLayout()
                PrintPreview.Measure(New Size(Double.PositiveInfinity, Double.PositiveInfinity))
                PrintPreview.Arrange(New Rect(PrintPreview.DesiredSize))
                PrintPreview.UpdateLayout()

                Dim A4Width As Double = 8.3 * 96
                Dim A4Height As Double = 11.69 * 96
                Dim scaleX = A4Width / PrintPreview.ActualWidth
                Dim scaleY = A4Height / PrintPreview.ActualHeight
                Dim scale = Math.Min(scaleX, scaleY)

                Dim container As New Grid()
                container.Width = A4Width
                container.Height = A4Height
                container.LayoutTransform = New ScaleTransform(scale, scale)
                container.Children.Add(PrintPreview)

                container.Measure(New Size(A4Width, A4Height))
                container.Arrange(New Rect(New Point(0, 0), New Size(A4Width, A4Height)))
                container.UpdateLayout()

                Dim fixedPage As New FixedPage()
                fixedPage.Width = A4Width
                fixedPage.Height = A4Height
                fixedPage.Children.Add(container)

                Dim pageContent As New PageContent()
                CType(pageContent, IAddChild).AddChild(fixedPage)

                Dim fixedDoc As New FixedDocument()
                fixedDoc.Pages.Add(pageContent)

                dlg.PrintDocument(fixedDoc.DocumentPaginator, If(docName, "Overtime Request"))

                container.Children.Clear()
                PrintPreview.LayoutTransform = originalTransform
                PrintPreview.Margin = originalMargin

                If panelParent IsNot Nothing AndAlso originalIndex >= 0 Then
                    panelParent.Children.Insert(originalIndex, PrintPreview)
                End If
            End If
        End Sub
    End Class
End Namespace