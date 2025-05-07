Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media.Animation

Namespace DPC.Components.ConfirmationModals
    Public Class LoginConfirmationModals
        Inherits UserControl

        ' Event handlers for modal actions
        Public Event SuccessConfirmed()
        Public Event ErrorRetry()
        Public Event ModalClosed()

        Public Sub New()
            InitializeComponent()
            ' Set the Panel to be invisible by default
            Me.Visibility = Visibility.Collapsed
        End Sub

        ' Show success modal with custom message
        Public Sub ShowSuccess(message As String, Optional buttonText As String = "Got It!")
            SuccessMessage.Text = message
            BtnSuccessAction.Content = buttonText
            Me.Visibility = Visibility.Visible
            SuccessModalOverlay.Visibility = Visibility.Visible
            ErrorModalOverlay.Visibility = Visibility.Collapsed

            ' Add focus to make keyboard navigation work
            BtnSuccessAction.Focus()
        End Sub

        ' Show error modal with custom message
        Public Sub ShowError(message As String, Optional buttonText As String = "Try Again")
            ErrorMessage.Text = message
            BtnErrorAction.Content = buttonText
            Me.Visibility = Visibility.Visible
            ErrorModalOverlay.Visibility = Visibility.Visible
            SuccessModalOverlay.Visibility = Visibility.Collapsed

            ' Add focus to make keyboard navigation work
            BtnErrorAction.Focus()
        End Sub

        ' Hide all modals with fade out animation
        Public Sub HideModals()
            ' Create fade out animation
            Dim fadeOut As New DoubleAnimation With {
                .From = 1,
                .To = 0,
                .Duration = New Duration(TimeSpan.FromSeconds(0.2))
            }

            ' Add completed event handler to update visibility after animation
            AddHandler fadeOut.Completed, Sub(s, e)
                                              SuccessModalOverlay.Visibility = Visibility.Collapsed
                                              ErrorModalOverlay.Visibility = Visibility.Collapsed
                                              Me.Visibility = Visibility.Collapsed
                                              RaiseEvent ModalClosed()
                                          End Sub

            ' Apply animation to the visible modal
            If SuccessModalOverlay.Visibility = Visibility.Visible Then
                SuccessModalOverlay.BeginAnimation(Grid.OpacityProperty, fadeOut)
            ElseIf ErrorModalOverlay.Visibility = Visibility.Visible Then
                ErrorModalOverlay.BeginAnimation(Grid.OpacityProperty, fadeOut)
            Else
                ' If no modal is visible, just collapse them
                SuccessModalOverlay.Visibility = Visibility.Collapsed
                ErrorModalOverlay.Visibility = Visibility.Collapsed
                Me.Visibility = Visibility.Collapsed
                RaiseEvent ModalClosed()
            End If
        End Sub

        ' Event handlers for buttons
        Private Sub BtnSuccessAction_Click(sender As Object, e As RoutedEventArgs)
            HideModals()
            RaiseEvent SuccessConfirmed()
        End Sub

        Private Sub BtnErrorAction_Click(sender As Object, e As RoutedEventArgs)
            HideModals()
            RaiseEvent ErrorRetry()
        End Sub

        Private Sub BtnCloseSuccess_Click(sender As Object, e As RoutedEventArgs)
            HideModals()
        End Sub

        Private Sub BtnCloseError_Click(sender As Object, e As RoutedEventArgs)
            HideModals()
        End Sub

        ' Add keyboard support - close on Escape key
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Key = Key.Escape Then
                HideModals()
                e.Handled = True
            End If
        End Sub
    End Class
End Namespace