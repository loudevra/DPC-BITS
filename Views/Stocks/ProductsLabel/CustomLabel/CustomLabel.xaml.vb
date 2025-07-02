Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Controllers.Stocks
Imports DPC.DPC.Data.Model

Namespace DPC.Views.Stocks.ProductsLabel.CustomLabel
    ''' <summary>
    ''' Interaction logic for CustomLabel.xaml
    ''' </summary>

    Public Class CustomLabel
        Inherits UserControl

        Private _product As ObservableCollection(Of Product)
        Private _selectedProduct As New Product
        Private _warehouseID As Integer
        Private SerialNumberList As List(Of String)
        Private wrapPanel As New WrapPanel

        Public Sub New()
            InitializeComponent()

            LoadWarehouses()

        End Sub
        ' Trigger the default selection on window load
        Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
            ' Set Add as the default checked radio button
            'AddProductName.IsChecked = True
            ' Apply styles accordingly
            'UpdateStyles(AddBorderProductName, AddIconProductName, "#456B2E", True)
            'UpdateStyles(AddBorderBusinessLocation, AddIconBusinessLocation, "#456B2E", True)
            'UpdateStyles(AddBorderWarehouse, AddIconWarehouse, "#456B2E", True)
            'UpdateStyles(AddBorderPrice, AddIconPrice, "#456B2E", True)
            'UpdateStyles(AddBorderProductCode, AddIconProductCode, "#456B2E", True)
            'UpdateStyles(AddBorderProducts, AddIconProducts, "#456B2E", True)
        End Sub

        ' General method to update styles
        'Private Sub UpdateStyles(border As Border, icon As MaterialDesignThemes.Wpf.PackIcon, color As String, isChecked As Boolean)
        '    If border IsNot Nothing AndAlso icon IsNot Nothing Then
        '        If isChecked Then
        '            Dim newColor As New SolidColorBrush(CType(ColorConverter.ConvertFromString(color), Color))
        '            border.BorderBrush = newColor
        '            icon.Foreground = newColor
        '        Else
        '            border.BorderBrush = Brushes.Black
        '            icon.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#474747"))
        '        End If
        '    End If
        'End Sub

        ' Event Handlers
        Private Sub LoadWarehouses()
            Dim warehouse = StocksLabelController.GetWarehouse()
            cmbWarehouses.ItemsSource = warehouse
            cmbWarehouses.DisplayMemberPath = "Value"
            cmbWarehouses.SelectedValuePath = "Key"
        End Sub

        Private Sub searchProducts(sender As Object, e As TextChangedEventArgs)

            _product = StocksLabelController.SearchProducts(txtProducts.Text, _warehouseID)

            LstItems.ItemsSource = _product

            ' Show popup if we have results
            AutoCompletePopup.IsOpen = _product.Count > 0

            ' Adjust popup width to match the textbox
            AutoCompletePopup.Width = txtProducts.ActualWidth
        End Sub

        Private Sub LstItems_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If LstItems.SelectedItem IsNot Nothing Then
                _selectedProduct = CType(LstItems.SelectedItem, Product)
                txtProducts.Text = _selectedProduct.ProductName
                AutoCompletePopup.IsOpen = False

                ' Store serial numbers
                SerialNumberList = StocksLabelController.GetSerialNumber(_selectedProduct.ProductID)
            End If
        End Sub

        Private Sub ItemsFromWarehouse(sender As Object, e As SelectionChangedEventArgs)
            _warehouseID = CType(cmbWarehouses.SelectedValue, Integer)
            _selectedProduct = Nothing
            txtProducts.Clear()
        End Sub
        Private Function MmToDiu(mm As Double) As Double
            Return mm * 96 / 25.4
        End Function

        Private Function CreateBarcodePanel(code As String) As StackPanel
            Dim _width = MmToDiu(barcodeWidth.Text)
            Dim _height = MmToDiu(barcodeHeight.Text)
            Dim _lblWidth = MmToDiu(lblWidth.Text)
            Dim _lblHeight = MmToDiu(lblHeight.Text)

            Dim sizeString As String = cmbFontSize.Text.Replace("pt", "")
            Dim fontSizeDouble As Double = CType(sizeString, Double)

            Dim productLabel As New TextBlock With {
        .Width = _lblWidth,
        .Height = _lblHeight,
        .TextWrapping = TextWrapping.Wrap,
        .FontSize = fontSizeDouble,
        .Text = _selectedProduct.ProductName,
        .VerticalAlignment = VerticalAlignment.Center,
        .HorizontalAlignment = HorizontalAlignment.Center,
        .TextAlignment = TextAlignment.Center,
        .Margin = New Thickness(0, 2, 0, 0)
    }

            Dim ProductCodeLabel As New TextBlock With {
        .Width = _lblWidth,
        .FontSize = fontSizeDouble,
        .Text = _selectedProduct.ProductCode,
        .VerticalAlignment = VerticalAlignment.Center,
        .HorizontalAlignment = HorizontalAlignment.Center,
        .TextAlignment = TextAlignment.Center,
        .Margin = New Thickness(0, 2, 0, 0)
    }
            Dim imageSource As BitmapImage = StocksLabelController.GenerateBarcode(code, _width, _height, barcodeType.SelectedIndex)
            Dim barcodeImage As New System.Windows.Controls.Image

            If imageSource IsNot Nothing Then
                barcodeImage.Source = imageSource
                barcodeImage.Height = _height
                barcodeImage.Width = _width
                barcodeImage.Margin = New Thickness(0, 2, 0, 0)
                barcodeImage.VerticalAlignment = VerticalAlignment.Center
                barcodeImage.Stretch = Stretch.None
                barcodeImage.HorizontalAlignment = HorizontalAlignment.Stretch
            Else
                MessageBox.Show("Failed to generate barcode image. Please check the code format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Dim nullPanel As New StackPanel
                Return nullPanel
            End If

            Dim barcodeLabel As New TextBlock With {
        .Width = _lblWidth,
        .FontSize = fontSizeDouble,
        .Text = code,
        .VerticalAlignment = VerticalAlignment.Center,
        .HorizontalAlignment = HorizontalAlignment.Center,
        .TextAlignment = TextAlignment.Center,
        .Margin = New Thickness(0, 2, 0, 0)
    }

            Dim retailPrice As New TextBlock With {
        .TextWrapping = TextWrapping.Wrap,
        .FontSize = fontSizeDouble,
        .Text = "₱ " + _selectedProduct.RetailPrice.ToString("N2"),
        .VerticalAlignment = VerticalAlignment.Center,
        .HorizontalAlignment = HorizontalAlignment.Center,
        .TextAlignment = TextAlignment.Center,
        .Margin = New Thickness(0, 2, 0, 0)
    }

            Dim warehouseLabel As New TextBlock With {
        .Width = _lblWidth,
        .FontSize = fontSizeDouble,
        .TextWrapping = TextWrapping.Wrap,
        .Text = cmbWarehouses.Text,
        .VerticalAlignment = VerticalAlignment.Center,
        .HorizontalAlignment = HorizontalAlignment.Center,
        .TextAlignment = TextAlignment.Center,
        .Margin = New Thickness(0, 2, 0, 0)
    }

            Dim sp As New StackPanel With {
        .Background = New SolidColorBrush(Colors.White),
        .Margin = New Thickness(0, 0, 4, 0)
    }

            If includeProductName.IsChecked Then
                sp.Children.Add(productLabel)
            End If
            If includeProductCode.IsChecked Then
                sp.Children.Add(ProductCodeLabel)
            End If
            sp.Children.Add(barcodeImage)
            sp.Children.Add(barcodeLabel)
            If includePrice.IsChecked Then
                sp.Children.Add(retailPrice)
            End If
            If includeWarehouse.IsChecked Then
                sp.Children.Add(warehouseLabel)
            End If

            Return sp
        End Function

        Private Sub PrintBarcodes(sender As Object, e As RoutedEventArgs)
            If SerialNumberList Is Nothing Then Exit Sub
            wrapPanel.Children.Clear()

            Dim copyCount As Integer = 1

            If Not Integer.TryParse(copies.Text, copyCount) OrElse copyCount < 1 Then
                copies.Text = "1"
                copyCount = 1
            End If

            Dim containerPanel As New StackPanel With {.Orientation = Orientation.Vertical}
            Dim currentBorder As New Border

            For i As Integer = 0 To copyCount - 1
                For Each serialNumber As String In SerialNumberList
                    If CreateBarcodePanel(serialNumber).Children.Count < 1 Then
                        Exit Sub
                    End If

                    wrapPanel.Children.Add(CreateBarcodePanel(serialNumber))
                Next
            Next

            Dim lblPreview As New LabelPrintPreview(wrapPanel)
            lblPreview.ShowDialog()
        End Sub


        'Product Name
        'Private Sub Add_CheckedProductName(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(AddBorderProductName, AddIconProductName, "#456B2E", True)
        'End Sub

        'Private Sub Add_UncheckedProductName(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(AddBorderProductName, AddIconProductName, "#456B2E", False)
        'End Sub

        'Private Sub Remove_CheckedProductName(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(RemoveBorderProductName, RemoveIconProductName, "#D23636", True)
        'End Sub

        'Private Sub Remove_UncheckedProductName(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(RemoveBorderProductName, RemoveIconProductName, "#D23636", False)
        'End Sub

        ''Business Location
        'Private Sub Add_CheckedBusinessLocation(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(AddBorderBusinessLocation, AddIconBusinessLocation, "#456B2E", True)
        'End Sub

        'Private Sub Add_UncheckedBusinessLocation(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(AddBorderBusinessLocation, AddIconBusinessLocation, "#456B2E", False)
        'End Sub

        'Private Sub Remove_CheckedBusinessLocation(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(RemoveBorderBusinessLocation, RemoveIconBusinessLocation, "#D23636", True)
        'End Sub

        'Private Sub Remove_UncheckedBusinessLocation(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(RemoveBorderBusinessLocation, RemoveIconBusinessLocation, "#D23636", False)
        'End Sub

        ''Warehouse
        'Private Sub Add_CheckedWarehouse(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(AddBorderWarehouse, AddIconWarehouse, "#456B2E", True)
        'End Sub

        'Private Sub Add_UncheckedWarehouse(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(AddBorderWarehouse, AddIconWarehouse, "#456B2E", False)
        'End Sub

        'Private Sub Remove_CheckedWarehouse(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(RemoveBorderWarehouse, RemoveIconWarehouse, "#D23636", True)
        'End Sub

        'Private Sub Remove_UncheckedWarehouse(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(RemoveBorderWarehouse, RemoveIconWarehouse, "#D23636", False)
        'End Sub

        ''Price
        'Private Sub Add_CheckedPrice(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(AddBorderPrice, AddIconPrice, "#456B2E", True)
        'End Sub

        'Private Sub Add_UncheckedPrice(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(AddBorderPrice, AddIconPrice, "#456B2E", False)
        'End Sub

        'Private Sub Remove_CheckedPrice(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(RemoveBorderPrice, RemoveIconPrice, "#D23636", True)
        'End Sub

        'Private Sub Remove_UncheckedPrice(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(RemoveBorderPrice, RemoveIconPrice, "#D23636", False)
        'End Sub

        ''Product Code
        'Private Sub Add_CheckedProductCode(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(AddBorderProductCode, AddIconProductCode, "#456B2E", True)
        'End Sub

        'Private Sub Add_UncheckedProductCode(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(AddBorderProductCode, AddIconProductCode, "#456B2E", False)
        'End Sub

        'Private Sub Remove_CheckedProductCode(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(RemoveBorderProductCode, RemoveIconProductCode, "#D23636", True)
        'End Sub

        'Private Sub Remove_UncheckedProductCode(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(RemoveBorderProductCode, RemoveIconProductCode, "#D23636", False)
        'End Sub

        ''Products
        'Private Sub Add_CheckedProducts(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(AddBorderProducts, AddIconProducts, "#456B2E", True)
        'End Sub

        'Private Sub Add_UncheckedProducts(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(AddBorderProducts, AddIconProducts, "#456B2E", False)
        'End Sub

        'Private Sub Remove_CheckedProducts(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(RemoveBorderProducts, RemoveIconProducts, "#D23636", True)
        'End Sub

        'Private Sub Remove_UncheckedProducts(sender As Object, e As RoutedEventArgs)
        '    UpdateStyles(RemoveBorderProducts, RemoveIconProducts, "#D23636", False)
        'End Sub
    End Class
End Namespace