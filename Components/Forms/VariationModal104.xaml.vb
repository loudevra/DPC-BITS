Namespace DPC.Components.Forms
    Public Class VariationModal104
        Public Event CloseRequested As EventHandler

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub ClosePopup(sender As Object, e As RoutedEventArgs)
            RaiseEvent CloseRequested(Me, EventArgs.Empty)
        End Sub

        Private Sub AddNewVariation(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("New Variation Added Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
            RaiseEvent CloseRequested(Me, EventArgs.Empty)
        End Sub
    End Class
End Namespace