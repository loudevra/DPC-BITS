Imports DPC.DPC.Data.Helpers

Namespace DPC.Views.Accounts.Transactions
    Public Class ClientsTransactions
        Public Sub New()
            InitializeComponent()
        End Sub
        Private Sub AddClient(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("addclienttabs", Me)
        End Sub
    End Class
End Namespace
