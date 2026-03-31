Imports DPC.Data.Helpers.ViewLoader
Imports DPC.DPC.Data.Helpers.ViewLoader

Namespace DPC.Views.Stocks.ProductsLabel.StandardLabel
    ''' <summary>
    ''' Interaction logic for StandardLabel.xaml
    ''' </summary>
    Public Class StandardLabel
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub

        ' Re-apply correct radio state every time this view becomes visible from cache
        Private Sub UserControl_IsVisibleChanged(sender As Object, e As DependencyPropertyChangedEventArgs)
            If CBool(e.NewValue) = True Then
                BtnStandardLabel.IsChecked = True
            End If
        End Sub

        ' Use Click instead of Checked — only fires on real user clicks, not programmatic IsChecked changes
        Private Sub BtnCustomLabel_Click(sender As Object, e As RoutedEventArgs)
            If Me.IsLoaded Then
                ViewNavigation.NavigateToCachedView("CustomLabel", TryCast(sender, DependencyObject))
            End If
        End Sub

        Private Sub BtnStandardLabel_Click(sender As Object, e As RoutedEventArgs)
            If Me.IsLoaded Then
                ViewNavigation.NavigateToCachedView("StandardLabel", TryCast(sender, DependencyObject))
            End If
        End Sub

    End Class
End Namespace