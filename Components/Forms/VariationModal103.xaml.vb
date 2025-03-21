Namespace DPC.Components.Forms
    Public Class VariationModal103
        Public Event CloseRequested As EventHandler

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub ClosePopup(sender As Object, e As RoutedEventArgs)
            RaiseEvent CloseRequested(Me, EventArgs.Empty)
        End Sub

        Private Sub ModifiedVariation(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Variation Modified Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
            RaiseEvent CloseRequested(Me, EventArgs.Empty)
        End Sub
    End Class
End Namespace