Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media

Namespace DPC.Views.POS
    Partial Public Class InvoicePayment
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
            AddHandler BtnClose.Click, AddressOf BtnClose_Click
        End Sub

        ' Close Button Handler – properly handles both Panel and ContentControl parents
        Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
            ' Case 1: Remove from parent Panel (e.g., Grid, StackPanel, etc.)
            Dim panel = TryCast(Me.Parent, Panel)
            If panel IsNot Nothing Then
                panel.Children.Remove(Me)
                Return
            End If

            ' Case 2: Clear Content from ContentControl
            Dim contentHost = TryCast(Me.Parent, ContentControl)
            If contentHost IsNot Nothing Then
                contentHost.Content = Nothing
                Return
            End If

            ' Case 3: Fallback - just hide it
            Me.Visibility = Visibility.Collapsed
        End Sub

    End Class
End Namespace
