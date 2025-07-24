Imports DPC.DPC.Data.Helpers
Imports Microsoft.VisualBasic.ApplicationServices

Namespace DPC.Views.Stocks.ProductsLabel.StandardLabel
    ''' <summary>
    ''' Interaction logic for CustomLabel.xaml
    ''' </summary>

    Public Class StandardLabel
        Inherits UserControl

        Public Sub New()
            InitializeComponent()

        End Sub

        Private Sub CustomLabel(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("addcustomlabel", Me)
        End Sub
        Private Sub StandardLabel(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("addstandardlabel", Me)
        End Sub
    End Class
End Namespace