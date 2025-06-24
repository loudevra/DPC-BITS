Imports MySql.Data.MySqlClient


Namespace DPC.Components.ConfirmationModals
    Public Class DeleteProductConfirmation

        Public Sub New()
            InitializeComponent()
        End Sub

        Public Event Confirm(sender As Object, e As RoutedEventArgs)

        Public Sub DeleteProductConfirmation_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            lblProductName.Text = cacheDeleteProductName
        End Sub

        Private Sub CloseDeleteDialog(sender As Object, e As RoutedEventArgs)
            PopupHelper.ClosePopup()
        End Sub

        Private Sub ConfirmDeletion_Click(sender As Object, e As RoutedEventArgs)
            PopupHelper.ClosePopup()
            RaiseEvent Confirm(Me, e)
        End Sub
    End Class

End Namespace
