Imports DPC.DPC.Data.Helpers

Namespace DPC.Views.Stocks.PurchaseOrder.WalkIn
    Partial Public Class WalkInNewOrder
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub BtnAddClient_Click(sender As Object, e As RoutedEventArgs) Handles BtnAddClient.Click
            ViewLoader.DynamicView.NavigateToView("newwalkinclient", Me)
        End Sub
    End Class
End Namespace