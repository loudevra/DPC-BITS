Imports System.Windows.Controls.Primitives
Imports System.Windows
Imports DPC.DPC.Data.Helpers

Namespace DPC.Views.Sales.Quotes
    Public Class CostEstimate
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

        End Sub

        Private Sub BackToUI_Click(sender As Object, e As MouseButtonEventArgs)
            ViewLoader.DynamicView.NavigateToView("salesnewquote", Me)
        End Sub
    End Class
End Namespace