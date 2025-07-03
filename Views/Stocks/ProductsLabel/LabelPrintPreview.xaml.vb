Imports System.Windows.Markup

Namespace DPC.Views.Stocks.ProductsLabel
    Public Class LabelPrintPreview
        Public Sub New(element As FrameworkElement)

            ' This call is required by the designer.
            InitializeComponent()

            Dim wp As WrapPanel = DirectCast(element, WrapPanel)
            wp.HorizontalAlignment = HorizontalAlignment.Stretch

            ' Disconnect wp from any existing parent
            If wp.Parent IsNot Nothing Then
                Dim parentBorder = TryCast(wp.Parent, Border)
                If parentBorder IsNot Nothing Then
                    parentBorder.Child = Nothing
                End If
            End If

            ' Assign the WrapPanel to the preview border
            preview.Child = wp
        End Sub

        Private Sub SavePrint(sender As Object, e As RoutedEventArgs)
            Dim dlg As New PrintDialog()
            Dim docName As String = "Barcode-" & DateTime.Now.ToString("yyyyMMdd-HHmmss")

            If dlg.ShowDialog() = True Then
                ' Save original parent and layout
                Dim originalParent = VisualTreeHelper.GetParent(preview)
                Dim originalIndex As Integer = -1
                Dim originalMargin = preview.Margin
                Dim originalTransform = preview.LayoutTransform

                ' Detach from parent if it's inside a Panel
                If TypeOf originalParent Is Panel Then
                    Dim panel = CType(originalParent, Panel)
                    originalIndex = panel.Children.IndexOf(preview)
                    panel.Children.Remove(preview)
                End If

                ' Remove margin and reset transform
                preview.Margin = New Thickness(0)
                preview.LayoutTransform = Transform.Identity

                ' Ensure full layout and rendering
                preview.UpdateLayout()
                preview.Measure(New Size(Double.PositiveInfinity, Double.PositiveInfinity))
                preview.Arrange(New Rect(preview.DesiredSize))
                preview.UpdateLayout()

                Dim borderWidth = preview.ActualWidth
                Dim borderHeight = preview.ActualHeight

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
                container.Children.Add(preview)

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
                preview.LayoutTransform = originalTransform
                preview.Margin = originalMargin

                If TypeOf originalParent Is Panel AndAlso originalIndex >= 0 Then
                    Dim panel = CType(originalParent, Panel)
                    panel.Children.Insert(originalIndex, preview)
                End If
            End If
        End Sub
    End Class
End Namespace
