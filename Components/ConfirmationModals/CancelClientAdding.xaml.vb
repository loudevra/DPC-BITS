Imports System.Windows.Controls

Namespace DPC.Components.ConfirmationModals
    Public Class CancelClientAdding
        Public Sub New()
            InitializeComponent()
        End Sub

        ' Event for "Keep Editing" Button
        Private Sub KeepEditing_Click(sender As Object, e As RoutedEventArgs)
            Me.Visibility = Visibility.Collapsed ' Hide Modal
        End Sub

        ' Event for "Cancel" Button
        Private Sub Cancel_Click(sender As Object, e As RoutedEventArgs)
            ' Close or perform an action for canceling client editing
            Dim parentWindow As Window = Window.GetWindow(Me)
            parentWindow.DialogResult = True
            parentWindow.Close()
        End Sub
    End Class
End Namespace
