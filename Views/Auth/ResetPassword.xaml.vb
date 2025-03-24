Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Controllers

Namespace DPC.Views.Auth
    Public Class ResetPassword
        Private ReadOnly email As String

        ' Constructor
        Public Sub New(userEmail As String)
            InitializeComponent()
            email = userEmail
        End Sub

        ' Password Strength Checker
        Private Sub TxtNewPassword_PasswordChanged(sender As Object, e As RoutedEventArgs)
            Dim password As String = TxtNewPassword.Password
            LblPasswordStrength.Text = GetPasswordStrength(password)
            LblPasswordStrength.Foreground = GetStrengthColor(password)
        End Sub

        ' Get Password Strength
        Private Function GetPasswordStrength(password As String) As String
            If password.Length < 6 Then
                Return "Weak"
            ElseIf password.Length < 10 Then
                Return "Medium"
            Else
                Return "Strong"
            End If
        End Function

        ' Get Strength Color
        Private Function GetStrengthColor(password As String) As SolidColorBrush
            If password.Length < 6 Then
                Return New SolidColorBrush(Colors.Red)
            ElseIf password.Length < 10 Then
                Return New SolidColorBrush(Colors.Orange)
            Else
                Return New SolidColorBrush(Colors.Green)
            End If
        End Function

        ' Reset Password Button Click
        ' Reset Password Button Click
        Private Sub BtnResetPassword_Click(sender As Object, e As RoutedEventArgs)
            Dim newPassword As String = TxtNewPassword.Password
            Dim confirmPassword As String = TxtConfirmPassword.Password

            ' Validate input
            If String.IsNullOrEmpty(newPassword) OrElse String.IsNullOrEmpty(confirmPassword) Then
                MessageBox.Show("Password fields cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return
            End If

            If newPassword <> confirmPassword Then
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return
            End If

            ' Reset password in database
            If ResetPassController.ResetPassword(email, newPassword) Then
                ' Send password reset confirmation email
                Dim resetDate As DateTime = DateTime.Now
                Dim username As String = ResetPassController.GetUsernameByEmail(email) ' Fetch username

                Dim emailSent As Boolean = EmailService.SendPasswordResetConfirmation(email, username, resetDate)

                If emailSent Then
                    MessageBox.Show("Your password has been reset successfully! A confirmation email has been sent.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                Else
                    MessageBox.Show("Your password has been reset, but the confirmation email could not be sent.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                End If

                ' Redirect to Sign-In
                Dim signInWindow As New MainWindow()
                signInWindow.Show()
                Me.Close()
            Else
                MessageBox.Show("An error occurred. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End If
        End Sub

    End Class
End Namespace
