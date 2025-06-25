
Namespace DPC.Components.ConfirmationModals
    Public Class ConfirmPurchaseOrder

        Public Property UserConfirmed As Boolean = False

        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

        End Sub

        Private Sub ConfirmButton(sender As Object, e As RoutedEventArgs)
            UserConfirmed = True
            Me.Close()
        End Sub

        Private Sub CancelButton(sender As Object, e As RoutedEventArgs)
            UserConfirmed = False
            Me.Close()
        End Sub
    End Class

End Namespace


