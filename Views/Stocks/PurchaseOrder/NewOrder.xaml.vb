Namespace DPC.Views.Stocks.PurchaseOrder
    Public Class NewOrder
        Inherits Window

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("You Clicked Me!")
        End Sub
    End Class
End Namespace