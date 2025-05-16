Imports DPC.DPC.Components.Dynamic ' Import for DynamicDialogs

Namespace DPC.Components.Forms
    Public Class AddNewSupplierForm
        Public Event CloseRequested As EventHandler

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub ClosePopup(sender As Object, e As RoutedEventArgs)
            RaiseEvent CloseRequested(Me, EventArgs.Empty)
        End Sub

        Private Sub AddSupplier(sender As Object, e As RoutedEventArgs)
            ' Replace MessageBox with DynamicDialogs
            Dim successDialog = DynamicDialogs.ShowSuccess(Me, "Supplier Added Successfully!")

            ' Handle the dialog closed event to raise CloseRequested when dialog is closed
            AddHandler successDialog.DialogClosed, AddressOf OnDialogClosed
        End Sub

        Private Sub OnDialogClosed(sender As Object, e As DynamicDialogs.DialogEventArgs)
            ' Close the form after the dialog is closed
            RaiseEvent CloseRequested(Me, EventArgs.Empty)
        End Sub
    End Class
End Namespace