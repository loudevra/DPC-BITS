Imports DPC.DPC.Views.Sales.Quotes
Imports Org.BouncyCastle.Crypto.Paddings

Namespace DPC.Components.Forms
    Public Class PopOutQuoteTextEditor
        Public Sub New(elementText As String)

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            txtEdited.Text = elementText
        End Sub

        Public Sub SaveEdit(sender As Object, e As RoutedEventArgs)
            CostEstimate.ModifyText(txtEdited.Text)
            Me.Close()
        End Sub
    End Class
End Namespace