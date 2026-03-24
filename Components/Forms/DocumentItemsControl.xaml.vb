Imports System.Collections.ObjectModel
Imports System.Globalization
Imports System.IO
Imports System.Windows.Data

Namespace DPC.Components.Forms
    Partial Public Class DocumentItemsControl
        ' Layout Constants from your logic
        Private Const PageMaxHeight As Double = 950
        Private Const FooterSectionHeight As Double = 250
        Private Const BaseItemHeight As Double = 55
        Private Const CategoryHeaderHeight As Double = 40
        Private Const ReservedSpaceForDescription As Double = 30

        ' State
        Private _allItems As List(Of Dictionary(Of String, String))
        Private _paginatedPages As New List(Of List(Of Integer))
        Private _showImages As Boolean = True

        Public ReadOnly Property TotalPages As Integer
            Get
                Return If(_paginatedPages.Count = 0, 1, _paginatedPages.Count)
            End Get
        End Property

        ''' <summary>
        ''' The main entry point. Logic moved from CostEstimate.
        ''' </summary>
        Public Sub LoadData(data As List(Of Dictionary(Of String, String)), showImages As Boolean)
            _allItems = data
            _showImages = showImages
            RecalculatePagination()
        End Sub

        ''' <summary>
        ''' Core Pagination Engine logic preserved from your existing code
        ''' </summary>
        Private Sub RecalculatePagination()
            _paginatedPages.Clear()
            If _allItems Is Nothing OrElse _allItems.Count = 0 Then
                _paginatedPages.Add(New List(Of Integer))
                Return
            End If

            Dim pageIndices As New List(Of Integer)
            Dim currentHeight As Double = 0

            For i As Integer = 0 To _allItems.Count - 1
                Dim h As Double = CalculateItemHeight(_allItems(i))

                If currentHeight + h > PageMaxHeight Then
                    _paginatedPages.Add(New List(Of Integer)(pageIndices))
                    pageIndices.Clear()
                    currentHeight = 0
                End If

                currentHeight += h
                pageIndices.Add(i)
            Next

            If pageIndices.Count > 0 Then _paginatedPages.Add(pageIndices)

            ' Footer space logic
            If currentHeight > (PageMaxHeight - FooterSectionHeight) Then
                _paginatedPages.Add(New List(Of Integer))
            End If
        End Sub

        ''' <summary>
        ''' Calculates height based on Category or Text wrapping
        ''' </summary>
        Private Function CalculateItemHeight(item As Dictionary(Of String, String)) As Double
            If item.ContainsKey("IsCategoryHeader") AndAlso item("IsCategoryHeader") = "True" Then
                Return CategoryHeaderHeight
            End If

            Dim h As Double = BaseItemHeight
            If item.ContainsKey("Description") AndAlso Not String.IsNullOrWhiteSpace(item("Description")) Then
                Dim text As String = item("Description").Trim()
                ' Use standard WPF FormattedText to predict wrapping
                Dim ft As New FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, New Typeface("Lexend"), 12, Brushes.Black, 1.0)
                ft.MaxTextWidth = 400
                h += ft.Height + ReservedSpaceForDescription
            End If
            Return h
        End Function

        ''' <summary>
        ''' Displays a specific calculated page
        ''' </summary>
        Public Sub DisplayPage(index As Integer)
            If index < 0 OrElse index >= _paginatedPages.Count Then Return

            Dim itemDataSource As New ObservableCollection(Of Object)
            Dim indices = _paginatedPages(index)

            For Each idx In indices
                itemDataSource.Add(CreateVisualItem(_allItems(idx)))
            Next

            ItemsGrid.ItemsSource = itemDataSource
            UpdateImageColumnVisibility()
        End Sub

        Private Function CreateVisualItem(item As Dictionary(Of String, String)) As Object
            Dim isHeader As Boolean = item.ContainsKey("IsCategoryHeader") AndAlso item("IsCategoryHeader") = "True"

            Dim rate As Decimal = 0
            Dim linePrice As Decimal = 0
            If Not isHeader Then
                If item.ContainsKey("Rate") Then Decimal.TryParse(item("Rate").Replace("₱", "").Replace(",", "").Trim(), rate)
                If item.ContainsKey("Amount") Then Decimal.TryParse(item("Amount").Replace("₱", "").Replace(",", "").Trim(), linePrice)
            End If

            Return New With {
                .Quantity = If(isHeader, "", item("Quantity")),
                .Description = If(item.ContainsKey("ProductName"), item("ProductName"), ""),
                .ProductDescription = If(item.ContainsKey("Description"), item("Description"), ""),
                .ProductDescriptionVisibility = If(String.IsNullOrWhiteSpace(.ProductDescription), Visibility.Collapsed, Visibility.Visible),
                .UnitPrice = If(isHeader, "", $"₱ {rate:N2}"),
                .LinePrice = If(isHeader, "", $"₱ {linePrice:N2}"),
                .IsHeaderRow = isHeader,
                .ProductImage = If(_showImages, ProcessImage(item), Nothing)
            }
        End Function

        Private Function ProcessImage(item As Dictionary(Of String, String)) As BitmapImage
            Try
                If item.ContainsKey("ProductImageBase64") AndAlso Not String.IsNullOrEmpty(item("ProductImageBase64")) Then
                    Return Base64ToBitmapImage(item("ProductImageBase64"))
                End If
            Catch
            End Try
            Return Nothing
        End Function

        Private Function Base64ToBitmapImage(b64 As String) As BitmapImage
            Try
                If b64.Contains(",") Then b64 = b64.Split(","c)(1)
                Dim bytes As Byte() = Convert.FromBase64String(b64)
                Using ms As New MemoryStream(bytes)
                    Dim bmp As New BitmapImage()
                    bmp.BeginInit()
                    bmp.CacheOption = BitmapCacheOption.OnLoad
                    bmp.StreamSource = ms
                    bmp.EndInit()
                    bmp.Freeze()
                    Return bmp
                End Using
            Catch
                Return Nothing
            End Try
        End Function

        Private Sub UpdateImageColumnVisibility()
            colImage.Visibility = If(_showImages, Visibility.Visible, Visibility.Collapsed)
        End Sub
    End Class

    Public Class StringToUpperConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Return If(value IsNot Nothing, value.ToString().ToUpper(), "")
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace