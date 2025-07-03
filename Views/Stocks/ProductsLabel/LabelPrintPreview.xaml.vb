Imports System.Windows.Markup
Imports System.IO
Imports System.Xml
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Media

Namespace DPC.Views.Stocks.ProductsLabel
    Public Class LabelPrintPreview
        Private wp As StackPanel ' Global stack panel reference

        Public Sub New(element As FrameworkElement)
            InitializeComponent()

            wp = DirectCast(element, StackPanel)
            wp.HorizontalAlignment = HorizontalAlignment.Stretch

            ' Disconnect wp from any existing parent
            If wp.Parent IsNot Nothing Then
                Dim parentBorder = TryCast(wp.Parent, Border)
                If parentBorder IsNot Nothing Then
                    parentBorder.Child = Nothing
                End If
            End If

            ' Assign wp to the preview border
            preview.Child = wp
        End Sub

        Private Sub SavePrint(sender As Object, e As RoutedEventArgs)
            Dim dlg As New PrintDialog()
            Dim docName As String = "Barcode-" & DateTime.Now.ToString("yyyyMMdd-HHmmss")

            If dlg.ShowDialog() = True Then
                Dim printableWidth = dlg.PrintableAreaWidth
                Dim printableHeight = dlg.PrintableAreaHeight

                Dim fixedDoc As New FixedDocument()
                Dim pagePanel As New StackPanel()
                Dim currentHeight As Double = 0

                ' Store and clear original children
                Dim originalChildren = wp.Children.Cast(Of UIElement).ToList()
                wp.Children.Clear()

                For Each child As UIElement In originalChildren
                    child.Measure(New Size(printableWidth, Double.PositiveInfinity))
                    Dim h = child.DesiredSize.Height

                    ' Only add a page if there's content
                    If currentHeight + h > printableHeight AndAlso pagePanel.Children.Count > 0 Then
                        AddFixedPage(fixedDoc, pagePanel, printableWidth, printableHeight)
                        pagePanel = New StackPanel()
                        currentHeight = 0
                    End If

                    pagePanel.Children.Add(child)
                    currentHeight += h
                Next

                ' Add remaining items as the last page if any
                If pagePanel.Children.Count > 0 Then
                    AddFixedPage(fixedDoc, pagePanel, printableWidth, printableHeight)
                End If

                dlg.PrintDocument(fixedDoc.DocumentPaginator, docName)

                ' Restore original layout
                For Each child In originalChildren
                    Dim parent = LogicalTreeHelper.GetParent(child)
                    If TypeOf parent Is Panel Then
                        CType(parent, Panel).Children.Remove(child)
                    End If
                    wp.Children.Add(child)
                Next
            End If
        End Sub


        Private Sub AddFixedPage(fixedDoc As FixedDocument, content As StackPanel, width As Double, height As Double)
            Dim fixedPage As New FixedPage With {.Width = width, .Height = height}
            content.Measure(New Size(width, height))
            content.Arrange(New Rect(New Size(width, height)))
            fixedPage.Children.Add(content)

            Dim pageContent As New PageContent()
            CType(pageContent, IAddChild).AddChild(fixedPage)
            fixedDoc.Pages.Add(pageContent)
        End Sub
    End Class
End Namespace
