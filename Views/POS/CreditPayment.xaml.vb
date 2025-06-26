Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media

Namespace DPC.Views.POS
    Partial Public Class CreditPayment
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
            AddHandler BtnClose.Click, AddressOf BtnClose_Click
        End Sub

        ' Close Button Handler – safely removes or hides this UserControl
        Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
            ' Case 1: If hosted in a Panel (like Grid, StackPanel)
            Dim panel = TryCast(Me.Parent, Panel)
            If panel IsNot Nothing Then
                panel.Children.Remove(Me)
            End If

            ' Case 2: If hosted in a ContentControl (like ContentPresenter)
            Dim contentHost = TryCast(Me.Parent, ContentControl)
            If contentHost IsNot Nothing Then
                contentHost.Content = Nothing
            End If

            ' Case 3: Fallback - just hide this control
            If Me.Visibility <> Visibility.Collapsed Then
                Me.Visibility = Visibility.Collapsed
            End If
        End Sub

    End Class
End Namespace
