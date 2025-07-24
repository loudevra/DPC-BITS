
Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Windows.Markup
Imports Newtonsoft.Json
Imports System.Windows.Media.Imaging
Imports PdfSharp.Pdf
Imports PdfSharp.Drawing
Imports System.Web.UI.WebControls
Imports System.Windows.Controls
Imports DPC.DPC.Data.Models
Imports System.IdentityModel.Protocols.WSTrust


Namespace DPC.Views.Misc.CashAdvance

    Public Class PreviewPrintCashAdvanceRequestForm

        Public Sub New()
            ' This call is required by the designer.
            InitializeComponent()
            ' Add any initialization after the InitializeComponent() call.
            LoadData()
        End Sub

        ' <summary>' Converts a JSON string to a list of EditCashAdvanceJsonToList objects.
        Public Function ConvertJSONToList(jsonString As String) As List(Of EditCashAdvanceJsonToList)
            Dim entries As List(Of EditCashAdvanceJsonToList) = JsonConvert.DeserializeObject(Of List(Of EditCashAdvanceJsonToList))(jsonString)
            Return entries
        End Function


        ' <summary>' Initializes the data for the Cash Advance Request Form.
        Private Sub LoadData()
            EmployeeName.Text = cacheCAREmployeeName
            Jobtitle.Text = cacheCARJobTitle
            EmployeeID.Text = cacheCAREmployeeID
            CashAdvanceDate.Text = cacheCARCADate
            SupervisorName.Text = cacheCARSupervisor
            Department.Text = cacheCARDepartment
            Rate.Text = cacheCARRate
            Dim requestInfo As List(Of EditCashAdvanceJsonToList) = ConvertJSONToList(cacheCARRequestInfo)
            CreateRows(8, requestInfo)
            TotalAmount.Text = cacheCARTotalAmount
            Remarks.Text = cacheCARRemarks
            RequestedBy.Text = cacheCARrequestedBy
            RequestedDate.Text = cacheCARrequestDate
            ApprovedBy.Text = cacheCARApprovedBy
            ApprovalDate.Text = cacheCARApprovalDate
        End Sub


        ' <summary>' Creates rows in the Cash Advance Request Form based on the maximum number of rows and the request information.
        Private Sub CreateRows(MaxNum As Integer, requestInfo As List(Of EditCashAdvanceJsonToList))
            If requestInfo.Count <= MaxNum Then
                Dim blankrows As Integer = MaxNum - requestInfo.Count
                AddRowsByRequest(requestInfo)
                If blankrows <= 0 Then
                    AddBlankRows(blankrows)
                End If
            End If
        End Sub

        ' <summary>' Adds rows to the Cash Advance Request Form based on the request information.
        Private Sub AddRowsByRequest(requestInfo As List(Of EditCashAdvanceJsonToList))
            For Each request As EditCashAdvanceJsonToList In requestInfo
                Dim grid As New Grid()

                ' Define 4 columns with equal width
                For i As Integer = 1 To 4
                    grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
                Next

                ' First Border (Column 0)
                Dim border1 As New Border With {
                    .BorderBrush = Brushes.Gray,
                    .BorderThickness = New Thickness(1),
                    .Height = 30
                }
                Grid.SetColumn(border1, 0)
                border1.Child = New TextBlock With {
                    .Text = request.Amount,
                    .HorizontalAlignment = HorizontalAlignment.Center,
                    .VerticalAlignment = VerticalAlignment.Center
                }
                grid.Children.Add(border1)

                ' Second Border (Column 1)
                Dim border2 As New Border With {
                    .BorderBrush = Brushes.Gray,
                    .BorderThickness = New Thickness(1),
                    .Margin = New Thickness(-1, 0, 0, 0),
                    .Height = 30
                }
                Grid.SetColumn(border2, 1)
                border2.Child = New TextBlock With {
                    .Text = request.DateRequested,
                    .HorizontalAlignment = HorizontalAlignment.Center,
                    .VerticalAlignment = VerticalAlignment.Center
                }
                grid.Children.Add(border2)

                ' Third Border (Column 2 + 3, ColumnSpan 2)
                Dim border3 As New Border With {
                    .BorderBrush = Brushes.Gray,
                    .BorderThickness = New Thickness(1),
                    .Margin = New Thickness(-1, 0, 0, 0),
                    .Height = 30
                }
                Grid.SetColumn(border3, 2)
                Grid.SetColumnSpan(border3, 2)
                border3.Child = New TextBlock With {
                    .Text = request.Reason,
                    .HorizontalAlignment = HorizontalAlignment.Center,
                    .VerticalAlignment = VerticalAlignment.Center
                }
                grid.Children.Add(border3)

                SPRequestAmountContainer.Children.Add(grid)
            Next
        End Sub


        ' <summary>' Adds blank rows to the Cash Advance Request Form.
        Private Sub AddBlankRows(blankrows As Integer)
            For i As Integer = 1 To blankrows
                Dim grid As New Grid()

                ' Define 4 columns with equal width
                For j As Integer = 1 To 4
                    grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
                Next

                ' First Border (Column 0)
                Dim border1 As New Border With {
                .BorderBrush = Brushes.Gray,
                .BorderThickness = New Thickness(1),
                .Height = 30
            }
                Grid.SetColumn(border1, 0)
                border1.Child = New TextBlock With {
                .Text = "",
                .HorizontalAlignment = HorizontalAlignment.Center,
                .VerticalAlignment = VerticalAlignment.Center
            }
                grid.Children.Add(border1)

                ' Second Border (Column 1)
                Dim border2 As New Border With {
                .BorderBrush = Brushes.Gray,
                .BorderThickness = New Thickness(1),
                .Margin = New Thickness(-1, 0, 0, 0),
                .Height = 30
            }
                Grid.SetColumn(border2, 1)
                border2.Child = New TextBlock With {
                .Text = "",
                .HorizontalAlignment = HorizontalAlignment.Center,
                .VerticalAlignment = VerticalAlignment.Center
            }
                grid.Children.Add(border2)

                ' Third Border (Column 2 + 3, ColumnSpan 2)
                Dim border3 As New Border With {
                .BorderBrush = Brushes.Gray,
                .BorderThickness = New Thickness(1),
                .Margin = New Thickness(-1, 0, 0, 0),
                .Height = 30
            }
                Grid.SetColumn(border3, 2)
                Grid.SetColumnSpan(border3, 2)
                border3.Child = New TextBlock With {
                .Text = "",
                .HorizontalAlignment = HorizontalAlignment.Center,
                .VerticalAlignment = VerticalAlignment.Center
            }
                grid.Children.Add(border3)


                SPRequestAmountContainer.Children.Add(grid)
            Next
        End Sub


        ' <summary>' Event handler for the Save and Print button.
        Private Sub SavePrint(sender As Object, e As RoutedEventArgs)
            ' Ask user: PDF or Print
            Dim result As MessageBoxResult = MessageBox.Show("Do you also want to save this file as PDF?", "Choose Output", MessageBoxButton.YesNoCancel, MessageBoxImage.Question)

            Dim docName As String = cacheCARCAID ' You can replace this with any string

            If result = MessageBoxResult.Yes Then
                ' ➤ Save as PDF
                SaveAsPDF(docName)
                PrintPhysically(docName)
            ElseIf result = MessageBoxResult.No Then
                ' ➤ Print physically
                PrintPhysically(docName)
            Else
                ' ➤ Cancelled
                MessageBox.Show("Printing Cancelled")
                Exit Sub
            End If
        End Sub


        ' <summary>' Saves the current PrintPreview control as a PDF file.
        <Obsolete>
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


        ' <summary>' Prints the current PrintPreview control physically.
        Private Sub PrintPhysically(docName As String)
            Dim dlg As New PrintDialog()
            If dlg.ShowDialog() = True Then
                ' Save original layout
                Dim originalParent As DependencyObject = VisualTreeHelper.GetParent(PrintPreview)
                Dim panelParent As System.Windows.Controls.Panel = Nothing

                ' Traverse up the visual tree to find the actual Panel
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

                If panelParent IsNot Nothing AndAlso originalIndex >= 0 Then
                    panelParent.Children.Insert(originalIndex, PrintPreview)
                End If
            End If
        End Sub

    End Class

End Namespace

