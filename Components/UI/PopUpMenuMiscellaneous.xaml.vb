Namespace DPC.Components.UI
    Public Class PopUpMenuMiscelleneous
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub
        Private Sub NavigateToNewInvoice(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Navigate to New Invoice", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub
    End Class
End Namespace
