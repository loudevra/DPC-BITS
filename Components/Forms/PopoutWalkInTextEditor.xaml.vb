Imports DPC.DPC.Views.Sales.Quotes
Imports DPC.DPC.Views.Stocks.PurchaseOrder.WalkIn
Imports Org.BouncyCastle.Crypto.Paddings

Namespace DPC.Components.Forms
    Public Class PopoutWalkInTextEditor
        Public Sub New(elementText As String)

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            txtEdited.Text = elementText
            txtEdited.Focus()
        End Sub

        Public Sub SaveEdit(sender As Object, e As RoutedEventArgs)
            WalkInBillingStatement.ModifyText(txtEdited.Text)
            Me.Close()
        End Sub
    End Class
End Namespace