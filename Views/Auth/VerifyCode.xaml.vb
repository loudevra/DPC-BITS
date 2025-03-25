Imports System.Windows.Threading
Imports DPC.DPC.Data.Controllers

Namespace DPC.Views.Auth
    Public Class VerifyCode
        Private ReadOnly email As String
        Private countdownTime As Integer = 300 ' 5 minutes (300 seconds)
        Private ReadOnly timer As New DispatcherTimer()

        ' Constructor
        Public Sub New(userEmail As String)
            InitializeComponent()
            email = userEmail
            EmailDisplay.Text = $"A code was sent to {email.Substring(0, 3)}*****@{email.Split("@"c)(1)}"
            StartCountdown()
        End Sub

        ' Start countdown timer
        Private Sub StartCountdown()
            timer.Interval = TimeSpan.FromSeconds(1)
            AddHandler timer.Tick, AddressOf UpdateCountdown
            timer.Start()
        End Sub

        ' Update countdown label
        Private Sub UpdateCountdown(sender As Object, e As EventArgs)
            countdownTime -= 1
            Dim minutes As Integer = countdownTime \ 60
            Dim seconds As Integer = countdownTime Mod 60
            LblCountdown.Text = $"Code expires in: {minutes:D2}:{seconds:D2}"

            ' Disable verification if expired
            If countdownTime <= 0 Then
                timer.Stop()
                LblCountdown.Text = "Code expired. Request a new one."
                BtnVerify.IsEnabled = False
            End If
        End Sub

        ' Verify Code Button Click
        Private Sub BtnVerify_Click(sender As Object, e As RoutedEventArgs)
            Dim code As String = TxtVerificationCode.Text.Trim()

            ' Validate input
            If String.IsNullOrEmpty(code) OrElse code.Length <> 6 Then
                MessageBox.Show("Please enter a valid 6-digit code.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return
            End If

            ' Call ResetPassController to validate code
            If ResetPassController.ValidateVerificationCode(email, code) Then
                timer.Stop() ' Stop the countdown

                ' Proceed to Reset Password Window
                Dim resetPasswordWindow As New ResetPassword(email)
                resetPasswordWindow.Show()
                Me.Close()
            Else
                MessageBox.Show("Invalid or expired verification code. Try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End If
        End Sub

        ' Resend Code Button Click
        Private Sub BtnResendCode_Click(sender As Object, e As RoutedEventArgs)
            Dim newCode As String = ResetPassController.GenerateAndStoreVerificationCode(email)
            MessageBox.Show("A new verification code has been sent to your email.", "Code Resent", MessageBoxButton.OK, MessageBoxImage.Information)

            ' Restart countdown
            countdownTime = 300
            BtnVerify.IsEnabled = True
            StartCountdown()
        End Sub
    End Class
End Namespace
