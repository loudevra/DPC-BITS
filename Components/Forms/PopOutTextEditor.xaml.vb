Imports DPC.DPC.Views.Stocks.PurchaseOrder.NewOrder
Imports Org.BouncyCastle.Crypto.Paddings

Namespace DPC.Components.Forms
    Public Class PopOutTextEditor
        Public Sub New(elementText As String)

            ' This call is required by the designer.
            InitializeComponent()

            txtEdited.Text = elementText
        End Sub

        Public Sub SaveEdit(sender As Object, e As RoutedEventArgs)
            BillingStatement.ModifyText(txtEdited.Text)
            Me.Close()
        End Sub
    End Class
End Namespace

