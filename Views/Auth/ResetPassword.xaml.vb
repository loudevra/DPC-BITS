Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Controllers
Imports System.Windows
Imports System.Text.RegularExpressions
Imports System.Windows.Media
Imports System.Windows.Controls
Imports System.Windows.Media.Imaging
Imports System.Windows.Media.Animation

Namespace DPC.Views.Auth
    Public Class ResetPassword
        Inherits UserControl

        Private ReadOnly email As String
        Private isNewPasswordVisible As Boolean = False
        Private isConfirmPasswordVisible As Boolean = False

        ' Workaround for PasswordBox visibility toggle
        Private NewPasswordText As New TextBox With {.Visibility = Visibility.Collapsed}
        Private ConfirmPasswordText As New TextBox With {.Visibility = Visibility.Collapsed}

        ' Constructor
        Public Sub New(userEmail As String)
            InitializeComponent()
            email = userEmail

            ' Ensure event handlers are set
            AddHandler TxtNewPassword.PasswordChanged, AddressOf TxtNewPassword_PasswordChanged
            AddHandler TxtConfirmPassword.PasswordChanged, AddressOf ValidateForm
        End Sub

        ' 🔹 Toggle New Password Visibility
        Private Sub BtnToggleNewPassword_Click(sender As Object, e As RoutedEventArgs)
            TogglePasswordVisibility(TxtNewPassword, NewPasswordText, IconToggleNewPassword, isNewPasswordVisible)
        End Sub

        ' 🔹 Toggle Confirm Password Visibility
        Private Sub BtnToggleConfirmPassword_Click(sender As Object, e As RoutedEventArgs)
            TogglePasswordVisibility(TxtConfirmPassword, ConfirmPasswordText, IconToggleConfirmPassword, isConfirmPasswordVisible)
        End Sub

        ' 🔹 Function to Toggle Password Visibility
        Private Sub TogglePasswordVisibility(passwordBox As PasswordBox, textBox As TextBox, icon As MaterialDesignThemes.Wpf.PackIcon, ByRef isVisible As Boolean)
            If isVisible Then
                ' Switch back to PasswordBox
                textBox.Visibility = Visibility.Collapsed
                passwordBox.Visibility = Visibility.Visible
                passwordBox.Password = textBox.Text
                icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOff
            Else
                ' Switch to TextBox
                textBox.Text = passwordBox.Password
                passwordBox.Visibility = Visibility.Collapsed
                textBox.Visibility = Visibility.Visible
                icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Eye
            End If
            isVisible = Not isVisible
        End Sub

        ' 🔹 Password Strength Checker
        Private Sub TxtNewPassword_PasswordChanged(sender As Object, e As RoutedEventArgs)
            If LblPasswordStrength IsNot Nothing Then
                Dim password As String = TxtNewPassword.Password
                Dim strength As String = GetPasswordStrength(password)
                LblPasswordStrength.Text = strength
                LblPasswordStrength.Foreground = GetStrengthColor(strength)
                ValidateForm()
            End If
        End Sub

        ' 🔹 Get Password Strength
        Private Function GetPasswordStrength(password As String) As String
            Dim score As Integer = 0

            If password.Length >= 8 Then score += 1
            If Regex.IsMatch(password, "[A-Z]") Then score += 1
            If Regex.IsMatch(password, "[a-z]") Then score += 1
            If Regex.IsMatch(password, "\d") Then score += 1
            If Regex.IsMatch(password, "[^a-zA-Z0-9]") Then score += 1

            Select Case score
                Case 1
                    Return "Weak"
                Case 2
                    Return "Fair"
                Case 3
                    Return "Good"
                Case 4
                    Return "Strong"
                Case 5
                    Return "Very Strong"
                Case Else
                    Return "Very Weak"
            End Select
        End Function

        ' 🔹 Get Strength Color
        Private Function GetStrengthColor(strength As String) As SolidColorBrush
            Select Case strength
                Case "Weak", "Very Weak"
                    Return New SolidColorBrush(Colors.Red)
                Case "Fair"
                    Return New SolidColorBrush(Colors.Orange)
                Case "Good"
                    Return New SolidColorBrush(Colors.Blue)
                Case "Strong", "Very Strong"
                    Return New SolidColorBrush(Colors.Green)
                Case Else
                    Return New SolidColorBrush(Colors.Gray)
            End Select
        End Function

        ' 🔹 Validate Form
        Private Sub ValidateForm()
            If BtnResetPassword IsNot Nothing Then
                Dim password As String = TxtNewPassword.Password
                Dim confirmPassword As String = TxtConfirmPassword.Password
                BtnResetPassword.IsEnabled = password.Length >= 8 AndAlso password = confirmPassword
            End If
        End Sub

        ' 🔹 Reset Password Button Click
        Private Sub BtnResetPassword_Click(sender As Object, e As RoutedEventArgs)
            Try
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


                ' Call ResetPassword function
                Dim isResetSuccessful As Boolean = ResetPassController.ResetPassword(email, newPassword)

                If isResetSuccessful Then
                    ' Send confirmation email
                    Dim resetDate As DateTime = DateTime.Now
                    Dim username As String = ResetPassController.GetUsernameByEmail(email)

                    Dim emailSent As Boolean = EmailService.SendPasswordResetConfirmation(email, username, resetDate)

                    If emailSent Then
                        MessageBox.Show("Your password has been reset successfully! A confirmation email has been sent.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    Else
                        MessageBox.Show("Your password has been reset, but the confirmation email could not be sent.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                    End If

                    ' Redirect to Sign-In
                    ' Switch to Reset Password View
                    Dim parentWindow As MainWindow = CType(Window.GetWindow(Me), MainWindow)
                    parentWindow.LoadView(0)
                Else
                    MessageBox.Show("Failed to reset password. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End If

            Catch ex As Exception
                ' Show error message and log it
                MessageBox.Show("An error occurred: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub



        ' Trigger animations on focus
        Private Sub TxtNewPassword_GotFocus(sender As Object, e As RoutedEventArgs)
            LblNewPassword.Foreground = Brushes.Black
            Dim anim As New DoubleAnimation(-5, TimeSpan.FromSeconds(0.2))
            LblNewPassword.RenderTransform.BeginAnimation(TranslateTransform.YProperty, anim)
            NewPasswordActiveUnderline.Opacity = 1
            NewPasswordActiveUnderline.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, New DoubleAnimation(1.1, TimeSpan.FromSeconds(0.2)))
        End Sub

        ' Reset label when field is empty
        Private Sub TxtNewPassword_LostFocus(sender As Object, e As RoutedEventArgs)
            If String.IsNullOrWhiteSpace(TxtNewPassword.Password) Then
                LblNewPassword.Foreground = Brushes.Gray
                Dim anim As New DoubleAnimation(20, TimeSpan.FromSeconds(0.2))
                LblNewPassword.RenderTransform.BeginAnimation(TranslateTransform.YProperty, anim)
            End If
            NewPasswordActiveUnderline.Opacity = 0
            NewPasswordActiveUnderline.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, New DoubleAnimation(0, TimeSpan.FromSeconds(0.2)))
        End Sub


        ' Trigger animations on focus
        Private Sub TxtConfirmPassword_GotFocus(sender As Object, e As RoutedEventArgs)
            LblConfirmPassword.Foreground = Brushes.Black
            Dim anim As New DoubleAnimation(-5, TimeSpan.FromSeconds(0.2))
            LblConfirmPassword.RenderTransform.BeginAnimation(TranslateTransform.YProperty, anim)
            ConfirmActiveUnderline.Opacity = 1
            ConfirmActiveUnderline.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, New DoubleAnimation(1.1, TimeSpan.FromSeconds(0.2)))
        End Sub

        ' Reset label when field is empty
        Private Sub TxtConfirmPassword_LostFocus(sender As Object, e As RoutedEventArgs)
            If String.IsNullOrWhiteSpace(TxtConfirmPassword.Password) Then
                LblConfirmPassword.Foreground = Brushes.Gray
                Dim anim As New DoubleAnimation(20, TimeSpan.FromSeconds(0.2))
                LblConfirmPassword.RenderTransform.BeginAnimation(TranslateTransform.YProperty, anim)
            End If
            ConfirmActiveUnderline.Opacity = 0
            ConfirmActiveUnderline.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, New DoubleAnimation(0, TimeSpan.FromSeconds(0.2)))
        End Sub



    End Class
End Namespace
