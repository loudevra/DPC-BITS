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

        Private Sub BtnCustomLabel_Checked(sender As Object, e As RoutedEventArgs)
            If Me.IsLoaded Then
                ViewNavigation.NavigateToView("CustomLabel", TryCast(sender, DependencyObject))
            End If
        End Sub

        Private Sub BtnStandardLabel_Checked(sender As Object, e As RoutedEventArgs)
            If Me.IsLoaded Then
                ViewNavigation.NavigateToView("StandardLabel", TryCast(sender, DependencyObject))
            End If
        End Sub

    End Class
End Namespace