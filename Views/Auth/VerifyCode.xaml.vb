Imports System.Windows.Threading
Imports DPC.DPC.Data.Controllers

Namespace DPC.Views.Auth
    Public Class VerifyCode
        Private ReadOnly email As String
        Private countdownTime As Integer = 300 ' 5 minutes
        Private ReadOnly timer As New DispatcherTimer()

        ' Constructor
        Public Sub New(userEmail As String)
            InitializeComponent()
            email = userEmail
            EmailDisplay.Text = $"A code was sent to {MaskEmail(email)}"
            StartCountdown()
        End Sub

        ' Mask email display for security
        Private Function MaskEmail(email As String) As String
            Dim parts = email.Split("@"c)
            If parts.Length = 2 Then
                Return $"{parts(0).Substring(0, 3)}*****@{parts(1)}"
            End If
            Return email
        End Function

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

            If countdownTime <= 0 Then
                timer.Stop()
                LblCountdown.Text = "Code expired. Request a new one."
                BtnVerify.IsEnabled = False
            End If
        End Sub

        ' Allow only digits and auto-move to the next box
        Private Sub OtpBox_PreviewTextInput(sender As Object, e As System.Windows.Input.TextCompositionEventArgs)
            Dim textBox As TextBox = CType(sender, TextBox)

            ' Allow only digits
            If Not Char.IsDigit(e.Text(0)) Then
                e.Handled = True
                Return
            End If

            ' Move to next box if the current box already has a digit
            If textBox.Text.Length >= 1 Then
                e.Handled = True
                MoveToNextBox(textBox, e.Text)
            End If
        End Sub

        ' Handle backspace and auto-focus previous box
        Private Sub OtpBox_KeyDown(sender As Object, e As System.Windows.Input.KeyEventArgs)
            Dim textBox As TextBox = CType(sender, TextBox)

            ' Move focus to the previous box if backspace is pressed and the box is empty
            If e.Key = Key.Back AndAlso textBox.Text = "" Then
                MoveToPreviousBox(textBox)
            End If
        End Sub

        ' Prevent pasting into OTP boxes
        Private Sub OtpBox_Pasting(sender As Object, e As DataObjectPastingEventArgs)
            e.CancelCommand()
        End Sub

        ' Move to the next OTP box
        Private Sub MoveToNextBox(textBox As TextBox, input As String)
            Select Case textBox.Name
                Case "OtpBox1" : OtpBox2.Focus()
                Case "OtpBox2" : OtpBox3.Focus()
                Case "OtpBox3" : OtpBox4.Focus()
                Case "OtpBox4" : OtpBox5.Focus()
                Case "OtpBox5" : OtpBox6.Focus()
            End Select
        End Sub

        ' Move to the previous OTP box when backspace is pressed
        Private Sub MoveToPreviousBox(textBox As TextBox)
            Select Case textBox.Name
                Case "OtpBox6" : OtpBox5.Focus()
                Case "OtpBox5" : OtpBox4.Focus()
                Case "OtpBox4" : OtpBox3.Focus()
                Case "OtpBox3" : OtpBox2.Focus()
                Case "OtpBox2" : OtpBox1.Focus()
            End Select
        End Sub


        ' Verify Code Button Click
        Private Sub BtnVerify_Click(sender As Object, e As RoutedEventArgs)
            Dim code As String = OtpBox1.Text & OtpBox2.Text & OtpBox3.Text & OtpBox4.Text & OtpBox5.Text & OtpBox6.Text

            If String.IsNullOrEmpty(code) OrElse code.Length <> 6 Then
                MessageBox.Show("Please enter a valid 6-digit code.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return
            End If

            If ResetPassController.ValidateVerificationCode(email, code) Then
                timer.Stop()

                ' Switch to Reset Password View
                Dim parentWindow As MainWindow = CType(Window.GetWindow(Me), MainWindow)
                parentWindow.LoadView(3, email)
            Else
                MessageBox.Show("Invalid or expired verification code. Try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End If
        End Sub

        ' Resend Code Button Click
        Private Sub BtnResendCode_Click(sender As Object, e As RoutedEventArgs)
            Dim newCode As String = ResetPassController.GenerateAndStoreVerificationCode(email)
            MessageBox.Show("A new verification code has been sent to your email.", "Code Resent", MessageBoxButton.OK, MessageBoxImage.Information)

            countdownTime = 300
            BtnVerify.IsEnabled = True
            StartCountdown()
        End Sub
    End Class
End Namespace
