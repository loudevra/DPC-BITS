Namespace DPC.Views.Stocks.ProductsLabel.CustomLabel
    ''' <summary>
    ''' Interaction logic for CustomLabel.xaml
    ''' </summary>

    Public Class CustomLabel
        Inherits UserControl

        Public Sub New()
            InitializeComponent()


        End Sub
        ' Trigger the default selection on window load
        Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
            ' Set Add as the default checked radio button
            AddProductName.IsChecked = True
            ' Apply styles accordingly
            UpdateStyles(AddBorderProductName, AddIconProductName, "#456B2E", True)
            UpdateStyles(AddBorderBusinessLocation, AddIconBusinessLocation, "#456B2E", True)
            UpdateStyles(AddBorderWarehouse, AddIconWarehouse, "#456B2E", True)
            UpdateStyles(AddBorderPrice, AddIconPrice, "#456B2E", True)
            UpdateStyles(AddBorderProductCode, AddIconProductCode, "#456B2E", True)
            UpdateStyles(AddBorderProducts, AddIconProducts, "#456B2E", True)
        End Sub

        ' General method to update styles
        Private Sub UpdateStyles(border As Border, icon As MaterialDesignThemes.Wpf.PackIcon, color As String, isChecked As Boolean)
            If border IsNot Nothing AndAlso icon IsNot Nothing Then
                If isChecked Then
                    Dim newColor As New SolidColorBrush(CType(ColorConverter.ConvertFromString(color), Color))
                    border.BorderBrush = newColor
                    icon.Foreground = newColor
                Else
                    border.BorderBrush = Brushes.Black
                    icon.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#474747"))
                End If
            End If
        End Sub

        ' Event Handlers

        'Product Name
        Private Sub Add_CheckedProductName(sender As Object, e As RoutedEventArgs)
            UpdateStyles(AddBorderProductName, AddIconProductName, "#456B2E", True)
        End Sub

        Private Sub Add_UncheckedProductName(sender As Object, e As RoutedEventArgs)
            UpdateStyles(AddBorderProductName, AddIconProductName, "#456B2E", False)
        End Sub

        Private Sub Remove_CheckedProductName(sender As Object, e As RoutedEventArgs)
            UpdateStyles(RemoveBorderProductName, RemoveIconProductName, "#D23636", True)
        End Sub

        Private Sub Remove_UncheckedProductName(sender As Object, e As RoutedEventArgs)
            UpdateStyles(RemoveBorderProductName, RemoveIconProductName, "#D23636", False)
        End Sub

        'Business Location
        Private Sub Add_CheckedBusinessLocation(sender As Object, e As RoutedEventArgs)
            UpdateStyles(AddBorderBusinessLocation, AddIconBusinessLocation, "#456B2E", True)
        End Sub

        Private Sub Add_UncheckedBusinessLocation(sender As Object, e As RoutedEventArgs)
            UpdateStyles(AddBorderBusinessLocation, AddIconBusinessLocation, "#456B2E", False)
        End Sub

        Private Sub Remove_CheckedBusinessLocation(sender As Object, e As RoutedEventArgs)
            UpdateStyles(RemoveBorderBusinessLocation, RemoveIconBusinessLocation, "#D23636", True)
        End Sub

        Private Sub Remove_UncheckedBusinessLocation(sender As Object, e As RoutedEventArgs)
            UpdateStyles(RemoveBorderBusinessLocation, RemoveIconBusinessLocation, "#D23636", False)
        End Sub

        'Warehouse
        Private Sub Add_CheckedWarehouse(sender As Object, e As RoutedEventArgs)
            UpdateStyles(AddBorderWarehouse, AddIconWarehouse, "#456B2E", True)
        End Sub

        Private Sub Add_UncheckedWarehouse(sender As Object, e As RoutedEventArgs)
            UpdateStyles(AddBorderWarehouse, AddIconWarehouse, "#456B2E", False)
        End Sub

        Private Sub Remove_CheckedWarehouse(sender As Object, e As RoutedEventArgs)
            UpdateStyles(RemoveBorderWarehouse, RemoveIconWarehouse, "#D23636", True)
        End Sub

        Private Sub Remove_UncheckedWarehouse(sender As Object, e As RoutedEventArgs)
            UpdateStyles(RemoveBorderWarehouse, RemoveIconWarehouse, "#D23636", False)
        End Sub

        'Price
        Private Sub Add_CheckedPrice(sender As Object, e As RoutedEventArgs)
            UpdateStyles(AddBorderPrice, AddIconPrice, "#456B2E", True)
        End Sub

        Private Sub Add_UncheckedPrice(sender As Object, e As RoutedEventArgs)
            UpdateStyles(AddBorderPrice, AddIconPrice, "#456B2E", False)
        End Sub

        Private Sub Remove_CheckedPrice(sender As Object, e As RoutedEventArgs)
            UpdateStyles(RemoveBorderPrice, RemoveIconPrice, "#D23636", True)
        End Sub

        Private Sub Remove_UncheckedPrice(sender As Object, e As RoutedEventArgs)
            UpdateStyles(RemoveBorderPrice, RemoveIconPrice, "#D23636", False)
        End Sub

        'Product Code
        Private Sub Add_CheckedProductCode(sender As Object, e As RoutedEventArgs)
            UpdateStyles(AddBorderProductCode, AddIconProductCode, "#456B2E", True)
        End Sub

        Private Sub Add_UncheckedProductCode(sender As Object, e As RoutedEventArgs)
            UpdateStyles(AddBorderProductCode, AddIconProductCode, "#456B2E", False)
        End Sub

        Private Sub Remove_CheckedProductCode(sender As Object, e As RoutedEventArgs)
            UpdateStyles(RemoveBorderProductCode, RemoveIconProductCode, "#D23636", True)
        End Sub

        Private Sub Remove_UncheckedProductCode(sender As Object, e As RoutedEventArgs)
            UpdateStyles(RemoveBorderProductCode, RemoveIconProductCode, "#D23636", False)
        End Sub

        'Products
        Private Sub Add_CheckedProducts(sender As Object, e As RoutedEventArgs)
            UpdateStyles(AddBorderProducts, AddIconProducts, "#456B2E", True)
        End Sub

        Private Sub Add_UncheckedProducts(sender As Object, e As RoutedEventArgs)
            UpdateStyles(AddBorderProducts, AddIconProducts, "#456B2E", False)
        End Sub

        Private Sub Remove_CheckedProducts(sender As Object, e As RoutedEventArgs)
            UpdateStyles(RemoveBorderProducts, RemoveIconProducts, "#D23636", True)
        End Sub

        Private Sub Remove_UncheckedProducts(sender As Object, e As RoutedEventArgs)
            UpdateStyles(RemoveBorderProducts, RemoveIconProducts, "#D23636", False)
        End Sub
    End Class
End Namespace