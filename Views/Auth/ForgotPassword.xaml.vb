Imports System.ComponentModel
Imports System.Windows
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports NuGet.Common

Namespace DPC.Views.Auth
    Public Class ForgotPassword
        Implements INotifyPropertyChanged

        ' Property Change for Binding
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Private Sub RaisePropertyChanged(propertyName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub


        ' Send Code Command
        Public ReadOnly Property SendCodeCommand As New RelayCommand(AddressOf SendVerificationCode)

        Private Sub SendVerificationCode()
            Dim email As String = txtEmail.Text.Trim()

            If String.IsNullOrWhiteSpace(email) Then
                lblMessage.Text = "Email is required."
                lblMessage.Visibility = Visibility.Visible
                Return
            End If

            ' Check if email exists
            If Not ResetPassController.DoesEmailExist(email) Then
                lblMessage.Text = "Email not found."
                lblMessage.Visibility = Visibility.Visible
                Return
            End If

            ' Generate and send the code
            Dim verificationCode As String = ResetPassController.GenerateAndStoreVerificationCode(email)
            If String.IsNullOrEmpty(verificationCode) Then
                lblMessage.Text = "Failed to send verification code. Try again."
                lblMessage.Visibility = Visibility.Visible
            Else
                ' Navigate to verification screen
                Dim verifyCodeWindow As New VerifyCode(email)
                verifyCodeWindow.Show()
                Me.Close()
            End If
        End Sub

        Private Sub BtnContinue_Click(sender As Object, e As RoutedEventArgs)
            Dim email As String = txtEmail.Text.Trim()

            If ResetPassController.DoesEmailExist(email) Then
                ' Generate and send verification code
                ResetPassController.GenerateAndStoreVerificationCode(email)

                ' Open Verify Code Window
                Dim verifyWindow As New VerifyCode(email)
                verifyWindow.Show()
                Me.Close()
            Else
                MessageBox.Show("This email is not registered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End If
        End Sub
    End Class
End Namespace
