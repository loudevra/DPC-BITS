Namespace DPC.Components.Forms
    Public Class AddNewSupplierForm
        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub CloseWindow_Click(sender As Object, e As RoutedEventArgs)
            Me.Close()
        End Sub

        Private Sub AddSupplier_Click(sender As Object, e As RoutedEventArgs)
            ' Logic to handle adding a new supplier
            MessageBox.Show("Supplier Added Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
            Me.Close()
        End Sub
    End Class
End Namespace